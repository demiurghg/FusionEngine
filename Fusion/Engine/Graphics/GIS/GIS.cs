using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Lidgren.Network;

namespace Fusion.Engine.Graphics.GIS
{
    public class Gis : GameModule
    {
	    public GlobeCamera Camera;

		ConstantBuffer constBuffer;

		// Camera stuff
	    //public void GoToPlace(Vector2 place, double height) {}
		//public void ProjectPoint()		{ }
		//public void UnProjectPoint()	{ }

	    public static DebugGisLayer Debug;

		// Batch stuff
	    public struct GeoPoint
	    {
		    [Vertex("TEXCOORD", 0)] public double	Lon;
		    [Vertex("TEXCOORD", 1)] public double	Lat;
		    [Vertex("TEXCOORD", 2)] public Vector4	Tex0;
		    [Vertex("TEXCOORD", 3)] public Vector4	Tex1;
		    [Vertex("Color")]		public Color4	Color;
	    }


		public struct CartPoint
	    {
		    [Vertex("TEXCOORD", 0)] public double	X;
		    [Vertex("TEXCOORD", 1)] public double	Y;
		    [Vertex("TEXCOORD", 2)] public double	Z;
		    [Vertex("TEXCOORD", 3)] public Vector4	Tex0;
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

	    public class GisLayer
	    {
		    protected Game Game;

			public bool IsActive = true;
			public bool IsVisible = true;

		    public uint ZOrder;

			public virtual void Draw	(GameTime gameTime, ConstantBuffer constBuffer) {}//, Settings config)	{ }
			public virtual void Update	(GameTime gameTime) {}//, Settings config)	{ }
			public virtual void Dispose	() {}


		    public GisLayer(Game engine)
		    {
			    Game = engine;
		    }
	    }

		Vector2 previousMousePosition;

	    public Gis(Game Game) : base(Game)
	    {
			
	    }


	    public override void Initialize()
	    {
			constBuffer = new ConstantBuffer(Game.GraphicsDevice, typeof(ConstData));

			#region Test input region
			Camera		= new GlobeCamera(Game);

			Camera.Viewport = new Viewport(0, 0, Game.GraphicsDevice.DisplayBounds.Width, Game.GraphicsDevice.DisplayBounds.Height);
			Camera.GoToPlace(GlobeCamera.Places.SaintPetersburg_VO);

		    Game.GraphicsDevice.DisplayBoundsChanged +=
			    (sender, args) =>
				    Camera.Viewport =
					    new Viewport(0, 0, Game.GraphicsDevice.DisplayBounds.Width,
						    Game.GraphicsDevice.DisplayBounds.Height);


			// Input bindings
		    Game.Mouse.Scroll += (sender, args) => {
				if(args.WheelDelta > 0)
					Camera.CameraZoom(-0.05f);
				else if(args.WheelDelta < 0)
					Camera.CameraZoom(0.05f);
		    };

		    Game.Mouse.Move += (sender, args) =>
		    {
				if (Game.InputDevice.IsKeyDown(Keys.LeftButton)) {
					DVector2 before, after;
					var beforeHit	= Camera.ScreenToSpherical(previousMousePosition.X, previousMousePosition.Y, out before, true);
					var afterHit	= Camera.ScreenToSpherical(args.Position.X, args.Position.Y, out after, true);

					if (beforeHit && afterHit) {
						Camera.Yaw		-= after.X - before.X;
						Camera.Pitch	+= after.Y - before.Y;
					}
				}
				if(Game.InputDevice.IsKeyDown(Keys.MiddleButton) && Camera.CameraState == GlobeCamera.CameraStates.ViewToPoint) {
					Camera.RotateViewToPointCamera(Game.InputDevice.RelativeMouseOffset);
			    }
				if(Game.InputDevice.IsKeyDown(Keys.RightButton) && Camera.CameraState == GlobeCamera.CameraStates.FreeSurface) {
					Camera.RotateFreeSurfaceCamera(Game.InputDevice.RelativeMouseOffset);
			    }
				previousMousePosition = new Vector2(args.Position.X, args.Position.Y);
		    };
			#endregion

			//Points = new PointsGisBatch(Game, 100)
		    //{
		    //	ImageSizeInAtlas	= new Vector2(36, 36),
		    //	TextureAtlas		= Game.Content.Load<Texture2D>("circles.tga")
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


			//lines = new LinesGisBatch(Game, 2, true);
			//
			//lines.PointsCpu[0] = new GeoPoint
			//{
			//	Lon		= DMathUtil.DegreesToRadians(29),
			//	Lat		= DMathUtil.DegreesToRadians(59),
			//	Color	= Color.White,
			//	Tex0	= new Vector4(1, 1, 1, 1)
			//};
			//lines.PointsCpu[1] = new GeoPoint
			//{
			//	Lon		= DMathUtil.DegreesToRadians(30),
			//	Lat		= DMathUtil.DegreesToRadians(60),
			//	Color	= Color.White,
			//	Tex0	= new Vector4(1, 1, 1, 1)
			//};
			//lines.UpdatePointsBuffer();

			//heatMap = PolyGisBatch.GenerateRegularGrid(30.165024, 30.332521, 59.965494, 59.911272, 10, 64, 64, Globe.CurrentMapSource.Projection);
		    //heatMap.MaxHeatMapLevel = 1.0f;
		    //heatMap.InterpFactor	= 1.0f;
			//
			//heatMap.ClearData();
		    //for (int i = 0; i < 100; i++) {
			//	heatMap.AddValue(r.NextDouble(heatMap.Left, heatMap.Right), r.NextDouble(heatMap.Bottom, heatMap.Top), 10.0f);			    
		    //}
			//
			//heatMap.UpdateHeatMap();
	    }


