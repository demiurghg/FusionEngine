using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Represents DOF processing properties.
	/// </summary>
	public class DofSettings {

		/// <summary>
		/// Is DOF effect enabled?
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Aperture 
		/// </summary>
		public float Aperture { get; set; }

		/// <summary>
		/// Focal length
		/// </summary>
		public float FocalLength { get; set; }

		/// <summary>
		/// Distance to object
		/// </summary>
		public float PlaneInFocus { get; set; }


		/// <summary>
		/// Gets COC scale
		/// </summary>
		internal float CocScale {
			get {
				return Aperture * FocalLength * PlaneInFocus / ( PlaneInFocus - FocalLength );
			}
		}

		/// <summary>
		/// Gets COC bias
		/// </summary>
		internal float CocBias {
			get {															  
				return - Aperture * FocalLength / ( PlaneInFocus - FocalLength );
			}
		}


		internal DofSettings ()
		{
			Enabled			=	false;
			Aperture		=	5;
			FocalLength		=	0.1f;
			PlaneInFocus	=	7;
		}
	}
}

	