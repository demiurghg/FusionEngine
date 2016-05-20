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

		//class DefaultCSMController : ICSMController {

		//	readonly Matrix viewMatrix;
		//	readonly Matrix camMatrix;
		//	readonly int	cascadeSize;
		//	readonly float	splitOffset;
		//	readonly float	splitFactor;
		//	readonly float	projDepth;

		//	public DefaultCSMController ( Matrix viewMatrix, int cascadeSize, float splitOffset, float splitFactor, float projDepth )
		//	{
		//		this.viewMatrix		=	viewMatrix	;
		//		this.cascadeSize	=	cascadeSize	;
		//		this.splitOffset	=	splitOffset	;
		//		this.splitFactor	=	splitFactor	;
		//		this.projDepth		=	projDepth;
		//		this.camMatrix		=	Matrix.Invert( viewMatrix );
		//	}



		//	public Matrix GetShadowViewMatrix ( DirectLight directLight, int cascadeIndex )
		//	{
		//		var	smSize			=	cascadeSize;
		//		var viewPos			=	camMatrix.TranslationVector;

		//		float	offset		=	splitOffset * (float)Math.Pow( splitFactor, cascadeIndex );
		//		float	radius		=	splitSize   * (float)Math.Pow( splitFactor, cascadeIndex );

		//		Vector3 viewDir		=	camMatrix.Forward.Normalized();
		//		Vector3	lightDir	=	directLight.Direction.Normalized();
		//		Vector3	origin		=	viewPos + viewDir * offset;

		//		Matrix	lightRot	=	Matrix.LookAtRH( Vector3.Zero, Vector3.Zero + lightDir, Vector3.UnitY );
		//		Matrix	lightRotI	=	Matrix.Invert( lightRot );
		//		Vector3	lsOrigin	=	Vector3.TransformCoordinate( origin, lightRot );
		//		float	snapValue	=	4.0f * radius / smSize;
		//		lsOrigin.X			=	(float)Math.Round(lsOrigin.X / snapValue) * snapValue;
		//		lsOrigin.Y			=	(float)Math.Round(lsOrigin.Y / snapValue) * snapValue;
		//		lsOrigin.Z			=	(float)Math.Round(lsOrigin.Z / snapValue) * snapValue;
		//		origin				=	Vector3.TransformCoordinate( lsOrigin, lightRotI );//*/

		//		shadowViews[i]				=	Matrix.LookAtRH( origin, origin + lightDir, Vector3.UnitY );
		//		shadowProjections[i]		=	Matrix.OrthoRH( radius*2, radius*2, -projDepth/2, projDepth/2);

		//		shadowViewProjections[i]	=	shadowViews[i] * shadowProjections[i];
		//	}



		//	public Matrix GetShadowProjectionMatrix ( int cascadeIndex )
		//	{
		//	}

		//}

	}
}
