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
	internal class VTTile {

		Image image;

		public VTAddress Address { 
			get; private set;
		}


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
			this.Address	=	address;
			image			=	Image.LoadTga( stream );
		}
		
	}
}
