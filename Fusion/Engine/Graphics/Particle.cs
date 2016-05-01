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
	/// Defines particle behavior.
	/// </summary>
	[Flags]
	public enum ParticleFX : uint {
		/// <summary>
		/// Default value, no special effects.
		/// </summary>
		None	=	0x0000,

		/// <summary>
		/// Particle is aimed billboard with defined tail position
		/// </summary>
		Beam	=	0x0001,

		/// <summary>
		/// Particles is lit. By default all particles are emissive.
		/// </summary>
		Lit		=	0x0002,

		/// <summary>
		/// Particles should cast a shadow
		/// </summary>
		Shadow	=	0x0004,
	}

	/// <summary>
	/// Particle structure
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size=144)]
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
		/// Particle tail position
		/// </summary>
		[FieldOffset( 36)] public Vector3	TailPosition;

		/// <summary>
		/// "Faded-out" color of the particle.
		/// </summary>
		[FieldOffset( 48)] public Color4	Color0;                

		/// <summary>
		/// "Faded-in" color of the particle.
		/// </summary>
		[FieldOffset( 64)] public Color4	Color1;                

		/// <summary>
		/// "Faded-in" color of the particle.
		/// </summary>
		[FieldOffset( 80)] public Color4	LightLevel;                

		/// <summary>
		/// Gravity influence.
		/// Zero means no gravity influence.
		/// Values between 0 and 1 means reduced gravity, like snowflakes or dust.
		/// Negative values means particle that has positive buoyancy.
		/// </summary>
		[FieldOffset( 96)] public float		Gravity;                

		/// <summary>
		/// NOT USED!
		/// </summary>
		[FieldOffset(100)] public float		Damping;                

		/// <summary>
		/// Initial size of the particle
		/// </summary>
		[FieldOffset(104)] public float		Size0;                  

		/// <summary>
		/// Terminal size of the particle
		/// </summary>
		[FieldOffset(108)] public float		Size1;                  

		/// <summary>
		/// Initial rotation of the particle
		/// </summary>
		[FieldOffset(112)] public float		Rotation0;                 

		/// <summary>
		/// Terminal rotation of the particle
		/// </summary>
		[FieldOffset(116)] public float		Rotation1;                 

		/// <summary>
		/// Total particle life-time
		/// </summary>
		[FieldOffset(120)] public float		LifeTime;          

		/// <summary>
		/// Lag between desired particle injection time and actual 
		/// particle injection time caused by discrete updating.
		/// Internally this field used as particle life-time counter.
		/// </summary>
		[FieldOffset(124)] public float		TimeLag;	           

		/// <summary>
		/// Fade in time fraction
		/// </summary>
		[FieldOffset(128)] public float		FadeIn;                

		/// <summary>
		/// Fade out time fraction
		/// </summary>
		[FieldOffset(132)] public float		FadeOut;               

		/// <summary>
		/// Index of the image in the texture atlas
		/// </summary>
		[FieldOffset(136)] public int		ImageIndex;            

		/// <summary>
		/// Index of the image in the texture atlas
		/// </summary>
		[FieldOffset(140)] public ParticleFX	Effects;            
	}
}
