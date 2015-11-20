using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public class ModelLayer : Gis.GisLayer
	{
		Ubershader		shader;
		StateFactory	factory;
		ConstantBuffer	modelBuf;

		[StructLayout(LayoutKind.Explicit)]
		struct ConstDataStruct
		{
			[FieldOffset(0)]	public Matrix	ModelWorld;
			[FieldOffset(64)]	public Vector4	ViewPositionTransparency;
		}

		ConstDataStruct constData;

		[Flags]
		public enum ModelFlags : int
		{
			VERTEX_SHADER	= 1 << 0,
			PIXEL_SHADER	= 1 << 1,
			GEOMETRY_SHADER = 1 << 2,
			DRAW_TEXTURED	= 1 << 3,
			DRAW_COLORED	= 1 << 4,
			COMPUTE_NORMALS = 1 << 5,
		}


		public string Name { get; protected set; }

		public DVector3 CartesianPos { get; protected set; }
		public DVector2 LonLatPosition;

		public float ScaleFactor = 1.0f;
		public float Yaw, Pitch, Roll;

		Scene model;
		Matrix[] transforms;

		public float			Transparency;

		public ModelLayer(GameEngine engine, DVector2 lonLatPosition, string fileName) : base(engine)
		{
			model = engine.Content.Load<Scene>(fileName);

			transforms = new Matrix[model.Nodes.Count];
			model.ComputeAbsoluteTransforms(transforms);

			LonLatPosition	= lonLatPosition;

			Transparency = 1.0f;

			constData	= new ConstDataStruct();
			modelBuf	= new ConstantBuffer(GameEngine.GraphicsDevice, typeof(ConstDataStruct));
			shader		= GameEngine.Content.Load<Ubershader>("globe.Model.hlsl");
			factory		= new StateFactory(shader, typeof(ModelFlags), Primitive.TriangleList, VertexInputElement.FromStructure<VertexColorTextureTBNRigid>(), BlendState.Opaque, RasterizerState.CullNone, DepthStencilState.Default);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			var dev = GameEngine.GraphicsDevice;
			var gis = GameEngine.GraphicsEngine.Gis;

			CartesianPos = GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(LonLatPosition), gis.Camera.EarthRadius);

			var viewPosition		= CartesianPos - gis.Camera.FreeCamPosition;

			var normal = DVector3.Normalize(CartesianPos);

			var xAxis = DVector3.Transform(DVector3.UnitX, DQuaternion.RotationAxis(DVector3.UnitY, DMathUtil.DegreesToRadians(LonLatPosition.X)));
			xAxis.Normalize();

			var zAxis =  DVector3.Cross(xAxis, normal);
			zAxis.Normalize();

			Matrix rotationMat	= Matrix.Identity;
			rotationMat.Forward = xAxis.ToVector3();
			rotationMat.Up		= normal.ToVector3();
			rotationMat.Right	= -zAxis.ToVector3();
			rotationMat.TranslationVector = Vector3.Zero;

			var viewPositionFloat = new Vector3((float)viewPosition.X, (float)viewPosition.Y, (float)viewPosition.Z);


			constData.ModelWorld				= Matrix.RotationYawPitchRoll(Yaw, Pitch, Roll) * Matrix.Scaling(ScaleFactor) * rotationMat;
			constData.ViewPositionTransparency	= new Vector4(viewPositionFloat, Transparency);
			modelBuf.SetData(constData);

			dev.VertexShaderConstants[0]	= constBuffer;
			dev.VertexShaderConstants[1]	= modelBuf;
			dev.PixelShaderConstants[1]		= modelBuf;

			dev.PipelineState = factory[(int) (ModelFlags.VERTEX_SHADER | ModelFlags.PIXEL_SHADER | ModelFlags.DRAW_COLORED)];
			
			for (int i = 0; i < model.Nodes.Count; i++) {
				var meshIndex = model.Nodes[i].MeshIndex;

				if (meshIndex < 0) {
					continue;
				}

				var mesh = model.Meshes[meshIndex];

				dev.SetupVertexInput(mesh.VertexBuffer, mesh.IndexBuffer);
				dev.DrawIndexed(mesh.IndexBuffer.Capacity, 0, 0);
			}
		}
	}
}
