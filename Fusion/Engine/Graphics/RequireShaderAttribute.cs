using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Marks class as requiring the list of shaders.
	/// </summary>
	public sealed class RequireShaderAttribute : Attribute {

		/// <summary>
		/// Gets collection of required shaders.
		/// </summary>
		public IEnumerable<string> RequiredShaders {
			get; private set;
		}
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="shaders"></param>
		public RequireShaderAttribute ( params string[] shaders )
		{
			RequiredShaders = shaders;
		}
	}
}
