using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Driver = Fusion.Drivers.Audio;


namespace Fusion.Engine.Audio {
	public class SoundScape {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="soundSystem"></param>
		public SoundScape ( SoundSystem soundSystem )
		{

		}


		/// <summary>
		/// Removes all sound instances.
		/// </summary>
		public void Clear ()
		{
		}	



		/// <summary>
		/// 
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		public SoundEffectInstance CreateSoundInstance ( SoundEffect soundEffect )
		{
			throw new NotImplementedException();
		}


		




		/// <summary>
		///	Pauses all sounds.
		/// </summary>
		public void Pause()
		{
		}


		/// <summary>
		/// Unpauses all sounds.
		/// </summary>
		public void Unpause()
		{
		}




	}
}
