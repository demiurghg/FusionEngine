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
using Fusion.Engine.Storage;
using Fusion.Core.IniParser.Model;
using Fusion.Core.Content;

namespace Fusion.Build.Mapping {


	internal class VTTexture {

		readonly BuildContext context;

		public readonly string	KeyPath;

		public int TexelOffsetX;
		public int TexelOffsetY;

		public readonly int Width;
		public readonly int Height;

		public int AddressX { get { return TexelOffsetX / VTConfig.PageSize; } }
		public int AddressY { get { return TexelOffsetY / VTConfig.PageSize; } }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="fullPath"></param>
		public VTTexture ( string keyPath, BuildContext context )
		{			
			this.context		=	context;
			const int pageSize	=	VTConfig.PageSize;

			this.KeyPath	=	keyPath;
			var fullPath	=	context.ResolveContentPath( keyPath );

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
		public void SplitIntoPages ( BuildContext context, VTTextureTable pageTable, IStorage storage )
		{
			var pageSize			=	VTConfig.PageSize;
			var pageSizeBorder		=	VTConfig.PageSizeBordered;
			var border				=	VTConfig.PageBorderWidth;
			var fullPathColorMap	=	context.ResolveContentPath( KeyPath );

			var colorMap			=	Image.LoadTga( File.OpenRead(fullPathColorMap) );
			var normalMap			=	LoadExtraTexture( colorMap, "_local", Color.FlatNormals );
			var specularMap			=	LoadExtraTexture( colorMap, "_spec", Color.Black );
			var emissiveMap			=	LoadExtraTexture( colorMap, "_glow", Color.Black );

			var pageCountX	=	colorMap.Width / pageSize;
			var pageCountY	=	colorMap.Height / pageSize;

			for (int x=0; x<pageCountX; x++) {
				for (int y=0; y<pageCountY; y++) {

					var pageC	=	new Image(pageSizeBorder, pageSizeBorder); 
					var pageN	=	new Image(pageSizeBorder, pageSizeBorder); 
					var pageS	=	new Image(pageSizeBorder, pageSizeBorder); 
					
					for ( int i=0; i<pageSizeBorder; i++) {
						for ( int j=0; j<pageSizeBorder; j++) {

							int srcX		=	x*pageSize + i - border;
							int srcY		=	y*pageSize + j - border;

							var color		=	colorMap.SampleClamp( srcX, srcY );
							var normal		=	normalMap.SampleClamp( srcX, srcY );
							var specular	=	specularMap.SampleClamp( srcX, srcY );
							var emission	=	emissiveMap.SampleClamp( srcX, srcY );

							specular.A		=	(byte)(emission.GetBrightness() * 255);

							pageC.Write( i,j, color );
							pageN.Write( i,j, normal );
							pageS.Write( i,j, specular );
						}
					}

					var address	=	new VTAddress( (short)(x + AddressX), (short)(y + AddressY), 0 );
					pageTable.Add( address );

					pageTable.SavePage( address, storage, pageC, "C" );
					pageTable.SavePage( address, storage, pageN, "N" );
					pageTable.SavePage( address, storage, pageS, "S" );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="baseImage"></param>
		/// <param name="postfix"></param>
		/// <param name="defaultColor"></param>
		/// <returns></returns>
		Image LoadExtraTexture ( Image baseImage, string postfix, Color defaultColor ) 
		{
			var keyPath	=	ContentUtils.AddSuffix( KeyPath, postfix );
			
			if (!context.ContentFileExists( keyPath )) {
				return new Image( Width, Height, defaultColor );
			}

			var image	=	Image.LoadTga( File.OpenRead( context.ResolveContentPath(keyPath) ) );

			if (image.Width!=Width || image.Height!=image.Height) {
				Log.Warning("Size of {0} is not equal to size of {1}. Default image is used.", keyPath, KeyPath );
				return new Image( Width, Height, defaultColor );
			}

			return image;
		}

	}
}
