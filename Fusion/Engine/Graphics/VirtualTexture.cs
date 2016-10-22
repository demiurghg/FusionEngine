using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Extensions;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Imaging;
using System.Diagnostics;
using Fusion.Engine.Storage;
using Fusion.Core.Content;

namespace Fusion.Engine.Graphics {
	
	/// <summary>
	/// Represents virtual texture resource
	/// </summary>
	public class VirtualTexture : DisposableBase {

		[ContentLoader(typeof(VirtualTexture))]
		internal class Loader : ContentLoader {

			public override object Load( ContentManager content, Stream stream, Type requestedType, string assetPath, IStorage storage )
			{
				//bool srgb = assetPath.ToLowerInvariant().Contains("|srgb");
				return new VirtualTexture(content.Game.RenderSystem, stream, storage );
			}
		}

		readonly IStorage tileStorage;


		/// <summary>
		/// Gets tile storage
		/// </summary>
		internal IStorage TileStorage {
			get {
				return tileStorage;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		internal VirtualTexture ( RenderSystem rs, Stream stream, IStorage storage )
		{
			this.tileStorage	=	storage;
		}



		/// <summary>
		/// Dispose virtual texture stuff
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose( bool disposing )
		{
			if (disposing) {
			}

			base.Dispose(disposing);
		}

	}
}
