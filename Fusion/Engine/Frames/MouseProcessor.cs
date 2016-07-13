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
using Size2 = Fusion.Core.Mathematics.Size2;
using SysInfo = System.Windows.Forms.SystemInformation;


namespace Fusion.Engine.Frames {

	class MouseProcessor {

		public readonly Game Game;
		public FrameProcessor ui;

		Point	oldMousePoint;

		Frame		hoveredFrame;
		Frame		heldFrame			=	null;
		bool		heldFrameLBM		=	false;
		bool		heldFrameRBM		=	false;
		bool		heldFrameDragging	=	false;
		Point		heldPoint;
		Rectangle	heldRectangle;


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

			oldMousePoint	=	Game.InputDevice.MousePosition;
		}


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


		Size2 PointDelta ( Point begin, Point end )
		{
			return new Size2( end.X - begin.X, end.Y - begin.Y );
		}


		static bool IsPointWithinDoubleClickArea ( Point a, Point b )
		{
			var dx = Math.Abs(a.X - b.X);
			var dy = Math.Abs(a.Y - b.Y);

			return ( dx < SysInfo.DoubleClickSize.Width && dy < SysInfo.DoubleClickSize.Height );
		}


		static bool IsPointWithinDragSize ( Point a, Point b )
		{
			var dx = Math.Abs(a.X - b.X);
			var dy = Math.Abs(a.Y - b.Y);

			return ( dx < SysInfo.DragSize.Width && dy < SysInfo.DragSize.Height );
		}


		static Rectangle MoveRectangle( Rectangle rectangle, Point origin, Point target )
		{
			var dx	= target.X - origin.X;
			var dy	= target.Y - origin.Y;

			return new Rectangle( rectangle.X + dx, rectangle.Y + dy, rectangle.Width, rectangle.Height );

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
					heldRectangle	=	heldFrame.GlobalRectangle;
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
				
				int dx		=	mousePoint.X - oldMousePoint.X;
				int dy		=	mousePoint.Y - oldMousePoint.Y;
				var dragDX  =	mousePoint.X - heldPoint.X;
				var dragDY  =	mousePoint.Y - heldPoint.Y;

				if (heldFrame!=null) {

					// move :
					heldFrame.OnMouseMove( mousePoint, dx, dy );

					// drag :

					if (!IsPointWithinDragSize(heldPoint, mousePoint) && !heldFrameDragging && heldFrame.IsDragEnabled) {
						heldFrame.OnDragBegin( mousePoint, MoveRectangle(heldRectangle, heldPoint, mousePoint), dragDX, dragDY );
						heldFrameDragging = true;
					}

					if (heldFrameDragging) {
						heldFrame.OnDragUpdate( mousePoint, MoveRectangle(heldRectangle, heldPoint, mousePoint), dragDX, dragDY );
					}

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



			if ( currentHovered!=heldFrame) {
				
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
				CallStatusChanged( mousePosition, heldFrame, FrameStatus.Hovered );

				if (!heldFrameDragging) {
					CallClick( mousePosition, heldFrame, key, doubleClick );
				}
			}


			//	end draging :
			var dragDX  =	mousePosition.X - heldPoint.X;
			var dragDY  =	mousePosition.Y - heldPoint.Y;

			if (heldFrameDragging) {
				heldFrame.OnDragEnd( mousePosition, MoveRectangle(heldRectangle, heldPoint, mousePosition), dragDX, dragDY );
				heldFrameDragging = false;
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
