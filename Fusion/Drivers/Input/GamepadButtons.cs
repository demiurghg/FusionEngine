﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.XInput;

namespace Fusion.Input
{
	public enum GamepadButtons {
		Y				= -32768,
		None			= 0,
		DPadUp			= 1,
		DPadDown		= 2,
		DPadLeft		= 4,
		DPadRight		= 8,
		Start			= 16,
		Back			= 32,
		LeftThumb		= 64,
		RightThumb		= 128,
		LeftShoulder	= 256,
		RightShoulder	= 512,
		A				= 4096,
		B				= 8192,
		X				= 16384,
	}
}
