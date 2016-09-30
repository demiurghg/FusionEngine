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
using Fusion.Core.Extensions;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Storage;


namespace Fusion.Engine.Graphics {

	[ContentLoader(typeof(TextureAtlas))]
	public class TextureAtlasLoader : ContentLoader {

		public override object Load ( ContentManager content, Stream stream, Type requestedType, string assetPath, IStorage storage )
		{
			bool srgb = assetPath.ToLowerInvariant().Contains("|srgb");
			return new TextureAtlas( content.Game.RenderSystem, stream, srgb );
		}
	}
		


	/// <summary>
	/// Represents texture atlas.
	/// </summary>
	public class TextureAtlas : DisposableBase {

		private	Texture	texture;

		struct Element {
			public string Name;
			public int Index;
			public int X;
			public int Y;
			public int Width;
			public int Height;

			public Rectangle GetRect ()
			{
				return new Rectangle(X, Y, Width, Height);
			}

			public RectangleF GetRectF (float width, float height)
			{
				return new RectangleF(X/width, Y/height, Width/width, Height/height);
			}
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
					element.Index	=	i;
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
		/// Gets number of images.
		/// </summary>
		public int Count {
			get {
				return elements.Count;
			}
		}


		
		/// <summary>
		/// Gets subimage rectangle by its index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Rectangle this [int index]
		{	
			get {					
				var e = elements[index];
				return e.GetRect();
			}
		}

					
		
		/// <summary>
		/// Gets subimage rectangle by its index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Rectangle this [string name]
		{	
			get {					
				Element e;

				var r = dictionary.TryGetValue( name, out e );

				if (!r) {
					throw new InvalidOperationException(string.Format("Texture atlas does not contain subimage '{0}'", name));
				}

				return new Rectangle(e.X, e.Y, e.Width, e.Height);
			}
		}

					

		/// <summary>
		/// Gets names of all subimages. 
		/// </summary>
		public string[] GetSubImageNames () 
		{
			return elements.Select( e => e.Name ).ToArray();
		}



		/// <summary>
		/// Gets rectangles of all subimages in texels.
		/// </summary>
		/// <param name="maxCount">Maximum number of recatangles. 
		/// If maxCount greater than number of images
		/// the rest of the array will be filled with zeroed rectangles.</param>
		/// <returns></returns>
		public Rectangle[] GetRectangles (int maxCount = -1 ) 
		{
			ThrowIfDisposed();

			if (maxCount<0) {
				maxCount = elements.Count;
			}
			return Enumerable.Range( 0, maxCount )
				.Select( i => (i<elements.Count)? elements[i].GetRect() : new Rectangle(0,0,0,0) )
				.ToArray();
		}



		/// <summary>
		/// Gets float rectangles of all subimages in normalized texture coordibates
		/// </summary>
		/// <param name="maxCount">Maximum number of recatangles. 
		/// If maxCount greater than number of images the rest of 
		/// he array will be filled with zeroed rectangles.</param>
		/// <returns></returns>
		public RectangleF[] GetNormalizedRectangles ( int maxCount = -1 ) 
		{
			ThrowIfDisposed();

			if (maxCount<0) {
				maxCount = elements.Count;
			}

			float w = Texture.Width;
			float h = Texture.Height;

			return Enumerable.Range( 0, maxCount )
				.Select( i => (i<elements.Count)? elements[i].GetRectF(w,h) : new RectangleF(0,0,0,0) )
				.ToArray();
		}



		/// <summary>
		/// Gets index if particular subimage.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public int IndexOf( string name )
		{
			return dictionary[name].Index;
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
