using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using System.Runtime.InteropServices;


namespace Fusion.Engine.Graphics {

	public class DebugRender : DisposableBase {

		readonly Game Game;

		struct LineVertex {
			[Vertex("POSITION")] public Vector4 Pos;
			[Vertex("COLOR", 0)] public Vector4 Color;
		}

		[Flags]
		public enum RenderFlags : int {
			SOLID = 0x0001,
			GHOST = 0x0002,
		}

		[StructLayout(LayoutKind.Explicit)]
		struct ConstData {
			[FieldOffset(  0)] public Matrix  View;
			[FieldOffset( 64)] public Matrix  ViewProjection;
			[FieldOffset(128)] public Vector4 ViewPosition;
			[FieldOffset(144)] public Vector4 PixelSize;
		}

		VertexBuffer		vertexBuffer;
		Ubershader			effect;
		StateFactory		factory;
		ConstantBuffer		constBuffer;

		List<LineVertex>	vertexDataAccum	= new List<LineVertex>();
		LineVertex[]		vertexArray = new LineVertex[vertexBufferSize];

		const int vertexBufferSize = 4096;

		ConstData	constData;


		/// <summary>
		/// Constructor
		/// </summary>
		public DebugRender(Game game) 
		{
			this.Game	=	game;
			var dev		=	Game.GraphicsDevice;

			LoadContent();
			
			constData	=	new ConstData();
			constBuffer =	new ConstantBuffer(dev, typeof(ConstData));

			//	create vertex buffer :
			vertexBuffer		= new VertexBuffer(dev, typeof(LineVertex), vertexBufferSize, VertexBufferOptions.Dynamic );
			vertexDataAccum.Capacity = vertexBufferSize;

			Game.Reloading += (s,e) => LoadContent();
		}



