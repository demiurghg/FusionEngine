using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
using Fusion.Drivers.Graphics;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Fusion.Core.Mathematics;
using Fusion.Core.Shell;


namespace Fusion.Drivers.Graphics {

	public class GraphicsParameters {

		/// <summary>
		/// Enables debug graphics device.
		/// </summary>
		public bool UseDebugDevice { get; set; }

		/// <summary>
		/// Display width
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Display height
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Fullscreen mode
		/// </summary>
		public bool FullScreen { get; set; }

		/// <summary>
		/// Stereo mode
		/// </summary>
		public StereoMode	StereoMode		{ get; set; }

		/// <summary>
		/// Stereo interlacing mode
		/// </summary>
		public InterlacingMode	InterlacingMode		{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public GraphicsProfile	GraphicsProfile		{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int MsaaLevel { get; set; }


		/// <summary>
		/// 
		/// </summary>
		public GraphicsParameters()
		{
			SetDefault();
		}


		/// <summary>
		/// 
		/// </summary>
		void SetDefault ()
		{
			Width			=	800;
			Height			=	600;
			FullScreen		=	false;
			StereoMode		=	StereoMode.Disabled;
			MsaaLevel		=	1;
		}
	}
}
