using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Engine.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Storage;
using Fusion.Core.Content;

namespace Fusion.Engine.Mapping {

	/// <summary>
	/// Represents entire map in game, including:
	///		Virtual textures
	///		All models
	/// </summary>
	[ContentLoader(typeof(Map))]
	public class MapLoader : ContentLoader {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <param name="stream"></param>
		/// <param name="requestedType"></param>
		/// <param name="assetPath"></param>
		/// <param name="storage"></param>
		/// <returns></returns>
		public override object Load(ContentManager content, Stream stream, Type requestedType, string assetPath, IStorage storage)
		{
			return new Map( content.Game, stream, storage );
		}

	}
}
