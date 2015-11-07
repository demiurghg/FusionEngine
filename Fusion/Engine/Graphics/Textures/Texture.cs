using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Base class for all texture types.
	/// </summary>
	public class Texture : DisposableBase {

		/// <summary>
		/// Gets width of the texture
		/// </summary>
		public int Width {
			get; protected set;
		}


		/// <summary>
		/// Gets height of the texture
		/// </summary>
		public int Height {
			get; protected set;
		}


		/// <summary>
		/// Shader resource view
		/// </summary>
		internal ShaderResource Srv = null;
		

		/// <summary>
		/// Forbid Texture creation.
		/// </summary>
		protected Texture() 
		{
		}
	}
}
