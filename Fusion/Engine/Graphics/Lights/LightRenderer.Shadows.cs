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

			
			Matrix[] shadowViews, shadowProjections;
			//	shadow is computed for both eyes :
			var view = camera.GetViewMatrix( StereoEye.Mono );
			ComputeCSMMatricies( view, renderWorld.LightSet.DirectLight.Direction, out shadowViews, out shadowProjections, out csmViewProjections );


			using (new PixEvent("Cascaded Shadow Maps")) {

				Game.GraphicsDevice.ResetStates();
			
				if ( csmDepth.Height!=CSMSize || spotDepth.Height!=SpotShadowSize * 4) {
					CreateShadowMaps();
				}

				device.Clear( csmDepth.Surface, 1, 0 );
				device.Clear( csmColor.Surface, Color4.White );

				for (int i=0; i<4; i++) {

					var smSize = CSMSize;
					var context = new ShadowContext();
					context.ShadowView			=	shadowViews[i];
					context.ShadowProjection	=	shadowProjections[i];
					context.ShadowViewport		=	new Viewport( smSize * i, 0, smSize, smSize );
					context.FarDistance			=	1;
					context.SlopeBias			=	CSMSlopeBias;
					context.DepthBias			=	CSMDepthBias;
					context.ColorBuffer			=	csmColor.Surface;
					context.DepthBuffer			=	csmDepth.Surface;

					Game.RenderSystem.SceneRenderer.RenderShadowMapCascade( context, instances );
				}
			}



			using (new PixEvent("Particle Shadows")) {
				
				device.Clear( particleShadow.Surface, new Color4(1,1,1,1) );

				var smSize = CSMSize;
				var viewport = new Viewport( smSize * 1, 0, smSize, smSize );

				renderWorld.ParticleSystem.RenderShadow( new GameTime(), viewport, shadowViews[1], shadowProjections[1], particleShadow.Surface, csmDepth.Surface );
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
		/// <param name="view"></param>
		/// <returns></returns>
		void ComputeCSMMatricies ( Matrix view, Vector3 lightDir2, out Matrix[] shadowViews, out Matrix[] shadowProjections, out Matrix[] shadowViewProjections )
		{
			shadowViews				=	new Matrix[4];
			shadowProjections		=	new Matrix[4];
			shadowViewProjections	=	new Matrix[4];

			var	smSize		=	CSMSize;
			var camMatrix	=	Matrix.Invert( view );
			var viewPos		=	camMatrix.TranslationVector;


			for ( int i = 0; i<4; i++ ) {

				float	offset		=	SplitOffset * (float)Math.Pow( SplitFactor, i );
				float	radius		=	SplitSize   * (float)Math.Pow( SplitFactor, i );

				Vector3 viewDir		=	camMatrix.Forward.Normalized();
				Vector3	lightDir	=	lightDir2.Normalized();
				Vector3	origin		=	viewPos + viewDir * offset;

				Matrix	lightRot	=	Matrix.LookAtRH( Vector3.Zero, Vector3.Zero + lightDir, Vector3.UnitY );
				Matrix	lightRotI	=	Matrix.Invert( lightRot );
				Vector3	lsOrigin	=	Vector3.TransformCoordinate( origin, lightRot );
				float	snapValue	=	4.0f * radius / smSize;
				lsOrigin.X			=	(float)Math.Round(lsOrigin.X / snapValue) * snapValue;
				lsOrigin.Y			=	(float)Math.Round(lsOrigin.Y / snapValue) * snapValue;
				lsOrigin.Z			=	(float)Math.Round(lsOrigin.Z / snapValue) * snapValue;
				origin				=	Vector3.TransformCoordinate( lsOrigin, lightRotI );//*/

				shadowViews[i]				=	Matrix.LookAtRH( origin, origin + lightDir, Vector3.UnitY );
				shadowProjections[i]		=	Matrix.OrthoRH( radius*2, radius*2, -CSMProjectionDepth/2, CSMProjectionDepth/2);

				shadowViewProjections[i]	=	shadowViews[i] * shadowProjections[i];
			}
		}

	}
}
