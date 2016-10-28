using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Fusion.Core.Mathematics;


namespace Fusion.Engine.Imaging {
	public partial class Image {

		public int	Width	{ get; protected set; }
		public int	Height	{ get; protected set; }

		public Color[]	RawImageData { get; protected set; }

		public object Tag { get; set; }
		

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="width">Image width</param>
		/// <param name="height">Image height</param>
		public Image ( int width, int height )
		{
			RawImageData	=	new Color[width*height];

			Width	=	width;
			Height	=	height;

			if (Width<=0) {
				throw new ArgumentOutOfRangeException("Image width must be > 0");
			}

			if (Height<=0) {
				throw new ArgumentOutOfRangeException("Image height must be > 0");
			}
		}



		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="width">Image width</param>
		/// <param name="height">Image height</param>
		/// <param name="fillColor">Color to fill image</param>
		public Image ( int width, int height, Color fillColor )
		{
			RawImageData	=	new Color[width*height];

			Width	=	width;
			Height	=	height;

			if (Width<=0) {
				throw new ArgumentOutOfRangeException("Image width must be > 0");
			}

			if (Height<=0) {
				throw new ArgumentOutOfRangeException("Image height must be > 0");
			}

			for (int i=0; i<RawImageData.Length; i++) {
				RawImageData[i]	=	fillColor;
			}
		}



		/// <summary>
		/// Returns address of pixel with given coordinates and adressing mode
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <param name="wrap"></param>
		/// <returns></returns>
		public int Address ( int x, int y, bool wrap = true )
		{
			if (wrap) {
				x =	Wrap( x, Width );
				y =	Wrap( y, Height );
			} else {
				x	=	Clamp( x, 0, Width - 1 );
				y	=	Clamp( y, 0, Height - 1 );
			}
			return x + y * Width;
		}



		/// <summary>
		/// Fills image with 
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="monochrome"></param>
		public void Fill ( Color color )
		{
			PerpixelProcessing( p => color );
		}


		/// <summary>
		/// Samples image at given coordinates with wraping addressing mode
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <returns></returns>
		public Color SampleWrap ( int x, int y )
		{
			x = Wrap(x, Width);
			y = Wrap(y, Height);
			var a = x + y * Width;
			return RawImageData[ a ];
		}


		/// <summary>
		/// Samples image at given coordinates with clamping addressing mode
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Color SampleClamp ( int x, int y )
		{
			x	=	Clamp( x, 0, Width - 1 );
			y	=	Clamp( y, 0, Height - 1 );
			var a = x + y * Width;
			return RawImageData[ a ];
		}



		/// <summary>
		/// Samples image at given coordinates with wraping addressing mode
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <returns></returns>
		public Color Sample ( int x, int y, bool wrap = true)
		{
			return RawImageData[ Address( x, y, wrap ) ];
		}


		/// <summary>
		/// Samples image at given coordinates with wraping addressing mode
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <returns></returns>
		public Color SampleMip ( int x, int y, bool wrap = true)
		{
			var c00 = RawImageData[ Address( x*2+0, y*2+0, true ) ];
			var c01 = RawImageData[ Address( x*2+0, y*2+1, true ) ];
			var c10 = RawImageData[ Address( x*2+1, y*2+0, true ) ];
			var c11 = RawImageData[ Address( x*2+1, y*2+1, true ) ];

			var c0x	= Color.Lerp( c00, c01, 0.5f );
			var c1x	= Color.Lerp( c10, c11, 0.5f );

			return Color.Lerp( c0x, c1x, 0.5f );
		}