		public void LoadContent ()
		{
			effect		=	Game.Content.Load<Ubershader>("debugRender.hlsl");
			factory		=	effect.CreateFactory( typeof(RenderFlags), (ps,i) => Enum(ps, (RenderFlags)i ) );

			//factory		=	effect.CreateFactory( typeof(RenderFlags), Primitive.LineList, VertexInputElement.FromStructure( typeof(LineVertex) ), BlendState.AlphaBlend, RasterizerState.CullNone, DepthStencilState.Default );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flags"></param>
		void Enum ( PipelineState ps, RenderFlags flags )
		{
			ps.Primitive			=	Primitive.LineList;
			ps.VertexInputElements	=	VertexInputElement.FromStructure( typeof(LineVertex) );
			ps.RasterizerState		=	RasterizerState.CullNone;

			if (flags.HasFlag( RenderFlags.SOLID )) {
				ps.BlendState			=	BlendState.Opaque;
				ps.DepthStencilState	=	DepthStencilState.Default;
			}
			
			if (flags.HasFlag( RenderFlags.GHOST )) {
				ps.BlendState			=	BlendState.AlphaBlend;
				ps.DepthStencilState	=	DepthStencilState.None;
			}
		}




		/// <summary>
		/// Dispose
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				vertexBuffer.Dispose();
				constBuffer.Dispose();
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Draws line between p0 and p1
		/// </summary>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="color"></param>
		public void DrawLine(Vector3 p0, Vector3 p1, Color color)
		{
			vertexDataAccum.Add(new LineVertex() { Pos = new Vector4(p0,0), Color = color.ToVector4() });
			vertexDataAccum.Add(new LineVertex() { Pos = new Vector4(p1,0), Color = color.ToVector4() });
			//DrawLine( p0, p1, color, Matrix.Identity );
		}



		/// <summary>
		/// Draws line between p0 and p1
		/// </summary>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="color"></param>
		public void DrawLine(Vector2 p0, Vector2 p1, Color color)
		{
			vertexDataAccum.Add(new LineVertex() { Pos = new Vector4(p0, 0,0), Color = color.ToVector4() });
			vertexDataAccum.Add(new LineVertex() { Pos = new Vector4(p1, 0,0), Color = color.ToVector4() });
			//DrawLine( p0, p1, color, Matrix.Identity );
		}



		/// <summary>
		/// Draws line between p0 and p1
		/// </summary>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="color"></param>
		public void DrawLine(Vector3 p0, Vector3 p1, Color color0, Color color1)
		{
			vertexDataAccum.Add(new LineVertex() { Pos = new Vector4(p0,0), Color = color0.ToVector4() });
			vertexDataAccum.Add(new LineVertex() { Pos = new Vector4(p1,0), Color = color1.ToVector4() });
			//DrawLine( p0, p1, color, Matrix.Identity );
		}


		public void DrawLine(Vector3 p0, Vector3 p1, Color color0, Color color1, float width0, float width1)
		{
			vertexDataAccum.Add(new LineVertex() { Pos = new Vector4(p0,width0), Color = color0.ToVector4() });
			vertexDataAccum.Add(new LineVertex() { Pos = new Vector4(p1,width1), Color = color1.ToVector4() });
			//DrawLine( p0, p1, color, Matrix.Identity );
		}



		/// <summary>
		/// 
		/// </summary>
		internal void Render ( RenderTargetSurface colorBuffer, DepthStencilSurface depthBuffer, Camera camera )
		{
			DrawTracers();

			if (!vertexDataAccum.Any()) {
				return;
			}

			if (Game.RenderSystem.SkipDebugRendering) {
				vertexDataAccum.Clear();	
				return;
			}

			var dev = Game.GraphicsDevice;
			dev.ResetStates();

			dev.SetTargets( depthBuffer, colorBuffer );


			var a = camera.GetProjectionMatrix(StereoEye.Mono).M11;
			var b = camera.GetProjectionMatrix(StereoEye.Mono).M22;
			var w = (float)colorBuffer.Width;
			var h = (float)colorBuffer.Height;

			constData.View				=	camera.GetViewMatrix(StereoEye.Mono);
			constData.ViewProjection	=	camera.GetViewMatrix(StereoEye.Mono) * camera.GetProjectionMatrix(StereoEye.Mono);
			constData.ViewPosition		=	camera.GetCameraPosition4(StereoEye.Mono);
			constData.PixelSize			=	new Vector4( 1/w/a, 1/b/h, 0, 0 );
			constBuffer.SetData(constData);

			dev.SetupVertexInput( vertexBuffer, null );
			dev.VertexShaderConstants[0]	=	constBuffer ;
			dev.PixelShaderConstants[0]		=	constBuffer ;
			dev.GeometryShaderConstants[0]	=	constBuffer ;

			var flags = new[]{ RenderFlags.SOLID, RenderFlags.GHOST };

	
			foreach ( var flag in flags ) {
				dev.PipelineState =	factory[(int)flag];

				int numDPs = MathUtil.IntDivUp(vertexDataAccum.Count, vertexBufferSize);

				for (int i = 0; i < numDPs; i++) {

					int numVerts = i < numDPs - 1 ? vertexBufferSize : vertexDataAccum.Count % vertexBufferSize;

					if (numVerts == 0) {
						break;
					}

					vertexDataAccum.CopyTo(i * vertexBufferSize, vertexArray, 0, numVerts);

					vertexBuffer.SetData(vertexArray, 0, numVerts);

					dev.Draw( numVerts, 0);

				}
			}

			vertexDataAccum.Clear();
		}




		/*-----------------------------------------------------------------------------------------
		 *	Tracers :
		-----------------------------------------------------------------------------------------*/

		class TraceRecord {
			public Vector3 Position;
			public Color Color;
			public float Size;
			public int LifeTime;
		}


		List<TraceRecord> tracers = new List<TraceRecord>();


		public void Trace ( Vector3 position, float size, Color color, int lifeTimeInFrames = 300 )
		{
			tracers.Add( new TraceRecord() {
					Position	=	position,
					Size		=	size,
					Color		=	color,
					LifeTime	=	lifeTimeInFrames,
				});
		}


		void DrawTracers ()
		{
			foreach ( var t in tracers ) {
				t.LifeTime --;

				DrawPoint( t.Position, t.Size, t.Color );
			}

			tracers.RemoveAll( t => t.LifeTime < 0 );
		}

		/*-----------------------------------------------------------------------------------------
		 *	Primitives :
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wireCount"></param>
		public void DrawGrid(int wireCount)
		{
			int gridsz = wireCount;
			for (int x = -gridsz; x <= gridsz; x += 1)
			{
				float dim = 0.7f;
				if (x == 0) dim = 1.0f;
				DrawLine(new Vector3(x, 0, gridsz), new Vector3(x, 0, -gridsz), Color.DarkGray * dim);
				DrawLine(new Vector3(gridsz, 0, x), new Vector3(-gridsz, 0, x), Color.DarkGray * dim);
			}
		}


		public void DrawBasis(Matrix basis, float scale)
		{
			Vector3 pos = Vector3.TransformCoordinate(Vector3.Zero, basis);
			Vector3 xaxis = Vector3.TransformNormal(Vector3.UnitX * scale, basis);
			Vector3 yaxis = Vector3.TransformNormal(Vector3.UnitY * scale, basis);
			Vector3 zaxis = Vector3.TransformNormal(Vector3.UnitZ * scale, basis);
			DrawLine(pos, pos + xaxis, Color.Red);
			DrawLine(pos, pos + yaxis, Color.Lime);
			DrawLine(pos, pos + zaxis, Color.Blue);
		}


		public void DrawVector(Vector3 origin, Vector3 dir, Color color, float scale = 1.0f)
		{
			DrawLine(origin, origin + dir * scale, color/*, Matrix.Identity*/ );
		}


