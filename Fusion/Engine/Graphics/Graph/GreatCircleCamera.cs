using System;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.Graph
{
	public class GreatCircleCamera : Camera
	{
		Vector3 up; // points up

		Quaternion quat = Quaternion.Identity;

		public float Altitude { set; get; }
		public float MaxAltitude { get; set; }
		public float MinAltitude { get; set; }

		public float FreeCamFov { get; set; }
		public float FreeCamZNear { get; set; }
		public float FreeCamZFar { get; set; }


		public Vector3 CenterOfOrbit		{ set; get; }
		public Vector3 TargetCenterOfOrbit	{ set; get; }

		public GreatCircleCamera()
		{
			up	= new Vector3();

			MaxAltitude = 50000;
			MinAltitude = 50;


			Altitude = 3100.0f;

			up.X = up.Y = 0;
			up.Z = 1;


			FreeCamFov = 70.0f;
			FreeCamZNear = 0.1f;
			FreeCamZFar = 60000;
		}


		public void Update(GameTime gameTime)
		{
			CenterOfOrbit = Vector3.Lerp(CenterOfOrbit, TargetCenterOfOrbit, gameTime.ElapsedSec);

			var pos = new Vector3(Altitude, 0, 0);
			var newUp = Vector3.Transform(up, quat);
			pos = Vector3.Transform(pos, quat);

			var cameraLocation = CenterOfOrbit + pos;
			float height = Game.Instance.RenderSystem.DisplayBounds.Height;
			float width = Game.Instance.RenderSystem.DisplayBounds.Width;

			SetupCameraFov(cameraLocation, CenterOfOrbit, newUp, FreeCamFov, FreeCamZNear, FreeCamZFar, 1.0f, 1.0f, (float) width / height);		
		}


		public void RotateCamera(Vector2 mouseDelata)
		{
			quat = quat * Quaternion.RotationYawPitchRoll(-mouseDelata.Y / 300.0f, 0, -mouseDelata.X / 300.0f);
			quat.Normalize();
		}


		public void Zoom(float scaleAmount)
		{
			Altitude += Altitude * scaleAmount;

			if (Altitude > MaxAltitude) Altitude = MaxAltitude;
			if (Altitude < MinAltitude) Altitude = MinAltitude;
		}

		public void MoveCamera(Vector2 mouseOffset)
		{
			TargetCenterOfOrbit  += new Vector3(0, -mouseOffset.X, mouseOffset.Y);
		}

	}
}
