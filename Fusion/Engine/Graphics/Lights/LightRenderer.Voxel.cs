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
	public partial class LightRenderer : GameModule {

		
		static Random rand = new Random(458);
		Vector4[] data = Enumerable.Range(0,VoxelCount).Select( i => new Vector4(rand.NextFloat(0,1),rand.NextFloat(0,1),rand.NextFloat(0,1),rand.GaussDistribution(0,0.5f)) ).ToArray();




		/// <summary>
		/// 
		/// </summary>
		/// <param name="viewLayer"></param>
		/// <param name="lightSet"></param>
		internal void RenderLightVoxelGrid ( RenderWorld viewLayer, LightSet lightSet )
		{
			using (new PixEvent("RenderLightVoxelGrid")) {

				var device = Game.GraphicsDevice;
				device.ResetStates();

				using (new PixEvent("CopyBufferToVoxel")) {
					
					lightVoxelBuffer.SetData( data );

					device.PipelineState				=	voxelFactory[ (int)VoxelFlags.COPY_BUFFER_TO_VOXEL ];
					device.ComputeShaderResources[0]	=	lightVoxelBuffer;
					device.SetCSRWTexture( 0, lightVoxelGrid );

					device.Dispatch( 8, 8, 8 );
				}



				var instances	=	viewLayer.Instances;

				//Game.RenderSystem.SceneRenderer.VoxelizeScene( instances, lightVoxelGrid, lightSet );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="viewLayer"></param>
		/// <param name="lightSet"></param>
		void DebugDrawVoxelGrid ( RenderWorld viewLayer, LightSet lightSet, HdrFrame hdrFrame )
		{
			using ( new PixEvent("DebugDrawVoxelGrid") ) {

				var device = Game.GraphicsDevice;
				device.ResetStates();
	
				//	target and viewport :
				device.SetTargets( hdrFrame.DepthBuffer, hdrFrame.HdrBuffer );
				device.SetViewport( 0,0, hdrFrame.DepthBuffer.Width, hdrFrame.DepthBuffer.Height );

				//	get matricies  and params :
				var wvp = viewLayer.Camera.GetViewMatrix( StereoEye.Mono ) * viewLayer.Camera.GetProjectionMatrix( StereoEye.Mono );
				voxelCB.SetData( wvp );

				//	voxel params CB :			
				device.ComputeShaderConstants[0]	= voxelCB ;
				device.VertexShaderConstants[0]		= voxelCB ;
				device.GeometryShaderConstants[0]	= voxelCB ;

				//	sampler & textures :
				device.PixelShaderSamplers[0]		= SamplerState.LinearClamp4Mips ;
				device.GeometryShaderSamplers[0]	= SamplerState.LinearClamp4Mips ;
				device.PixelShaderSamplers[0]		= SamplerState.LinearClamp4Mips ;

				device.VertexShaderResources[0]		=	lightVoxelGrid ;
				device.GeometryShaderResources[0]	=	lightVoxelGrid ;
				device.PixelShaderResources[0]		=	lightVoxelGrid;

				//	setup PS :
				device.PipelineState	=	voxelFactory[ (int)VoxelFlags.DEBUG_DRAW_VOXEL ];

				//	GPU time : 0.81 ms	-> 0.91 ms
				device.Draw( VoxelCount, 0 );
			}
		}

	}
}
