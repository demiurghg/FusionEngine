#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Leap;

namespace Fusion.Engine.Input
{
	public class LeapMotion : GameModule
	{
#region LeapMotion classes wrappers
			public class LeapArm
			{
				public Vector3 ElbowPosition { internal set; get; }
				public Vector3 WristPosition { internal set; get; }

				internal LeapArm(Arm arm)
				{
					ElbowPosition = arm.ElbowPosition.ToVector3();
					WristPosition = arm.WristPosition.ToVector3();
				}
			}

			public class LeapFinger
			{
				internal LeapFinger(Finger f)
				{
					Direction = f.Direction.ToVector3();
					HandId					= f.HandId								;
					Id						= f.Id									;
					IsExtended				= f.IsExtended							;
					Length					= f.Length								;
					StabilizedTipPosition	= f.StabilizedTipPosition.ToVector3()	;
					TimeVisible				= f.TimeVisible							;
					TipPosition				= f.TipPosition.ToVector3()				;
					TipVelocity				= f.TipVelocity.ToVector3()				;
					Type					= (FingerType)((int)f.Type)				;	
					Width					= f.Width								;

					Bones = new List<LeapBone>();
					for (int b = 0; b < 4; b++) {
						Bones.Add(new LeapBone(f.Bone((Bone.BoneType) b)));
					}
				}


				public Vector3		Direction				{ internal set; get; }
				public int			HandId					{ internal set; get; }
				public int			Id						{ internal set; get; }
				public bool			IsExtended				{ internal set; get; }
				public float		Length					{ internal set; get; }
				public Vector3		StabilizedTipPosition	{ internal set; get; }
				public float		TimeVisible				{ internal set; get; }
				public Vector3		TipPosition				{ internal set; get; }
				public Vector3		TipVelocity				{ internal set; get; }
				public FingerType	Type					{ internal set; get; }
				public float		Width					{ internal set; get; }

				public List<LeapBone> Bones { internal set; get; }

				public enum FingerType
				{
					TYPE_UNKNOWN = -1,
					TYPE_THUMB	= 0,
					TYPE_INDEX	= 1,
					TYPE_MIDDLE = 2,
					TYPE_RING	= 3,
					TYPE_PINKY	= 4,
				}
			}

			public class LeapBone
			{
				internal LeapBone(Bone b)
				{
					Basis		= b.Basis.ToMatrix()		;
					Center		= b.Center.ToVector3()		;
					Direction	= b.Direction.ToVector3()	;
					Length		= b.Length					;
					NextJoint	= b.NextJoint.ToVector3()	;
					PrevJoint	= b.PrevJoint.ToVector3()	;
					Rotation	= b.Rotation.ToQuaternion()	;
					Type		= (BoneType)((int)b.Type)	;
					Width		= b.Width;
				}

				public Matrix		Basis		{ internal set; get; }
				public Vector3		Center		{ internal set; get; }
				public Vector3		Direction	{ internal set; get; }
				public float		Length		{ internal set; get; }
				public Vector3		NextJoint	{ internal set; get; }
				public Vector3		PrevJoint	{ internal set; get; }
				public Quaternion	Rotation	{ internal set; get; }
				public BoneType		Type		{ internal set; get; }
				public float		Width		{ internal set; get; }

				public enum BoneType
				{
					TYPE_INVALID = -1,
					TYPE_METACARPAL		= 0,
					TYPE_PROXIMAL		= 1,
					TYPE_INTERMEDIATE	= 2,
					TYPE_DISTAL			= 3,
				}
			}

			public class LeapHand
			{
				internal LeapHand(Hand h)
				{
					Arm						= new LeapArm(h.Arm)					;
					Basis					= h.Basis.ToMatrix()					;
					Confidence				= h.Confidence							;
					Direction				= h.Direction.ToVector3()				;
					Fingers					= h.Fingers.Select(x=> new LeapFinger(x)).ToList()   ;
					FrameId					= h.FrameId								;
					GrabAngle				= h.GrabAngle							;
					GrabStrength			= h.GrabStrength						;
					Id						= h.Id									;
					IsLeft					= h.IsLeft								;
					IsRight					= h.IsRight								;
					PalmNormal				= h.PalmNormal.ToVector3()				;
					PalmPosition			= h.PalmPosition.ToVector3()			;
					PalmVelocity			= h.PalmVelocity.ToVector3()			;
					PalmWidth				= h.PalmWidth							;
					PinchDistance			= h.PinchDistance						;
					PinchStrength			= h.PinchStrength						;
					Rotation				= h.Rotation.ToQuaternion()				;
					StabilizedPalmPosition	= h.StabilizedPalmPosition.ToVector3()	;
					TimeVisible				= h.TimeVisible							;
					WristPosition			= h.WristPosition.ToVector3()			;
				}