	    protected override void Dispose(bool disposing)
	    {
		    if (disposing) {
				constBuffer.Dispose();
		    }
		    base.Dispose(disposing);
	    }


	    public void Update(GameTime gameTime)
	    {
		    //Camera.Update(gameTime);
	    }


	    public  void Draw(GameTime gameTime, StereoEye stereoEye, ICollection<GisLayer> layers)
	    {
		    if (!layers.Any()) return;

			constantData.ViewProj		= Camera.ViewMatrixFloat * Camera.ProjMatrixFloat;
			constantData.ViewPositionX	= Camera.FinalCamPosition.X;
			constantData.ViewPositionY	= Camera.FinalCamPosition.Y;
			constantData.ViewPositionZ	= Camera.FinalCamPosition.Z;

			constBuffer.SetData(constantData);

			var batches = layers.OrderByDescending(x => x.ZOrder);
			
		    foreach (var batch in batches) {
			    if (!batch.IsVisible) continue;

				batch.Draw(gameTime, constBuffer);
		    }
	    }



		public static void UtmToLatLon(double utmX, double utmY, string utmZone, out double longitude, out double latitude)
		{
			bool isNorthHemisphere = utmZone.Last() >= 'N';

			var diflat = -0.00066286966871111111111111111111111111;
			var diflon = -0.0003868060578;

			var zone = int.Parse(utmZone.Remove(utmZone.Length - 1));
			var c_sa = 6378137.000000;
			var c_sb = 6356752.314245;
			var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
			var e2cuadrada = Math.Pow(e2, 2);
			var c = Math.Pow(c_sa, 2) / c_sb;
			var x = utmX - 500000;
			var y = isNorthHemisphere ? utmY : utmY - 10000000;

			var s = ((zone * 6.0) - 183.0);
			var lat = y / (c_sa * 0.9996);
			var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
			var a = x / v;
			var a1 = Math.Sin(2 * lat);
			var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
			var j2 = lat + (a1 / 2.0);
			var j4 = ((3 * j2) + a2) / 4.0;
			var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
			var alfa = (3.0 / 4.0) * e2cuadrada;
			var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
			var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
			var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
			var b = (y - bm) / v;
			var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
			var eps = a * (1 - (epsi / 3.0));
			var nab = (b * (1 - epsi)) + lat;
			var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
			var delt = Math.Atan(senoheps / (Math.Cos(nab)));
			var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

			longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
			latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
		}
    }
}
