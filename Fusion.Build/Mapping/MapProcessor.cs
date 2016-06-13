using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Mathematics;
using Fusion.Core.Shell;
using Fusion.Build.ImageUtils;
using Fusion.Build.Processors;

namespace Fusion.Build.Mapping {

	[AssetProcessor("Map", "Performs map assembly")]
	public class MapProcessor : AssetProcessor {

		public const int VTPageSize	=	128;
		public const int VTSize		=	128 * 128;
		public const int VTPageNum	=	VTSize / VTPageSize;
		public const int VTMips		=	6;




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

			Log.Message("-------- map: {0} --------", assetFile.KeyPath );

			//	build each scene :
			foreach ( var mapScene in mapScenes ) {
				mapScene.BuildScene( context, pageTable );
			}

			Log.Message("{0} textures", pageTable.Textures.Count);

			Log.Message("Packing textures to atlas...");
			PackTextureAtlas( pageTable.Textures );

			Log.Message("Generating pages...");
			GeneratePages( pageTable.Textures, context, pageTable );

			for (int mip=0; mip<VTMips-1; mip++) {
				Log.Message("Generating mip level {0}/{1}...", mip, VTMips);
				GenerateMipLevels( context, pageTable, mip );
			}

			Log.Message("Generating fallback image...");
			GenerateFallbackImage( context, pageTable, VTMips-1 );

			Log.Message("Remapping texture coordinates...");
			foreach ( var mapScene in mapScenes ) {
				Log.Message("...{0}", mapScene.KeyPath );
				mapScene.RemapTexCoords( pageTable );

				mapScene.SaveScene( Path.Combine( context.GetFullVTOutputPath(), assetFile.KeyPath + ".scene" ) );
			}



			Log.Message("----------------" );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="textures"></param>
		void PackTextureAtlas ( IEnumerable<MapTexture> textures )
		{
			var sortedList = textures
						.OrderByDescending( img0 => img0.Width * img0.Height )
						.ThenByDescending( img1 => img1.Width )
						.ThenByDescending( img2 => img2.Height )
						.ToList();

			var root = new VTAtlasNode(0,0, VTSize, VTSize, 0 );
			
			foreach ( var img in sortedList ) {
				var n = root.Insert( img );
				if (n==null) {
					throw new BuildException("No enough room to place texture. Are U mad, dude???");
				}
			}

			/*foreach ( var tex in textures ) {
				Log.Message("{0,6} {1,6} - {2}", tex.TexelOffsetX, tex.TexelOffsetY, tex.Name );
			}*/
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="textures"></param>
		void GeneratePages ( ICollection<MapTexture> textures, BuildContext context, VTPageTable pageTable )
		{
			int totalCount = textures.Count;
			int counter = 0;

			foreach ( var texture in textures ) {
				Log.Message("...{0}/{1} - {2}", counter, totalCount, texture.KeyPath );
				texture.GeneratePages( context, pageTable );
				counter++;
			}
		}



		void GenerateMipLevels ( BuildContext buildContext, VTPageTable pageTable, int mipLevel )
		{
			if (mipLevel>=MapProcessor.VTMips) {
				throw new ArgumentOutOfRangeException("mipLevel");
			}

			int count = (MapProcessor.VTSize / MapProcessor.VTPageSize) >> mipLevel;

			for ( int x = 0; x < count; x+=2 ) {
				for ( int y = 0; y < count; y+=2 ) {

					int address00 = VTPageTable.ComputeAddress( x + 0, y + 0, mipLevel );
					int address01 = VTPageTable.ComputeAddress( x + 0, y + 1, mipLevel );
					int address10 = VTPageTable.ComputeAddress( x + 1, y + 0, mipLevel );
					int address11 = VTPageTable.ComputeAddress( x + 1, y + 1, mipLevel );
					
					//	there are no images touching target mip-level :
					if ( !pageTable.IsAnyExists( address00, address01, address10, address11 ) ) {
						continue;
					}

					var dir		=	buildContext.GetFullVTOutputPath();

					var image00 =	pageTable.LoadPage( address00, dir );
					var image01 =	pageTable.LoadPage( address01, dir );
					var image10 =	pageTable.LoadPage( address10, dir );
					var image11 =	pageTable.LoadPage( address11, dir );

					int address =	VTPageTable.ComputeAddress( x/2, y/2, mipLevel+1 );
					var image	=	MipImages( image00, image01, image10, image11 );

					pageTable.Add( address );
					pageTable.SavePage( address, dir, image );
				}
			}

		}


		/// <summary>
		/// 
		/// </summary>
		void GenerateFallbackImage ( BuildContext buildContext, VTPageTable pageTable, int sourceMipLevel )
		{
			int		pageSize		=	MapProcessor.VTPageSize;
			int		numPages		=	MapProcessor.VTPageNum >> sourceMipLevel;
			int		fallbackSize	=	VTSize >> sourceMipLevel;
			string	baseDir			=	buildContext.GetFullVTOutputPath();

			Image fallbackImage =	new Image( fallbackSize, fallbackSize, Color.Black );

			for ( int pageX=0; pageX<numPages; pageX++) {
				for ( int pageY=0; pageY<numPages; pageY++) {

					var addr	=	VTPageTable.ComputeAddress( pageX, pageY, sourceMipLevel );
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
			const int pageSize = MapProcessor.VTPageSize;

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
