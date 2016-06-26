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
		Game Game;

		public GlobeCameraParameters Parameters { protected set; get; }

		public enum CameraStates
		{
			TopDown,
			ViewToPoint,
			FreeSurface
		}

		public CameraStates CameraState = CameraStates.TopDown;

		double pitch = 0.0f;
		public double Yaw { set; get; }
		public double Pitch
		{
			set {
				pitch = DMathUtil.Clamp(value, Parameters.MinPitch, Parameters.MaxPitch);
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

		public DVector3 CameraPosition { get; private set; }
		public double CameraDistance {
			get { return cameraDistance; }
			set {
				cameraDistance = DMathUtil.Clamp(value, Parameters.MinCameraDistance, Parameters.MaxCameraDistance);
			}
		}

		public DVector3 FinalCamPosition = DVector3.Zero;

		
		double ViewToPointYaw	=  Math.PI;
		double ViewToPointPitch = -Math.PI/2.01;

		#region Free Surface Camera

		DVector3	FreeSurfacePosition;
		double		FreeSurfaceVelocityMagnitude = 0.001;

		double FreeSurfaceYaw	= Math.PI;
		double FreeSurfacePitch = -Math.PI / 2.01;

		bool		freezeFreeCamRotation = false;
		DQuaternion freeSurfaceRotation;

		private DVector3 velocityDirection;

		#endregion


		public readonly double EarthRadius = 6378.137;
		double cameraDistance	= 90000.0;
		


		public enum Places
		{
			SaintPetersburg_VO,
			Vladivostok,
			Sochi,
		}



		public class GlobeCameraParameters
		{
			public double MaxPitch			{ set; get; }
			public double MinPitch			{ set; get; }
			public double MaxCameraDistance { set; get; }

			public double MaxViewToPointPitch { set; get; }
			public double MinViewToPointPitch { set; get; }
			
			public double CameraFovDegrees	{ set; get; }
			public double FrustumZNear		{ set; get; }
			public double FrustumZFar		{ set; get; }
			public double FrustumWidth		{ internal set; get; }
			public double FrustumHeight		{ internal set; get; }

			public double MaxDistVelocityThreshold	{ set; get; }
			public double MinDistVelocityThreshold	{ set; get; }
			public double MaxVelocityFreeSurfCam	{ set; get; }
			public double MinVelocityFreeSurfCam	{ set; get; }

			public double MinCameraDistance { set; get; }

		}

		public GlobeCamera(Game engine)
		{
			Game = engine;

			Parameters = new GlobeCameraParameters {
				MaxPitch					= DMathUtil.DegreesToRadians(87.5),
				MinPitch					= DMathUtil.DegreesToRadians(-87.5),
				MaxCameraDistance			= 100000.0,
				MinCameraDistance			= EarthRadius + 0.01,
				CameraFovDegrees			= 45,
				FrustumZNear				= 0.0009765625f,
				FrustumZFar					= 131072,
				MaxDistVelocityThreshold	= 10000,
				MinDistVelocityThreshold	= 0.5,
				MaxVelocityFreeSurfCam	= 800,
				MinVelocityFreeSurfCam	= 0.01,
				MaxViewToPointPitch = -Math.PI / 2.01,
				MinViewToPointPitch = 0
			};


			Viewport = new Viewport(0, 0, 1920, 1080);

			ViewToPointYaw		= Math.PI;
			ViewToPointPitch	= -Math.PI / 2.01;
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
				case Places.Sochi:
					Yaw		= DMathUtil.DegreesToRadians(39.729996);
					Pitch	= -DMathUtil.DegreesToRadians(43.600035);
					break;
			}
		}



		public void ToggleViewToPointCamera()
		{
			if (CameraState == CameraStates.ViewToPoint) {

			} else {
				CameraState = CameraStates.ViewToPoint;
			} 
		}

		public void ToggleFreeSurfaceCamera()
		{
			if (CameraState == CameraStates.FreeSurface) {
				//CameraState = CameraStates.TopDown;
			} else {
				CameraState			= CameraStates.FreeSurface;
				FreeSurfacePosition = CameraPosition;
			}
		}

		public void ToggleTopDownCamera()
		{
			CameraState = CameraStates.TopDown;
		}


		/// <summary>
		/// Move TopDown and ViewToPoint cameras
		/// </summary>
		/// <param name="prevMousePos"></param>
		/// <param name="currentMousePos"></param>
		public void MoveCamera(Vector2 prevMousePos, Vector2 currentMousePos)
		{
			DVector2 before, after;
			var beforeHit	= ScreenToSpherical(prevMousePos.X, prevMousePos.Y, out before);
			var afterHit	= ScreenToSpherical(currentMousePos.X, currentMousePos.Y, out after);

			if (beforeHit && afterHit) {
				Yaw		-= after.X - before.X;
				Pitch	+= after.Y - before.Y;
			}
		}

		public void RotateViewToPointCamera(Vector2 relativeMouseOffset)
		{
			var yawDelta = relativeMouseOffset.X * 0.003;
			var pitDelta = relativeMouseOffset.Y * 0.003;

			RotateViewToPointCamera(yawDelta, pitDelta);
		}
		
		public void RotateViewToPointCamera(double yawDelta, double pitchDelta)
		{
			ViewToPointYaw		+= yawDelta;
			ViewToPointPitch	-= pitchDelta;

			ViewToPointPitch = DMathUtil.Clamp(ViewToPointPitch, -Math.PI / 2.01, Parameters.MinViewToPointPitch);
		}


		public void RotateFreeSurfaceCamera(Vector2 relativeMouseOffset)
		{
			var yawDelta = relativeMouseOffset.X * 0.003;
			var pitDelta = relativeMouseOffset.Y * 0.003;

			RotateFreeSurfaceCamera(yawDelta, pitDelta);
		}

		public void RotateFreeSurfaceCamera(double yawDelta, double pitchDelta)
		{
			FreeSurfaceYaw		+= yawDelta;
			FreeSurfacePitch	-= pitchDelta;

			ViewToPointPitch = DMathUtil.Clamp(ViewToPointPitch, -Math.PI / 2.01, 0.0);
		}


		public void MoveFreeSurfaceCamera(DVector3 direction)
		{
			velocityDirection = direction;
		}



		public void Update(GameTime gameTime)
		{
			UpdateProjectionMatrix();

			CameraPosition	= DVector3.Transform(new DVector3(0, 0, CameraDistance), Rotation);
			ViewMatrix		= DMatrix.LookAtRH(CameraPosition, DVector3.Zero, DVector3.UnitY);

			ViewMatrixWithTranslation		= ViewMatrix;
			ViewMatrix.TranslationVector	= DVector3.Zero;
			FinalCamPosition				= CameraPosition;


			if (CameraState == CameraStates.ViewToPoint) {
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
			} else if (CameraState == CameraStates.FreeSurface) {
				var mat = CalculateBasisOnSurface();
				
				#region Input
					// Update surface camera yaw and pitch
					//if (input.IsKeyDown(Keys.RightButton)) {
					//	FreeSurfaceYaw		+= input.RelativeMouseOffset.X*0.0003;
					//	FreeSurfacePitch	-= input.RelativeMouseOffset.Y*0.0003;
					//
					//	input.IsMouseCentered	= false;
					//	input.IsMouseHidden		= true;
					//}
					//else {
					//	input.IsMouseCentered	= false;
					//	input.IsMouseHidden		= false;
					//}


					//if (Game.Keyboard.IsKeyDown(Input.Keys.Left))		FreeSurfaceYaw		-= gameTime.ElapsedSec * 0.7;
					//if (Game.Keyboard.IsKeyDown(Input.Keys.Right))	FreeSurfaceYaw		+= gameTime.ElapsedSec * 0.7;
					//if (Game.Keyboard.IsKeyDown(Input.Keys.Up))		FreeSurfacePitch	-= gameTime.ElapsedSec * 0.7;
					//if (Game.Keyboard.IsKeyDown(Input.Keys.Down))		FreeSurfacePitch	+= gameTime.ElapsedSec * 0.7;


					//FreeSurfaceYaw = DMathUtil.Clamp(FreeSurfaceYaw, -DMathUtil.PiOverTwo, DMathUtil.PiOverTwo);
					if (FreeSurfaceYaw > DMathUtil.TwoPi)	FreeSurfaceYaw -= DMathUtil.TwoPi;
					if (FreeSurfaceYaw < -DMathUtil.TwoPi)	FreeSurfaceYaw += DMathUtil.TwoPi;

					// Calculate free cam rotation matrix

					if (!freezeFreeCamRotation)
						freeSurfaceRotation = DQuaternion.RotationAxis(DVector3.UnitY, FreeSurfaceYaw) * DQuaternion.RotationAxis(DVector3.UnitX, FreeSurfacePitch);

					var quat	= freeSurfaceRotation;
					var qRot	= DMatrix.RotationQuaternion(quat);
					var matrix	= qRot * mat;

					var velDir = matrix.Forward * velocityDirection.X + mat.Up * velocityDirection.Y + matrix.Right * velocityDirection.Z;

					if (velDir.Length() != 0) {
						velDir.Normalize();
					}
				
				#endregion

				double fac = ((CameraDistance - EarthRadius) - Parameters.MinDistVelocityThreshold) / (Parameters.MaxDistVelocityThreshold - Parameters.MinDistVelocityThreshold);
				fac = DMathUtil.Clamp(fac, 0.0, 1.0);

				FreeSurfaceVelocityMagnitude = DMathUtil.Lerp(Parameters.MinVelocityFreeSurfCam, Parameters.MaxVelocityFreeSurfCam, fac);

				// Update camera position
				FinalCamPosition	= FreeSurfacePosition = FreeSurfacePosition + velDir * FreeSurfaceVelocityMagnitude * gameTime.ElapsedSec;
				CameraPosition		= FinalCamPosition;


				velocityDirection = DVector3.Zero;


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


		void UpdateProjectionMatrix()
		{
			double aspect = Viewport.AspectRatio;

			double nearHeight	= Parameters.FrustumZNear * Math.Tan(DMathUtil.DegreesToRadians(Parameters.CameraFovDegrees / 2));
			double nearWidth	= nearHeight * aspect;

			Parameters.FrustumWidth		= nearWidth;
			Parameters.FrustumHeight	= nearHeight;

			ProjMatrix = DMatrix.PerspectiveOffCenterRH(-nearWidth / 2, nearWidth / 2, -nearHeight / 2, nearHeight / 2, Parameters.FrustumZNear, Parameters.FrustumZFar);
		}



		public void CameraZoom(float delta)
		{
			CameraDistance += (CameraDistance - EarthRadius) * delta;
		}


		public bool ScreenToSpherical(float x, float y, out DVector2 lonLat)
		{
			DVector3 near, far;
			GetRayFromScreenPoint(x, y, out near, out far);

			lonLat = DVector2.Zero;

			DVector3[] res;
			if (GeoHelper.LineIntersection(near, far, EarthRadius, out res)) {
				GeoHelper.CartesianToSpherical(res[0], out lonLat.X, out lonLat.Y);
				return true;
			}

			return false;
		}


		public void GetRayFromScreenPoint(float x, float y, out DVector3 near, out DVector3 far)
		{
			var w = Viewport.Width;
			var h = Viewport.Height;

			var nearPoint	= new DVector3(x, y, Parameters.FrustumZNear);
			var farPoint	= new DVector3(x, y, Parameters.FrustumZFar);
			
			var vm	= ViewMatrixWithTranslation;
			var mVP = vm * ProjMatrix;

			near	= DVector3.Unproject(nearPoint, 0, 0, w, h, Parameters.FrustumZNear, Parameters.FrustumZFar, mVP);
			far		= DVector3.Unproject(farPoint,	0, 0, w, h, Parameters.FrustumZNear, Parameters.FrustumZFar, mVP);
		}


		public Vector2 CartesianToScreen(DVector3 cartPos)
		{
			var p = DVector3.Project(cartPos, Viewport.X, Viewport.Y, Viewport.Width, Viewport.Height, Parameters.FrustumZNear, Parameters.FrustumZFar, ViewMatrixWithTranslation * ProjMatrix);

			return new Vector2((float)p.X, (float)p.Y);
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

		class SurfaceCameraState
		{
			public enum TransitionType : uint
			{
				SmoothStep	= 0,
				Linear		= 1
			}

			public string	Id;

			public DVector3	FreeSurfacePosition;
			public double	FreeSurfaceYaw;
			public double	FreeSurfacePitch;
			
			public float	WaitTime;
			public float	TransitionTime;

			public DQuaternion FreeRotation;
			public TransitionType Transition;
		}

		private string GenerateId() {
			Guid g = Guid.NewGuid();
			string GuidString = Convert.ToBase64String(g.ToByteArray());
			GuidString = GuidString.Replace("=", "");
			GuidString = GuidString.Replace("+", "");

			return GuidString;
		}

		List<SurfaceCameraState> cameraAnimTrackStates;
		List<SurfaceCameraState> cameraStates;
		int		curStateInd = 0;
		float	curTime		= 0;


		public void SaveCurrentStateToFile(string fileName = "cameraAnimation.txt", string stateName = "")
		{
			// Id FreeCameraPosition FreeSurfaceYaw FreeSurfacePitch WaitTime TransitionTime
			File.AppendAllText(fileName,
				(stateName == "" ? GenerateId() : stateName) + "\t"
					+ FreeSurfacePosition.X + "\t" + FreeSurfacePosition.Y + "\t" + FreeSurfacePosition.Z + "\t"
					+ FreeSurfaceYaw + "\t" + FreeSurfacePitch + "\t" + (uint)SurfaceCameraState.TransitionType.SmoothStep + "\t1\t1\n"
				);
		}

		public void LoadAnimation(string fileName = "cameraAnimation.txt")
		{
			var states = LoadCameraStatesFromFile(fileName);

			if (states == null) {
				Log.Error("File: " + fileName + " not found");
				return;
			}

			cameraAnimTrackStates = states;
			ResetAnimation();
		}

		public void LoadCameraStates(string fileName = "cameraStates.txt")
		{
			var states = LoadCameraStatesFromFile(fileName);

			if (states == null) {
				Log.Error("File: " + fileName + " not found");
				return;
			}

			cameraStates = states;
		}


		List<SurfaceCameraState> LoadCameraStatesFromFile(string fileName)
		{
			if (!File.Exists(fileName)) return null;

			var states = new List<SurfaceCameraState>();
			var lines = File.ReadAllLines(fileName);

			for (int i = 0; i < lines.Length; i++) {
				var line = lines[i];
				
				var s = line.Split(new char[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);

				var curState = new SurfaceCameraState();

				curState.Id						= s[0];
				curState.FreeSurfacePosition	= new DVector3(double.Parse(s[1]), double.Parse(s[2]), double.Parse(s[3]));
				curState.FreeSurfaceYaw			= double.Parse(s[4]);
				curState.FreeSurfacePitch		= double.Parse(s[5]);
				curState.Transition				= (SurfaceCameraState.TransitionType) uint.Parse(s[6]);
				curState.WaitTime				= float.Parse(s[7]);
				curState.TransitionTime			= float.Parse(s[8]);

				curState.FreeRotation = DQuaternion.RotationAxis(DVector3.UnitY, curState.FreeSurfaceYaw) * DQuaternion.RotationAxis(DVector3.UnitX, curState.FreeSurfacePitch);

				states.Add(curState);
			}

			return states;
		}


		public void PlayAnimation(GameTime gameTime)
		{
			if(cameraAnimTrackStates == null) return;

			freezeFreeCamRotation = true;

			var state = cameraAnimTrackStates[curStateInd];


			curTime += gameTime.ElapsedSec; // 0.016f; //
			
			if (curTime < state.WaitTime || curStateInd >= cameraAnimTrackStates.Count - 1) {
				SetState(state);

				if (curStateInd >= cameraAnimTrackStates.Count - 1)
					StopAnimation();

				return;
			}

			float time		= curTime - state.WaitTime;
			float amount	= time/state.TransitionTime;


			var nextState = cameraAnimTrackStates[curStateInd + 1];


			float factor = 0;

			switch (nextState.Transition) {
				case SurfaceCameraState.TransitionType.SmoothStep:
					factor = MathUtil.SmoothStep(amount);
					break;
				case SurfaceCameraState.TransitionType.Linear:
					factor = amount;
					break;
			};

			factor = MathUtil.Clamp(factor, 0.0f, 1.0f);

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
			cameraAnimTrackStates	= null;
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

		public void SetState(string name)
		{
			if (cameraStates == null) return;

			var state = cameraStates.FirstOrDefault(x => x.Id == name);
			if(state != null) SetState(state);
			else Log.Warning("No such camera state found: " + name);
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
