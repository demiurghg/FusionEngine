using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.Concurrent;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Lidgren.Network;

namespace Fusion.Engine.Graphics.GIS
{
    public class Gis : GameModule
    {
	    public GlobeCamera Camera;

		ConstantBuffer constBuffer;

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

	    public readonly MessageQueue JobQueue;
	    public readonly MessageQueue DoneQueue;


	    public enum PointCoordsType
		{
			Geo,
			Cartesian
		}



	    public class GisLayer
	    {
		    protected Game Game;

			public bool IsActive	= true;
			public bool IsVisible	= true;

		    public uint ZOrder;

			public virtual void Draw	(GameTime gameTime, ConstantBuffer constBuffer) {}
			public virtual void Update	(GameTime gameTime) {}

			public virtual List<SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		    {
			    return null;
		    }

		    public virtual void Dispose	() {}


		    public GisLayer(Game engine)
		    {
			    Game = engine;
		    }
	    }



	    public class SelectedItem
	    {
		    public string Name;
		    public double Distance;

	    }


	    public Gis(Game Game) : base(Game)
	    {
			JobQueue	= new MessageQueue();
			DoneQueue	= new MessageQueue();

			JobQueue.StartInAnotherThread();
	    }


	    public override void Initialize()
	    {
			constBuffer = new ConstantBuffer(Game.GraphicsDevice, typeof(ConstData));
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

	    }


	    public  void Draw(GameTime gameTime, StereoEye stereoEye, ICollection<GisLayer> layers)
	    {
		    if (!layers.Any()) return;

			using (new PixEvent("GIS")) {

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
	    }



		public static void UtmToLatLon(double utmX, double utmY, string utmZone, out double longitude, out double latitude)
		{
			bool isNorthHemisphere = utmZone.Last() >= 'N';

			var diflat = -0.00066286966871111111111111111111111111;
			var diflon = -0.0003868060578;

			var zone = int.Parse(utmZone.Remove(utmZone.Length - 1));
			var c_sa = 6378137.000000;
			var c_sb = 6356752.314245;

			var e2 = Math.Pow(((c_sa * c_sa) - (c_sb * c_sb)), 0.5) / c_sb;
			var e2cuadrada = (e2 * e2);
			var c = (c_sa * c_sa) / c_sb;
			
			var x = utmX - 500000;
			var y = isNorthHemisphere ? utmY : utmY - 10000000;

			var s = ((zone * 6.0) - 183.0);
			var lat = y / (c_sa * 0.9996);
			
			var latCos		= Math.Cos(lat);
			var latCosSqr	= latCos*latCos;
			var v = (c / Math.Pow(1 + (e2cuadrada * latCosSqr), 0.5)) * 0.9996;
			
			var a = x / v;
			var a1 = Math.Sin(2 * lat);
			var a2 = a1 * Math.Pow((latCos), 2);
			
			var j2 = lat + (a1 / 2.0);
			var j4 = ((3 * j2) + a2) / 4.0;
			var j6 = ((5 * j4) + Math.Pow(a2 * latCos, 2)) / 3.0;
			
			var alfa = (3.0 / 4.0) * e2cuadrada;
			var beta = (5.0 / 3.0) * (alfa * alfa);
			var gama = (35.0 / 27.0) * (alfa * alfa * alfa);
			var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
			var b = (y - bm) / v;

			var epsi = ((e2cuadrada * (a*a)) / 2.0) * latCosSqr;
			var eps = a * (1 - (epsi / 3.0));
			
			var nab = (b * (1 - epsi)) + lat;
			var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
			var delt = Math.Atan(senoheps / (Math.Cos(nab)));
			var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

			longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
			latitude = ((lat + (1 + e2cuadrada * latCosSqr - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * latCos * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
		}
    }
}