		/// <summary>
		/// Samples average of four neighbouring texels with given top-left corener with clamp addressing mode
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <returns></returns>
		public Color SampleQ4Clamp ( int x, int y )
		{
			var c00 = RawImageData[ Address( x+0, y+0, false ) ];
			var c01 = RawImageData[ Address( x+0, y+1, false ) ];
			var c10 = RawImageData[ Address( x+1, y+0, false ) ];
			var c11 = RawImageData[ Address( x+1, y+1, false ) ];

			var c0x	= Color.Lerp( c00, c01, 0.5f );
			var c1x	= Color.Lerp( c10, c11, 0.5f );

			return Color.Lerp( c0x, c1x, 0.5f );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="img"></param>
		public void Copy ( int offsetX, int offsetY, Image img )
		{
			for (int x=0; x<img.Width; x++) {
				for (int y=0; y<img.Height; y++) {
					Write( offsetX + x, offsetY + y, img.Sample( x, y ) );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="img"></param>
		public void CopySubImageTo ( int srcX, int srcY, int srcWidth, int srcHeight, int dstX, int dstY, Image destination )
		{
			for (int x=0; x<srcWidth; x++) {
				for (int y=0; y<srcHeight; y++) {
					destination.Write( dstX + x, dstY + y, Sample( srcX+x, srcY+y ) );
				}
			}
		}



		/// <summary>
		/// Sample with filtering
		/// </summary>
		/// <param name="x">value within range 0..1</param>
		/// <param name="y">value within range 0..1</param>
		/// <param name="wrap"></param>
		/// <returns></returns>
		public Color Sample ( float x, float y, bool wrap = true )
		{
			var	tx	=	Frac( x * Width );
			var	ty	=	Frac( y * Height );
			int	x0	=	Wrap( (int)(x * Width)		, Width );
			int	x1	=	Wrap( (int)(x * Width + 1)	, Width );
			int	y0	=	Wrap( (int)(y * Height)		, Height );
			int	y1	=	Wrap( (int)(y * Height + 1) , Height );
			
			//   xy
			var v00	=	Sample( x0, y0, wrap );
			var v01	=	Sample( x0, y1, wrap );
			var v10	=	Sample( x1, y0, wrap );
			var v11	=	Sample( x1, y1, wrap );

			var v0x	=	Color.Lerp( v00, v01, ty );
			var v1x	=	Color.Lerp( v10, v11, ty );
			return		Color.Lerp( v0x, v1x, tx );
		}



		/// <summary>
		/// Create half-sized image using bilinear filtering
		/// </summary>
		/// <returns></returns>
		public Image DownsampleBilinear ()
		{
			var image = new Image( Width/2, Height/2 );

			image.PerpixelProcessing( (x,y,c) => this.SampleMip( x,y ) );

			return image;
		}



		/// <summary>
		/// Create half-sized image using bilinear filtering
		/// </summary>
		/// <returns></returns>
		public Image DownsampleAndUpscaleBilinear ()
		{
			float w = Width;
			float h = Height;

			var downsampled	=	DownsampleBilinear();
			var upscaled	=	new Image( Width, Height );

			upscaled.PerpixelProcessing( (x,y,input) => downsampled.Sample( (x-0.5f)/w, (y-0.5f)/h ) );

			return upscaled;
		}



		/// <summary>
		/// Writes pixel to image
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <param name="color"></param>
		/// <param name="wrap"></param>
		public void Write ( int u, int v, Color value, bool wrap = true )
		{
			RawImageData[ Address( u, v, wrap ) ] = value;
		}


		
		/// <summary>
		/// Does perpixel processing with given function
		/// </summary>
		/// <param name="procFunc"></param>
		public void PerpixelProcessing ( Func<Color, Color> procFunc, float amount = 1 )
		{
			for (int i=0; i<RawImageData.Length; i++) {
				RawImageData[i] = Color.Lerp( RawImageData[i], procFunc( RawImageData[i] ), amount );
			}
		}

		
		/// <summary>
		/// Does perpixel processing with given function
		/// </summary>
		/// <param name="procFunc"></param>
		public void PerpixelProcessing ( Func<int, int, Color, Color> procFunc )
		{
			for (int x=0; x<Width; x++) 
			for (int y=0; y<Height; y++)  
				Write(x,y, procFunc( x, y, Sample( x,y ) ) );
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Simple Math :
		 * 
		-----------------------------------------------------------------------------------------*/

		public static int Clamp ( int x, int min, int max ) 
		{
			if (x < min) return min;
			if (x > max) return max;
			return x;
		}



		public static int Wrap ( int x, int wrapSize ) 
		{
			if ( x<0 ) {
				x = x % wrapSize + wrapSize;
			}
			return	x % wrapSize;
		}



		public static float Frac ( float x )
		{
			return x < 0 ? x%1+1 : x%1;
		}



		public static float Lerp ( float a, float b, float x ) 
		{
			return a*(1-x) + b*x;
		}
	}

}
