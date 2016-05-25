using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {

	internal class RenderCounters {

		static readonly Guid HudCategory = Guid.NewGuid();

		public int	SceneDIPs		{ get; set;	}
		public int	ShadowDIPs		{ get; set;	}
		public int	TotalDIPs		{ get { return SceneDIPs + ShadowDIPs; } }
		public int	DeadParticles	{ get; set; }


		/// <summary>
		/// 
		/// </summary>
		public RenderCounters ()
		{
			Reset();
		}


		/// <summary>
		/// Resets all counters
		/// </summary>
		public void Reset ()
		{
			SceneDIPs	=	0;
			ShadowDIPs	=	0;
		}

		/// <summary>
		/// Prints counters to HUD
		/// </summary>
		public void PrintCounters ()
		{
			Hud.Clear(HudCategory);
			Hud.Add(HudCategory, Color.White, "DIPs: Scene:{0}  Shadow:{1} Total: {2}", SceneDIPs, ShadowDIPs, TotalDIPs );
			Hud.Add(HudCategory, Color.White, "Dead particles: {0}", DeadParticles );
		}
	}
}
