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
	public class VTPage {

		public readonly VTAddress Address;

		public VTPage ( VTAddress address )
		{
			this.Address	=	address;	
		}
	}
}
