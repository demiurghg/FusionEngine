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
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Imaging;
using System.Threading;


namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Represents image data of particular tile.
	/// </summary>
	public class VTTile {

		Image image;
		Image imageLow;
		
		/// <summary>
		/// Virtual address of given tile.
		/// </summary>
		public VTAddress VirtualAddress { 
			get; private set;
		}



		/// <summary>
		/// Gets image data as RGBA8 array.
		/// </summary>
		public Color[] Data {
			get {
				return image.RawImageData;
			}
		}

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <param name="stream"></param>
		public VTTile ( VTAddress address, Stream stream )
		{
			this.VirtualAddress	=	address;
			this.image		=	Image.LoadTga( stream );
			this.imageLow	=	new Image( image.Width, image.Height );

			float w = image.Width;
			float h = image.Height;

			var mipped	=	image.Downsample();
			this.imageLow.PerpixelProcessing( (x,y,input) => mipped.Sample( (x-0.5f)/w, (y-0.5f)/h ) );
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="factor"></param>
		public Color[] GetLerpedData ( float factor )
		{
			var length	=	image.RawImageData.Length;
			var data	=	new Color[ length ];

			factor	=	MathUtil.Clamp( factor, 0, 1 );

			for ( int i=0; i<length; i++) {
				data[ i ] = Color.Lerp( imageLow.RawImageData[i], image.RawImageData[i], factor );
			}			

			return data;
		}




		public void DrawBorder (bool draw)
		{
			if (!draw) {
				return;
			}

			int s	=	VTConfig.PageSize;
			var b	=	VTConfig.PageBorderWidth;

			for (int i=b; i<s+b; i++) {
				image.Write( b,     i,		Color.Red );
				image.Write( b+s-1,	i,		Color.Red );
				image.Write( i,		b,      Color.Red );
				image.Write( i,		b+s-1,	Color.Red );
			}
		}
		
	}
}
