using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Audio {

	/// <summary>
	/// Represnts audio emitter, the entity that emit sound in physical world.
	/// </summary>
	public sealed class AudioEmitter {

		SoundEffectInstance	soundInstance;


		/// <summary>
		/// 
		/// </summary>
		internal AudioEmitter ()
		{
            dopplerScale = 1.0f;
			Position = Vector3.Zero;
			Velocity = Vector3.Zero;
			DistanceScale  = 1;
		}




		/// <summary>
		/// Starts playing sound
		/// </summary>
		/// <param name="soundEffect"></param>
		/// <param name="options"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		public void PlaySound ( SoundEffect soundEffect, PlayOptions options = PlayOptions.None )
		{
			soundInstance	=	soundEffect.CreateInstance();

			soundInstance.IsLooped	=	options.HasFlag(PlayOptions.Looped);

			soundInstance.Play(); 
		}



		/// <summary>
		/// Stops playing sound
		/// </summary>
		/// <param name="immediate"></param>
		public void StopSound ( bool immediate )
		{
			if (soundInstance!=null) {
				soundInstance.Stop( immediate );
				soundInstance	=	null;
			}
		}



		internal void Pause ()
		{
			if (soundInstance!=null) {
				soundInstance.Pause();
			}
		}



		internal void Resume ()
		{
			if (soundInstance!=null) {
				soundInstance.Resume();
			}
		}



		/// <summary>
		/// Gets sound state.
		/// </summary>
		public SoundState SoundState {
			get {
				return soundInstance.State;
			}
		}



		/// <summary>
		/// Sets and gets Doppler scale.
		/// Value must be non-negative.
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

        float dopplerScale;


		/// <summary>
		/// Gets and sets whether sound should be played locally on applying 3D effects.
		/// </summary>
		public bool LocalSound {
			get; set;
		}
		

		/// <summary>
		/// Gets and sets emitter's position.
		/// </summary>
		public Vector3 Position {
			get;
			set;
		}


		/// <summary>
		/// Gets and sets absolute emitter velocity.
		/// </summary>
		public Vector3 Velocity {
			get;
			set;
		}


		/// <summary>
		/// Sets and gets local emitter distance scale
		/// </summary>
		public float DistanceScale {
			get; set;
		}




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

		private SharpDX.X3DAudio.CurvePoint[] volumeCurve = null;



		/// <summary>
		/// 
		/// </summary>
		/// <param name="listener"></param>
		internal void Apply3D ( AudioListener listener, int operationSet )
		{
			if (soundInstance!=null) {
				soundInstance.Apply3D( listener, this, operationSet );
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
            var fwd = Vector3.ForwardRH;
            var up	= Vector3.Up;

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
