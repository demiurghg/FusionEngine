using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Input;
using Fusion.Engine.Common;
using System.Diagnostics;


namespace Fusion.Engine.Frames {

	class MouseProcessor {

		public readonly Game Game;
		public FrameProcessor ui;

		Point	oldMousePoint;

		Frame	hoveredFrame;
		Frame	heldFrame		=	null;
		bool	heldFrameLBM	=	false;
		bool	heldFrameRBM	=	false;
		Point	heldPoint;


		static int SysInfoDoubleClickTime {
			get {
				return System.Windows.Forms.SystemInformation.DoubleClickTime;
			}
		}


		static int SysInfoDoubleClickWidth {
			get {
				return System.Windows.Forms.SystemInformation.DoubleClickSize.Width;
			}
		}

		static int SysInfoDoubleClickHeight {
			get {
				return System.Windows.Forms.SystemInformation.DoubleClickSize.Height;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public MouseProcessor ( Game game, FrameProcessor ui )
		{
			this.Game	=	game;
			this.ui		=	ui;
		}



		/// <summary>
		/// 
		/// </summary>
		public void Initialize ()
		{
			Game.Keyboard.KeyDown += InputDevice_KeyDown;
			Game.Keyboard.KeyUp += InputDevice_KeyUp;
			Game.Mouse.Scroll += InputDevice_MouseScroll;
			Game.Mouse.Move += Mouse_Move;


			//Game.Touch.PointerDown += Touch_PointerDown;
			//Game.Touch.PointerUp += Touch_PointerUp;
			//Game.Touch.PointerUpdate += Touch_PointerUpdate;


			oldMousePoint	=	Game.InputDevice.MousePosition;
		}


		//void Touch_PointerDown ( object sender, Touch.TouchEventArgs e )
		//{
		//	PushFrame( ui.GetHoveredFrame(e.Location), Keys.LeftButton, e.Location );
		//}


		//void Touch_PointerUp ( object sender, Touch.TouchEventArgs e )
		//{
		//	ReleaseFrame( ui.GetHoveredFrame(e.Location), Keys.LeftButton, e.Location );
		//	Update( e.Location, true );
		//}


		//void Touch_PointerUpdate ( object sender, Touch.TouchEventArgs e )
		//{
		//	Update( e.Location );
		//}



		void Mouse_Move ( object sender, MouseMoveEventArgs e )
		{
			Update( new Point( (int)e.Position.X, (int)e.Position.Y ) );
		}


		void InputDevice_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.LeftButton || e.Key==Keys.RightButton) {
				PushFrame( ui.GetHoveredFrame(Game.Mouse.Position), e.Key, Game.Mouse.Position );
			}
		}


