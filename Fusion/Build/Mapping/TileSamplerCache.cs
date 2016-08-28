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

namespace Fusion.Build.Mapping {

	class TileSamplerCache {

		LRUCache<int, Image> cache;

		readonly string baseDir;
		readonly VTPageTable pageTable;
			
		public TileSamplerCache ( string baseDir, VTPageTable pageTable )
		{
			this.cache		=	new LRUCache<int,Image>(128);
			this.baseDir	= baseDir;
			this.pageTable	= pageTable;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Image LoadImage ( int address )
		{
			Image image;

			if (!cache.TryGetValue(address, out image)) {
				
				var path		=	Path.Combine( baseDir, address.ToString("X8") + ".tga" );

				if (pageTable.Contains(address)) {
					image		=	Image.LoadTga( File.OpenRead(path) );
				} else {
					image		=	new Image( VTConfig.PageSizeBordered, VTConfig.PageSizeBordered, Color.Black );
				}

				cache.Add( address, image );

			}
			
			return image;
		}
	}
}
