using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Mathematics;
using Fusion.Core.Shell;
using Fusion.Build.ImageUtils;

namespace Fusion.Build.Processors {

	[AssetProcessor("TextureAtlas", "Merges multiple textures to single one.")]
	public class TextureAtlasProcessor : AssetProcessor {

		[CommandLineParser.Name("nomips", "do not generate mipmap levels")]
		public bool	NoMips { get; set; }

		[CommandLineParser.Name("dxt", "use DXT compression method")]
		public bool	UseDXT { get; set; }

		[CommandLineParser.Name("halfoffset", "adds half-pixel offset to texture coordinates")]
		public bool	HalfTexel { get; set; }

		[CommandLineParser.Name("width", "output image width (default 256)")]
		public int Width { get; set; }

		[CommandLineParser.Name("height", "output image height (default 256)")]
		public int Height { get; set; }

		[CommandLineParser.Name("pad", "padding between images (default 0)")]
		public int Padding { get; set; }

		[CommandLineParser.Name("fill", "color to fill gaps (default black)")]
		public Color FillColor { get; set; }


		/// <summary>
		/// 
		/// </summary>
		public TextureAtlasProcessor ()
		{
			Width		=	256;
			Height		=	256;
			Padding		=	0;
			FillColor	=	Color.Black;
		}




		Image LoadImage ( string fileName )
		{
			try {
				return Image.LoadTga( fileName );
			} catch ( Exception e ) {
				throw new BuildException( string.Format("Failed to load atlas fragment {0}: {1}", fileName, e.Message ) );
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildContext"></param>
		public override void Process ( AssetSource assetFile, BuildContext context )
		{
			var fileDir		=	Path.GetDirectoryName( assetFile.FullSourcePath );

			var fileNames	=	File.ReadAllLines(assetFile.FullSourcePath)
								.Select( f1 => f1.Trim() )
								.Where( f2 => !f2.StartsWith("#") )
								.Select( f3 => Path.Combine( fileDir, f3 ) )
								.ToArray();


			var depNames	=	File.ReadAllLines(assetFile.FullSourcePath)
								.Select( f1 => f1.Trim() )
								.Where( f2 => !f2.StartsWith("#") )
								.Select( f3 => Path.Combine( Path.GetDirectoryName(assetFile.KeyPath), f3 ) )
								.ToArray();

			var images		=	fileNames
								.Select( fn => LoadImage( fn ) )
								.OrderByDescending( img0 => img0.Width * img0.Height )
								.ThenByDescending( img1 => img1.Width )
								.ThenByDescending( img2 => img2.Height )
								.ToList();

			if (!images.Any()) {
				throw new InvalidOperationException("At least one subimage must be added to the texture atlas");
			}


			//
			//	Pack atlas :
			//			
			AtlasNode root = new AtlasNode(0,0, Width, Height, Padding );

			foreach ( var img in images ) {
				var n = root.Insert( img );
				if (n==null) {
					throw new InvalidOperationException("No enough room to place image");
				}
			}

			//
			//	Create image and fill it with atlas elements :
			//	
			var targetImage	=	new Image( Width, Height );
			targetImage.Fill( FillColor );

			root.WriteImages( targetImage );

			//
			//	Save and compress :
			//
			var tgaOutput	=	context.GetTempFileName( assetFile.KeyPath, ".tga" );
			var ddsOutput	=	context.GetTempFileName( assetFile.KeyPath, ".dds" );
			Image.SaveTga( targetImage, tgaOutput );

			var compression =	UseDXT ? TextureProcessor.TextureCompression.BC3 : TextureProcessor.TextureCompression.RGB;
			TextureProcessor.RunNVCompress( context, tgaOutput, ddsOutput, NoMips, false, false, true, true, false, compression );


			//
			//	Write binary blob (text + dds texture):
			//
			using ( var fs = assetFile.OpenTargetStream(depNames) ) {
				var bw = new BinaryWriter( fs );

				bw.Write(new[]{'A','T','L','S'});
				bw.Write( images.Count ); 

				root.WriteLayout( bw );

				bw.Write( (int)(new FileInfo(ddsOutput).Length) );
				
				using ( var dds = File.OpenRead( ddsOutput ) ) {
					dds.CopyTo( fs );
				}
			}
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Layouting stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		class AtlasNode {
			public AtlasNode left;
			public AtlasNode right;
			public Image tex;
			public int x;
			public int y;
			public int width;
			public int height;
			public bool in_use;
			public int padding;


			public override string ToString ()
			{
				return string.Format("{0} {1} {2} {3}", x,y, width,height);
			}



			public AtlasNode (int x, int y, int width, int height, int padding)
			{
				left = null;
				right = null;
				tex = null;
				this.x = x;
				this.y = y;
				this.width = width;
				this.height = height;
				in_use = false;
				this.padding = padding;
			}



			public AtlasNode Insert( Image surface )
			{
				if (left!=null) {
					AtlasNode rv;
					
					if (right==null) {
						throw new InvalidOperationException("AtlasNode(): error");
					}

					rv = left.Insert(surface);
					
					if (rv==null) {
						rv = right.Insert(surface);
					}
					
					return rv;
				}

				int img_width  = surface.Width  + padding * 2;
				int img_height = surface.Height + padding * 2;

				if (in_use || img_width > width || img_height > height) {
					return null;
				}

				if (img_width == width && img_height == height) {
					in_use = true;
					tex = surface;
					return this;
				}

				if (width - img_width > height - img_height) {
					/* extend to the right */
					left = new AtlasNode(x, y, img_width, height, padding);
					right = new AtlasNode(x + img_width, y,
										   width - img_width, height, padding);
				} else {
					/* extend to bottom */
					left = new AtlasNode(x, y, width, img_height, padding);
					right = new AtlasNode(x, y + img_height,
										   width, height - img_height, padding);
				}

				return left.Insert(surface);
			}



			public void WriteImages ( Image targetImage ) 
			{
				if (tex!=null) {
					targetImage.Copy( x + padding, y + padding, tex );
				}

				if (left!=null)  left.WriteImages( targetImage );
				if (right!=null) right.WriteImages( targetImage );
			}



			public void WriteLayout ( BinaryWriter bw ) 
			{
				if (tex!=null) {
					var name = Path.GetFileNameWithoutExtension( tex.Name );
					//stringBuilder.AppendFormat("{0} {1} {2} {3} {4}\r\n", name, x + padding, y + padding, tex.Width, tex.Height );
					bw.Write( name );
					bw.Write( x + padding );
					bw.Write( y + padding );
					bw.Write( tex.Width );
					bw.Write( tex.Height );
				}

				if (left!=null)  left.WriteLayout( bw );
				if (right!=null) right.WriteLayout( bw );
			}
		}
	
	}}
