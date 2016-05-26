using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Engine.Common;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Driver = Fusion.Engine.Audio;
using SharpDX.X3DAudio;

namespace Fusion.Engine.Audio {
	public sealed class SoundWorld {

		public readonly Game Game;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="soundSystem"></param>
		public SoundWorld ( Game game )
		{
			Game		=	game;
			Listener	=	new AudioListener();
		}


		/// <summary>
		/// Removes all sound instances.
		/// </summary>
		public void Clear ()
		{
			foreach ( var e in emitters ) {
				e.StopSound(true);
			}
			emitters.Clear();
		}	



		/// <summary>
		/// Indicates whether world is paused.
		/// </summary>
		public bool IsPaused {
			get {
				return isPaused;
			}
			set {	
				if (isPaused!=value) {
					isPaused = value;
				}
			}
		}


		bool isPaused;


		/// <summary>
		/// Pauses sound world.
		/// </summary>
		public void Pause ()
		{
			IsPaused	=	true;

			foreach ( var e in emitters ) {
				e.Pause();
			}
		}



		/// <summary>
		/// Resumes sound world.
		/// </summary>
		public void Resume ()
		{
			IsPaused	=	false;

			foreach ( var e in emitters ) {
				e.Resume();
			}
		}



		/// <summary>
		/// Sets and gets audio listener.
		/// </summary>
		public AudioListener Listener {
			get;
			set;
		}



		HashSet<AudioEmitter>	emitters = new HashSet<AudioEmitter>();


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public AudioEmitter AllocEmitter ()
		{
			var emitter = new AudioEmitter();
			emitters.Add(emitter);
			return emitter;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns>True if the emitter is successfully found and removed; otherwise, false</returns>
		public bool FreeEmitter ( AudioEmitter emitter )
		{
			emitter.StopSound(true);
			return emitters.Remove(emitter);
		}




		/// <summary>
		/// Updates sound world.
		/// </summary>
		/// <param name="gameTime"></param>
		internal void Update ( GameTime gameTime, int operationSet )
		{
			foreach ( var e in emitters ) {
				if (!e.LocalSound) {
					e.Apply3D( Listener, operationSet );
				}
			}
		}
	}
}
