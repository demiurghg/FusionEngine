using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.Lights;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {
	public class ViewLayer {
		
		readonly GameEngine		GameEngine;
		readonly GraphicsEngine	ge;

		
		/// <summary>
		/// Indicates whether view should be drawn.
		/// Default value is True.
		/// </summary>
		public bool Visible {
			get; set;
		}

		

		/// <summary>
		/// Indicates whether view should be drawn.
		/// </summary>
		public int Order {
			get; set;
		}



		/// <summary>
		/// Gets and sets view's camera.
		/// This value is already initialized when View object is created.
		/// </summary>
		public Camera Camera {
			get; set;
		}



		/// <summary>
		/// Gets and sets view target.
		/// Null value indicates that view will be rendered to backbuffer.
		/// Default value is null.
		/// </summary>
		public TargetTexture Target {
			get; set;
		}



		/// <summary>
		/// Gets and sets view light set.
		/// This value is already initialized when View object is created.
		/// </summary>
		public LightSet LightSet {
			get; set;
		}



		/// <summary>
		/// Gets and sets view bounds.
		/// </summary>
		public Rectangle ViewBounds {
			get; set;
		}



		/// <summary>
		/// Indicated whether target buffer should be cleared before rendering.
		/// </summary>
		public bool Clear {	
			get; set;
		}



		/// <summary>
		/// Gets and sets clear color
		/// </summary>
		public Color4 ClearColor {
			get; set;
		}



		/// <summary>
		/// Creates view's instance.
		/// </summary>
		/// <param name="ge"></param>
		public ViewLayer ( GameEngine gameEngine )
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



		/// <summary>
		/// Renders view
		/// </summary>
		internal void RenderView ( GameTime gameTime, StereoEye stereoEye )
		{
			var targetRT = (Target!=null) ? Target.RenderTarget : ge.Device.BackbufferColor;

			//	clear target buffer if necassary :
			if (Clear) {
				ge.Device.Clear( targetRT.Surface, ClearColor );
			}

			#warning TODO: set render target!

			//	draw sprites :
			ge.SpriteEngine.DrawSprites( gameTime, stereoEye, SpriteLayers );
		}
	}
}
