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
using System.Runtime.InteropServices;

namespace Fusion.Engine.Graphics {
	public partial class LightRenderer {

		DefaultCSMController	csmController	=	new DefaultCSMController();


		/// <summary>
		/// 
		/// </summary>
		/// <param name="?"></param>
		internal void RenderShadows ( RenderWorld renderWorld, LightSet lightSet )
		{
			var device = Game.GraphicsDevice;
			var camera		=	renderWorld.Camera;
			var instances	=	renderWorld.Instances;

			if (SkipShadows) {
				return;
			}


			CheckShadowSize();


			csmController.ComputeMatricies( 
				camera.GetViewMatrix(StereoEye.Mono), 
				lightSet.DirectLight.Direction, 
				cascadedShadowMap.CascadeSize,
				this.SplitSize,
				this.SplitOffset,
				this.SplitFactor,
				this.CSMProjectionDepth );


			ICSMController csmCtrl	=	lightSet.DirectLight.CSMController ?? csmController;


			int activeCascadeCount	=	Math.Min( cascadedShadowMap.CascadeCount, csmCtrl.GetActiveCascadeCount() );


			using (new PixEvent("Cascaded Shadow Maps")) {

				Game.GraphicsDevice.ResetStates();
			
				cascadedShadowMap.Clear();

				for (int i=0; i<activeCascadeCount; i++) {

					var context = new ShadowContext();
					context.ShadowView			=	csmCtrl.GetShadowViewMatrix( i );
					context.ShadowProjection	=	csmCtrl.GetShadowProjectionMatrix( i );
					context.ShadowViewport		=	cascadedShadowMap.GetCascadeViewport( i );
					context.FarDistance			=	1;
					context.SlopeBias			=	CSMSlopeBias;
					context.DepthBias			=	CSMDepthBias;
					context.ColorBuffer			=	cascadedShadowMap.ColorBuffer.Surface;
					context.DepthBuffer			=	cascadedShadowMap.DepthBuffer.Surface;

					Game.RenderSystem.SceneRenderer.RenderShadowMapCascade( context, instances );
				}
			}



			using (new PixEvent("Particle Shadows")) {
			
				for (int i=0; i<activeCascadeCount; i++) {

					var viewport = cascadedShadowMap.GetCascadeViewport(i);
					var colorBuffer = cascadedShadowMap.ParticleShadow.Surface;
					var depthBuffer = cascadedShadowMap.DepthBuffer.Surface;
					var viewMatrix	= csmController.GetShadowViewMatrix( i );
					var projMatrix	= csmController.GetShadowProjectionMatrix( i );

					renderWorld.ParticleSystem.RenderShadow( new GameTime(), viewport, viewMatrix, projMatrix, colorBuffer, depthBuffer );
				}
			}



			using (new PixEvent("Spotlight Shadow Maps")) {

				device.Clear( spotDepth.Surface, 1, 0 );
				device.Clear( spotColor.Surface, Color4.White );
				int index = 0;

				foreach ( var spot in lightSet.SpotLights ) {

					var smSize	= SpotShadowSize;
					var context = new ShadowContext();
					var dx      = index % 4;
					var dy		= index / 4;
					var far		= spot.Projection.GetFarPlaneDistance();

					index++;

					context.ShadowView			=	spot.SpotView;
					context.ShadowProjection	=	spot.Projection;
					context.ShadowViewport		=	new Viewport( smSize * dx, smSize * dy, smSize, smSize );
					context.FarDistance			=	far;
					context.SlopeBias			=	SpotSlopeBias;
					context.DepthBias			=	SpotDepthBias;
					context.ColorBuffer			=	spotColor.Surface;
					context.DepthBuffer			=	spotDepth.Surface;

					Game.RenderSystem.SceneRenderer.RenderShadowMapCascade( context, instances );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		void CheckShadowSize ()
		{
			if (CSMCascadeCount!=cascadedShadowMap.CascadeCount || CSMCascadeSize!=cascadedShadowMap.CascadeSize) {

				SafeDispose( ref cascadedShadowMap );
				cascadedShadowMap	=	new CascadedShadowMap( Game.GraphicsDevice, CSMCascadeSize, CSMCascadeCount );

			}
		}
	}
}
