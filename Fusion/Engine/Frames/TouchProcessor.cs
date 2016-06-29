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
				Location	=	location;
				Frame		=	frame;
			}
			public readonly int ID;
			public readonly Point Location;
			public readonly Frame Frame;
		}


		class Manipulation {

			public Manipulation ( Frame frame ) 
			{
				TargetFrame = frame;
				TotalTranslation	=	Vector2.Zero;
				TotalScaling		=	1;
			}

			public readonly Frame TargetFrame;
			public Vector2  TotalTranslation;
			public float    TotalScaling;
		}


		Dictionary<int,TouchRecord> touchRecords = new Dictionary<int,TouchRecord>();
		Dictionary<Frame,Manipulation> manipulations = new Dictionary<Frame,Manipulation>();


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
			Log.Message("PointerDown:   {0} {1} {2}", e.PointerID, e.Location.X, e.Location.Y );
			PushFrame( ui.GetHoveredFrame(e.Location), e.PointerID, e.Location );
		}


		void Touch_PointerUp ( object sender, Touch.TouchEventArgs e )
		{
			Log.Message("PointerUp:     {0} {1} {2}", e.PointerID, e.Location.X, e.Location.Y );
			ReleaseFrame( ui.GetHoveredFrame(e.Location), e.PointerID, e.Location );
		}


		void Touch_PointerUpdate ( object sender, Touch.TouchEventArgs e )
		{
			Log.Message("PointerUpdate: {0} {1} {2}", e.PointerID, e.Location.X, e.Location.Y );
			UpdateFrame( e.PointerID, e.Location );
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
		public void UpdateFrame ( int pointerId, Point location )
		{
			TouchRecord touchRecord;

			if (!touchRecords.TryGetValue( pointerId, out touchRecord )) {
				return;
			} 

			if (touchRecord.Frame==null) {
				return;
			}
			
			var hoveredFrame = ui.GetHoveredFrame( location );

			if ( hoveredFrame == touchRecord.Frame ) {
				touchRecord.Frame.OnTouchMove( pointerId, location );
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

			//	update simple touch :
			hoveredFrame.OnTouchDown( pointerId, location );
			hoveredFrame.OnStatusChanged( FrameStatus.Pushed );


			//	update manipulation :
			if (!manipulations.ContainsKey(hoveredFrame)) {
				
				var m = new Manipulation( hoveredFrame );
				manipulations.Add( m.TargetFrame, m );
				m.TargetFrame.OnManipulationStart( m.TotalTranslation, m.TotalScaling );
			}
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


			if (hoveredFrame==touchRecord.Frame) {

				touchRecord.Frame.OnTouchUp( pointerId, location );
				touchRecord.Frame.OnTap( pointerId, location );
				touchRecord.Frame.OnStatusChanged( FrameStatus.None );

			} else {
				
				touchRecord.Frame.OnTouchUp( pointerId, location );
				touchRecord.Frame.OnStatusChanged( FrameStatus.None );

			}
		}


	}
}
