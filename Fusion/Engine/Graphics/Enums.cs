using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics {


	public enum VertexFormat {
		Sprite,
		LitRigid,
		ShadowRigid,
		LitSkinned,
		ShadowSkinned,
	}


	/// <summary>
	/// Defines stereo mode
	/// </summary>
	public enum StereoMode {
		Disabled,	///	No stereo
		NV3DVision,	///	NVidia 3DVision stereo
		DualHead,	///	Dual-head projector stereo
		OculusRift,	///	Oculus Rift
		Interlaced,	///	Interlaced mode for TVs
	}


	/// <summary>
	/// Defines interlacing mode for interlaced stereo
	/// </summary>
	public enum InterlacingMode {
		VerticalLR,
		VerticalRL,
		HorizontalLR,
		HorizontalRL,
	}


	/// <summary>
	/// Defines set of applied to instance surface effects.
	/// No XRay - replace material with narrowed glow.
	/// </summary>
	public enum InstanceFX {
		None,
		Wet,
		Frozen,
		Selection,
	}


	/// <summary>
	/// Sprite layer filtering mode.
	/// </summary>
	public enum SpriteFilterMode {
		/// <summary>
		/// Approximate filtering with clamping. Low quality.
		/// </summary>
		PointClamp,

		/// <summary>
		/// Approximate filtering with wrapping. Low quality.
		/// </summary>
		PointWrap,

		/// <summary>
		/// Linear filtering with clamping. Normal quality.
		/// </summary>
		LinearClamp,

		/// <summary>
		/// Linear filtering with wrapping. Normal quality.
		/// </summary>
		LinearWrap,

		/// <summary>
		/// Anisotropic filtering with clamping. High quality.
		/// </summary>
		AnisotropicClamp,

		/// <summary>
		/// Anisotropic filtering with wrapping. High quality.
		/// </summary>
		AnisotropicWrap,
	}


	/// <summary>
	/// Sprite blend mode.
	/// </summary>
	public enum SpriteBlendMode {

		/// <summary>
		/// Opaque
		/// </summary>
		Opaque,

		/// <summary>
		/// Alpha blending
		/// </summary>
		AlphaBlend,

		/// <summary>
		/// Alpha blending with premultiplied color
		/// </summary>
		AlphaBlendPremul,

		/// <summary>
		/// Additive blending
		/// </summary>
		Additive,
		
		/// <summary>
		/// Similar to Photoshop's 'Screen' blend mode.
		/// </summary>
		Screen,

		/// <summary>
		/// Multiply
		/// </summary>
		Multiply,

		/// <summary>
		/// Inverse multiply
		/// </summary>
		NegMultiply,
	}


	/// <summary>
	/// Stereo filter for sprites.
	/// </summary>
	public enum SpriteStereoMode {

		/// <summary>
		/// Sprite layer will be rendered for left and right eyes.
		/// </summary>
		All,		

		/// <summary>
		/// Sprite layer will be rendered only for left eye.
		/// Required to render stereo content like stereo-photos and stereo-movies.
		/// </summary>
		Left,

		/// <summary>
		/// Sprite layer will be rendered only for right eye.
		/// Required to render stereo content like stereo-photos and stereo-movies.
		/// </summary>
		Right,
	}
}
