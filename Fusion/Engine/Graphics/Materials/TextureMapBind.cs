using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Content;
using System.Xml.Serialization;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// 
	/// </summary>
	public sealed class TextureMapBind {

		public readonly TextureMap TextureMap;
		public readonly string FallbackPath;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textureMap"></param>
		/// <param name="fallbackPath"></param>
		public TextureMapBind ( TextureMap textureMap, string fallbackPath )
		{
			this.TextureMap		=	textureMap;
			this.FallbackPath	=	fallbackPath;
		}
		
	}
}
