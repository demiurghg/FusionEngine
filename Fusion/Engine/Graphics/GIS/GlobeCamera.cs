using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public class GlobeCamera
	{
		GameEngine gameEngine;


		double pitch = 0.0f;
		readonly double maxPitch = 0;
		readonly double minPitch = 0;

		public double Yaw { set; get; }
		public double Pitch
		{
			set
			{
				pitch = value;
				if (pitch > maxPitch) pitch = maxPitch;
				if (pitch < minPitch) pitch = minPitch;
			}
			get { return pitch; }
		}

		public DQuaternion Rotation { get { return DQuaternion.RotationAxis(DVector3.UnitY, Yaw) * DQuaternion.RotationAxis(DVector3.UnitX, Pitch); } }


		// Camera stuff
		public DMatrix ViewMatrix;
		public DMatrix ProjMatrix;
		
		public Matrix ViewMatrixFloat;
		public Matrix ProjMatrixFloat;

		public DMatrix ViewMatrixWithTranslation;

		Viewport viewport;
		public Viewport Viewport { set { viewport = value; UpdateProjectionMatrix(); } get { return viewport; } }

		public bool PerspSwitcher = false;


		public DVector3 CameraPosition { get; private set; }
		public double CameraDistance
		{
			get { return cameraDistance; }
			set
			{
				cameraDistance = value;
				if (cameraDistance - EarthRadius < 0.35) cameraDistance = EarthRadius + 0.35;
				if (cameraDistance > maxCameraDistance) cameraDistance = maxCameraDistance;
			}
		}

		public DVector3 FreeCamPosition = DVector3.Zero;



		double frustumWidth;
		double frustumHeight;
		double frustumZNear = 0.1;
		double frustumZFar	= 100000.0;
		public readonly double camFov = 20;

		double FreeCamYaw	=  Math.PI / 2.0;
		double FreeCamPitch = -Math.PI / 10.0;



		public readonly double EarthRadius = 6378.137;
		double maxCameraDistance	= 100000.0;
		double cameraDistance		= 6500.0;


		public enum Places
		{
			SaintPetersburg_VO,
			Vladivostok,
		}

		

		public GlobeCamera(GameEngine engine)
		{
			gameEngine = engine;

			maxPitch = DMathUtil.DegreesToRadians(87.5);
			minPitch = DMathUtil.DegreesToRadians(-87.5);


			Viewport = new Viewport(0, 0, 1920, 1080);
		}


		public void GoToPlace(Places place)
		{
			switch (place) {
				case Places.SaintPetersburg_VO:
					Yaw = 0.52932849788406378;
					Pitch = -1.0458657020378879;
					break;
				case Places.Vladivostok:
					Yaw = DMathUtil.DegreesToRadians(131.881642);
					Pitch = -DMathUtil.DegreesToRadians(43.111248);
					break;
			}
		}


		public void Update(GameTime gameTime)
		{
			CameraPosition = DVector3.Transform(new DVector3(0, 0, CameraDistance), Rotation);

			UpdateFreeCamera();

			ViewMatrix = DMatrix.LookAtRH(CameraPosition, DVector3.Zero, DVector3.UnitY);

			ViewMatrixWithTranslation = ViewMatrix;

			ViewMatrix.TranslationVector = DVector3.Zero;

			FreeCamPosition = CameraPosition;


			var input = gameEngine.InputDevice;

			if (input.IsKeyDown(Keys.LeftShift))	{ PerspSwitcher = true; }
			if (input.IsKeyDown(Keys.RightShift))	{ PerspSwitcher = false; }

			if (PerspSwitcher) {
				DVector3 cameraUp		= CameraPosition / CameraPosition.Length();
				DVector3 lookAtPoint	= cameraUp * EarthRadius;

				double length = CameraDistance - EarthRadius;

				var quat = DQuaternion.RotationAxis(DVector3.UnitY, FreeCamYaw) * DQuaternion.RotationAxis(DVector3.UnitX, FreeCamPitch);
				var qRot = DMatrix.RotationQuaternion(quat);
				var mat	 = DMatrix.Identity;

				var xAxis = DVector3.TransformNormal(DVector3.UnitX, DMatrix.RotationAxis(DVector3.UnitY, Yaw));
				xAxis.Normalize();

				mat.Up = cameraUp;
				mat.Right = xAxis;
				mat.Forward = DVector3.Cross(xAxis, cameraUp);
				mat.Forward.Normalize();

				var matrix = qRot * mat;

				var c = DVector3.Transform(new DVector3(0, 0, length), matrix);

				var camPoint = new DVector3(c.X, c.Y, c.Z) + lookAtPoint;

				FreeCamPosition = camPoint;

				ViewMatrix					= DMatrix.LookAtRH(camPoint, lookAtPoint, cameraUp);
				ViewMatrixWithTranslation	= ViewMatrix;

				ViewMatrix.TranslationVector = DVector3.Zero;
			}

			ViewMatrixFloat = DMatrix.ToFloatMatrix(ViewMatrix);
			ProjMatrixFloat = DMatrix.ToFloatMatrix(ProjMatrix);

			var viewDir = CameraPosition / CameraPosition.Length();
		}


		void UpdateFreeCamera()
		{
			var input = gameEngine.InputDevice;

			if (PerspSwitcher && input.IsKeyDown(Keys.MiddleButton)) {
				FreeCamYaw		+= input.RelativeMouseOffset.X * 0.003;
				FreeCamPitch	-= input.RelativeMouseOffset.Y * 0.003;

				FreeCamPitch = DMathUtil.Clamp(FreeCamPitch, -Math.PI / 2.01, 0.0);
			}
		}


		void UpdateProjectionMatrix()
		{
			double aspect = Viewport.AspectRatio;

			double nearHeight	= frustumZNear * Math.Tan(DMathUtil.DegreesToRadians(camFov / 2));
			double nearWidth	= nearHeight * aspect;

			frustumWidth	= nearWidth;
			frustumHeight	= nearHeight;

			ProjMatrix = DMatrix.PerspectiveOffCenterRH(-nearWidth / 2, nearWidth / 2, -nearHeight / 2, nearHeight / 2, frustumZNear, frustumZFar);
		}



		public void CameraZoom(float delta)
		{
			CameraDistance += (CameraDistance - EarthRadius) * delta;
		}


		public bool ScreenToSpherical(float x, float y, out DVector2 lonLat, bool useGlobalView = false)
		{
			var w = Viewport.Width;
			var h = Viewport.Height;

			var nearPoint	= new DVector3(x, y, frustumZNear);
			var farPoint	= new DVector3(x, y, frustumZFar);


			var vm	= useGlobalView ? ViewMatrixWithTranslation : DMatrix.LookAtRH(CameraPosition, DVector3.Zero, DVector3.UnitY);
			var mVP = vm * ProjMatrix;

			var near	= DVector3.Unproject(nearPoint, 0, 0, w, h, frustumZNear, frustumZFar, mVP);
			var far		= DVector3.Unproject(farPoint, 0, 0, w, h, frustumZNear, frustumZFar, mVP);

			lonLat = DVector2.Zero;

			DVector3[] res;
			if (GeoHelper.LineIntersection(near, far, EarthRadius, out res)) {
				GeoHelper.CartesianToSpherical(res[0], out lonLat.X, out lonLat.Y);
				return true;
			}

			return false;
		}


		public DVector2 GetCameraLonLat()
		{
			var nearPoint = new DVector3((CameraPosition.X / CameraDistance), (CameraPosition.Y / CameraDistance), (CameraPosition.Z / CameraDistance));
			var farPoint = new DVector3(0, 0, 0);

			DVector3[] res;
			DVector2 ret = DVector2.Zero;

			if (GeoHelper.LineIntersection(nearPoint, farPoint, 1.0, out res)) {
				if (res.Length > 0) {
					GeoHelper.CartesianToSpherical(res[0], out ret.X, out ret.Y);
				}
			}

			return ret;
		}
	}
}