				public LeapArm			Arm						{ internal set; get; }
				public Matrix			Basis					{ internal set; get; }
				public float			Confidence				{ internal set; get; }
				public Vector3			Direction				{ internal set; get; }
				public List<LeapFinger> Fingers					{ internal set; get; }
				public long				FrameId					{ internal set; get; }
				public float			GrabAngle				{ internal set; get; }
				public float			GrabStrength			{ internal set; get; }
				public int				Id						{ internal set; get; }
				public bool				IsLeft					{ internal set; get; }
				public bool				IsRight					{ internal set; get; }
				public Vector3			PalmNormal				{ internal set; get; }
				public Vector3			PalmPosition			{ internal set; get; }
				public Vector3			PalmVelocity			{ internal set; get; }
				public float			PalmWidth				{ internal set; get; }
				public float			PinchDistance			{ internal set; get; }
				public float			PinchStrength			{ internal set; get; }
				public Quaternion		Rotation				{ internal set; get; }
				public Vector3			StabilizedPalmPosition	{ internal set; get; }
				public float			TimeVisible				{ internal set; get; }
				public Vector3			WristPosition			{ internal set; get; }
			}

			public class LeapFrame
			{
				public float			CurrentFps		{ internal set; get; }
				public List<LeapHand>	Hands			{ get; internal set; }
				public long				Id				{ internal set; get; }
				public BoundingBox		InteractionBox	{ internal set; get; }
				public long				Timestamp		{ internal set; get; }

				internal LeapFrame(Leap.Frame frame)
				{
					Id			= frame.Id;
					CurrentFps	= frame.CurrentFramesPerSecond;
					Timestamp	= frame.Timestamp;

					Hands		= frame.Hands.Select(x => new LeapHand(x)).ToList();

					var c = frame.InteractionBox.Center.ToVector3();
					var ib = frame.InteractionBox;
					InteractionBox = new BoundingBox(new Vector3(c.X - ib.Width / 2.0f, c.Y - ib.Height / 2.0f, c.Z - ib.Depth / 2.0f), new Vector3(c.X + ib.Width / 2.0f, c.Y + ib.Height / 2.0f, c.Z + ib.Depth / 2.0f));
				}
			}
		#endregion

		private Leap.IController	controller;
		public	LeapFrame			Frame { private set; get; }

		private Frame frame;


		public bool IsConnected = false;

		public LeapMotion(Game game) : base (game)
		{
			try {
				controller = new Leap.Controller();
				controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_ALLOW_PAUSE_RESUME);
				
				// Set up our listener:
				controller.Connect += OnServiceConnect;
				controller.Disconnect += OnServiceDisconnect;
				controller.FrameReady += OnFrame;
				controller.Device += OnConnect;
				controller.DeviceLost += OnDisconnect;
				controller.DeviceFailure += OnDeviceFailure;
				controller.LogMessage += OnLogMessage;

				IsConnected = true;
			}
			catch (Exception e) {
				Log.Error(e.Message);
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize()
		{
		}



		public LeapFrame GetTransformedFrame(Matrix transform)
		{
			if (frame == null) return null;

			Quaternion rot;
			Vector3 translation, scale;

			transform.Decompose(out scale, out rot, out translation);

			//rot.Invert(); rot.Normalize();

			return new LeapFrame(frame.TransformedCopy(
				new LeapTransform(new Vector(translation.X, translation.Y, translation.Z), 
				new LeapQuaternion(rot.X, rot.Y, rot.Z, rot.W), 
				new Vector(scale.X, scale.Y, scale.Z)))
			);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && IsConnected) {
				controller.Connect			-= OnServiceConnect;
				controller.Disconnect		-= OnServiceDisconnect;
				controller.FrameReady		-= OnFrame;
				controller.Device			-= OnConnect;
				controller.DeviceLost		-= OnDisconnect;
				controller.DeviceFailure	-= OnDeviceFailure;
				controller.LogMessage		-= OnLogMessage;

				controller.Dispose();
			}

			base.Dispose(disposing);
		}



		public void OnInit(Controller controller)
		{
			Console.WriteLine("Initialized");
		}

		public void OnConnect(object sender, DeviceEventArgs args)
		{
			Console.WriteLine("Connected");
		}

		public void OnDisconnect(object sender, DeviceEventArgs args)
		{
			Console.WriteLine("Disconnected");
		}

