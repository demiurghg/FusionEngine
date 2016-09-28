﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Mathematics;
using Fusion.Core.Shell;
using Fusion.Engine.Imaging;
using Fusion.Build.Processors;
using Fusion.Engine.Graphics;

namespace Fusion.Build.Mapping {

	[AssetProcessor("Map", "Performs map assembly")]
	public class MapProcessor : AssetProcessor {



		/// <summary>
		/// 
		/// </summary>
		public MapProcessor ()
		{
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildContext"></param>
		public override void Process ( AssetSource assetFile, BuildContext context )
		{
			var fileDir		=	Path.GetDirectoryName( assetFile.FullSourcePath );
			var pageTable	=	new VTPageTable();

			//	get list of scenes :
			var mapScenes	=	File.ReadAllLines(assetFile.FullSourcePath)
								.Select( f1 => f1.Trim() )
								.Where( f2 => !f2.StartsWith("#") && !string.IsNullOrWhiteSpace(f2) )
								.Select( f3 => new MapScene( f3, Path.Combine( fileDir, f3 ) ) )
								.ToArray();

			var pageOutputDirectory	=	 context.GetFullVTOutputPath(assetFile);
			Directory.CreateDirectory( pageOutputDirectory );

			Log.Message("-------- map: {0} --------", assetFile.KeyPath );

			//	build each scene :
			foreach ( var mapScene in mapScenes ) {
				mapScene.BuildScene( context, pageTable );
			}

			Log.Message("{0} textures", pageTable.SourceTextures.Count);

			//	packing textures to atlas :
			Log.Message("Packing textures to atlas...");
			PackTextureAtlas( pageTable.SourceTextures );

			//	generating pages :
			Log.Message("Generating pages...");
			GenerateMostDetailedPages( pageTable.SourceTextures, context, pageTable, pageOutputDirectory );

			//	generating mipmaps :
			Log.Message("Generating mipmaps...");
			for (int mip=0; mip<VTConfig.MipCount-1; mip++) {
				Log.Message("Generating mip level {0}/{1}...", mip, VTConfig.MipCount);
				GenerateMipLevels( context, pageTable, mip, pageOutputDirectory );
			}

			//	generating fallback image :
			Log.Message("Generating fallback image...");
			GenerateFallbackImage( context, pageTable, VTConfig.MipCount-1, pageOutputDirectory );

			//	remapping texture coordinates :
			Log.Message("Remapping texture coordinates...");
			foreach ( var mapScene in mapScenes ) {
				Log.Message("...{0}", mapScene.KeyPath );
				mapScene.RemapTexCoords( pageTable );

				mapScene.SaveScene( Path.Combine( pageOutputDirectory, assetFile.KeyPath + ".scene" ) );
			}



			Log.Message("----------------" );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="textures"></param>
		void PackTextureAtlas ( IEnumerable<MapTexture> textures )
		{
			var allocator = new Allocator2D( VTConfig.VirtualPageCount );

			foreach ( var tex in textures ) {

				var size = Math.Max(tex.Width/128, tex.Height/128);

				var addr = allocator.Alloc( size, tex );

				tex.TexelOffsetX	=	addr.X * 128;
				tex.TexelOffsetY	=	addr.Y * 128;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="textures"></param>
		void GenerateMostDetailedPages ( ICollection<MapTexture> textures, BuildContext context, VTPageTable pageTable, string pageOutputDirectory )
		{
			int totalCount = textures.Count;
			int counter = 0;

			foreach ( var texture in textures ) {
				Log.Message("...{0}/{1} - {2}", counter, totalCount, texture.KeyPath );
				texture.SplitIntoPages( context, pageTable, pageOutputDirectory );
				counter++;
			}
		}





		void GenerateMipLevels ( BuildContext buildContext, VTPageTable pageTable, int sourceMipLevel, string outputDirectory )
		{
			if (sourceMipLevel>=VTConfig.MipCount) {
				throw new ArgumentOutOfRangeException("mipLevel");
			}

			var dir		= outputDirectory;	
			int count	= VTConfig.VirtualPageCount >> sourceMipLevel;
			int sizeB	= VTConfig.PageSizeBordered;
			var cache	= new TileSamplerCache( dir, pageTable ); 

			for ( int pageX = 0; pageX < count; pageX+=2 ) {
				for ( int pageY = 0; pageY < count; pageY+=2 ) {

					int address00 = VTAddress.ComputeIntAddress( pageX + 0, pageY + 0, sourceMipLevel );
					int address01 = VTAddress.ComputeIntAddress( pageX + 0, pageY + 1, sourceMipLevel );
					int address10 = VTAddress.ComputeIntAddress( pageX + 1, pageY + 0, sourceMipLevel );
					int address11 = VTAddress.ComputeIntAddress( pageX + 1, pageY + 1, sourceMipLevel );
					
					//	there are no images touching target mip-level.
					//	NOTE: we can skip images that are touched by border.
					if ( !pageTable.IsAnyExists( address00, address01, address10, address11 ) ) {
						continue;
					}

					int address =	VTAddress.ComputeIntAddress( pageX/2, pageY/2, sourceMipLevel+1 );

					var image	=	new Image( sizeB, sizeB, Color.Black );

					int offsetX	=	(pageX) * VTConfig.PageSize;
					int offsetY	=	(pageY) * VTConfig.PageSize;
					int border	=	VTConfig.PageBorderWidth;

					for ( int x=0; x<sizeB; x++) {
						for ( int y=0; y<sizeB; y++) {

							int srcX	=	offsetX + x*2 - border * 2;
							int srcY	=	offsetY + y*2 - border * 2;
							var color	=	SampleMegatextureQ4( cache, srcX, srcY, sourceMipLevel );
							
							image.Write( x,y, color );

						}
					}

					pageTable.Add( address );
					pageTable.SavePage( address, dir, image );
				}
			}

		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="texelX"></param>
		/// <param name="texelY"></param>
		/// <param name="mipLevel"></param>
		/// <returns></returns>
		Color SampleMegatexture ( TileSamplerCache cache, int texelX, int texelY, int mipLevel )
		{
			int textureSize	=	VTConfig.TextureSize >> mipLevel;
			
			texelX = MathUtil.Clamp( 0, texelX, textureSize );
			texelY = MathUtil.Clamp( 0, texelY, textureSize );

			int pageX	= texelX / VTConfig.PageSize;
			int pageY	= texelY / VTConfig.PageSize;
			int x		= texelX % VTConfig.PageSize;
			int y		= texelY % VTConfig.PageSize;
			int b		= VTConfig.PageBorderWidth;

			int address = VTAddress.ComputeIntAddress( pageX, pageY, mipLevel );

			return cache.LoadImage( address ).SampleClamp( x+b, y+b );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="texelX"></param>
		/// <param name="texelY"></param>
		/// <param name="mipLevel"></param>
		/// <returns></returns>
		Color SampleMegatextureQ4 ( TileSamplerCache cache, int texelX, int texelY, int mipLevel )
		{
			int textureSize	=	VTConfig.TextureSize >> mipLevel;
			
			texelX = MathUtil.Clamp( 0, texelX, textureSize );
			texelY = MathUtil.Clamp( 0, texelY, textureSize );

			int pageX	= texelX / VTConfig.PageSize;
			int pageY	= texelY / VTConfig.PageSize;
			int x		= texelX % VTConfig.PageSize;
			int y		= texelY % VTConfig.PageSize;
			int b		= VTConfig.PageBorderWidth;

			int address = VTAddress.ComputeIntAddress( pageX, pageY, mipLevel );

			return cache.LoadImage( address ).SampleQ4Clamp( x+b, y+b );
		}



		/// <summary>
		/// 
		/// </summary>
		void GenerateFallbackImage ( BuildContext buildContext, VTPageTable pageTable, int sourceMipLevel, string pageOutputDirectory )
		{
			int		pageSize		=	VTConfig.PageSize;
			int		numPages		=	VTConfig.VirtualPageCount >> sourceMipLevel;
			int		fallbackSize	=	VTConfig.TextureSize >> sourceMipLevel;
			string	baseDir			=	pageOutputDirectory;

			Image fallbackImage =	new Image( fallbackSize, fallbackSize, Color.Black );

			for ( int pageX=0; pageX<numPages; pageX++) {
				for ( int pageY=0; pageY<numPages; pageY++) {

					var addr	=	VTAddress.ComputeIntAddress( pageX, pageY, sourceMipLevel );
					var image	=	pageTable.LoadPage( addr, baseDir );

					for ( int x=0; x<pageSize; x++) {
						for ( int y=0; y<pageSize; y++) {

							int u = pageX * pageSize + x;
							int v = pageY * pageSize + y;

							fallbackImage.Write( u, v, image.Sample( x, y ) );
						}
					}
				}
			}

			Image.SaveTga( fallbackImage, Path.Combine( baseDir, "fallback.tga" ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="image00"></param>
		/// <param name="image01"></param>
		/// <param name="image10"></param>
		/// <param name="image11"></param>
		/// <returns></returns>
		Image MipImages ( Image image00, Image image01, Image image10, Image image11 )
		{
			const int pageSize = VTConfig.PageSize;

			if (image00.Width!=image00.Height || image00.Width != pageSize ) {
				throw new ArgumentException("Bad image size");
			}
			if (image01.Width!=image01.Height || image01.Width != pageSize ) {
				throw new ArgumentException("Bad image size");
			}
			if (image10.Width!=image10.Height || image10.Width != pageSize ) {
				throw new ArgumentException("Bad image size");
			}
			if (image11.Width!=image11.Height || image11.Width != pageSize ) {
				throw new ArgumentException("Bad image size");
			}

			var image = new Image( pageSize, pageSize, Color.Black );

			for ( int i=0; i<pageSize/2; i++) {
				for ( int j=0; j<pageSize/2; j++) {
					image.Write( i,j, image00.SampleMip( i, j ) );
				}
			}

			for ( int i=pageSize/2; i<pageSize; i++) {
				for ( int j=pageSize/2; j<pageSize; j++) {
					image.Write( i,j, image11.SampleMip( i, j ) );
				}
			}

			for ( int i=0; i<pageSize/2; i++) {
				for ( int j=pageSize/2; j<pageSize; j++) {
					image.Write( i,j, image01.SampleMip( i, j ) );
				}
			}

			for ( int i=pageSize/2; i<pageSize; i++) {
				for ( int j=0; j<pageSize/2; j++) {
					image.Write( i,j, image10.SampleMip( i, j ) );
				}
			}

			return image;
		} 


	}
}
