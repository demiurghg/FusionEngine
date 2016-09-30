using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Extensions;
using Fusion.Engine.Common;
using System.IO;
using Fusion.Engine.Storage;

namespace Fusion.Core.Content {

	[ContentLoader(typeof(byte[]))]
	public class BytesLoader : ContentLoader {
		
		public override object Load ( ContentManager content, Stream stream, Type requestedType, string assetPath, IStorage storage )
		{
			return stream.ReadAllBytes();
		}
	}
}
