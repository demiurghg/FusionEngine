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

	class TouchProcessor {

		public readonly Game Game;
		public FrameProcessor ui;


		class TouchRecord {
			public TouchRecord( int id, Point location, Frame frame ) 
			{
				Current		=	location;
				Location	=	location;
				Frame		=	frame;
			}
			public readonly int ID;
			public readonly Point Location;
			public Point Current;
			public readonly Frame Frame;
		}


		Dictionary<int,TouchRecord> touchRecords = new Dictionary<int,TouchRecord>();
		Dictionary<Frame,Manipulator> manipulators = new Dictionary<Frame,Manipulator>();


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public TouchProcessor ( Game game, FrameProcessor ui )
		{
			this.Game	=	game;
			this.ui		=	ui;
		}



		/// <summary>
		/// 
		/// </summary>
		public void Initialize ()
		{
			Game.Touch.PointerDown += Touch_PointerDown;
			Game.Touch.PointerUp += Touch_PointerUp;
			Game.Touch.PointerUpdate += Touch_PointerUpdate;
		}


		void Touch_PointerDown ( object sender, Touch.TouchEventArgs e )
		{
			PushFrame( ui.GetHoveredFrame(e.Location), e.PointerID, e.Location );
		}


		void Touch_PointerUp ( object sender, Touch.TouchEventArgs e )
		{
			ReleaseFrame( ui.GetHoveredFrame(e.Location), e.PointerID, e.Location );
		}


		void Touch_PointerUpdate ( object sender, Touch.TouchEventArgs e )
		{
			UpdateFrame( e.PointerID, e.Location );
		}




		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		public void UpdateManipulations ( GameTime gameTime )
		{
			foreach ( var m in manipulators ) {
				m.Value.Update();
			}
		}






		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		public void UpdateFrame ( int pointerId, Point location )
		{
			TouchRecord touchRecord;

			if (!touchRecords.TryGetValue( pointerId, out touchRecord )) {
				return;
			} 

			if (touchRecord.Frame==null) {
				return;
			}

			//	do not call event if location not changed
			/*if (touchRecord.Current==location) {
				return;
			}*/
			touchRecord.Current = location;

			
			var hoveredFrame = ui.GetHoveredFrame( location );

			//
			//	Touch :
			//
			if ( hoveredFrame == touchRecord.Frame ) {
				touchRecord.Frame.OnTouchMove( pointerId, location );
			}

			//
			//	Manipulator :
			//	
			Manipulator m;
			if (manipulators.TryGetValue( touchRecord.Frame, out m )) {
				m.TouchMove( pointerId, location );
			}
		}



		/// <summary>
		/// Holds frame
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="key"></param>
		void PushFrame ( Frame currentHovered, int pointerId, Point location )
		{
			var hoveredFrame = ui.GetHoveredFrame( location );

			if (hoveredFrame==null) {
				//	ignore touch if no frame is pushed.
				return;
			}

			touchRecords.Add( pointerId, new TouchRecord(pointerId, location, hoveredFrame) ); 

			//
			//	Touch events :
			//
			hoveredFrame.OnTouchDown( pointerId, location );
			hoveredFrame.OnStatusChanged( FrameStatus.Pushed );

			//
			//	Manipulator :
			//
			if (!manipulators.ContainsKey(hoveredFrame)) {
				var m = new Manipulator( hoveredFrame );
				manipulators.Add( hoveredFrame, m );
			} 

			manipulators[hoveredFrame].TouchDown( pointerId, location );
		}



		/// <summary>
		/// Releases frame
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="key"></param>
		void ReleaseFrame ( Frame currentHovered, int pointerId, Point location )
		{
			TouchRecord touchRecord;

			if (!touchRecords.TryGetValue( pointerId, out touchRecord )) {
				return;
			} 

			if (touchRecord.Frame==null) {
				return;
			}
			
			var hoveredFrame = ui.GetHoveredFrame( location );

			//
			//	Touch events:
			//
			if (hoveredFrame==touchRecord.Frame) {

				touchRecord.Frame.OnTouchUp( pointerId, location );
				touchRecord.Frame.OnTap( pointerId, location );
				touchRecord.Frame.OnStatusChanged( FrameStatus.None );

			} else {
				
				touchRecord.Frame.OnTouchUp( pointerId, location );
				touchRecord.Frame.OnStatusChanged( FrameStatus.None );

			}

			//
			//	Manipulator :
			//
			Manipulator m;

			if (manipulators.TryGetValue( touchRecord.Frame, out m )) {
				
				int count = m.TouchUp( pointerId, location );
				
				if (count==0) {	
					Log.Message("Drop manipulator");
					m.Stop();
					manipulators.Remove( touchRecord.Frame );
				}
			}
		}


	}
}
