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
		Vector4[] data = Enumerable.Range(0,64*64*64).Select( i => new Vector4(rand.NextFloat(0,8),rand.NextFloat(0,1),1,rand.NextFloat(0,1)) ).ToArray();

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

	}
}
