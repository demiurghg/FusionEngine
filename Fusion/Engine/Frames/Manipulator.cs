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

			public float GetScaling ( Vector2 oldCenter, Vector2 newCenter )
			{
				var oldDistance = Vector2.Distance( OldLocation, oldCenter );
				var newDistance = Vector2.Distance( NewLocation, newCenter );

				if (newDistance<1 || oldDistance<1) {
					return 1;
				}
				return (newDistance / oldDistance);
			}
		}


		readonly Frame targetFrame;
		Dictionary<int,Touch> touches = new Dictionary<int,Touch>();


		Vector2 totalTranslation = Vector2.Zero;
		float	totalScaling = 1;


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


		/// <summary>
		/// 
		/// </summary>
		/// <param name="frame"></param>
		public Manipulator ( Frame frame )
		{
			targetFrame = frame;
		}


		/// <summary>
		/// Updates manipulator state on each game update
		/// </summary>
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

				var deltaScaling		=	touches.Average( p => p.Value.GetScaling( oldCenter, newCenter ) );
				var deltaTranlsation	=	newCenter - oldCenter;

				totalTranslation += deltaTranlsation;
				totalScaling *= deltaScaling;

				if (deltaTranlsation.Length()>0 || deltaScaling!=1) {

					//Log.Message("Delta: {0} {1}", delta.X, delta.Y );

					targetFrame.OnManipulationUpdate( totalTranslation, totalScaling, deltaTranlsation, deltaScaling );

					foreach ( var touch in touches ) {
						touch.Value.OldLocation = touch.Value.NewLocation;
					}
				}
			}
		}



		/// <summary>
		/// Activates manipulator
		/// </summary>
		public void Activate()
		{
			Activated = true;

			targetFrame.OnManipulationStart( Vector2.Zero, 1, Vector2.Zero, 1 );
		}


		/// <summary>
		/// 
		/// </summary>
		public void Stop()
		{
			targetFrame.OnManipulationEnd( totalTranslation, totalScaling, Vector2.Zero, 1 );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="location"></param>
		/// <returns></returns>
		public int TouchDown ( int id, Point location )
		{	
			touches.Add( id, new Touch( id, location ) );
			return touches.Count;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="location"></param>
		/// <returns></returns>
		public int TouchUp ( int id, Point location )
		{
			touches.Remove( id );
			return touches.Count;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="location"></param>
		public void TouchMove ( int id, Point location )
		{
			touches[ id ].Move( location );
		}


	}
}

