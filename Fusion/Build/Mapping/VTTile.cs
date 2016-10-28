using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Extensions;
using Fusion.Engine.Graphics;
using Fusion.Core.Mathematics;
using Fusion.Engine.Imaging;

namespace Fusion.Build.Mapping {
	
	public class VTTile {

		static Random rand = new Random();		

		VTAddress	address;

		/// <summary>
		/// Gets address of the tile
		/// </summary>
		public VTAddress VirtualAddress {
			get {
				return address;
			}
		}

		Image	colorData;
		Image	normalData;
		Image	specularData;

		
		/// <summary>
		/// Creates instance of VTTile
		/// </summary>
		public VTTile ( VTAddress address )
		{
			this.address	=	address;
			var size		=	VTConfig.PageSizeBordered;
			colorData		=	new Image(size, size);
			normalData		=	new Image(size, size);
			specularData	=	new Image(size, size);
		}



		/// <summary>
		/// Create instance of tile from three images. Images must be the same size and has equal width and height.
		/// Width and height must be equal VTConfig.PageBorderWidth
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		public VTTile ( VTAddress address, Image a, Image b, Image c )
		{
			this.address	=	address;

			if ( a.Width!=a.Height && a.Width!=VTConfig.PageSizeBordered) {
				throw new ArgumentException("Image width and height must be equal " + VTConfig.PageBorderWidth ); 
			}
			if ( b.Width!=a.Height && b.Width!=VTConfig.PageSizeBordered) {
				throw new ArgumentException("Image width and height must be equal " + VTConfig.PageBorderWidth ); 
			}
			if ( c.Width!=a.Height && c.Width!=VTConfig.PageSizeBordered) {
				throw new ArgumentException("Image width and height must be equal " + VTConfig.PageBorderWidth ); 
			}

			colorData		=	a;
			normalData		=	b;
			specularData	=	c;
		}



		/// <summary>
		/// Clears tile with particular color, flat normal and no specular.
		/// </summary>
		/// <param name="color"></param>
		public void Clear( Color color )
		{
			colorData.Fill( color );
			normalData.Fill( Color.FlatNormals );
			specularData.Fill( Color.Black );
		}



		/// <summary>
		/// Sampling perfomed using coordinates from top-left corner including border
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		public void SampleQ4( int x, int y, ref Color a, ref Color b, ref Color c )
		{
			a	=	colorData.SampleQ4Clamp( x, y );
			b	=	normalData.SampleQ4Clamp( x, y );
			c	=	specularData.SampleQ4Clamp( x, y );
		}



		/// <summary>
		/// Gets GPU-ready data
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Color[] GetGpuData(int index) 
		{
			switch ( index ) {
				case 0: return colorData.RawImageData;
				case 1: return normalData.RawImageData;
				case 2: return specularData.RawImageData;
				default: return null;
			}
		}



		/// <summary>
		/// Sets values.
		/// Addressing is performed from top-left corner including border
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		public void SetValues ( int x, int y, ref Color a, ref Color b, ref Color c )
		{
			colorData.Write( x, y, a );
			normalData.Write( x, y, b );
			specularData.Write( x, y, c );
		}



		/// <summary>
		/// Writes tile data to stream.
		/// </summary>
		/// <param name="stream"></param>
		public void Write ( Stream stream )
		{
			using ( var writer = new BinaryWriter( stream ) ) {
				writer.WriteFourCC( "TILE" );
				writer.Write( colorData.RawImageData );
				writer.Write( normalData.RawImageData );
				writer.Write( specularData.RawImageData );
			}
		}
		


		/// <summary>
		/// Reads tile data from stream.
		/// </summary>
		/// <param name="stream"></param>
		public void Read ( Stream stream )
		{
			using ( var reader = new BinaryReader( stream ) ) {
				if (reader.ReadFourCC()!="TILE") {
					throw new IOException("Bad virtual texture tile format");
				}

				var size		=	VTConfig.PageSizeBordered;
				colorData		=	new Image(size, size);
				normalData		=	new Image(size, size);
				specularData	=	new Image(size, size);

				var length		=	size * size;

				reader.Read( colorData.RawImageData, length );
				reader.Read( normalData.RawImageData, length );
				reader.Read( specularData.RawImageData, length );
			}
		}





		/// <summary>
		/// Draws text
		/// </summary>
		/// <param name="font"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="text"></param>
		public void DrawText ( Image font, int x, int y, string text )
		{
			for (int i=0; i<text.Length; i++) {

				var ch		=	((int)text[i]) & 0xFF;

				int srcX	=	(ch % 16) * 8;
				int srcY	=	(ch / 16) * 8;
				int dstX	=	x + i * 8;
				int dstY	=	y;

				font.CopySubImageTo( srcX, srcY, 9,8, dstX, dstY, colorData );
			}
		}



		/// <summary>
		/// Fills tile with random color
		/// </summary>
		public void FillRandomColor ()
		{
			var color = rand.NextColor();

			Clear( color );
		}



		/// <summary>
		/// Draws checkers
		/// </summary>
		public void DrawChecker ()
		{
			int s	=	VTConfig.PageSize;
			var b	=	VTConfig.PageBorderWidth;

			for (int i=-b; i<s+b; i++) {
				for (int j=-b; j<s+b; j++) {
					
					var m = this.VirtualAddress.MipLevel;
					int u = i + b;
					int v = j + b;

					var c = (((i << m ) >> 5) + ((j << m ) >> 5)) & 0x01;

					if (c==0) {
						colorData.Write( u,v, Color.Black );
					} else {
						colorData.Write( u,v, Color.White );
					}
				}			
			}
		}



		/// <summary>
		/// Draws border
		/// </summary>
		public void DrawBorder ()
		{
			int s	=	VTConfig.PageSize;
			var b	=	VTConfig.PageBorderWidth;

			for (int i=b; i<s+b; i++) {
				colorData.Write( b,     i,		Color.Red );
				colorData.Write( b+s-1,	i,		Color.Red );
				colorData.Write( i,		b,      Color.Red );
				colorData.Write( i,		b+s-1,	Color.Red );
			}
		}

	}
}
