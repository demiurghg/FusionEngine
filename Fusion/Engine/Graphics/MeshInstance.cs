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
		/// Tag
		/// </summary>
		public object Tag {
			get; set;
		}


		readonly internal VertexBuffer	vb;
		readonly internal IndexBuffer	ib;

		readonly internal int indexCount;
		readonly internal int vertexCount;

		internal struct ShadingGroup {

			public ShadingGroup( MeshSubset subset, MaterialInstance material )
			{
				StartIndex	=	subset.StartPrimitive * 3;
				IndicesCount=	subset.PrimitiveCount * 3;
				Material	=	material;
			}
			
			public int StartIndex;
			public int IndicesCount;
			public MaterialInstance Material;
		}


		internal readonly ShadingGroup[] ShadingGroups;


		/// <summary>
		/// Creates instance from mesh in scene.
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="mesh"></param>
		public MeshInstance ( RenderSystem rs, Scene scene, Mesh mesh, MaterialInstance[] materials )
		{
			Visible		=	true;
			World		=	Matrix.Identity;
			Color		=	Color4.Zero;
			Blending	=	new Vector4(1,1,1,1);

			vb			=	mesh.VertexBuffer;
			ib			=	mesh.IndexBuffer;
			
			vertexCount	=	mesh.VertexCount;
			indexCount	=	mesh.IndexCount;

			ShadingGroups	=	mesh.Subsets	
							.Select( s => new ShadingGroup( s, materials[s.MaterialIndex] ) )
							.ToArray();

			IsSkinned	=	mesh.IsSkinned;

			if (IsSkinned && scene.Nodes.Count > SceneRenderer.MaxBones) {
				throw new ArgumentOutOfRangeException( string.Format("Scene contains more than {0} bones and cannot be used for skinning.", SceneRenderer.MaxBones ) );
			}

			if (IsSkinned) {
				BoneTransforms	=	Enumerable
					.Range(0, SceneRenderer.MaxBones)
					.Select( i => Matrix.Identity )
					.ToArray();
			}
		}
	}
}
