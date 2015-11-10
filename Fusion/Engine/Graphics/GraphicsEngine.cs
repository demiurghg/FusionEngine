using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {

	public class GraphicsEngine : GameModule {

		internal readonly GraphicsDevice Device;

		[GameModule("Sprites", "sprite", InitOrder.After)]
		public SpriteEngine	SpriteEngine { get { return spriteEngine; } }
		SpriteEngine	spriteEngine;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="engine"></param>
		public GraphicsEngine ( GameEngine gameEngine ) : base(gameEngine)
		{
			this.Device	=	gameEngine.GraphicsDevice;

			Compositions	=	new List<Composition>();
			spriteEngine	=	new SpriteEngine( this );
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
			var comps = Compositions
				.OrderBy( c1 => c1.Order )
				.Where( c2 => c2.Visible );

			foreach ( var composition in Compositions ) {
				spriteEngine.DrawSprites( gameTime, stereoEye, composition.SpriteLayers );
			}
		}



		/// <summary>
		/// Gets collection of Composition.
		/// </summary>
		public ICollection<Composition> Compositions {
			get; private set;
		}
	}
}
