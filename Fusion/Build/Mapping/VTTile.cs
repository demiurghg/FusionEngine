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

		/// <summary>
		/// The size of sub tile data buffer.
		/// </summary>
		public const int SubTileDataSize	=	4 * VTConfig.PageSizeBordered * VTConfig.PageSizeBordered;

		byte[]	rawColorData;
		byte[]	rawNormalData;
		byte[]	rawSpecularData;

		
		/// <summary>
		/// Creates instance of VTTile
		/// </summary>
		public VTTile ()
		{
			var dataSize	=	SubTileDataSize;
			rawColorData	=	new byte[ dataSize ];
			rawNormalData	=	new byte[ dataSize ];
			rawSpecularData	=	new byte[ dataSize ];
		}



		/// <summary>
		/// Clears tile with particular color, flat normal and no specular.
		/// </summary>
		/// <param name="color"></param>
		public void Clear( Color color )
		{
			for ( int i=0; i<SubTileDataSize/4; i++ ) {
				rawColorData[i*4+0]		=	color.R;
				rawColorData[i*4+1]		=	color.G;
				rawColorData[i*4+2]		=	color.B;
				rawColorData[i*4+3]		=	color.A;

				rawNormalData[i*4+0]	=	128;
				rawNormalData[i*4+1]	=	128;
				rawNormalData[i*4+2]	=	255;
				rawNormalData[i*4+3]	=	255;

				rawSpecularData[i*4+0]	=	0;
				rawSpecularData[i*4+1]	=	128;
				rawSpecularData[i*4+2]	=	0;
				rawSpecularData[i*4+3]	=	255;
			}
		}



		public void AssembleFromImages ( Image a, Image b, Image c )
		{
			if ( a.Width!=a.Height && a.Width!=VTConfig.PageBorderWidth) {
				throw new ArgumentException("Image width and height must be equal " + VTConfig.PageBorderWidth ); 
			}
			if ( b.Width!=a.Height && b.Width!=VTConfig.PageBorderWidth) {
				throw new ArgumentException("Image width and height must be equal " + VTConfig.PageBorderWidth ); 
			}
			if ( c.Width!=a.Height && c.Width!=VTConfig.PageBorderWidth) {
				throw new ArgumentException("Image width and height must be equal " + VTConfig.PageBorderWidth ); 
			}

			for (int i=0; i<a.RawImageData.Length; i++) {
				rawColorData[i*4+0]		=	a.RawImageData[i].R;
				rawColorData[i*4+1]		=	a.RawImageData[i].G;
				rawColorData[i*4+2]		=	a.RawImageData[i].B;
				rawColorData[i*4+3]		=	a.RawImageData[i].A;

				rawNormalData[i*4+0]	=	b.RawImageData[i].R;
				rawNormalData[i*4+1]	=	b.RawImageData[i].G;
				rawNormalData[i*4+2]	=	b.RawImageData[i].B;
				rawNormalData[i*4+3]	=	b.RawImageData[i].A;

				rawSpecularData[i*4+0]	=	c.RawImageData[i].R;
				rawSpecularData[i*4+1]	=	c.RawImageData[i].G;
				rawSpecularData[i*4+2]	=	c.RawImageData[i].B;
				rawSpecularData[i*4+3]	=	c.RawImageData[i].A;
			}
		}




		/// <summary>
		/// Writes tile data to stream.
		/// </summary>
		/// <param name="stream"></param>
		public void Write ( Stream stream )
		{
			stream.WriteFourCC( "TILE" );
			stream.Write( rawColorData, 0, SubTileDataSize );
			stream.Write( rawNormalData, 0, SubTileDataSize );
			stream.Write( rawSpecularData, 0, SubTileDataSize );
		}
		


		/// <summary>
		/// Reads tile data from stream.
		/// </summary>
		/// <param name="stream"></param>
		public void Read ( Stream stream )
		{
			if (stream.ReadFourCC()!="TILE") {
				throw new IOException("Bad virtual texture tile formar");
			}

			stream.Read( rawColorData, 0, SubTileDataSize );
			stream.Read( rawNormalData, 0, SubTileDataSize );
			stream.Read( rawSpecularData, 0, SubTileDataSize );
		}
	}
}
