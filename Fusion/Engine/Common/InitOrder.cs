using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Drivers.Graphics;
using Fusion.Core;


namespace Fusion.Engine.Common {

	/// <summary>
	/// Defines game module initialization order.
	/// </summary>
	public enum InitOrder {

		/// <summary>
		/// Module will be initialized before initializing of parent module.
		/// </summary>
		Before	=	-1,

		/// <summary>
		/// Module will be initialized and disposed explicitly by user.
		/// </summary>
		Manual	=	0,

		/// <summary>
		/// Module will be initialized after initializing of parent module.
		/// </summary>
		After	=	1,
	}
}