		public void DrawPoint(Vector3 p, float size, Color color)
		{
			float h = size / 2;	// half size
			DrawLine(p + Vector3.UnitX * h, p - Vector3.UnitX * h, color);
			DrawLine(p + Vector3.UnitY * h, p - Vector3.UnitY * h, color);
			DrawLine(p + Vector3.UnitZ * h, p - Vector3.UnitZ * h, color);
		}


		public void DrawWaypoint(Vector3 p, float size, Color color)
		{
			float h = size / 2;	// half size
			DrawLine(p + Vector3.UnitX * h, p - Vector3.UnitX * h, color);
			DrawLine(p + Vector3.UnitZ * h, p - Vector3.UnitZ * h, color);
		}


		public void DrawRing(Vector3 origin, float radius, Color color, int numSegments = 32, float angle = 0)
		{
			int N = numSegments;
			Vector3[] points = new Vector3[N + 1];

			for (int i = 0; i <= N; i++)
			{
				points[i].X = origin.X + radius * (float)Math.Cos(Math.PI * 2 * i / N + angle);
				points[i].Y = origin.Y;
				points[i].Z = origin.Z + radius * (float)Math.Sin(Math.PI * 2 * i / N + angle);
			}

			for (int i = 0; i < N; i++)
			{
				DrawLine(points[i], points[i + 1], color);
			}
		}


		public void DrawSphere(Vector3 origin, float radius, Color color, int numSegments = 32)
		{
			int N = numSegments;
			Vector3[] points = new Vector3[N + 1];

			for (int i = 0; i <= N; i++)
			{
				points[i].X = origin.X + radius * (float)Math.Cos(Math.PI * 2 * i / N);
				points[i].Y = origin.Y;
				points[i].Z = origin.Z + radius * (float)Math.Sin(Math.PI * 2 * i / N);
			}

			for (int i = 0; i < N; i++)
			{
				DrawLine(points[i], points[i + 1], color);
			}

			for (int i = 0; i <= N; i++)
			{
				points[i].X = origin.X + radius * (float)Math.Cos(Math.PI * 2 * i / N);
				points[i].Y = origin.Y + radius * (float)Math.Sin(Math.PI * 2 * i / N);
				points[i].Z = origin.Z;
			}

			for (int i = 0; i < N; i++)
			{
				DrawLine(points[i], points[i + 1], color);
			}

			for (int i = 0; i <= N; i++)
			{
				points[i].X = origin.X;
				points[i].Y = origin.Y + radius * (float)Math.Cos(Math.PI * 2 * i / N);
				points[i].Z = origin.Z + radius * (float)Math.Sin(Math.PI * 2 * i / N);
			}

			for (int i = 0; i < N; i++)
			{
				DrawLine(points[i], points[i + 1], color);
			}
		}


