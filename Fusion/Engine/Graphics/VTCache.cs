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

namespace Fusion.Engine.Graphics {
	public class VTCache {

		public readonly int Width;
		public readonly int Height;

		HashSet<VTAddress> table;
		VTPage[] pages;

		int index	=	0;


		public VTCache ( int width, int height )
		{
			this.Width	= width;
			this.Height	= height;

			table	=	new HashSet<VTAddress>();
			pages	=	new VTPage[width*height];
		}



		void Add ( VTAddress address )
		{
			
		}
	}
}
