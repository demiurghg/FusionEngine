using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Engine.Imaging;
using Fusion.Core.Mathematics;
using Fusion.Core.Extensions;
using Fusion.Engine.Graphics;
using Fusion.Engine.Storage;

namespace Fusion.Build.Mapping {

	class TileSamplerCache {

		LRUCache<VTAddress, Image> cache;

		readonly IStorage storage;
		readonly VTTextureTable pageTable;
			
		public TileSamplerCache ( IStorage mapStorage, VTTextureTable pageTable )
		{
			this.storage	=	mapStorage;
			this.cache		=	new LRUCache<VTAddress,Image>(128);
			this.pageTable	= pageTable;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Image LoadImage ( VTAddress address )
		{
			Image image;

			if (!cache.TryGetValue(address, out image)) {
				
				var path		=	address.GetFileNameWithoutExtension("C.tga");

				if (pageTable.Contains(address)) {
					image		=	Image.LoadTga( storage.OpenFile(path, FileMode.Open, FileAccess.Read) );
				} else {
					image		=	new Image( VTConfig.PageSizeBordered, VTConfig.PageSizeBordered, Color.Black );
				}

				cache.Add( address, image );

			}
			
			return image;
		}
	}
}