		public void DrawRing(Matrix basis, float radius, Color color, int numSegments = 32, float width=0, float stretch = 1)
		{
			int N = numSegments;
			Vector3[] points = new Vector3[N + 1];
			Vector3 origin = basis.TranslationVector;

			for (int i = 0; i <= N; i++)
			{
				points[i] = origin + radius * basis.Forward * (float)Math.Cos(Math.PI * 2 * i / N) * stretch
									+ radius * basis.Left * (float)Math.Sin(Math.PI * 2 * i / N);
			}

			for (int i = 0; i < N; i++)
			{
				DrawLine(points[i], points[i + 1], color, color, width, width);
			}
		}


		public void DrawFrustum ( BoundingFrustum frustum, Color color )
		{
			var points = frustum.GetCorners();

			DrawLine( points[0], points[1], color );
			DrawLine( points[1], points[2], color );
			DrawLine( points[2], points[3], color );
			DrawLine( points[3], points[0], color );

			DrawLine( points[4], points[5], color );
			DrawLine( points[5], points[6], color );
			DrawLine( points[6], points[7], color );
			DrawLine( points[7], points[4], color );

			DrawLine( points[0], points[4], color );
			DrawLine( points[1], points[5], color );
			DrawLine( points[2], points[6], color );
			DrawLine( points[3], points[7], color );
		}


		public void DrawBox(BoundingBox bbox, Color color)
		{
			var crnrs = bbox.GetCorners();

			var p = bbox.Maximum;
			var n = bbox.Minimum;

			DrawLine(new Vector3(p.X, p.Y, p.Z), new Vector3(n.X, p.Y, p.Z), color);
			DrawLine(new Vector3(n.X, p.Y, p.Z), new Vector3(n.X, p.Y, n.Z), color);
			DrawLine(new Vector3(n.X, p.Y, n.Z), new Vector3(p.X, p.Y, n.Z), color);
			DrawLine(new Vector3(p.X, p.Y, n.Z), new Vector3(p.X, p.Y, p.Z), color);

			DrawLine(new Vector3(p.X, n.Y, p.Z), new Vector3(n.X, n.Y, p.Z), color);
			DrawLine(new Vector3(n.X, n.Y, p.Z), new Vector3(n.X, n.Y, n.Z), color);
			DrawLine(new Vector3(n.X, n.Y, n.Z), new Vector3(p.X, n.Y, n.Z), color);
			DrawLine(new Vector3(p.X, n.Y, n.Z), new Vector3(p.X, n.Y, p.Z), color);

			DrawLine(new Vector3(p.X, p.Y, p.Z), new Vector3(p.X, n.Y, p.Z), color);
			DrawLine(new Vector3(n.X, p.Y, p.Z), new Vector3(n.X, n.Y, p.Z), color);
			DrawLine(new Vector3(n.X, p.Y, n.Z), new Vector3(n.X, n.Y, n.Z), color);
			DrawLine(new Vector3(p.X, p.Y, n.Z), new Vector3(p.X, n.Y, n.Z), color);
		}


		public void DrawBox(BoundingBox bbox, Matrix transform, Color color)
		{
			var crnrs = bbox.GetCorners();

			//Vector3.TransformCoordinate( crnrs, ref transform, crnrs );

			var p = bbox.Maximum;
			var n = bbox.Minimum;

			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, p.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, p.Y, p.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, p.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, p.Y, n.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, p.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, p.Y, n.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, p.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, p.Y, p.Z), transform), color);

			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, n.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, n.Y, p.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, n.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, n.Y, n.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, n.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, n.Y, n.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, n.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, n.Y, p.Z), transform), color);

			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, p.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, n.Y, p.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, p.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, n.Y, p.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, p.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, n.Y, n.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, p.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, n.Y, n.Z), transform), color);
		}
	}
}
