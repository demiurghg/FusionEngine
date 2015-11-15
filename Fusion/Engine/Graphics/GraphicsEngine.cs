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

namespace Fusion.Engine.Graphics {

	public class GraphicsEngine : GameModule {

		internal readonly GraphicsDevice Device;

		[GameModule("Sprites", "sprite", InitOrder.After)]
		public SpriteEngine	SpriteEngine { get { return spriteEngine; } }
		SpriteEngine	spriteEngine;

		[GameModule("GIS", "gis", InitOrder.After)]
		public GIS.GIS GIS { get { return gis; } }
		GIS.GIS gis;

		[GameModule("Filter", "filter", InitOrder.After)]
		public DeferredDemo.Filter Filter { get{ return filter; } }
		public DeferredDemo.Filter filter;

		[GameModule("HdrFilter", "hdr", InitOrder.After)]
		public DeferredDemo.HdrFilter HdrFilter { get{ return hdrFilter; } }
		public DeferredDemo.HdrFilter hdrFilter;


		[Config]
		public  GraphicsEngineConfig Config { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="engine"></param>
		public GraphicsEngine ( GameEngine gameEngine ) : base(gameEngine)
		{
			Config		=	new GraphicsEngineConfig();
			this.Device	=	gameEngine.GraphicsDevice;

			ViewLayers	=	new List<ViewLayer>();
			spriteEngine	=	new SpriteEngine( this );
			gis				=	new GIS.GIS(gameEngine);
			filter			=	new DeferredDemo.Filter( gameEngine );
			hdrFilter		=	new DeferredDemo.HdrFilter( gameEngine );

			Device.DisplayBoundsChanged += (s,e) => {
				var handler = DisplayBoundsChanged;
				if (handler!=null) {
					handler(s,e);
				}
			};
		}


										  
		/// <summary>
		/// Applies graphics parameters.
		/// </summary>
		/// <param name="p"></param>
		internal void ApplyParameters ( ref GraphicsParameters p )
		{
			p.Width				=	Config.Width;
			p.Height			=	Config.Height;
			p.FullScreen		=	Config.Fullscreen;
			p.StereoMode		=	Config.StereoMode;
			p.InterlacingMode	=	Config.InterlacingMode;
		}



		/// <summary>
		/// Intializes graphics engine.
		/// </summary>
		public override void Initialize ()
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref spriteEngine );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		internal void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			//GIS.Update(gameTime);
			//GIS.Draw(gameTime, StereoEye.Mono);

			var layersToDraw = ViewLayers
				.OrderBy( c1 => c1.Order )
				.Where( c2 => c2.Visible );

			foreach ( var viewLayer in layersToDraw ) {
				viewLayer.RenderView( gameTime, stereoEye );
			}
		}



		/// <summary>
		/// Gets collection of Composition.
		/// </summary>
		public ICollection<ViewLayer> ViewLayers {
			get; private set;
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Display stuff :
		 * 
		-----------------------------------------------------------------------------------------*/
		
		/// <summary>
		/// Gets display bounds.
		/// </summary>
		public Rectangle DisplayBounds {
			get {
				return Device.DisplayBounds;
			}
		}


		/// <summary>
		/// Raises when display bound changes.
		/// DisplayBounds property is already has actual value when this event raised.
		/// </summary>
		public event EventHandler	DisplayBoundsChanged;
	}
}
