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

		/// <summary>
		/// Environment light position
		/// </summary>
		public Vector3	Position;

		/// <summary>
		/// Inner radius of the environment light.
		/// </summary>
		public float	RadiusInner;

		/// <summary>
		/// Outer radius of the environment light.
		/// </summary>
		public float	RadiusOuter;


		/// <summary>
		/// Creates instance of EnvLight
		/// </summary>
		public EnvLight ()
		{
			Position	=	Vector3.Zero;
			RadiusInner	=	0;
			RadiusOuter	=	1;
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
