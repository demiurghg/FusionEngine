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


		int SysInfoDoubleClickTime {
			get {
				return System.Windows.Forms.SystemInformation.DoubleClickTime;
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

			oldMousePoint	=	Game.InputDevice.MousePosition;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		public void Update ( Frame root )
		{
			var mousePoint	=	Game.InputDevice.MousePosition;

			var hovered		=	GetHoveredFrame();
			
			//
			//	update mouse move :
			//	
			if ( mousePoint!=oldMousePoint ) {
				
				int dx =	mousePoint.X - oldMousePoint.X;
				int dy =	mousePoint.Y - oldMousePoint.Y;

				if (heldFrame!=null) {
					heldFrame.OnMouseMove( dx, dy );
				} else if ( hovered!=null ) {
					hovered.OnMouseMove( dx, dy );
				}

				oldMousePoint = mousePoint;
			}

			//
			//	Mouse down/up events :
			//
			if (heldFrame==null) {
				var oldHoveredFrame	=	hoveredFrame;
				var newHoveredFrame	=	GetHoveredFrame();

				hoveredFrame		=	newHoveredFrame;

				if (oldHoveredFrame!=newHoveredFrame) {

					CallMouseOut		( oldHoveredFrame );
					CallStatusChanged	( oldHoveredFrame, FrameStatus.None );

					CallMouseIn			( newHoveredFrame );
					CallStatusChanged	( newHoveredFrame, FrameStatus.Hovered );
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


		/// <summary>
		/// Holds frame
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="key"></param>
		void PushFrame ( Frame currentHovered, Keys key )
		{

			//	frame pushed:
			if (currentHovered!=null) {

				if (heldFrame!=currentHovered) {
					heldPoint = Game.InputDevice.MousePosition;
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

				CallMouseDown		( heldFrame, key );
				CallStatusChanged	( heldFrame, FrameStatus.Pushed );
			}
		}


		/// <summary>
		/// Releases frame
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="key"></param>
		void ReleaseFrame ( Frame currentHovered, Keys key )
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
			CallMouseUp( heldFrame, key );

			//	button are still pressed, no extra action :
			if ( heldFrameLBM || heldFrameRBM ) {
				return;
			}

			//	do stuff :
			hoveredFrame	=	currentHovered;

			if ( currentHovered!=heldFrame ) {
				
				CallMouseOut		( heldFrame );
				CallStatusChanged	( heldFrame, FrameStatus.None );
				CallMouseIn			( currentHovered );
				CallStatusChanged	( currentHovered, FrameStatus.Hovered );

			} else {

				//	track activation/deactivation on click :
				ui.TargetFrame = currentHovered;

				//	track double clicks :
				bool doubleClick = false;

				Log.Verbose("DC: {0} {1}", doubleClickStopwatch.Elapsed, SysInfoDoubleClickTime );

				if ( (currentHovered==doubleClickPushedFrame) && (doubleClickButton==key) && (doubleClickStopwatch.ElapsedMilliseconds < SysInfoDoubleClickTime) ) {
					doubleClick				=	true;
					doubleClickStopwatch.Restart();
					doubleClickPushedFrame	=	null;
					doubleClickButton		=	Keys.None;
				} else {
					doubleClickStopwatch.Restart();
					doubleClickPushedFrame	=	currentHovered;
					doubleClickButton		=	key;
				}

				//	handle click :
				CallStatusChanged	( heldFrame, FrameStatus.Hovered );
				CallClick			( heldFrame, doubleClick );
			}

			heldFrame	=	null;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.LeftButton || e.Key==Keys.RightButton) {
				PushFrame( GetHoveredFrame(), e.Key );
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyUp ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.LeftButton || e.Key==Keys.RightButton) {
				ReleaseFrame( GetHoveredFrame(), e.Key );
			}
		}



		void InputDevice_MouseScroll ( object sender, MouseScrollEventArgs e )
		{
			var hovered = GetHoveredFrame();
			if ( hovered!=null ) {
				hovered.OnMouseWheel( e.WheelDelta );
			}
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Callers :
		 * 
		-----------------------------------------------------------------------------------------*/

		void CallClick ( Frame frame, bool doubleClick )
		{
			if (frame!=null && frame.CanAcceptControl) {
				frame.OnClick(doubleClick);
			}
		}

		void CallMouseDown ( Frame frame, Keys key ) 
		{
			if (frame!=null && frame.CanAcceptControl) {
				frame.OnMouseDown( key );
			}
		}

		void CallMouseUp ( Frame frame, Keys key ) 
		{
			if (frame!=null) {
				frame.OnMouseUp( key );
			}
		}

		void CallMouseIn ( Frame frame ) 
		{
			if (frame!=null && frame.CanAcceptControl) {
				frame.OnMouseIn();
			}
		}

		void CallMouseOut ( Frame frame ) 
		{
			if (frame!=null) {
				frame.OnMouseOut();
			}
		}

		void CallStatusChanged ( Frame frame, FrameStatus status )
		{
			if (frame!=null) {
				frame.ForEachAncestor( f => f.OnStatusChanged( status ) );
			}
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		Frame GetHoveredFrame ()
		{
			Frame mouseHoverFrame = null;

			UpdateHoverRecursive( ui.RootFrame, Game.InputDevice.MousePosition, ref mouseHoverFrame );

			return mouseHoverFrame;
		}



		/// <summary>
		/// Updates current hovered frame
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="viewCtxt"></param>
		void UpdateHoverRecursive ( Frame frame, Point p, ref Frame mouseHoverFrame )
		{
			if (frame==null) {
				return;
			}

			var absLeft		=	frame.GlobalRectangle.Left;
			var absTop		=	frame.GlobalRectangle.Top;
			var absRight	=	frame.GlobalRectangle.Right;
			var absBottom	=	frame.GlobalRectangle.Bottom;

			if (!frame.CanAcceptControl) {
				return;
			}
			
			bool hovered	=	p.X >= absLeft 
							&&	p.X <  absRight 
							&&	p.Y >= absTop
							&&	p.Y <  absBottom;

			if (hovered) {
				mouseHoverFrame = frame;
				foreach (var child in frame.Children) {
					UpdateHoverRecursive( child, p, ref mouseHoverFrame );
				}
			}

		}
	}
}
