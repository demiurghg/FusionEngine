using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Content;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Base material with color texture, surface texture, normal map and emission texture.
	/// Material has dirt texture and optional detail map.
	/// Emission OR detail map could be modulated by color texture alpha.
	/// </summary>
	public class BaseIllum {

		public float ColorLevel { get; set; }
		public float SpecularLevel { get; set; }
		public float EmissionLevel { get; set; }
		public float RoughnessMinimum { get; set; }
		public float RoughnessMaximum { get; set; }

		public TextureMap ColorTexture { get; set; }
		public TextureMap SurfaceTexture { get; set; }
		public TextureMap NormalMapTexture { get; set; }
		public TextureMap EmissionTexture { get; set; }

		public TextureMap DirtTexture { get; set; }
		public TextureMap DetailTexture { get; set; }


		/// <summary>
		/// Constructor
		/// </summary>
		public BaseIllum()
		{
			ColorLevel			=	1;
			SpecularLevel		=	1;
			EmissionLevel		=	1;
			RoughnessMinimum	=	0.05f;
			RoughnessMaximum	=	1.00f;

			ColorTexture		=	new TextureMap( "defaultColor"  , true );
			SurfaceTexture		=	new TextureMap( "defaultMatte"  , false );
			NormalMapTexture	=	new TextureMap( "defaultNormals", false );
			EmissionTexture		=	new TextureMap( "defaultBlack"  , true );
			DirtTexture			=	new TextureMap( "defaultDirt"   , true );
			DetailTexture		=	new TextureMap( "defaultDetail" , true );
		}



		/// <summary>
		/// Creates gpu material
		/// </summary>
		/// <param name="rs"></param>
		/// <returns></returns>
		internal Material CreateGpuMaterial ( RenderSystem rs, ContentManager content )
		{
			var data = new MaterialData();
			
			GetMaterialData( ref data );

			var mtrl = new Material( rs, data, GetTextures(content) );

			return mtrl;
		}



		/// <summary>
		/// Updates MaterialData.
		/// </summary>
		/// <param name="data"></param>
		protected virtual void GetMaterialData ( ref MaterialData data )
		{
			data.ColorLevel			=	ColorLevel		;
			data.SpecularLevel		=	SpecularLevel	;
			data.EmissionLevel		=	EmissionLevel	;
			data.RoughnessMinimum	=	RoughnessMinimum;
			data.RoughnessMaximum	=	RoughnessMaximum;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		protected virtual Texture[] GetTextures ( ContentManager content )
		{
			return new Texture[] {
				ColorTexture	.LoadTexture( content, "defaultColor"	),
				SurfaceTexture	.LoadTexture( content, "defaultMatte"	),
				NormalMapTexture.LoadTexture( content, "defaultNormals" ),
				EmissionTexture	.LoadTexture( content, "defaultBlack"	),
				DirtTexture		.LoadTexture( content, "defaultDirt"	),
				DetailTexture	.LoadTexture( content, "defaultDetail"	),
			};
		}



		/// <summary>
		/// Gets all textures and shaders on which this material depends.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetDependencies ()
		{
			var list = GetType().GetProperties()
						.Where( p0 => p0.PropertyType == typeof(TextureMap) )
						.Select( p1 => p1.GetValue(this) as TextureMap )
						.Where( v0 => v0 != null )
						.Select( v1 => v1.Path )
						.Where( s0 => !string.IsNullOrWhiteSpace(s0) )
						.Where( s1 => !s1.StartsWith("*") )
						.Distinct()
						.ToList();

			list.Add("surface.hlsl");

			return list;
		}



		/// <summary>
		/// Exports material to xml
		/// </summary>
		/// <returns></returns>
		public static string ExportToXml ( BaseIllum material )
		{
			return Misc.SaveObjectToXml( material, material.GetType(), Misc.GetAllSubclassedOf( typeof(BaseIllum) ) );
		}



		/// <summary>
		/// Imports material from xml.
		/// </summary>
		/// <param name="xmlString"></param>
		/// <returns></returns>
		public static BaseIllum ImportFromXml ( string xmlString )
		{
			return (BaseIllum)Misc.LoadObjectFromXml( typeof(BaseIllum), xmlString, Misc.GetAllSubclassedOf( typeof(BaseIllum) ) );
		}

	}
}
