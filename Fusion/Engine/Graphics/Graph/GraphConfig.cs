using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics.Graph
{
	public class GraphConfig {
		//link

        public float Max_mass { get; set; }
        public float Min_mass { get; set; }

        public IntegratorType IType		{ get; set; } 
        public float Rotation			{ get; set; } 
        public GraphType GraphType		{ get; set; } 
        public float MaxLinkWidth		{ get; set; } 
        public float LinkSize			{ get; set; } 
        public float EdgeMaxOpacity		{ get; set; } 
        public float MinParticleRadius	{ get; set; }
		public float NodeScale			{ get; set; } 

        public GraphConfig()
        {
			Min_mass = 0.5f;
			Max_mass = 0.5f;
			Rotation = 2.6f;
			IType		= IntegratorType.RUNGE_KUTTA;
			GraphType	= GraphType.DYNAMIC;

			MaxLinkWidth	= 1;//10
			LinkSize		= 20.0f;
			EdgeMaxOpacity	= 0.03f;

			MinParticleRadius	= 30;//250
			NodeScale			= 1;


        }
	}
}