		public void OnFrame(object sender, FrameEventArgs args)
		{
			// Get the most recent frame and report some basic information
			frame = args.frame.TransformedCopy(new LeapTransform(Vector.Zero, LeapQuaternion.Identity, new Vector(0.01f, 0.01f, 0.01f)));
			Frame = new LeapFrame(frame);
#if false
			Console.WriteLine(
			  "Frame id: {0}, timestamp: {1}, hands: {2}",
			  frame.Id, frame.Timestamp, frame.Hands.Count
			);

			foreach (Hand hand in frame.Hands)
			{
				Console.WriteLine("  Hand id: {0}, palm position: {1}, fingers: {2}",
				  hand.Id, hand.PalmPosition, hand.Fingers.Count);
				// Get the hand's normal vector and direction
				Vector normal = hand.PalmNormal;
				Vector direction = hand.Direction;

				// Calculate the hand's pitch, roll, and yaw angles
				Console.WriteLine(
				  "  Hand pitch: {0} degrees, roll: {1} degrees, yaw: {2} degrees",
				  direction.Pitch * 180.0f / (float)Math.PI,
				  normal.Roll * 180.0f / (float)Math.PI,
				  direction.Yaw * 180.0f / (float)Math.PI
				);

				// Get the Arm bone
				Arm arm = hand.Arm;
				Console.WriteLine(
				  "  Arm direction: {0}, wrist position: {1}, elbow position: {2}",
				  arm.Direction, arm.WristPosition, arm.ElbowPosition
				);

				// Get fingers
				foreach (Finger finger in hand.Fingers)
				{
					Console.WriteLine(
					  "    Finger id: {0}, {1}, length: {2}mm, width: {3}mm",
					  finger.Id,
					  finger.Type.ToString(),
					  finger.Length,
					  finger.Width
					);

					// Get finger bones
					Bone bone;
					for (int b = 0; b < 4; b++)
					{
						bone = finger.Bone((Bone.BoneType)b);
						Console.WriteLine(
						  "      Bone: {0}, start: {1}, end: {2}, direction: {3}",
						  bone.Type, bone.PrevJoint, bone.NextJoint, bone.Direction
						);
					}
				}
			}

			if (frame.Hands.Count != 0)
			{
				Console.WriteLine("");
			}
#endif
		}

		public void OnServiceConnect(object sender, ConnectionEventArgs args)
		{
			Console.WriteLine("Service Connected");
		}

		public void OnServiceDisconnect(object sender, ConnectionLostEventArgs args)
		{
			Console.WriteLine("Service Disconnected");
		}

		public void OnServiceChange(Controller controller)
		{
			Console.WriteLine("Service Changed");
		}

		public void OnDeviceFailure(object sender, DeviceFailureEventArgs args)
		{
			Console.WriteLine("Device Error");
			Console.WriteLine("  PNP ID:" + args.DeviceSerialNumber);
			Console.WriteLine("  Failure message:" + args.ErrorMessage);
		}

		public void OnLogMessage(object sender, LogEventArgs args)
		{
			switch (args.severity)
			{
				case Leap.MessageSeverity.MESSAGE_CRITICAL:
					Console.WriteLine("[Critical]");
					break;
				case Leap.MessageSeverity.MESSAGE_WARNING:
					Console.WriteLine("[Warning]");
					break;
				case Leap.MessageSeverity.MESSAGE_INFORMATION:
					Console.WriteLine("[Info]");
					break;
				case Leap.MessageSeverity.MESSAGE_UNKNOWN:
					Console.WriteLine("[Unknown]");
					break;
			}
			Console.WriteLine("[{0}] {1}", args.timestamp, args.message);
		}

	}

	static class LeapMathExtention
	{
		public static Vector3 ToVector3(this Vector vec)
		{
			return new Vector3(vec.x, vec.y, vec.z);
		}

		public static Quaternion ToQuaternion(this LeapQuaternion q)
		{
			return new Quaternion(q.x, q.y, q.z, q.w);
		}

		public static Matrix ToMatrix(this LeapTransform t)
		{
			//float[] f = new[] { t.xBasis.x, t.xBasis.y, t.xBasis.z, 0.0f, 
			//					t.yBasis.x, t.yBasis.y, t.yBasis.z, 0.0f,
			//					t.zBasis.x, t.zBasis.y, t.zBasis.z, 0.0f,
			//					t.translation.x, t.translation.y, t.translation.z, 1.0f
			//				};
			//return new Matrix(f);

			return Matrix.Scaling(t.scale.ToVector3()) * Matrix.RotationQuaternion(t.rotation.ToQuaternion()) * Matrix.Translation(t.translation.ToVector3());
		}
	}

}

#endif
