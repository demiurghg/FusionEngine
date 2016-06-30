using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Input;
using Fusion.Engine.Graphics;
using Fusion.Engine.Common;
using Forms = System.Windows.Forms;


namespace Fusion.Engine.Frames {

	class Manipulator {

		class Touch {	

			public readonly int ID;
			public readonly Vector2 Origin;
			public Vector2 NewLocation;
			public Vector2 OldLocation;

			public Touch ( int id, Point location ) 
			{
				ID = id;
				Origin		= new Vector2( location.X, location.Y );
				NewLocation = new Vector2( location.X, location.Y );
				OldLocation = new Vector2( location.X, location.Y );
			}

			public void Move ( Point newLocation ) 
			{
				NewLocation = new Vector2( newLocation.X, newLocation.Y );
			}
		}


		readonly Frame targetFrame;
		Dictionary<int,Touch> touches = new Dictionary<int,Touch>();


		Vector2 totalTranslation = Vector2.Zero;



		/// <summary>
		/// Indicates that manipulator has been activated.
		/// </summary>
		public bool Activated {
			get; private set;
		}

		/// <summary>
		/// Indicates that manipulator has been deactivated and all pointers are removed.
		/// </summary>
		public bool Deactivated {
			get; private set;
		}





		public Manipulator ( Frame frame )
		{
			Log.Message("Manipulator created");
			targetFrame = frame;
		}


		public void Update ()
		{
			if (!Activated) {
				foreach ( var touch in touches ) {
					var delta = touch.Value.NewLocation - touch.Value.Origin;
					if ( delta.Length() > 5 ) {
						Activate();
						break;
					}
				}

			} else {

				Vector2 oldCenter;
				Vector2 newCenter;

				oldCenter.X	=	touches.Average( p => p.Value.OldLocation.X );
				oldCenter.Y	=	touches.Average( p => p.Value.OldLocation.Y );
				newCenter.X	=	touches.Average( p => p.Value.NewLocation.X );
				newCenter.Y	=	touches.Average( p => p.Value.NewLocation.Y );

				var delta = newCenter - oldCenter;

				totalTranslation += delta;

				if (delta.Length()>1) {
					targetFrame.OnManipulationUpdate( totalTranslation, 1 );

					foreach ( var touch in touches ) {
						touch.Value.OldLocation = touch.Value.NewLocation;
					}
				}
			}

			//if (Activated
		}



		public void Activate()
		{
			Log.Message("Manipulator activated");
			Activated = true;

			targetFrame.OnManipulationStart( Vector2.Zero, 1 );
		}


		public void Stop()
		{
			targetFrame.OnManipulationEnd( totalTranslation, 1 );
			Log.Message("Manipulator stopped");
		}



		public int TouchDown ( int id, Point location )
		{	
			touches.Add( id, new Touch( id, location ) );
			return touches.Count;
		}


		public int TouchUp ( int id, Point location )
		{
			touches.Remove( id );
			return touches.Count;
		}


		public void TouchMove ( int id, Point location )
		{
			touches[ id ].Move( location );
		}


	}
}

