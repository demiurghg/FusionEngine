using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Drivers.Audio;
using Fusion.Engine.Common;


namespace Fusion.Engine.Audio {

	public class SoundSystem : GameModule {

		[Config]
		public SoundSystemConfig Config { get; set; }


		List<SoundScape> soundscapes = new List<SoundScape>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public SoundSystem ( Game game ) : base(game)
		{
			Config	=	new SoundSystemConfig();
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			//throw new NotImplementedException();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
			}

			base.Dispose( disposing );
		}	



		/// <summary>
		/// Updates internal state and all soundscapes.
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update ( GameTime gameTime )
		{
			foreach ( var soundscape in soundscapes ) {
				//	...
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="soundScape"></param>
		public void AddSoundScape ( SoundScape soundScape )
		{
			soundscapes.Add( soundScape );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="soundScape"></param>
		public void RemoveSoundScape ( SoundScape soundScape )
		{
			soundscapes.Remove( soundScape );
		}
	}
}
