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


	internal class MapTexture {
		
		public readonly string KeyPath;
		public readonly string FullPath;

		public int TexelOffsetX;
		public int TexelOffsetY;

		public readonly int Width;
		public readonly int Height;

		public int AddressX { get { return TexelOffsetX / MapProcessor.VTPageSize; } }
		public int AddressY { get { return TexelOffsetY / MapProcessor.VTPageSize; } }

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
			const int pageSize	=	MapProcessor.VTPageSize;

			References	=	new HashSet<MapScene>();
			FullPath		=	fullPath;
			KeyPath		=	keyPath;

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


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="uv"></param>
		/// <returns></returns>
		public Vector2 RemapTexCoords ( Vector2 uv )
		{
			double size	= MapProcessor.VTSize;

			double u = ( uv.X * Width + TexelOffsetX ) / size;
			double v = ( uv.X * Width + TexelOffsetX ) / size;

			return new Vector2( (float)u, (float)v );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="pageTable"></param>
		public void GeneratePages ( BuildContext context, VTPageTable pageTable )
		{
			var pageSize	=	MapProcessor.VTPageSize;
			var image		=	Image.LoadTga( FullPath );

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

					var address	=	VTPageTable.ComputeAddress( x + AddressX, y + AddressY, 0 );
					pageTable.Add( address );

					pageTable.SavePage( address, context.GetFullVTOutputPath(), page );
				}
			}
		}

	}
}
