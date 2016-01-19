using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using System.Runtime.InteropServices;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Particle structure
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size=112)]
	public struct Particle {
		
		/// <summary>
		/// Initial position of the particle
		/// </summary>
		[FieldOffset(  0)] public Vector3	Position;              

		/// <summary>
		/// Initial velocity of the particle
		/// </summary>
		[FieldOffset( 12)] public Vector3	Velocity;              

		/// <summary>
		/// Acceleration of the particle regardless of gravity.
		/// </summary>
		[FieldOffset( 24)] public Vector3	Acceleration;          

		/// <summary>
		/// "Faded-out" color of the particle.
		/// </summary>
		[FieldOffset( 36)] public Color4	Color0;                

		/// <summary>
		/// "Faded-in" color of the particle.
		/// </summary>
		[FieldOffset( 52)] public Color4	Color1;                

		/// <summary>
		/// Gravity influence.
		/// Zero means no gravity influence.
		/// Values between 0 and 1 means reduced gravity, like fluffs.
		/// Negative values means particle that have positive buoyancy.
		/// </summary>
		[FieldOffset( 68)] public float		Gravity;                

		/// <summary>
		/// NOT USED!
		/// </summary>
		[FieldOffset( 72)] public float		Damping;                

		/// <summary>
		/// Initial size of the particle
		/// </summary>
		[FieldOffset( 76)] public float		Size0;                  

		/// <summary>
		/// Terminal size of the particle
		/// </summary>
		[FieldOffset( 80)] public float		Size1;                  

		/// <summary>
		/// Initial rotation of the particle
		/// </summary>
		[FieldOffset( 84)] public float		Rotation0;                 

		/// <summary>
		/// Terminal rotation of the particle
		/// </summary>
		[FieldOffset( 88)] public float		Rotation1;                 

		/// <summary>
		/// Total particle life-time
		/// </summary>
		[FieldOffset( 92)] public float		LifeTime;          

		/// <summary>
		/// Lag between desired particle injection time and actual 
		/// particle injection time caused by discrete updating.
		/// Internally this field used as particle life-time counter.
		/// </summary>
		[FieldOffset( 96)] public float		TimeLag;	           

		/// <summary>
		/// Fade in time fraction
		/// </summary>
		[FieldOffset(100)] public float		FadeIn;                

		/// <summary>
		/// Fade out time fraction
		/// </summary>
		[FieldOffset(104)] public float		FadeOut;               

		/// <summary>
		/// Index of the image in the texture atlas
		/// </summary>
		[FieldOffset(108)] public int		ImageIndex;            
	}
}
