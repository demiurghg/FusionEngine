using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Build.ImageUtils;
using Fusion.Core.Mathematics;

namespace Fusion.Build.Mapping {
	internal class VTPageTable {

		HashSet<int> pages = new HashSet<int>();

		Dictionary<string,MapTexture> textures = new Dictionary<string,MapTexture>();


		public VTPageTable ()
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyPath"></param>
		/// <param name="scene"></param>
		/// <param name="meshIndex"></param>
		public void AddTexture ( string keyPath, string fullPath, MapScene mapScene )
		{
			MapTexture texture;
			
			if ( textures.TryGetValue( keyPath, out texture ) ) {
				
				texture.References.Add( mapScene );

			} else {

				texture = new MapTexture( keyPath, fullPath );

				texture.References.Add( mapScene );

				textures.Add( keyPath, texture );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ICollection<MapTexture> Textures {
			get {
				return textures
					.Select( pair => pair.Value )
					.OrderBy( tex => tex.FullPath )
					.ToArray();
			}
		}



		/// <summary>
		/// Gets texture
		/// </summary>
		/// <param name="keyPath"></param>
		/// <returns></returns>
		public MapTexture GetTextureByKeyPath ( string keyPath )
		{
			return textures[ keyPath ];
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		public void Add ( int address )
		{
			if (pages.Contains(address)) {
				Log.Warning("Address {0,X} is already added", address);
			}
			pages.Add( address );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <param name="baseDir"></param>
		/// <returns></returns>
		public Image LoadPage ( int address, string baseDir )
		{
			if (pages.Contains(address)) {
				
				var path	=	Path.Combine( baseDir, address.ToString("X8") + ".tga" );
				var image	=	Image.LoadTga( path );

				return image;

			} else {
				return new Image( MapProcessor.VTPageSize, MapProcessor.VTPageSize, Color.Black );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <param name="baseDir"></param>
		/// <param name="image"></param>
		public void SavePage ( int address, string baseDir, Image image )
		{
			var dir		=	baseDir;
			var name	=	address.ToString("X8") + ".tga";

			Image.SaveTga( image, Path.Combine( dir, name ) );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="address0"></param>
		/// <param name="address1"></param>
		/// <param name="address2"></param>
		/// <param name="address3"></param>
		/// <returns></returns>
		public bool IsAnyExists ( int address0, int address1, int address2, int address3 )
		{
			return pages.Contains( address0 )
				|| pages.Contains( address1 )
				|| pages.Contains( address2 )
				|| pages.Contains( address3 );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="pageX"></param>
		/// <param name="pageY"></param>
		/// <param name="mipLevel"></param>
		/// <returns></returns>
		public static int ComputeAddress ( int pageX, int pageY, int mipLevel )
		{
			if (pageX>=MapProcessor.VTSize) {
				throw new ArgumentOutOfRangeException("pageX");
			}
			if (pageY>=MapProcessor.VTSize) {
				throw new ArgumentOutOfRangeException("pageY");
			}
			if (mipLevel>=MapProcessor.VTMips) {
				throw new ArgumentOutOfRangeException("mipLevel");
			}


			pageX		= pageX & (MapProcessor.VTSize-1);
			pageY		= pageY & (MapProcessor.VTSize-1);
			mipLevel	= mipLevel & 0x7;

			return (mipLevel << 20) | (pageY << 10) | pageX;
		}




	}
}
