using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Fusion.Core.Mathematics;


namespace Fusion.Engine.Imaging {
	partial class Image {

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Image loading :
		 * 
		-----------------------------------------------------------------------------------------*/

		[StructLayout(LayoutKind.Sequential,Pack=1)]
		public struct TgaHeader {
		   public byte	idlength;
		   public byte	colourmaptype;
		   public byte	datatypecode;
		   public short	colourmaporigin;
		   public short	colourmaplength;
		   public byte	colourmapdepth;
		   public short	x_origin;
		   public short	y_origin;
		   public short	width;
		   public short	height;
		   public byte	bitsperpixel;
		   public byte	imagedescriptor;
		}


		public static void SaveTga ( Image image, Stream stream )
		{
			using (var bw = new BinaryWriter(stream)) {

				bw.Write((byte)0);										  /* idlength;			*/
				bw.Write( (byte)0 );									  /* colourmaptype;		*/
				bw.Write( (byte)2 );									  /* datatypecode;		*/
				bw.Write( (byte)0 ); bw.Write( (byte)0 ); 				  /* colourmaporigin;	*/
				bw.Write( (byte)0 ); bw.Write( (byte)0 ); 				  /* colourmaplength;	*/
				bw.Write( (byte)0 );									  /* colourmapdepth;	*/
				bw.Write( (byte)0 ); bw.Write( (byte)0 ); 				  /* x_origin;			*/
				bw.Write( (byte)0 ); bw.Write( (byte)0 ); 				  /* y_origin;			*/
				bw.Write( (byte)( image.Width  & 0x00FF) );				  /* width;				*/
				bw.Write( (byte)((image.Width  & 0xFF00) >> 8) );		  /* -					*/
				bw.Write( (byte)( image.Height & 0x00FF) );				  /* height;			*/
				bw.Write( (byte)((image.Height & 0xFF00) >> 8) );		  /* -					*/
				bw.Write( (byte)32   );									  /* bitsperpixel;		*/
				bw.Write( (byte)0x28 );	/* 0010 1000 */					  /* imagedescriptor;	*/

				foreach ( var c in image.RawImageData ) {
					byte r = c.R;
					byte g = c.G;
					byte b = c.B;
					byte a = c.A;
					bw.Write(b);
					bw.Write(g);
					bw.Write(r);
					bw.Write(a);
				}
			}
		}


		public static void SaveTga ( Image image, string path )
		{
			using ( var fs = File.Open( path, FileMode.Create ) ) {
				SaveTga( image, fs );
			}
   		}


		public static TgaHeader TakeTga ( Stream stream )
		{
			using (var br = new BinaryReader( stream )) {

				var header	=	new TgaHeader();

				/* char	 */	header.idlength			=	br.ReadByte();
				/* char	 */	header.colourmaptype	=	br.ReadByte();
				/* char	 */	header.datatypecode		=	br.ReadByte();
				/* short */	header.colourmaporigin	=	br.ReadInt16();
				/* short */	header.colourmaplength	=	br.ReadInt16();
				/* char	 */	header.colourmapdepth	=	br.ReadByte();
				/* short */	header.x_origin			=	br.ReadInt16();
				/* short */	header.y_origin			=	br.ReadInt16();
				/* short */	header.width			=	br.ReadInt16();
				/* short */	header.height			=	br.ReadInt16();
				/* char	 */	header.bitsperpixel		=	br.ReadByte();
				/* char	 */	header.imagedescriptor	=	br.ReadByte();

				if ( header.datatypecode != 2 ) {
					throw new Exception(string.Format("Only uncompressed RGB and RGBA images are supported. Got {0} data type code", header.datatypecode));
				}

				if ( header.bitsperpixel != 24 && header.bitsperpixel != 32 ) {
					throw new Exception(string.Format("Only 24- and 32-bit images are supported. Got {0} bits per pixel", header.bitsperpixel));
				}

				return header;
			}
		}


		public static Image LoadTga ( Stream stream )
		{
			using (var br = new BinaryReader( stream )) {
				
				TgaHeader header  = new TgaHeader();

				/* char	 */	header.idlength			=	br.ReadByte();
				/* char	 */	header.colourmaptype	=	br.ReadByte();
				/* char	 */	header.datatypecode		=	br.ReadByte();
				/* short */	header.colourmaporigin	=	br.ReadInt16();
				/* short */	header.colourmaplength	=	br.ReadInt16();
				/* char	 */	header.colourmapdepth	=	br.ReadByte();
				/* short */	header.x_origin			=	br.ReadInt16();
				/* short */	header.y_origin			=	br.ReadInt16();
				/* short */	header.width			=	br.ReadInt16();
				/* short */	header.height			=	br.ReadInt16();
				/* char	 */	header.bitsperpixel		=	br.ReadByte();
				/* char	 */	header.imagedescriptor	=	br.ReadByte();

				if ( header.datatypecode != 2 ) {
					throw new Exception(string.Format("Only uncompressed RGB and RGBA images are supported. Got {0} data type code", header.datatypecode));
				}

				if ( header.bitsperpixel != 24 && header.bitsperpixel != 32 ) {
					throw new Exception(string.Format("Only 24- and 32-bit images are supported. Got {0} bits per pixel", header.bitsperpixel));
				}
			
				int w = header.width;
				int h = header.height;
				int bytePerPixel = header.bitsperpixel / 8;

				Image	image	= new Image( w, h );
				byte[]	data	= new byte[ w * h * bytePerPixel ];

				//	skip ID :
				br.ReadBytes( header.idlength );

				//	read image data :
				br.Read( data, 0, w * h * bytePerPixel );

				bool flip	=	!MathUtil.IsBitSet(header.imagedescriptor, 5);

				unsafe {
					if ( bytePerPixel==3 ) {
						for ( int x=0; x<w; ++x ) {
							for ( int y=0; y<h; ++y ) {
								int p =  flip ? ((h-y-1) * w + x) : (y * w + x);

								image.RawImageData[ y * w + x ] = new Color( data[p*3+2], data[p*3+1], data[p*3+0], (byte)255 );
							}
						}
					} else if (bytePerPixel==4) {
						for ( int x=0; x<w; ++x ) {
							for ( int y=0; y<h; ++y ) {
								int p =  flip ? ((h-y-1) * w + x) : (y * w + x);

								image.RawImageData[ y * w + x ] = new Color( data[p*4+2], data[p*4+1], data[p*4+0], data[p*4+3] );
							}
						}
					} else if (bytePerPixel==1) {
						for ( int x=0; x<w; ++x ) {
							for ( int y=0; y<h; ++y ) {
								int p =  flip ? ((h-y-1) * w + x) : (y * w + x);

								image.RawImageData[ y * w + x ] = new Color( data[p], data[p], data[p], (byte)255 );
							}
						}
					}
				}

				return image;
			}
		}
	}
}
