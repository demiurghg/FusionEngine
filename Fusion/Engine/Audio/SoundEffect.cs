using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Audio = Fusion.Drivers.Audio;



namespace Fusion.Engine.Audio {

	/// <summary>
	/// Represents sound buffer loaded from disk.
	/// </summary>
	public class SoundEffect {

		Audio.SoundEffect	soundEffect;		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		internal SoundEffect ( Stream stream )
		{
			throw new NotImplementedException();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public SoundEmitter CreateInstance ()
		{
			throw new NotImplementedException();
		}
	}
}
