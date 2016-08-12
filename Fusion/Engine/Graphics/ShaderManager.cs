using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Extensions;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Imaging;
using System.Diagnostics;

namespace Fusion.Engine.Graphics {
	internal class ShaderManager : GameComponent {


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		public ShaderManager ( RenderSystem rs ) : base(rs.Game)
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			//	preload shaders
			foreach ( var name in RequireShaderAttribute.GatherRequiredShaders() ) {
				Load( name );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Ubershader Load ( string name )
		{
			return Game.Content.Load<Ubershader>( name );
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

	}
}
