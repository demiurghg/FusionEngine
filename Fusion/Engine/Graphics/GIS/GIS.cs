using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Lidgren.Network;

namespace Fusion.Engine.Graphics.GIS
{
    public class GIS : GameModule
    {
	    public GlobeCamera Camera;

		ConstantBuffer constBuffer;

		// Camera stuff
	    //public void GoToPlace(Vector2 place, double height) {}
		//public void ProjectPoint()		{ }
		//public void UnProjectPoint()	{ }



		// Batch stuff
	    public struct GeoPoint
	    {
		    [Vertex("TEXCOORD", 0)] public double	Lon;
		    [Vertex("TEXCOORD", 1)] public double	Lat;
		    [Vertex("TEXCOORD", 2)] public Vector4	Tex0;
		    [Vertex("TEXCOORD", 3)] public Vector4	Tex1;
		    [Vertex("Color")]		public Color4	Color;
	    }


	    [StructLayout(LayoutKind.Explicit)]
	    public struct ConstData
	    {
		    [FieldOffset(0)]	public Matrix ViewProj;
		    [FieldOffset(64)]	public double ViewPositionX;
		    [FieldOffset(72)]	public double ViewPositionY;
		    [FieldOffset(80)]	public double ViewPositionZ;
			[FieldOffset(88)]	public double Dummy;
	    }

	    ConstData constantData;


	    public enum PointCoordsType
		{
			Geo,
			Cartesian
		}

	    public class GisBatch
	    {
		    protected GameEngine GameEngine;

			public bool IsActive;
			public bool IsVisible;

		    public uint ZOrder;

			public virtual void Draw	(GameTime gameTime, ConstantBuffer constBuffer) {}//, Settings config)	{ }
			public virtual void Update	(GameTime gameTime) {}//, Settings config)	{ }
			public virtual void Dispose	() {}


		    public GisBatch(GameEngine engine)
		    {
			    GameEngine = engine;
		    }
	    }


	    List<GisBatch> currentBatches	= new List<GisBatch>();


	    Context currentContext;

		Vector2 previousMousePosition;


	    // Context stuff
		public class Context
		{
			public List<GisBatch> Batches;

			//public Settings Config;

		}

		TilesGisBatch  Globe;
	    //PointsGisBatch Points;


	    public GIS(GameEngine gameEngine) : base(gameEngine)
	    {
			
	    }


	    public override void Initialize()
	    {
			constBuffer = new ConstantBuffer(GameEngine.GraphicsDevice, typeof(ConstData));
			Globe		= new TilesGisBatch(GameEngine);
			Camera		= new GlobeCamera(GameEngine);

			Camera.Viewport = new Viewport(0, 0, GameEngine.GraphicsDevice.DisplayBounds.Width, GameEngine.GraphicsDevice.DisplayBounds.Height);
			Camera.GoToPlace(GlobeCamera.Places.SaintPetersburg_VO);

		    GameEngine.GraphicsDevice.DisplayBoundsChanged +=
			    (sender, args) =>
				    Camera.Viewport =
					    new Viewport(0, 0, GameEngine.GraphicsDevice.DisplayBounds.Width,
						    GameEngine.GraphicsDevice.DisplayBounds.Height);


			// Input bindings
		    GameEngine.Mouse.Scroll += (sender, args) => {
				if(args.WheelDelta > 0)
					Camera.CameraZoom(-0.05f);
				else if(args.WheelDelta < 0)
					Camera.CameraZoom(0.05f);
		    };

		    GameEngine.Mouse.Move += (sender, args) =>
		    {
				if (GameEngine.InputDevice.IsKeyDown(Keys.LeftButton)) {
					DVector2 before, after;
					var beforeHit	= Camera.ScreenToSpherical(previousMousePosition.X, previousMousePosition.Y, out before, true);
					var afterHit	= Camera.ScreenToSpherical(args.Position.X, args.Position.Y, out after, true);

					if (beforeHit && afterHit) {
						Camera.Yaw		-= after.X - before.X;
						Camera.Pitch	+= after.Y - before.Y;
					}
				}

				previousMousePosition = new Vector2(args.Position.X, args.Position.Y);
		    };



			//Points = new PointsGisBatch(GameEngine, 100)
			//{
			//	ImageSizeInAtlas	= new Vector2(36, 36),
			//	TextureAtlas		= GameEngine.Content.Load<Texture2D>("circles.tga")
			//};
			//
			//var r = new Random();
			//
			//for (int i = 0; i < Points.PointsCpu.Length; i++) {
			//    Points.PointsCpu[i] = new GeoPoint {
			//		Lon		= DMathUtil.DegreesToRadians(30.301419 + 0.125 * r.NextDouble()),
			//		Lat		= DMathUtil.DegreesToRadians(59.942562 + 0.125 * r.NextDouble()),
			//		Color	= Color.White,
			//		Tex0	= new Vector4(r.Next(0, 10), 0, 0.5f, 3.14f)
			//    };
		    //}
			//Points.UpdatePointsBuffer();
		    
	    }


	    protected override void Dispose(bool disposing)
	    {
		    if (disposing) {
			    if (Globe != null) Globe.Dispose();

				constBuffer.Dispose();
		    }
		    base.Dispose(disposing);
	    }


	    public void Update(GameTime gameTime)
	    {
		    Camera.Update(gameTime);

			Globe.Update(gameTime);
			//Points.Update(gameTime);
	    }


	    public  void Draw(GameTime gameTime, StereoEye stereoEye)
	    {
			constantData.ViewProj		= Camera.ViewMatrixFloat * Camera.ProjMatrixFloat;
			constantData.ViewPositionX	= Camera.FreeCamPosition.X;
			constantData.ViewPositionY	= Camera.FreeCamPosition.Y;
			constantData.ViewPositionZ	= Camera.FreeCamPosition.Z;

			constBuffer.SetData(constantData);


			var batches = currentBatches.OrderByDescending(x => x.ZOrder);


			Globe.Draw(gameTime, constBuffer);
		    foreach (var batch in batches) {
				batch.Draw(gameTime, constBuffer);
		    }

			//Points.Draw(gameTime, constBuffer);
	    }


	    public void AddBatch(GisBatch gisBatch)
	    {
			if (!currentBatches.Contains(gisBatch))	currentBatches.Add(gisBatch);
	    }


	    public void		LoadContext		(Context context)	{ }
		public Context	SaveContext		()					{ return new Context(); }
		public void		ClearContext	()					{ }

    }
}
