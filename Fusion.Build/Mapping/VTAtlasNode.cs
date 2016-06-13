using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Build.ImageUtils;
using Fusion.Core.Mathematics;

namespace Fusion.Build.Mapping {

	class VTAtlasNode {
		public VTAtlasNode Left;
		public VTAtlasNode Right;
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



		public VTAtlasNode (int x, int y, int width, int height, int padding)
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



		public VTAtlasNode Insert( MapTexture texture )
		{
			if (Left!=null) {
				VTAtlasNode rv;
					
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
				Left = new VTAtlasNode(X, Y, img_width, Height, Padding);
				Right = new VTAtlasNode(X + img_width, Y,
										Width - img_width, Height, Padding);
			} else {
				/* extend to bottom */
				Left = new VTAtlasNode(X, Y, Width, img_height, Padding);
				Right = new VTAtlasNode(X, Y + img_height,
										Width, Height - img_height, Padding);
			}

			return Left.Insert(texture);
		}



		public void WriteLayout ( BinaryWriter bw ) 
		{
			if (Texture!=null) {
				var name = Path.GetFileNameWithoutExtension( Texture.FullPath );
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
}
