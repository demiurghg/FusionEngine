using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;
using Fusion.Core.Extensions;
using System.IO;
using Fusion.Engine.Storage;


namespace Fusion.Core.Content {

	[ContentLoader(typeof(string))]
	public class StringLoader : ContentLoader {
		
		public override object Load ( ContentManager content, Stream stream, Type requestedType, string assetPath, IStorage storage )
		{
			return Encoding.UTF8.GetString( stream.ReadAllBytes() );
		}
	}
}
