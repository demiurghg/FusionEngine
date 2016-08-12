using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {

	internal class RenderComponent : GameComponent {

		protected readonly RenderSystem rs;
		protected readonly GraphicsDevice device;

		/// <summary>
		/// 
		/// </summary>
		public RenderComponent ( RenderSystem rs ) : base(rs.Game)
		{
			this.rs		=	rs;
			this.device	=	rs.Game.GraphicsDevice;
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
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
