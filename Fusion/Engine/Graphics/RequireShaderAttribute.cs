using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Extensions;

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



		static public IEnumerable<string> GatherRequiredShaders ()
		{
			var list = new List<string>();

			var attributes = 	
				Misc.GetAllClassesWithAttribute<RequireShaderAttribute>()
					.Select( type1 => type1.GetCustomAttribute<RequireShaderAttribute>() )
					.ToArray();

			foreach ( var attr in attributes ) {	
				list.AddRange( attr.RequiredShaders );
			}

			
			return	list.Distinct().ToArray();
		}
	}
}
