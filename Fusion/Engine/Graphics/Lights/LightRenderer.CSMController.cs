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
	internal partial class LightRenderer : RenderComponent {

		class DefaultCSMController : ICSMController {

			
			Matrix[]	shadowViews			=	new Matrix[CascadedShadowMap.MaxCascadeCount];
			Matrix[]	shadowProjections	=	new Matrix[CascadedShadowMap.MaxCascadeCount];


			public DefaultCSMController ()
			{
			}



			public bool IsCascadeDirty ( int cascadeIndex )
			{
				return true;
			}


			public int GetActiveCascadeCount()
			{
				return CascadedShadowMap.MaxCascadeCount;
			}


			public void ComputeMatricies ( Matrix viewMatrix, Vector3 lightDir, int cascadeSize, float splitSize, float splitOffset, float splitFactor, float projDepth )
			{
				var	smSize		=	cascadeSize;
				var camMatrix	=	Matrix.Invert( viewMatrix );
				var viewPos		=	camMatrix.TranslationVector;

				lightDir.Normalize();


				for ( int i = 0; i<4; i++ ) {

					float	offset		=	splitOffset * (float)Math.Pow( splitFactor, i );
					float	radius		=	splitSize   * (float)Math.Pow( splitFactor, i );

					Vector3 viewDir		=	camMatrix.Forward.Normalized();
					Vector3	origin		=	viewPos + viewDir * offset;

					Matrix	lightRot	=	Matrix.LookAtRH( Vector3.Zero, Vector3.Zero + lightDir, Vector3.UnitY );
					Matrix	lightRotI	=	Matrix.Invert( lightRot );
					Vector3	lsOrigin	=	Vector3.TransformCoordinate( origin, lightRot );
					float	snapValue	=	4.0f * radius / smSize;
					lsOrigin.X			=	(float)Math.Round(lsOrigin.X / snapValue) * snapValue;
					lsOrigin.Y			=	(float)Math.Round(lsOrigin.Y / snapValue) * snapValue;
					lsOrigin.Z			=	(float)Math.Round(lsOrigin.Z / snapValue) * snapValue;
					origin				=	Vector3.TransformCoordinate( lsOrigin, lightRotI );//*/

					shadowViews[i]			=	Matrix.LookAtRH( origin, origin + lightDir, Vector3.UnitY );
					shadowProjections[i]	=	Matrix.OrthoRH( radius*2, radius*2, -projDepth/2, projDepth/2);
				}
			}



			public Matrix GetShadowViewMatrix ( int cascadeIndex )
			{
				return shadowViews[cascadeIndex];
			}



			public Matrix GetShadowProjectionMatrix ( int cascadeIndex )
			{
				return shadowProjections[cascadeIndex];
			}

		}

	}
}
