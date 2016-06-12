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
		public const int VTSize		=	128 * 256;




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
			var textures	=	new List<MapTexture>();

			//	get list of scenes :
			var mapScenes	=	File.ReadAllLines(assetFile.FullSourcePath)
								.Select( f1 => f1.Trim() )
								.Where( f2 => !f2.StartsWith("#") && !string.IsNullOrWhiteSpace(f2) )
								.Select( f3 => new MapScene( f3, Path.Combine( fileDir, f3 ) ) )
								.ToArray();

			Log.Message("-------- map: {0} --------", assetFile.KeyPath );

			//	build each scene :
			foreach ( var mapScene in mapScenes ) {
				mapScene.BuildScene( context );

				textures.AddRange( mapScene.Textures );
			}

			Log.Message("{0} textures", textures.Count);

			LayoutTextures( textures );

			GeneratePages( textures, context );

			Log.Message("----------------" );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="textures"></param>
		void LayoutTextures ( IEnumerable<MapTexture> textures )
		{
			var sortedList = textures
						.OrderByDescending( img0 => img0.Width * img0.Height )
						.ThenByDescending( img1 => img1.Width )
						.ThenByDescending( img2 => img2.Height )
						.ToList();

			var root = new AtlasNode(0,0, VTSize, VTSize, 0 );
			
			foreach ( var img in sortedList ) {
				var n = root.Insert( img );
				if (n==null) {
					throw new BuildException("No enough room to place texture. Are U mad, dude???");
				}
			}

			foreach ( var tex in textures ) {
				Log.Message("{0,6} {1,6} - {2}", tex.TexelOffsetX, tex.TexelOffsetY, tex.Name );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="textures"></param>
		void GeneratePages ( IEnumerable<MapTexture> textures, BuildContext context )
		{
			foreach ( var texture in textures ) {
				texture.GeneratePages( context );
			}
			
			//Parallel.ForEach( textures, (texture) => texture.GeneratePages( context ) );
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Layouting stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		class AtlasNode {
			public AtlasNode Left;
			public AtlasNode Right;
			public MapTexture Texture;
			public int X;
			public int Y;
			public int Width;
			public int Height;
			public bool InUse;
			public int Padding;


			public override string ToString ()
			{
				return string.Format("{0} {1} {2} {3}", X,Y, Width,Height);
			}



			public AtlasNode (int x, int y, int width, int height, int padding)
			{
				Left = null;
				Right = null;
				Texture = null;
				this.X = x;
				this.Y = y;
				this.Width = width;
				this.Height = height;
				InUse = false;
				this.Padding = padding;
			}



			public AtlasNode Insert( MapTexture texture )
			{
				if (Left!=null) {
					AtlasNode rv;
					
					if (Right==null) {
						throw new InvalidOperationException("AtlasNode(): error");
					}

					rv = Left.Insert(texture);
					
					if (rv==null) {
						rv = Right.Insert(texture);
					}
					
					return rv;
				}

				int img_width  = texture.Width  + Padding * 2;
				int img_height = texture.Height + Padding * 2;

				if (InUse || img_width > Width || img_height > Height) {
					return null;
				}

				if (img_width == Width && img_height == Height) {
					InUse = true;
					Texture = texture;
					Texture.TexelOffsetX	=	X;
					Texture.TexelOffsetY	=	Y;
					return this;
				}

				if (Width - img_width > Height - img_height) {
					/* extend to the right */
					Left = new AtlasNode(X, Y, img_width, Height, Padding);
					Right = new AtlasNode(X + img_width, Y,
										   Width - img_width, Height, Padding);
				} else {
					/* extend to bottom */
					Left = new AtlasNode(X, Y, Width, img_height, Padding);
					Right = new AtlasNode(X, Y + img_height,
										   Width, Height - img_height, Padding);
				}

				return Left.Insert(texture);
			}



			public void WriteLayout ( BinaryWriter bw ) 
			{
				if (Texture!=null) {
					var name = Path.GetFileNameWithoutExtension( Texture.Name );
					bw.Write( name );
					bw.Write( X + Padding );
					bw.Write( Y + Padding );
					bw.Write( Texture.Width );
					bw.Write( Texture.Height );
				}

				if (Left!=null)  Left.WriteLayout( bw );
				if (Right!=null) Right.WriteLayout( bw );
			}
		}
	
	}}
