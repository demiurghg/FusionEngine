using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Graphics.Lights;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {
	public class Composition {

		
		readonly GameEngine		GameEngine;
		readonly GraphicsEngine	ge;

		
		/// <summary>
		/// Indicates whether composition should be drawn.
		/// Default value is True.
		/// </summary>
		public bool Visible {
			get; set;
		}

		

		/// <summary>
		/// Indicates whether composition should be drawn.
		/// </summary>
		public int Order {
			get; set;
		}



		/// <summary>
		/// Gets and sets composition's camera.
		/// This value is already initialized when Composition object is created.
		/// </summary>
		public Camera Camera {
			get; set;
		}



		/// <summary>
		/// Gets and sets composition target.
		/// Null value indicates that composition will be rendered to backbuffer.
		/// Default value is null.
		/// </summary>
		public TargetTexture Target {
			get; set;
		}



		/// <summary>
		/// Gets and sets composition's light set.
		/// This value is already initialized when Composition object is created.
		/// </summary>
		public LightSet LightSet {
			get; set;
		}



		/// <summary>
		/// Creates composition's instance.
		/// </summary>
		/// <param name="ge"></param>
		public Composition ( GameEngine gameEngine )
		{
			GameEngine	=	gameEngine;
			this.ge		=	gameEngine.GraphicsEngine;

			Visible		=	true;
			Order		=	0;

			Camera		=	new Camera();
			Target		=	null;

			LightSet	=	new LightSet();

			SpriteLayers	=	new List<SpriteLayer>();
		}



		/// <summary>
		/// Gets collection of sprite layers.
		/// </summary>
		public ICollection<SpriteLayer>	SpriteLayers {
			get; private set;
		}
	}
}
