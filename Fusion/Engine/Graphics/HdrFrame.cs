using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics {
	
	/// <summary>
	/// Represents 
	/// </summary>
	internal class HdrFrame : DisposableBase {

		public const int FeedbackBufferWidth	=	80;
		public const int FeedbackBufferHeight	=	60;

		public FeedbackBuffer	FeedbackBufferColor	;
		public DepthStencil2D	FeedbackBufferDepth	;

		public RenderTarget2D	HdrBuffer			;	
		public RenderTarget2D	LightAccumulator	;
		public RenderTarget2D	SSSAccumulator		;
		public DepthStencil2D	DepthBuffer			;	
		public RenderTarget2D	DiffuseBuffer		;
		public RenderTarget2D	SpecularBuffer		;
		public RenderTarget2D	NormalMapBuffer		;
		public RenderTarget2D	ScatteringBuffer	;
		public RenderTarget2D	SSAOBuffer;


		public HdrFrame ( Game game, int width, int height )
		{
			int fbbw = FeedbackBufferWidth;
			int fbbh = FeedbackBufferHeight;

			FeedbackBufferColor	=	new FeedbackBuffer( game.GraphicsDevice,							fbbw,	fbbh	  );
			FeedbackBufferDepth	=	new DepthStencil2D( game.GraphicsDevice, DepthFormat.D24S8,			fbbw,	fbbh,	1 );
			HdrBuffer			=	new RenderTarget2D( game.GraphicsDevice, ColorFormat.Rgba16F,		width,	height,	false, false );
			LightAccumulator	=	new RenderTarget2D( game.GraphicsDevice, ColorFormat.Rgba16F,		width,	height,	false, true );
			SSSAccumulator		=	new RenderTarget2D( game.GraphicsDevice, ColorFormat.Rgba16F,		width,	height,	false, true );
			DepthBuffer			=	new DepthStencil2D( game.GraphicsDevice, DepthFormat.D24S8,			width,	height,	1 );
			DiffuseBuffer		=	new RenderTarget2D( game.GraphicsDevice, ColorFormat.Rgba8_sRGB,	width,	height,	false, false );
			SpecularBuffer 		=	new RenderTarget2D( game.GraphicsDevice, ColorFormat.Rgba8_sRGB,	width,	height,	false, false );
			NormalMapBuffer		=	new RenderTarget2D( game.GraphicsDevice, ColorFormat.Rgb10A2,		width,	height,	false, false );
			ScatteringBuffer	=	new RenderTarget2D( game.GraphicsDevice, ColorFormat.Rgba8_sRGB,	width,	height,	false, false );
			SSAOBuffer			=	new RenderTarget2D( game.GraphicsDevice, ColorFormat.Rgba8,			width,	height, false, false );

			Clear();
		}


		public void Clear ()
		{
			var device = HdrBuffer.GraphicsDevice;
			device.Clear( SSAOBuffer.Surface, Color4.White );
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				SafeDispose( ref FeedbackBufferColor	);
				SafeDispose( ref FeedbackBufferDepth	);
				SafeDispose( ref HdrBuffer				);
				SafeDispose( ref LightAccumulator		);
				SafeDispose( ref SSSAccumulator			);
				SafeDispose( ref DepthBuffer			);
				SafeDispose( ref DiffuseBuffer			);
				SafeDispose( ref SpecularBuffer 		);
				SafeDispose( ref NormalMapBuffer		);
				SafeDispose( ref ScatteringBuffer		);
				SafeDispose( ref SSAOBuffer				);
			} 

			base.Dispose(disposing);
		}
	}
}
