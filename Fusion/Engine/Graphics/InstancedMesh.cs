using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {
	
	/// <summary>
	/// Represnets mesh instance
	/// </summary>
	public class MeshInstance {

		/// <summary>
		/// Indicates whether mesh instance visible.
		/// Default value is True.
		/// </summary>
		public bool Visible {
			get; set;
		}

		/// <summary>
		/// Instance world matrix. Default value is Matrix.Identity.
		/// </summary>
		public Matrix World {
			get; set;
		}

		/// <summary>
		/// Instance color. Default value 0,0,0,0
		/// </summary>
		public Color4 Color {
			get; set;
		}

		/// <summary>
		/// Instance material layer blending. Default value 1,1,1,1
		/// </summary>
		public Vector4 Blending {
			get; set;
		}

		/// <summary>
		/// Gets and sets mesh.
		/// </summary>
		public Mesh Mesh {
			get; private set;
		}

		/// <summary>
		/// Gets and sets surface effect.
		/// </summary>
		public InstanceFX InstanceFX {
			get; set;
		}

		/// <summary>
		/// Gets whether mesh is skinned.
		/// </summary>
		public bool IsSkinned {
			get; private set;
		}

		/// <summary>
		/// Gets array of bone transforms.
		/// If skinning is not applied to mesh, this array is Null.
		/// </summary>
		public Matrix[] BoneTransforms {
			get; private set;
		}

		/// <summary>
		/// Gets array of bone colors.
		/// If skinning is not applied to mesh, this array is Null.
		/// </summary>
		public Color4[] BoneColors {
			get; private set;
		}

		/// <summary>
		/// Gets array of bone material blend options..
		/// If skinning is not applied to mesh, this array is Null.
		/// </summary>
		public Color4[] BoneBlending {
			get; private set;
		}


		readonly internal VertexBuffer	vb;
		readonly internal IndexBuffer	ib;

		readonly internal int indexCount;
		readonly internal int vertexCount;

		//struct ShadingGroup {
		//	public int StartIndex;
		//	public int IndicesCount;
		//	public Material Material;
		//}



		/// <summary>
		/// Creates instance from mesh in scene.
		/// </summary>
		/// <param name="ge"></param>
		/// <param name="mesh"></param>
		public MeshInstance ( GraphicsEngine ge, Scene scene, Mesh mesh )
		{
			Visible		=	true;
			World		=	Matrix.Identity;
			Color		=	Color4.Zero;
			Blending	=	new Vector4(1,1,1,1);

			vb			=	mesh.VertexBuffer;
			ib			=	mesh.IndexBuffer;
			
			vertexCount	=	mesh.VertexCount;
			indexCount	=	mesh.IndexCount;


			/*foreach ( var subset in mesh.Subsets ) {
				
				//var material	=	ge.GameEngine.C

			} */

			//	TODO : Get materials here
			//	TODO : Get textures here
			//	Keep in mind hot reloading!
		}





		/// <summary>
		/// 
		/// </summary>
		/// <param name="instanceIndex"></param>
		/// <param name="world"></param>
		/// <param name="color"></param>
		/// <param name="blending"></param>
		public void SetInstance ( int instanceIndex, Matrix world, Color4 color, Vector4 blending )
		{
			throw new NotImplementedException();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="instanceIndex"></param>
		/// <param name="bonesTransforms"></param>
		/// <param name="boneColors"></param>
		/// <param name="boneBlending"></param>
		public void SetInstanceBones ( int instanceIndex, Matrix[] bonesTransforms, Color4[] boneColors, Vector4[] boneBlending )
		{
			throw new NotImplementedException();
		}
		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="materialIndex"></param>
		/// <param name="material"></param>
		public void ReplaceMaterial ( int materialIndex, Material material )
		{
			throw new NotImplementedException();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="materialIndex"></param>
		/// <param name="material"></param>
		public void RestoreMaterial ( int materialIndex, Material material )
		{
			throw new NotImplementedException();
		}



		
	}
}
