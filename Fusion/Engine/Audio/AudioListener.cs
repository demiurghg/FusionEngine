using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Audio
{
	public sealed class AudioListener
	{
		public AudioListener ()
		{
			Forward = Vector3.ForwardRH;
			Position = Vector3.Zero;
			Up = Vector3.Up;
			Velocity = Vector3.Zero;
		}
		
		public Vector3 Forward {
			get;
			set;
		}

		public Vector3 Position {
			get;
			set;
		}

		public Vector3 Up {
			get;
			set;
		}

		public Vector3 Velocity {
			get;
			set;
		}
	}
}

