using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Glow color?
	/// </summary>
	public class Particle {

		/// <summary>
		/// Initial position of the particle.
		/// </summary>
		public Vector3 Position;

		/// <summary>
		///	Initial velocity of the particle
		/// </summary>
		public Vector3 Velocity;

		/// <summary>
		/// Acceleation of the particle.
		/// </summary>
		public Vector3 Acceleration;

		/// <summary>
		/// Total particle life time.
		/// </summary>
		public float LifeTime;

		/// <summary>
		/// Lag between desired particle injection time and actual 
		/// particle injection time caused by discrete updating.
		/// </summary>
		public float TimeLag;

		/// <summary>
		/// Gravity influence.
		/// Zero means no gravity influence.
		/// Values between 0 and 1 means reduced gravity, like fluffs.
		/// Negative values means particle that have positive buoyancy.
		/// </summary>
		public float Gravity;

		/// <summary>
		/// Fade in time fraction
		/// </summary>
		public float FadeIn;

		/// <summary>
		/// Fade out time fraction
		/// </summary>
		public float FadeOut;

		/// <summary>
		/// Initial color and terminal color.
		/// </summary>
		public Color4 FadedColor;

		/// <summary>
		/// Color while particle is fully bright.
		/// </summary>
		public Color4 BrightColor;

		/// <summary>
		/// Initial particle size
		/// </summary>
		public float InitialSize;
		
		/// <summary>
		///	Terminal particle size
		/// </summary>
		public float TerminalSize;

		/// <summary>
		/// Initial particle rotation
		/// </summary>
		public float InitialRotation;
		
		/// <summary>
		/// Terminal particle rotation
		/// </summary>
		public float TerminalRotation;

		
		/// <summary>
		/// Texture coodinates in texture atlas.
		/// </summary>
		public RectangleF AtlasCoordinates;
	}
}
