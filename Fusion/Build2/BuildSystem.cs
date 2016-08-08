using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Core.Extensions;
using Fusion.Engine.Graphics;


namespace Fusion.Build2 {
	public class BuildSystem {

		public BuildSystem ( Game game )
		{
		}


		/// <summary>
		/// Analyzes entire engine searching for RequireShaderAttribute.
		/// Returns collection of required shaders.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetRequiredShaders ()
		{
			var list = new List<string>();

			var attributes = 	
				Misc.GetAllClassesWithAttribute<RequireShaderAttribute>()
					.Select( type1 => type1.GetCustomAttribute<RequireShaderAttribute>() )
					.ToArray();

			foreach ( var attr in attributes ) {	
				list.AddRange( attr.RequiredShaders );
			}

			return list;
		}

	}
}
