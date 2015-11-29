using System;
using System.Collections.Generic;
using System.IO;
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
		double frustumZNear = 0.0001;
		double frustumZFar	= 100000.0;
		public readonly double camFov = 45;

		double ViewToPointYaw	=  Math.PI;
		double ViewToPointPitch = -Math.PI/2.01;

		#region Free Surface Camera

		DVector3	FreeSurfacePosition;
		DVector3	FreeSurfaceVelocityDirection;
		double		FreeSurfaceVelocityMagnitude = 0.1;

		double FreeSurfaceYaw	= Math.PI;
		double FreeSurfacePitch = -Math.PI / 2.01;

		bool		freezeFreeCamRotation = false;
		DQuaternion freeSurfaceRotation;

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
				prevMouseScroll = input.TotalMouseScroll;
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
				
				#region Input
					// Update surface camera yaw and pitch
					if (input.IsKeyDown(Keys.RightButton)) {
						FreeSurfaceYaw		+= input.RelativeMouseOffset.X*0.0003;
						FreeSurfacePitch	-= input.RelativeMouseOffset.Y*0.0003;

						if (prevMouseScroll > input.TotalMouseScroll)
							FreeSurfaceVelocityMagnitude -= 0.001;
						if (prevMouseScroll < input.TotalMouseScroll)
							FreeSurfaceVelocityMagnitude += 0.001;

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
					if (FreeSurfaceYaw > DMathUtil.TwoPi)	FreeSurfaceYaw -= DMathUtil.TwoPi;
					if (FreeSurfaceYaw < -DMathUtil.TwoPi)	FreeSurfaceYaw += DMathUtil.TwoPi;

					// Calculate free cam rotation matrix

					if (!freezeFreeCamRotation)
						freeSurfaceRotation = DQuaternion.RotationAxis(DVector3.UnitY, FreeSurfaceYaw) * DQuaternion.RotationAxis(DVector3.UnitX, FreeSurfacePitch);

					var quat = freeSurfaceRotation;
					var qRot = DMatrix.RotationQuaternion(quat);
					var matrix = qRot * mat;

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

		struct SurfaceCameraState
		{
			public DVector3	FreeSurfacePosition;
			public double	FreeSurfaceYaw;
			public double	FreeSurfacePitch;
			public float	WaitTime;
			public float	TransitionTime;

			public DQuaternion FreeRotation;
		}

		List<SurfaceCameraState> cameraStates;
		int		curStateInd = 0;
		float	curTime		= 0;

		public void SaveCurrentStateToFile(string fileName = "cameraAnimation.txt")
		{
			File.AppendAllText(fileName,
				"Free camera position\n"	+
				FreeSurfacePosition.X + " " + FreeSurfacePosition.Y + " " + FreeSurfacePosition.Z +
				"\nFreeSurfaceYaw\n"		+
				FreeSurfaceYaw				+
				"\nFreeSurfacePitch\n"		+
				FreeSurfacePitch +
				"\nWaitTime\n" +
				3 +
				"\nTransitionTime\n" +
				3 +
				"\n\n"
			);
		}

		public void LoadAnimation(string fileName = "cameraAnimation.txt")
		{
			cameraStates = new List<SurfaceCameraState>();

			var lines = File.ReadAllLines(fileName);

			SurfaceCameraState curState = new SurfaceCameraState();

			for (int i = 0; i < lines.Length; i++) {
				var line = lines[i];

				if (line == "") {
					curState.FreeRotation = DQuaternion.RotationAxis(DVector3.UnitY, curState.FreeSurfaceYaw) * DQuaternion.RotationAxis(DVector3.UnitX, curState.FreeSurfacePitch);

					cameraStates.Add(curState);

					curState = new SurfaceCameraState();
					continue;
				}

				switch (line) {
					case "Free camera position" :
						var vals = lines[++i].Split(' ');
						curState.FreeSurfacePosition = new DVector3(double.Parse(vals[0]), double.Parse(vals[1]), double.Parse(vals[2]));
						break;
					case "FreeSurfaceYaw" :
						curState.FreeSurfaceYaw = double.Parse(lines[++i]);
						break;
					case "FreeSurfacePitch" :
						curState.FreeSurfacePitch = double.Parse(lines[++i]);
						break;
					case "WaitTime" :
						curState.WaitTime = float.Parse(lines[++i]);
						break;
					case "TransitionTime" :
						curState.TransitionTime = float.Parse(lines[++i]);
						break;
				}
			}
		}


		public void PlayAnimation(GameTime gameTime)
		{
			if(cameraStates == null) return;

			freezeFreeCamRotation = true;

			var state = cameraStates[curStateInd];


			curTime += 0.016f; //gameTime.ElapsedSec;
			
			if (curTime < state.WaitTime || curStateInd >= cameraStates.Count - 1) {
				SetState(state);
				return;
			}

			float time		= curTime - state.WaitTime;
			float amount	= time/state.TransitionTime;

			float	factor = MathUtil.SmoothStep(amount);
					factor = MathUtil.Clamp(factor, 0.0f, 1.0f);

			var nextState = cameraStates[curStateInd+1];

			var curPos		= DVector3.Lerp(state.FreeSurfacePosition, nextState.FreeSurfacePosition, factor);
			var curFreeRot	= DQuaternion.Slerp(state.FreeRotation, nextState.FreeRotation, factor);

			freeSurfaceRotation = curFreeRot;

			CameraPosition = FreeSurfacePosition = curPos;

			var newLonLat = GetCameraLonLat();
			Yaw		= newLonLat.X;
			Pitch	= -newLonLat.Y;
			
			if (curTime > state.WaitTime + state.TransitionTime) {
				curStateInd++;
				curTime = 0;
			}
		}

		public void ResetAnimation()
		{
			curStateInd = 0;
			curTime		= 0;
		}

		public void StopAnimation()
		{
			freezeFreeCamRotation	= false;
			cameraStates			= null;
		}

		void SetState(SurfaceCameraState state)
		{
			CameraPosition = FreeSurfacePosition = state.FreeSurfacePosition;
			CameraDistance = CameraPosition.Length();

			FreeSurfaceYaw		= state.FreeSurfaceYaw;
			FreeSurfacePitch	= state.FreeSurfacePitch;

			freeSurfaceRotation = state.FreeRotation;

			var newLonLat = GetCameraLonLat();
			Yaw		= newLonLat.X;
			Pitch	= -newLonLat.Y;
		}
		

		void GetEulerAngles(DMatrix q, out double yaw, out double pitch, out double roll)
		{
			yaw		= Math.Atan2(q.M32, q.M33);
			pitch	= Math.Atan2(-q.M31, Math.Sqrt(q.M32*q.M32 + q.M33*q.M33));
			roll	= Math.Atan2(q.M21, q.M11);
		}

		void GetEulerAngles(DQuaternion q, out double yaw, out double pitch, out double roll)
		{
		    double w2 = q.W*q.W;
		    double x2 = q.X*q.X;
		    double y2 = q.Y*q.Y;
		    double z2 = q.Z*q.Z;
		    double unitLength = w2 + x2 + y2 + z2;    // Normalised == 1, otherwise correction divisor.
		    double abcd = q.W*q.X + q.Y*q.Z;
		    double eps = 1e-7;    // TODO: pick from your math lib instead of hardcoding.
		    double pi = 3.14159265358979323846;   // TODO: pick from your math lib instead of hardcoding.
		    
			if (abcd > (0.5-eps)*unitLength) {
		        yaw		= 2 * Math.Atan2(q.Y, q.W);
		        pitch	= pi;
		        roll	= 0;
		    }
		    else if (abcd < (-0.5+eps)*unitLength) {
		        yaw		= -2 * Math.Atan2(q.Y, q.W);
		        pitch	= -pi;
		        roll	= 0;
		    }
		    else {
		        double adbc = q.W*q.Z - q.X*q.Y;
		        double acbd = q.W*q.Y - q.X*q.Z;
		        yaw		= Math.Atan2(2*adbc, 1 - 2*(z2+x2));
		        pitch	= Math.Asin(2*abcd/unitLength);
				roll	= Math.Atan2(2 * acbd, 1 - 2 * (y2 + x2));
		    }
		}

		#endregion

	}
}
