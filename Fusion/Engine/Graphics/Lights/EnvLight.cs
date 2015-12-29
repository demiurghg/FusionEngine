using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {
	public class EnvLight {

		internal int radianceCacheIndex;

		/// <summary>
		/// Environment light position
		/// </summary>
		public Vector3	Position { get; set; }

		/// <summary>
		/// Inner radius of the environment light.
		/// </summary>
		public float	RadiusInner { get; set; }

		/// <summary>
		/// Outer radius of the environment light.
		/// </summary>
		public float	RadiusOuter { get; set; }

		/// <summary>
		/// Environment light intensity.
		/// A filter color to apply to the cubemaps. Possible uses: animate a sky color change, lighting environment change. 
		/// Alpha channel is used as offset applied to environment light index to lerp between sequential lights.
		/// Default value is 1,1,1,0.
		/// </summary>
		public Color4	Intensity { get; set; }

		/// <summary>
		/// Creates instance of EnvLight
		/// </summary>
		public EnvLight ()
		{
			Position	=	Vector3.Zero;
			RadiusInner	=	0;
			RadiusOuter	=	1;
			Intensity	=	new Color4(1,1,1,0);
		}


		/// <summary>
		/// Creates instance of EnvLight
		/// </summary>
		/// <param name="position"></param>
		/// <param name="innerRadius"></param>
		/// <param name="outerRadius"></param>
		public EnvLight ( Vector3 position, float innerRadius, float outerRadius )
		{
			this.Position		=	position;
			this.RadiusInner	=	innerRadius;
			this.RadiusOuter	=	outerRadius;
		}
		
	}
}
