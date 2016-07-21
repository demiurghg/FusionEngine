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
using Fusion.Engine.Imaging;

namespace Fusion.Build.Mapping {


	internal class MapTexture {
		
		public readonly string KeyPath;
		public readonly string FullPath;

		public int TexelOffsetX;
		public int TexelOffsetY;

		public readonly int Width;
		public readonly int Height;

		public int AddressX { get { return TexelOffsetX / VTConfig.PageSize; } }
		public int AddressY { get { return TexelOffsetY / VTConfig.PageSize; } }

		/// <summary>
		/// Gets list of scenes that reference this texture
		/// </summary>
		public HashSet<MapScene> References { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="fullPath"></param>
		public MapTexture ( string keyPath, string fullPath )
		{			
			const int pageSize	=	VTConfig.PageSize;

			References	=	new HashSet<MapScene>();
			FullPath		=	fullPath;
			KeyPath		=	keyPath;

			var header = Image.TakeTga( File.OpenRead(fullPath) );

			if (header.height%pageSize!=0) {
				throw new BuildException(string.Format("Width of '{0}' must be multiple of {1}", fullPath, pageSize));
			}
			if (header.width%pageSize!=0) {
				throw new BuildException(string.Format("Height of '{0}' must be multiple of {1}",fullPath, pageSize));
			}

			Width	=	header.width;
			Height	=	header.height;
		}


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="uv"></param>
		/// <returns></returns>
		public Vector2 RemapTexCoords ( Vector2 uv )
		{
			double size	= VTConfig.TextureSize;

			double u = ( MathUtil.Wrap(uv.X,0,1) * Width  + (double)TexelOffsetX ) / size;
			double v = ( MathUtil.Wrap(uv.Y,0,1) * Height + (double)TexelOffsetY ) / size;

			return new Vector2( (float)u, (float)v );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="pageTable"></param>
		public void SplitIntoPages ( BuildContext context, VTPageTable pageTable )
		{
			var pageSize		=	VTConfig.PageSize;
			var pageSizeBorder	=	VTConfig.PageSizeBordered;
			var border			=	VTConfig.PageBorderWidth;
			var image			=	Image.LoadTga( File.OpenRead(FullPath) );

			var pageCountX	=	image.Width / pageSize;
			var pageCountY	=	image.Height / pageSize;

			for (int x=0; x<pageCountX; x++) {
				for (int y=0; y<pageCountY; y++) {

					var page = new Image(pageSizeBorder, pageSizeBorder); 
					
					for ( int i=0; i<pageSizeBorder; i++) {
						for ( int j=0; j<pageSizeBorder; j++) {
							page.Write( i,j, image.SampleClamp( x*pageSize + i - border, y*pageSize + j - border ) );
						}
					}

					var address	=	VTAddress.ComputeIntAddress( x + AddressX, y + AddressY, 0 );
					pageTable.Add( address );

					pageTable.SavePage( address, context.GetFullVTOutputPath(), page );
				}
			}
		}

	}
}
