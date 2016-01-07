using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Content;
using System.Xml.Serialization;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// 
	/// </summary>
	public class TextureMap {
		
		/// <summary>
		/// Path to textyre
		/// </summary>
		[XmlAttribute]
		public string Path { get; set; }

		/// <summary>
		/// Indicated that texture should be sRGB
		/// </summary>
		[XmlAttribute]
		public bool SRgb { get; set; }

		/// <summary>
		/// Scale along U-axis
		/// </summary>
		[XmlAttribute]
		public float ScaleU { get; set; }

		/// <summary>
		/// Scale along V-axis
		/// </summary>
		[XmlAttribute]
		public float ScaleV { get; set; }

		/// <summary>
		/// Offset along V-axis
		/// </summary>
		[XmlAttribute]
		public float OffsetU { get; set; }

		/// <summary>
		/// Offset along V-axis
		/// </summary>
		[XmlAttribute]
		public float OffsetV { get; set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="defaultPath"></param>
		public TextureMap ()
		{
			Path		=	"defaultColor";
			ScaleU		=	1;
			ScaleV		=	1;
			OffsetU		=	0;
			OffsetV		=	0;
			SRgb		=	false;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="defaultPath"></param>
		public TextureMap (string defaultTexture, bool srgb)
		{
			Path		=	defaultTexture;
			ScaleU		=	1;
			ScaleV		=	1;
			OffsetU		=	0;
			OffsetV		=	0;
			SRgb		=	srgb;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		internal DiscTexture LoadTexture ( ContentManager content, string fallbackPath )
		{
			if (content==null) {
				throw new ArgumentNullException("content");
			}

			if (string.IsNullOrWhiteSpace(fallbackPath)) {
				throw new ArgumentNullException("fallbackPath");
			}

			if (string.IsNullOrWhiteSpace(Path)) {
				return content.Load<DiscTexture>( fallbackPath );
			}

			if (!content.Exists(Path)) {
				return content.Load<DiscTexture>( fallbackPath );
			}

			return content.Load<DiscTexture>( Path + (SRgb ? "|srgb" : "") );
		}
	}
}
