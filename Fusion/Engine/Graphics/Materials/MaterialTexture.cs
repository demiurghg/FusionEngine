using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// 
	/// </summary>
	public class MaterialTexture {
		
		/// <summary>
		/// Path to textyre
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Scale along U-axis
		/// </summary>
		public float ScaleU { get; set; }

		/// <summary>
		/// Scale along V-axis
		/// </summary>
		public float ScaleV { get; set; }

		/// <summary>
		/// Offset along V-axis
		/// </summary>
		public float OffsetU { get; set; }

		/// <summary>
		/// Offset along V-axis
		/// </summary>
		public float OffsetV { get; set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="defaultPath"></param>
		public MaterialTexture ( string defaultPath )
		{
			Path		=	defaultPath;
			ScaleU		=	1;
			ScaleV		=	1;
			OffsetU		=	0;
			OffsetV		=	0;
		}
	}
}
