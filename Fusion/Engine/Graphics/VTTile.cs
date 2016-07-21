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
			var tempImage		=	Image.LoadTga( stream );
			var border			=	VTConfig.PageBorderWidth;

			image	=	new Image( VTConfig.PageSizeBordered, VTConfig.PageSizeBordered );

			for (int i=0; i<image.Width; i++) {
				for (int j=0; j<image.Height; j++) {
					image.Write( i, j, tempImage.SampleClamp( i - border, j - border ) );
				}
			}
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