		void InputDevice_KeyUp ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.LeftButton || e.Key==Keys.RightButton) {
				ReleaseFrame( ui.GetHoveredFrame(Game.Mouse.Position), e.Key, Game.Mouse.Position);
			}
		}



		void InputDevice_MouseScroll ( object sender, MouseScrollEventArgs e )
		{
			var hovered = ui.GetHoveredFrame(Game.Mouse.Position);
			if ( hovered!=null ) {
				hovered.OnMouseWheel( e.WheelDelta );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		public void Update ( Point mousePoint, bool releaseTouch = false )
		{
			var root		=	ui.RootFrame;

			var hovered		=	ui.GetHoveredFrame(mousePoint);
			
			//
			//	update mouse move :
			//	
			if ( mousePoint!=oldMousePoint ) {
				
				int dx =	mousePoint.X - oldMousePoint.X;
				int dy =	mousePoint.Y - oldMousePoint.Y;

				if (heldFrame!=null) {
					heldFrame.OnMouseMove( mousePoint, dx, dy );
				} else if ( hovered!=null ) {
					hovered.OnMouseMove( mousePoint, dx, dy );
				}

				oldMousePoint = mousePoint;
			}

			//
			//	Mouse down/up events :
			//
			if (heldFrame==null) {
				var oldHoveredFrame	=	hoveredFrame;
				var newHoveredFrame	=	ui.GetHoveredFrame(mousePoint);

				hoveredFrame		=	newHoveredFrame;

				if (oldHoveredFrame!=newHoveredFrame) {

					CallMouseOut		( mousePoint, oldHoveredFrame );
					CallStatusChanged	( mousePoint, oldHoveredFrame, FrameStatus.None );

					CallMouseIn			( mousePoint, newHoveredFrame );
					CallStatusChanged	( mousePoint, newHoveredFrame, FrameStatus.Hovered );
				}
			}
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		Stopwatch	doubleClickStopwatch	=	new Stopwatch();
		Frame		doubleClickPushedFrame;
		Keys		doubleClickButton;
		Point		doubleClickPosition;


		/// <summary>
		/// Holds frame
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="key"></param>
		void PushFrame ( Frame currentHovered, Keys key, Point location )
		{

			//	frame pushed:
			if (currentHovered!=null) {

				if (heldFrame!=currentHovered) {
					heldPoint = location;
				}

				//	record pushed frame :
				if (heldFrame==null) {
					heldFrame		=	currentHovered;
				}

				if (key==Keys.LeftButton) {
					heldFrameLBM	=	true;
				}

				if (key==Keys.RightButton) {
					heldFrameRBM	=	true;
				}

				CallMouseDown		( location, heldFrame, key );
				CallStatusChanged	( location, heldFrame, FrameStatus.Pushed );
			}
		}



		static bool IsPointWithinDoubleClickArea ( Point a, Point b )
		{
			var dx = Math.Abs(a.X - b.X);
			var dy = Math.Abs(a.Y - b.Y);
			return (dx<SysInfoDoubleClickWidth && dy<SysInfoDoubleClickHeight);
		}



		/// <summary>
		/// Releases frame
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="key"></param>
		void ReleaseFrame ( Frame currentHovered, Keys key, Point mousePosition )
		{
			//	no frame is held, ignore :
			if (heldFrame==null) {
				return;
			}

			if (key==Keys.LeftButton) {
				heldFrameLBM	=	false;
			}

			if (key==Keys.RightButton) {
				heldFrameRBM	=	false;
			}

			//	call MouseUp :
			CallMouseUp( mousePosition, heldFrame, key );

			//	button are still pressed, no extra action :
			if ( heldFrameLBM || heldFrameRBM ) {
				return;
			}

			//	do stuff :
			hoveredFrame	=	currentHovered;

			if ( currentHovered!=heldFrame ) {
				
				CallMouseOut		( mousePosition, heldFrame );
				CallStatusChanged	( mousePosition, heldFrame, FrameStatus.None );
				CallMouseIn			( mousePosition, currentHovered );
				CallStatusChanged	( mousePosition, currentHovered, FrameStatus.Hovered );

			} else {

				//	track activation/deactivation on click :
				ui.TargetFrame = currentHovered;

				//	track double clicks :
				bool doubleClick	=	false;

				//Log.Verbose("DC: {0} {1}", doubleClickStopwatch.Elapsed, SysInfoDoubleClickTime );

				if ( (currentHovered==doubleClickPushedFrame)
					&& (currentHovered.IsDoubleClickEnabled) 
					&& (doubleClickButton==key) 
					&& (doubleClickStopwatch.ElapsedMilliseconds < SysInfoDoubleClickTime) 
					&& IsPointWithinDoubleClickArea( doubleClickPosition, mousePosition ) 
				) {
					doubleClick				=	true;
					doubleClickStopwatch.Restart();
					doubleClickPushedFrame	=	null;
					doubleClickButton		=	Keys.None;
					doubleClickPosition		=	mousePosition;
				} else {
					doubleClickStopwatch.Restart();
					doubleClickPushedFrame	=	currentHovered;
					doubleClickButton		=	key;
					doubleClickPosition		=	mousePosition;
				}

				//	handle click :
				CallStatusChanged	( mousePosition, heldFrame, FrameStatus.Hovered );
				CallClick			( mousePosition, heldFrame, key, doubleClick );
			}

			heldFrame	=	null;
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Callers :
		 * 
		-----------------------------------------------------------------------------------------*/

		void CallClick ( Point location, Frame frame, Keys key, bool doubleClick )
		{
			if (frame!=null && frame.CanAcceptControl) {
				frame.OnClick(location, key, doubleClick);
			}
		}

		void CallMouseDown ( Point location, Frame frame, Keys key ) 
		{
			if (frame!=null && frame.CanAcceptControl) {
				frame.OnMouseDown( location, key );
			}
		}

		void CallMouseUp ( Point location, Frame frame, Keys key ) 
		{
			if (frame!=null) {
				frame.OnMouseUp( location, key );
			}
		}

		void CallMouseIn ( Point location, Frame frame ) 
		{
			if (frame!=null && frame.CanAcceptControl) {
				frame.OnMouseIn(location);
			}
		}

		void CallMouseOut ( Point location, Frame frame ) 
		{
			if (frame!=null) {
				frame.OnMouseOut(location);
			}
		}

		void CallStatusChanged ( Point location, Frame frame, FrameStatus status )
		{
			if (frame!=null) {
				frame.ForEachAncestor( f => f.OnStatusChanged( status ) );
			}
		}

	}
}
