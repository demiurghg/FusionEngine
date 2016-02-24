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
		StateFactory	factoryXray;
		ConstantBuffer	modelBuf;

		[StructLayout(LayoutKind.Explicit)]
		struct ConstDataStruct
		{
			[FieldOffset(0)]	public Matrix	ModelWorld;
			[FieldOffset(64)]	public Vector4	ViewPositionTransparency;
			[FieldOffset(80)]	public Color4	OverallColor;
		}

		ConstDataStruct constData;

		[Flags]
		public enum ModelFlags : int
		{
			VERTEX_SHADER		= 1 << 0,
			PIXEL_SHADER		= 1 << 1,
			GEOMETRY_SHADER		= 1 << 2,
			DRAW_TEXTURED		= 1 << 3,
			DRAW_COLORED		= 1 << 4,
			COMPUTE_NORMALS		= 1 << 5,
			XRAY				= 1 << 6,
			INSTANCED			= 1 << 7,
			USE_OVERALL_COLOR	= 1 << 8,
		}


		public class SelectedItem : Gis.SelectedItem
		{
			public int			NodeInd;
			public int			InstanceIndex;
			public BoundingBox	BoundingBox;
			public DMatrix		BoundingBoxTransform;
		}


		public string Name { get; protected set; }

		public DVector3 CartesianPos { get; protected set; }
		public DVector2 LonLatPosition;

		private Matrix			modelRotationMatrix;

		public struct InstancedDataStruct
		{
			public Matrix World;
		}

		public InstancedDataStruct[] InstancedDataCPU { get; private set; }

		StructuredBuffer instDataGpu;

		public int InstancedCountToDraw;


		public float ScaleFactor = 1.0f;
		public float Yaw, Pitch, Roll;

		protected Scene		model;
		protected Matrix[]	transforms;

		public float	Transparency;
		public bool		XRay			= false;
		public bool		UseOverallColor = false;

		public Color4 OverallColor { get { return constData.OverallColor; } set { constData.OverallColor = value; }}


		public class SelectInfo
		{
			public BoundingBox	BoundingBox;
			public string		NodeName;
			public int			NodeIndex;
			public int			MeshIndex;
			public int			InstanceIndex;
		}

		private SelectInfo[] selectInfo;


		public ModelLayer(Game engine, DVector2 lonLatPosition, string fileName, int maxInstancedCount = 0) : base(engine)
		{
			model = engine.Content.Load<Scene>(fileName);

			transforms = new Matrix[model.Nodes.Count];
			model.ComputeAbsoluteTransforms(transforms);

			LonLatPosition	= lonLatPosition;

			Transparency = 1.0f;

			constData	= new ConstDataStruct();
			modelBuf	= new ConstantBuffer(Game.GraphicsDevice, typeof(ConstDataStruct));
			shader		= Game.Content.Load<Ubershader>("globe.Model.hlsl");
			
			factory		= shader.CreateFactory( typeof(ModelFlags), Primitive.TriangleList, VertexInputElement.FromStructure<VertexColorTextureTBNRigid>(), BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.Default);
			factoryXray = shader.CreateFactory( typeof(ModelFlags), Primitive.TriangleList, VertexInputElement.FromStructure<VertexColorTextureTBNRigid>(), BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.Default);
			
			if (maxInstancedCount > 0) {
				InstancedCountToDraw	= maxInstancedCount;
				InstancedDataCPU		= new InstancedDataStruct[maxInstancedCount];

				instDataGpu = new StructuredBuffer(engine.GraphicsDevice, typeof(InstancedDataStruct), maxInstancedCount, StructuredBufferFlags.None);
			}

			selectInfo = new SelectInfo[model.Nodes.Count];
			for (int i = 0; i < model.Nodes.Count; i++)
			{
				var meshindex = model.Nodes[i].MeshIndex;
				if (meshindex < 0) continue;

				selectInfo[i] = new SelectInfo
				{
					BoundingBox = model.Meshes[meshindex].ComputeBoundingBox(),
					MeshIndex = meshindex,
					NodeIndex = i,
					NodeName = model.Nodes[i].Name
				};
			}

		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{ 
			var dev = Game.GraphicsDevice;
			var gis = Game.RenderSystem.Gis;

			CartesianPos = GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(LonLatPosition), gis.Camera.EarthRadius);

			var viewPosition	= CartesianPos - gis.Camera.FinalCamPosition;

			var normal = DVector3.Normalize(CartesianPos);

			var xAxis = DVector3.Transform(DVector3.UnitX, DQuaternion.RotationAxis(DVector3.UnitY, DMathUtil.DegreesToRadians(LonLatPosition.X)));
			xAxis.Normalize();

			var zAxis =  DVector3.Cross(xAxis, normal);
			zAxis.Normalize();

			Matrix rotationMat	= Matrix.Identity;
			rotationMat.Forward = xAxis.ToVector3();
			rotationMat.Up		= normal.ToVector3();
			rotationMat.Right	= zAxis.ToVector3();
			rotationMat.TranslationVector = Vector3.Zero;

			var viewPositionFloat = new Vector3((float)viewPosition.X, (float)viewPosition.Y, (float)viewPosition.Z);


			modelRotationMatrix = Matrix.RotationYawPitchRoll(Yaw, Pitch, Roll) * Matrix.Scaling(ScaleFactor) * rotationMat;
			constData.ViewPositionTransparency = new Vector4(viewPositionFloat, Transparency);


			dev.VertexShaderConstants[0]	= constBuffer;
			dev.VertexShaderConstants[1]	= modelBuf;
			dev.PixelShaderConstants[1]		= modelBuf;


			int flags = (int) (ModelFlags.VERTEX_SHADER | ModelFlags.PIXEL_SHADER);
			if (XRay) {
				flags |= (int) ModelFlags.XRAY;
				if(UseOverallColor) flags |= (int) ModelFlags.USE_OVERALL_COLOR;
			} else {
				if (UseOverallColor) flags |= (int) ModelFlags.USE_OVERALL_COLOR;
				else flags |= (int) ModelFlags.DRAW_COLORED;
			}


			if (InstancedDataCPU != null) {
				flags |= (int)(ModelFlags.INSTANCED);

				instDataGpu.SetData(InstancedDataCPU);
				dev.VertexShaderResources[1] = instDataGpu;

				InstancedCountToDraw = InstancedCountToDraw > InstancedDataCPU.Length
					? InstancedDataCPU.Length
					: InstancedCountToDraw;
			}

			if (XRay) {
				dev.PipelineState = factoryXray[flags];
			} else {
				dev.PipelineState = factory[flags];
			}
			

			for (int i = 0; i < model.Nodes.Count; i++) {
				var meshIndex = model.Nodes[i].MeshIndex;

				if (meshIndex < 0) {
					continue;
				}

				var mesh = model.Meshes[meshIndex];

				var translation = transforms[i] * Matrix.Scaling(0.001f);

				constData.ModelWorld = translation * modelRotationMatrix;
				modelBuf.SetData(constData);

				dev.SetupVertexInput(mesh.VertexBuffer, mesh.IndexBuffer);

				if (InstancedDataCPU != null) {
					dev.DrawInstancedIndexed(mesh.IndexBuffer.Capacity, InstancedCountToDraw, 0, 0, 0);
				}
				else {
					dev.DrawIndexed(mesh.IndexBuffer.Capacity, 0, 0);
				}
			}
		}


		public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		{
			if (selectInfo == null) return null;

			var selectedItems = new List<Gis.SelectedItem>();

			for (int i = 0; i < model.Nodes.Count; i++)
			{
				var meshIndex = model.Nodes[i].MeshIndex;

				if (meshIndex < 0) {
					continue;
				}

				var mesh = model.Meshes[meshIndex];
				var info = selectInfo[i];

				var translation			= transforms[i] * Matrix.Scaling(0.001f);
				var modelWorld			= translation * modelRotationMatrix * Matrix.Translation(CartesianPos.ToVector3());
				var modelWorldinvert	= Matrix.Invert(modelWorld);

				var localNear	= Vector3.TransformCoordinate(nearPoint.ToVector3(),	modelWorldinvert);
				var localFar	= Vector3.TransformCoordinate(farPoint.ToVector3(),		modelWorldinvert);

				var localRay = new Ray(localNear, Vector3.Normalize(localFar - localNear));

				float distance;
				if (info.BoundingBox.Intersects(ref localRay, out distance)) {
					selectedItems.Add(new SelectedItem {
						BoundingBox				= info.BoundingBox,
						BoundingBoxTransform	= DMatrix.FromFloatMatrix(modelWorld),
						Distance				= distance,
						InstanceIndex			= 0,
						Name					= info.NodeName,
						NodeInd					= info.NodeIndex
					});
				}
				
			}

			return selectedItems;
		}
	}
}
