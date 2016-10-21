using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Engine.Imaging;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics;
using Fusion.Engine.Storage;

namespace Fusion.Build.Mapping {
	internal class VTTextureTable {

		HashSet<int> pages = new HashSet<int>();

		Dictionary<string,VTTexture> textures = new Dictionary<string,VTTexture>();


		public VTTextureTable ()
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyPath"></param>
		/// <param name="scene"></param>
		/// <param name="meshIndex"></param>
		public void AddTexture ( string keyPath, string fullPath )
		{
			VTTexture texture;

			if ( !textures.ContainsKey(keyPath) ) {
				texture = new VTTexture( keyPath, fullPath );
				textures.Add( keyPath, texture );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ICollection<VTTexture> SourceTextures {
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
		public VTTexture GetSourceTextureByKeyPath ( string keyPath )
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
				Log.Warning("Address {0:X} is already added", address);
			}
			pages.Add( address );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public bool Contains ( int address )
		{
			return pages.Contains(address);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <param name="baseDir"></param>
		/// <returns></returns>
		public Image LoadPage ( int address, IStorage storage )
		{
			if (pages.Contains(address)) {
				
				var path	=	address.ToString("X8") + ".tga";
				var image	=	Image.LoadTga( storage.OpenRead(path) );

				return image;

			} else {
				return new Image( VTConfig.PageSize, VTConfig.PageSize, Color.Black );
			}
		}





		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <param name="baseDir"></param>
		/// <param name="image"></param>
		public void SavePage ( int address, IStorage storage, Image image )
		{
			var name	=	address.ToString("X8") + ".tga";

			Image.SaveTga( image, storage.OpenWrite(name) );
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
	}
}
