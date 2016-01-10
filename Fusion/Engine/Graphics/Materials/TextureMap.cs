using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Content;
using System.Xml.Serialization;
using System.ComponentModel;
using Fusion.Engine.Graphics.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// 
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public sealed class TextureMap {
		
		/// <summary>
		/// Path to textyre
		/// </summary>
		[XmlAttribute]
		[Editor(typeof(TextureLocationEditor), typeof(UITypeEditor))]
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

		public override string ToString ()
		{
			return Path;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="defaultPath"></param>
		public TextureMap ()
		{
			Path		=	"";
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



	public class TextureMapAttribute : Attribute {

		public readonly int Slot;
		public readonly string Default;
		public readonly bool SRgb;

		public TextureMapAttribute ( int slot, string defaultPath, bool srgb )
		{
			Slot	=	slot;
			Default	=	defaultPath;
			SRgb	=	srgb;
		}
	}
}
