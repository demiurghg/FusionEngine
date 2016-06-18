using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;


namespace Fusion.Engine.Graphics.Lights {
	public class SkyLight : GameModule {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		public SkyLight ( RenderSystem rs ) : base( rs.Game )
		{
		}



		/// <summary>
		/// Initializes stuff
		/// </summary>
		public override void Initialize ()
		{
		}



		/// <summary>
		/// Disposes stuff
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		public void RenderLightMap ()
		{
		}

	}
}
