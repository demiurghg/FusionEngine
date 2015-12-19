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
	public class AnimatedModelLayer : ModelLayer
	{
		public int FramesCount { get; protected set; }

		private Matrix[] localMatrices;
		private int firstFrame;
		private int lastFrame;


		public AnimatedModelLayer(Game engine, DVector2 lonLat, string fileName, int maxInstanceCount = 0) : base(engine, lonLat, fileName, maxInstanceCount)
		{
			localMatrices = new Matrix[model.Nodes.Count];

			firstFrame	= model.FirstFrame;
			lastFrame	= model.LastFrame;
		}
		

		public void UpdateAnimation(float t)
		{
			model.GetAnimSnapshot((float)firstFrame + (lastFrame-firstFrame)*t, firstFrame, lastFrame, AnimationMode.Clamp, localMatrices);

			model.ComputeAbsoluteTransforms(localMatrices, transforms); 
		}
	}
}
