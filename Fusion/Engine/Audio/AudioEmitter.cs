using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Audio
{
	internal class AudioEmitter
	{
		/// <summary>
		/// 
		/// </summary>
		public AudioEmitter ()
		{
            dopplerScale = 1.0f;
			Forward = Vector3.ForwardRH;
			Position = Vector3.Zero;
			Up = Vector3.Up;
			Velocity = Vector3.Zero;
			DistanceScale  = 1;
		}

        private float dopplerScale;
		
		/// <summary>
		/// Doppler scale
		/// </summary>
		public float DopplerScale {
            get	{
                return dopplerScale;
            }

            set {
                if (value < 0.0f) {
                    throw new ArgumentOutOfRangeException("AudioEmitter.DopplerScale must be greater than or equal to 0.0f");
				}

                dopplerScale = value;
            }
		}

		/// <summary>
		/// Emitter's forward direction.
		/// </summary>
		public Vector3 Forward {
			get;
			set;
		}

		/// <summary>
		/// Emitter's position.
		/// </summary>
		public Vector3 Position {
			get;
			set;
		}

		/// <summary>
		/// Emitter's up.
		/// </summary>
		public Vector3 Up {
			get;
			set;
		}

		/// <summary>
		/// Absolute emitter velocity.
		/// </summary>
		public Vector3 Velocity {
			get;
			set;
		}

		/// <summary>
		/// Local emitter distance scale
		/// </summary>
		public float DistanceScale {
			get; set;
		}



		private SharpDX.X3DAudio.CurvePoint[] volumeCurve = null;

		/// <summary>
		/// Volume falloff curve. Null value means inverse square law.
		/// </summary>
		public CurvePoint[] VolumeCurve {
			set {
				volumeCurve	=	SharpDXHelper.Convert( value );
			} 
			get {
				return SharpDXHelper.Convert( volumeCurve );
			}
		}



		/// <summary>
		/// Converts to X3DAudio emitter.
		/// </summary>
		/// <returns></returns>
        internal SharpDX.X3DAudio.Emitter ToEmitter()
        {           
            // Pulling out Vector properties for efficiency.
            var pos = this.Position;
            var vel = this.Velocity;
            var fwd = this.Forward;
            var up = this.Up;

            // From MSDN:
            //  X3DAudio uses a left-handed Cartesian coordinate system, 
            //  with values on the x-axis increasing from left to right, on the y-axis from bottom to top, 
            //  and on the z-axis from near to far. 
            //  Azimuths are measured clockwise from a given reference direction. 
            //
            // From MSDN:
            //  The XNA Framework uses a right-handed coordinate system, 
            //  with the positive z-axis pointing toward the observer when the positive x-axis is pointing to the right, 
            //  and the positive y-axis is pointing up. 
            //
            // Programmer Notes:         
            //  According to this description the z-axis (forward vector) is inverted between these two coordinate systems.
            //  Therefore, we need to negate the z component of any position/velocity values, and negate any forward vectors.

            fwd *= -1.0f;
            pos.Z *= -1.0f;
            vel.Z *= -1.0f;

			var emitter =	 new SharpDX.X3DAudio.Emitter();

			emitter.ChannelCount		=	1;
			emitter.Position			=	SharpDXHelper.Convert( pos );
			emitter.Velocity			=	SharpDXHelper.Convert( vel );
			emitter.OrientFront			=	SharpDXHelper.Convert( fwd );
			emitter.OrientTop			=	SharpDXHelper.Convert( up );
			emitter.DopplerScaler		=	DopplerScale;
			emitter.CurveDistanceScaler	=	DistanceScale;
			emitter.VolumeCurve			=	volumeCurve;

            return emitter;
        }


	}
}
