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

	[AssetProcessor("Map", "Performs map assembly")]
	public class MapProcessor : AssetProcessor {

		const int MegatextureSize = 128 * 1024;

		/// <summary>
		/// 
		/// </summary>
		public MapProcessor ()
		{
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
								.Where( f2 => !f2.StartsWith("#") && !string.IsNullOrWhiteSpace(f2) )
								.Select( f3 => Path.Combine( fileDir, f3 ) )
								.ToArray();

			foreach ( var scene in fileNames ) {
				Log.Message("...scene: {0}", scene );
			}

			var depNames	=	File.ReadAllLines(assetFile.FullSourcePath)
								.Select( f1 => f1.Trim() )
								.Where( f2 => !f2.StartsWith("#") && !string.IsNullOrWhiteSpace(f2) )
								.Select( f3 => Path.Combine( Path.GetDirectoryName(assetFile.KeyPath), f3 ) )
								.ToArray();

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
