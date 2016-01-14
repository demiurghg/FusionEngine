using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Drivers.Audio;
using Fusion.Drivers.Audio;



namespace Fusion.Engine.Audio {

	/// <summary>
	/// Represents sound.
	/// </summary>
	public class SoundEffect {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		internal SoundEffect ( Stream stream )
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public SoundEffectInstance CreateInstance ()
		{
			return new SoundEffectInstance( this );
		}


	}
}
