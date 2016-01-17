using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Audio;
using Fusion.Engine.Common;


namespace Fusion.Engine.Audio {

	public class SoundSystemConfig {


		public float MasterVolume { get; set; }

		public float SpeedOfSound { get; set; }
		
		public float DopplerScale { get; set; }

		

		/// <summary>
		/// 
		/// </summary>
		public SoundSystemConfig ()
		{
			MasterVolume	=	1;
			SpeedOfSound	=	343;
			DopplerScale	=	1;
		}
	}
}
