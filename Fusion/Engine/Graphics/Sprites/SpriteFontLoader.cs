using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using SharpDX;
using Fusion.Drivers.Graphics;
using System.Diagnostics;
using System.ComponentModel;
using Fusion.Core.Content;
using Fusion.Engine.Common;


namespace Fusion.Engine.Graphics {
	[ContentLoader(typeof(SpriteFont))]
	public class SpriteFontLoader : ContentLoader {

		public override object Load ( ContentManager content, Stream stream, Type requestedType, string assetPath )
		{
			return new SpriteFont( content.GameEngine.GraphicsDevice, stream );
		}
	}
}
