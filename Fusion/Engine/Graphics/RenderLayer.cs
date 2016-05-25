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

	/// <summary>
	/// Represents entire visible world.
	/// </summary>
	public class RenderLayer : DisposableBase {
		
		protected readonly Game		Game;
		protected readonly RenderSystem	rs;

		/// <summary>
		/// Indicates whether view should be drawn.
		/// Default value is True.
		/// </summary>
		public bool Visible {
			get; set;
		}

		/// <summary>
		/// Indicates in which order view should be drawn.
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
		/// Gets view target.
		/// Null value indicates that view will be rendered to backbuffer.
		/// Default value is null.
		/// </summary>
		public TargetTexture Target {
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
		/// Gets collection of sprite layers.
		/// </summary>
		public ICollection<SpriteLayer>	SpriteLayers {
			get; private set;
		}


		/// <summary>
		/// Creates ViewLayer instance
		/// </summary>
		/// <param name="Game">Game engine</param>
		public RenderLayer ( Game game )
		{
			Game		=	game;
			this.rs		=	Game.RenderSystem;

			Visible		=	true;
			Order		=	0;

			Camera		=	new Camera();

			SpriteLayers	=	new SpriteLayerCollection();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
			}
			base.Dispose( disposing );
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Rendering :
		 * 
		-----------------------------------------------------------------------------------------*/


		/// <summary>
		/// Renders view
		/// </summary>
		internal virtual void Render ( GameTime gameTime, StereoEye stereoEye )
		{
			var targetSurface = (Target == null) ? rs.Device.BackbufferColor.Surface : Target.RenderTarget.Surface;

			//	clear target buffer if necassary :
			if (Clear) {
				rs.Device.Clear( targetSurface, ClearColor );
			}

			var viewport	=	new Viewport( 0,0, targetSurface.Width, targetSurface.Height );

			//	draw debug stuff :
			//  ...

			//	draw sprites :
			rs.SpriteEngine.DrawSprites( gameTime, stereoEye, targetSurface, SpriteLayers );

			rs.Filter.FillAlphaOne( targetSurface );
		}
	}
}
