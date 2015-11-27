using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using SharpDX.DirectSound;

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
			set {
				pitch = DMathUtil.Clamp(value, minPitch, maxPitch);
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

		public Viewport Viewport { set; get; }

		public bool ViewToPointSwitcher = false;
		public bool FreeSurfaceSwitcher	= false;

		public DVector3 CameraPosition { get; private set; }
		public double CameraDistance {
			get { return cameraDistance; }
			set {
				cameraDistance = DMathUtil.Clamp(value, EarthRadius + 0.35, maxCameraDistance);
			}
		}

		public DVector3 FinalCamPosition = DVector3.Zero;

		
		double frustumWidth;
		double frustumHeight;
		double frustumZNear = 0.1;
		double frustumZFar	= 100000.0;
		public readonly double camFov = 20;

		double ViewToPointYaw	=  Math.PI;
		double ViewToPointPitch = -Math.PI/2.01;

		#region Free Surface Camera

		DVector3	FreeSurfacePosition;
		DVector3	FreeSurfaceVelocityDirection;
		double		FreeSurfaceVelocityMagnitude = 0.1;

		double FreeSurfaceYaw	= Math.PI;
		double FreeSurfacePitch = -Math.PI / 2.01;

		#endregion


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
					Yaw		= 0.52932849788406378;
					Pitch	= -1.0458657020378879;
					break;
				case Places.Vladivostok:
					Yaw		= DMathUtil.DegreesToRadians(131.881642);
					Pitch	= -DMathUtil.DegreesToRadians(43.111248);
					break;
			}
		}

		int prevMouseScroll = 0;
		public void Update(GameTime gameTime)
		{
			UpdateProjectionMatrix();

			CameraPosition = DVector3.Transform(new DVector3(0, 0, CameraDistance), Rotation);

			UpdateViewToPointCamera();

			ViewMatrix = DMatrix.LookAtRH(CameraPosition, DVector3.Zero, DVector3.UnitY);

			ViewMatrixWithTranslation = ViewMatrix;

			ViewMatrix.TranslationVector = DVector3.Zero;

			FinalCamPosition = CameraPosition;


			var input = gameEngine.InputDevice;

			if (input.IsKeyDown(Keys.LeftShift))	{ ViewToPointSwitcher = true; }
			if (input.IsKeyDown(Keys.RightShift))	{ ViewToPointSwitcher = false; }
			if (input.IsKeyDown(Keys.LeftControl)) {
				FreeSurfaceSwitcher = true;
				FreeSurfacePosition = CameraPosition;
			}
			if (input.IsKeyDown(Keys.RightControl)) {
				FreeSurfaceSwitcher = false;
				CameraPosition = FreeSurfacePosition;
				CameraDistance = CameraPosition.Length();
			}

			if (ViewToPointSwitcher) {
				var mat = CalculateBasisOnSurface();

				DVector3	lookAtPoint = mat.Up * EarthRadius;
				double		length		= CameraDistance - EarthRadius;

				var quat	= DQuaternion.RotationAxis(DVector3.UnitY, ViewToPointYaw) * DQuaternion.RotationAxis(DVector3.UnitX, ViewToPointPitch);
				var qRot	= DMatrix.RotationQuaternion(quat);
				var matrix	= qRot * mat;

				var pointOffset = DVector3.Transform(new DVector3(0, 0, length), matrix);
				var camPoint	= new DVector3(pointOffset.X, pointOffset.Y, pointOffset.Z) + lookAtPoint;

				FinalCamPosition = camPoint;

				ViewMatrix					= DMatrix.LookAtRH(camPoint, lookAtPoint, mat.Up);
				ViewMatrixWithTranslation	= ViewMatrix;

				ViewMatrix.TranslationVector = DVector3.Zero;
			} else if (FreeSurfaceSwitcher) {

				var mat = CalculateBasisOnSurface();

				// Update surface camera yaw and pitch
				if (input.IsKeyDown(Keys.RightButton)) {
					FreeSurfaceYaw		+= input.RelativeMouseOffset.X*0.0003;
					FreeSurfacePitch	-= input.RelativeMouseOffset.Y*0.0003;
					
					FreeSurfaceVelocityMagnitude += (input.TotalMouseScroll - prevMouseScroll)*0.0001;
					//Console.WriteLine(FreeSurfaceVelocityMagnitude);

					input.IsMouseCentered	= false;
					input.IsMouseHidden		= true;
				}
				else {
					input.IsMouseCentered	= false;
					input.IsMouseHidden		= false;
				}
				prevMouseScroll = input.TotalMouseScroll;


				if (gameEngine.Keyboard.IsKeyDown(Input.Keys.Left))		FreeSurfaceYaw		-= gameTime.ElapsedSec * 0.5;
				if (gameEngine.Keyboard.IsKeyDown(Input.Keys.Right))	FreeSurfaceYaw		+= gameTime.ElapsedSec * 0.5;
				if (gameEngine.Keyboard.IsKeyDown(Input.Keys.Up))		FreeSurfacePitch	-= gameTime.ElapsedSec * 0.1;
				if (gameEngine.Keyboard.IsKeyDown(Input.Keys.Down))		FreeSurfacePitch	+= gameTime.ElapsedSec * 0.1;


				//FreeSurfaceYaw = DMathUtil.Clamp(FreeSurfaceYaw, -DMathUtil.PiOverTwo, DMathUtil.PiOverTwo);
				if (FreeSurfaceYaw > DMathUtil.TwoPi) FreeSurfaceYaw -= DMathUtil.TwoPi;
				if (FreeSurfaceYaw < -DMathUtil.TwoPi) FreeSurfaceYaw += DMathUtil.TwoPi;

				// Calculate free cam rotation matrix

				var quat = DQuaternion.RotationAxis(DVector3.UnitY, FreeSurfaceYaw) * DQuaternion.RotationAxis(DVector3.UnitX, FreeSurfacePitch);
				var qRot = DMatrix.RotationQuaternion(quat);
				var matrix = qRot * mat;

				#region Mouse direction and velocity
					double velocityMag = 0;
					var velDir = DVector3.Zero;

					if (input.IsKeyDown(Keys.W)) {
						velDir += matrix.Forward;
						velocityMag = FreeSurfaceVelocityMagnitude;
					}
					if (input.IsKeyDown(Keys.S)) {
						velDir -= matrix.Forward;
						velocityMag = FreeSurfaceVelocityMagnitude;
					}
					if (input.IsKeyDown(Keys.A)) {
						velDir += matrix.Right;
						velocityMag = FreeSurfaceVelocityMagnitude;
					}
					if (input.IsKeyDown(Keys.D)) {
						velDir += matrix.Left;
						velocityMag = FreeSurfaceVelocityMagnitude;
					}
					if (input.IsKeyDown(Keys.Space)) {
						velDir += mat.Up;
						velocityMag = FreeSurfaceVelocityMagnitude;
					}
					if (input.IsKeyDown(Keys.C)) {
						velDir += mat.Down;
						velocityMag = FreeSurfaceVelocityMagnitude;
					}

					if (velocityMag != 0) {
						velDir.Normalize();
					}
				#endregion

				// Update camera position
				FinalCamPosition	= FreeSurfacePosition = FreeSurfacePosition + velDir * velocityMag;
				CameraPosition		= FinalCamPosition;

				//Calculate view matrix
				ViewMatrix = DMatrix.LookAtRH(FinalCamPosition, FinalCamPosition + matrix.Forward, matrix.Up);
				ViewMatrixWithTranslation		= ViewMatrix;
				ViewMatrix.TranslationVector	= DVector3.Zero;

				// Calculate new yaw and pitch
				CameraDistance = CameraPosition.Length();

				var newLonLat = GetCameraLonLat();
				Yaw		= newLonLat.X;
				Pitch	= -newLonLat.Y;
			}

			ViewMatrixFloat = DMatrix.ToFloatMatrix(ViewMatrix);
			ProjMatrixFloat = DMatrix.ToFloatMatrix(ProjMatrix);

			//var viewDir = CameraPosition / CameraPosition.Length();
		}


		DMatrix CalculateBasisOnSurface()
		{
			DVector3 cameraUp = CameraPosition / CameraPosition.Length();

			var xAxis = DVector3.TransformNormal(DVector3.UnitX, DMatrix.RotationAxis(DVector3.UnitY, Yaw));
			xAxis.Normalize();

			var mat = DMatrix.Identity;
			mat.Up		= cameraUp;
			mat.Right	= xAxis;
			mat.Forward = DVector3.Cross(xAxis, cameraUp);
			mat.Forward.Normalize();

			return mat;
		}


		void UpdateViewToPointCamera()
		{
			var input = gameEngine.InputDevice;

			if (ViewToPointSwitcher && input.IsKeyDown(Keys.MiddleButton)) {
				var yawDelta = input.RelativeMouseOffset.X * 0.003;
				var pitDelta = input.RelativeMouseOffset.Y * 0.003;

				RotateViewToPointCamera(yawDelta, pitDelta);
			}
		}


		public void RotateViewToPointCamera(double yawDelta, double pitchDelta)
		{
			ViewToPointYaw		+= yawDelta;
			ViewToPointPitch	-= pitchDelta;

			ViewToPointPitch = DMathUtil.Clamp(ViewToPointPitch, -Math.PI / 2.01, 0.0);
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
			var nearPoint	= new DVector3((CameraPosition.X / CameraDistance), (CameraPosition.Y / CameraDistance), (CameraPosition.Z / CameraDistance));
			var farPoint	= new DVector3(0, 0, 0);

			DVector3[] res;
			DVector2 ret = DVector2.Zero;

			if (GeoHelper.LineIntersection(nearPoint, farPoint, 1.0, out res)) {
				if (res.Length > 0) {
					GeoHelper.CartesianToSpherical(res[0], out ret.X, out ret.Y);
				}
			}

			return ret;
		}


		#region Free Surface Camera Animation Stuff

		public void SaveCurrentStateToFile(string fileName = "d")
		{
			
		}

		#endregion

	}
}
