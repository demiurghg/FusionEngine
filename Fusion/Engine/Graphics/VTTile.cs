using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Extensions;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Imaging;
using System.Threading;


namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Represents image data of particular tile.
	/// </summary>
	public class VTTile {
	
		static Random rand = new Random();		


		public const int ImageCount = 1;
		
		Image[] images;

		Image MostDetailedImage {
			get {
				return images[ ImageCount - 1 ];
			}
		}

		
		/// <summary>
		/// Virtual address of given tile.
		/// </summary>
		public VTAddress VirtualAddress { 
			get; private set;
		}

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <param name="stream"></param>
		public VTTile ( VTAddress address, Stream stream )
		{
			this.VirtualAddress	=	address;

			this.images			=	new Image[ ImageCount ];

			if (ImageCount==1) {

				images[0]	=	Image.LoadTga( stream );

			} else {

				var highDetailed	=	Image.LoadTga( stream );
				var lowDetailed		=	highDetailed.DownsampleAndUpscaleBilinear();

				for ( int i=0; i<ImageCount; i++ ) {
				
					float factor	=	i / (float)(ImageCount-1);

					images[i]		=	new Image(highDetailed.Width, highDetailed.Height);

					LerpImages( images[i], lowDetailed, highDetailed, factor );
				}
			}
		}



		/// <summary>
		/// Lerps two images to third one.
		/// </summary>
		/// <param name="dst"></param>
		/// <param name="srcA"></param>
		/// <param name="srcB"></param>
		/// <param name="factor"></param>
		static void LerpImages ( Image dst, Image srcA, Image srcB, float factor )
		{
			var length = dst.RawImageData.Length;

			for (int i=0; i<length; i++) {

				dst.RawImageData[ i ] = Color.Lerp( srcA.RawImageData[i], srcB.RawImageData[i], factor );
			}
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="factor"></param>
		public Color[] GetLerpedData ( int index )
		{
			if ( index<0 || index>=ImageCount ) {
				throw new ArgumentOutOfRangeException("index", "Index must be within range [0..VTTile.ImageCount)");
			}


			return images[index].RawImageData;
		}



		public void DrawText ( Image font, int x, int y, string text )
		{
			var image	=	MostDetailedImage;
			
			for (int i=0; i<text.Length; i++) {

				var ch		=	((int)text[i]) & 0xFF;

				int srcX	=	(ch % 16) * 8;
				int srcY	=	(ch / 16) * 8;
				int dstX	=	x + i * 8;
				int dstY	=	y;

				font.CopySubImageTo( srcX, srcY, 9,8, dstX, dstY, image );
			}
		}



		public void FillRandomColor ()
		{
			var color = rand.NextColor();

			var image	=	MostDetailedImage;

			image.Fill( color );			
		}



		public void DrawChecker ()
		{
			int s	=	VTConfig.PageSize;
			var b	=	VTConfig.PageBorderWidth;

			var image	=	MostDetailedImage;

			for (int i=-b; i<s+b; i++) {
				for (int j=-b; j<s+b; j++) {
					
					var m = this.VirtualAddress.MipLevel;
					int u = i + b;
					int v = j + b;

					var c = (((i << m ) >> 5) + ((j << m ) >> 5)) & 0x01;

					if (c==0) {
						image.Write( u,v, Color.Black );
					} else {
						image.Write( u,v, Color.White );
					}
				}			
			}
		}



		public void DrawBorder ()
		{
			int s	=	VTConfig.PageSize;
			var b	=	VTConfig.PageBorderWidth;

			var image	=	MostDetailedImage;

			for (int i=b; i<s+b; i++) {
				image.Write( b,     i,		Color.Red );
				image.Write( b+s-1,	i,		Color.Red );
				image.Write( i,		b,      Color.Red );
				image.Write( i,		b+s-1,	Color.Red );
			}
		}
		
	}
}
