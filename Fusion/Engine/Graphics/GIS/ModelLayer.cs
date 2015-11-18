using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public class ModelLayer : Gis.GisLayer
	{
		public string Name { get; protected set; }

		DVector2 CartesianPos;
		DVector2 LonLatOffset;

		Matrix Rotation;



		public ModelLayer(GameEngine engine, string fileName) : base(engine)
		{
			
		}
	}
}
