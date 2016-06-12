using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Extensions;
using Fusion.Engine.Graphics;
using Fusion.Build.ImageUtils;

namespace Fusion.Build.Mapping {


	public class MapTexture {
		
		public readonly string Name;

		public int TexelOffsetX;
		public int TexelOffsetY;

		public readonly int Width;
		public readonly int Height;

		public int AddressX { get { return TexelOffsetX / MapProcessor.VTPageSize; } }
		public int AddressY { get { return TexelOffsetY / MapProcessor.VTPageSize; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fullPath"></param>
		public MapTexture ( string fullPath )
		{					
			var pageSize	=	MapProcessor.VTPageSize;

			Name	=	fullPath;

			var header = Image.TakeTga( fullPath );

			if (header.height%pageSize!=0) {
				throw new BuildException(string.Format("Width of '{0}' must be multiple of {1}", fullPath, pageSize));
			}
			if (header.width%pageSize!=0) {
				throw new BuildException(string.Format("Height of '{0}' must be multiple of {1}",fullPath, pageSize));
			}

			Width	=	header.width;
			Height	=	header.height;
		}



		public void GeneratePages ( BuildContext context )
		{
			var pageSize	=	MapProcessor.VTPageSize;
			var image		=	Image.LoadTga( Name );

			var pageCountX	=	image.Width / pageSize;
			var pageCountY	=	image.Height / pageSize;

			for (int x=0; x<pageCountX; x++) {
				for (int y=0; y<pageCountY; y++) {

					var page = new Image(pageSize, pageSize); 
					
					for ( int i=0; i<pageSize; i++) {
						for ( int j=0; j<pageSize; j++) {
							page.Write( i,j, image.Sample( x*pageSize + i, y*pageSize + j ) );
						}
					}

					var dir		=	context.GetFullVTOutputPath();
					var name	=	string.Format("{0:000000}_{1:000000}.tga", x + AddressX, y + AddressX );

					Image.SaveTga( page, Path.Combine( dir, name ) );
				}
			}
		}

	}
}
