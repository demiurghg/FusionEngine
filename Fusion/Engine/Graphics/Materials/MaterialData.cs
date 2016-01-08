using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics {

	[StructLayout(LayoutKind.Explicit, Size=128)]
	public unsafe struct MaterialData {
		[FieldOffset(  0)] public float ColorLevel;
		[FieldOffset(  4)] public float SpecularLevel;
		[FieldOffset(  8)] public float EmissionLevel;
		[FieldOffset( 12)] public float RoughnessMinimum;
		[FieldOffset( 16)] public float RoughnessMaximum;
	}
}
