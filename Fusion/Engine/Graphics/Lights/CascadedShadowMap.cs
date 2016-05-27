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

	class CascadedShadowMap : DisposableBase {


		readonly GraphicsDevice device;

		public const int MaxCascadeSize		= 8192;
		public const int MaxCascadeCount	= 4;


		/// <summary>
		/// Gets cascaded shadow map split count.
		/// </summary>
		public int CascadeCount {
			get {
				return cascadeCount;
			}
		}



		/// <summary>
		/// Gets cascaded shadow map split size.
		/// </summary>
		public int CascadeSize {
			get {
				return cascadeSize;
			}
		}



		/// <summary>
		/// Gets color shadow map buffer.
		/// Actually stores depth value.
		/// </summary>
		public RenderTarget2D ColorBuffer {
			get {
				return csmColor;
			}
		}



		/// <summary>
		/// Gets color shadow map buffer.
		/// Actually stores depth value.
		/// </summary>
		public RenderTarget2D ParticleShadow {
			get {
				return prtShadow;
			}
		}



		/// <summary>
		/// Gets color shadow map buffer.
		/// </summary>
		public DepthStencil2D DepthBuffer {
			get {
				return csmDepth;
			}
		}


		readonly int	cascadeSize;
		readonly int	cascadeCount;
		DepthStencil2D	csmDepth;
		RenderTarget2D	csmColor;
		RenderTarget2D	prtShadow;



		/// <summary>
		/// 
		/// </summary>
		/// <param name="singleShadowMapSize"></param>
		/// <param name="splitCount"></param>
		public CascadedShadowMap ( GraphicsDevice device, int cascadeSize, int cascadeCount )
		{
			this.device			=	device;
			this.cascadeCount	=	cascadeCount;
			this.cascadeSize	=	cascadeSize;

			if (cascadeCount<1 || cascadeCount>MaxCascadeCount) {
				throw new ArgumentOutOfRangeException("cascadeCount must be within range 1.." + MaxCascadeCount.ToString());
			}

			if (cascadeSize<64 || cascadeSize > MaxCascadeSize) {
				throw new ArgumentOutOfRangeException("cascadeSize must be within range 64.." + MaxCascadeSize.ToString());
			}

			if (!MathUtil.IsPowerOfTwo( cascadeSize )) {
				Log.Warning("CascadedShadowMap : splitSize is not power of 2");
			}

			csmColor	=	new RenderTarget2D( device, ColorFormat.R32F,		cascadeSize * cascadeCount, cascadeSize );
			csmDepth	=	new DepthStencil2D( device, DepthFormat.D24S8,		cascadeSize * cascadeCount, cascadeSize );
			prtShadow	=	new RenderTarget2D( device, ColorFormat.Rgba8_sRGB,	cascadeSize * cascadeCount, cascadeSize );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref csmColor );
				SafeDispose( ref csmDepth );
				SafeDispose( ref prtShadow );
			}

			base.Dispose( disposing );
		}


		
		/// <summary>
		/// 
		/// </summary>
		public void Clear ()
		{
			device.Clear( csmDepth.Surface, 1, 0 );
			device.Clear( csmColor.Surface, Color4.White );
			device.Clear( prtShadow.Surface, Color4.White );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="splitIndex"></param>
		/// <returns></returns>
		public Viewport GetCascadeViewport( int cascadeIndex )
		{
			if ( cascadeIndex < 0 || cascadeIndex >= cascadeCount ) {
				throw new ArgumentOutOfRangeException("cascadeIndex must be within 0.." + (cascadeCount-1).ToString() );
			}

			return new Viewport( cascadeSize * cascadeIndex, 0, cascadeSize, cascadeSize );
		}



		/*public Matrix GetCascadeViewMatrix ( Matrix view, Vector3 lightDir )
		{
			
		} */

	}
}
