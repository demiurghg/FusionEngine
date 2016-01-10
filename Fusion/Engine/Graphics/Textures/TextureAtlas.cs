#define DIRECTX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Fusion.Core.Content;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;


namespace Fusion.Engine.Graphics {

	[ContentLoader(typeof(TextureAtlas))]
	public class TextureAtlasLoader : ContentLoader {

		public override object Load ( ContentManager content, Stream stream, Type requestedType, string assetPath )
		{
			bool srgb = assetPath.ToLowerInvariant().Contains("|srgb");
			return new TextureAtlas( content.Game.RenderSystem, stream, srgb );
		}
	}
		


	/// <summary>
	/// 
	/// </summary>
	public class TextureAtlas : DisposableBase {

		private	Texture	texture;

		struct Element {
			public string Name;
			public int X;
			public int Y;
			public int Width;
			public int Height;
		}

		List<Element> elements = new List<Element>();
		Dictionary<string,Element> dictionary;


		/// <summary>
		/// Atlas texture.
		/// </summary>
		public Texture Texture { 
			get { 
				return texture; 
			} 
		}



		/// <summary>
		/// Creates texture atlas from stream.
		/// </summary>
		/// <param name="device"></param>
		public TextureAtlas ( RenderSystem rs, Stream stream, bool useSRgb = false )
		{
			var device = rs.Game.GraphicsDevice;

			using ( var br = new BinaryReader(stream) ) {
			
				br.ExpectFourCC("ATLS", "texture atlas");
				
				int count = br.ReadInt32();
				
				for ( int i=0; i<count; i++ ) {
					var element = new Element();
					element.Name	=	br.ReadString();
					element.X		=	br.ReadInt32();
					element.Y		=	br.ReadInt32();
					element.Width	=	br.ReadInt32();
					element.Height	=	br.ReadInt32();

					elements.Add( element );
				}				

				int ddsFileLength	=	br.ReadInt32();
				
				var ddsImageBytes	=	br.ReadBytes( ddsFileLength );

				texture	=	new UserTexture( rs, ddsImageBytes, useSRgb );
			}


			dictionary	=	elements.ToDictionary( e => e.Name );
		}

					

		/// <summary>
		/// Gets names of all subimages. 
		/// </summary>
		public string[] SubImageNames {
			get {
				return elements.Select( e => e.Name ).ToArray();
			}
		}
		


		/// <summary>
		/// Gets subimage rectangle in this atlas.
		/// </summary>
		/// <param name="name">Subimage name. Case sensitive. Without extension.</param>
		/// <returns>Rectangle</returns>
		public Rectangle GetSubImageRectangle ( string name )
		{
			Element e;
			var r = dictionary.TryGetValue( name, out e );

			if (!r) {
				throw new InvalidOperationException(string.Format("Texture atlas does not contain subimage '{0}'", name));
			}

			return new Rectangle( e.X, e.Y, e.Width, e.Height );
		}



		/// <summary>
		/// Disposes texture atlas.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref texture );
			}
			base.Dispose( disposing );
		}
	}
}
