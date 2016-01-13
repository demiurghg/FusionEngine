using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using System.ComponentModel;


namespace Fusion.Engine.Graphics {
	
	public enum AlphaUsage {
		None,
		EmissionMask,
		DetailMask,
	}


	/// <summary>
	/// Base material with color texture, surface texture, normal map and emission texture.
	/// Material has dirt texture and optional detail map.
	/// Emission OR detail map could be modulated by color texture alpha.
	/// </summary>
	public class BaseIllum {

		[Category("Parameters")]
		public float ColorLevel { get; set; }
		[Category("Parameters")]
		public float SpecularLevel { get; set; }
		[Category("Parameters")]
		public float EmissionLevel { get; set; }
		[Category("Parameters")]
		public float RoughnessMinimum { get; set; }
		[Category("Parameters")]
		public float RoughnessMaximum { get; set; }
		[Category("Parameters")]
		public float DirtLevel { get; set; }
		
		[Category("Generation")]
		public AlphaUsage AlphaUsage { get; set; }

		[Category("Texture Maps")]
		public TextureMap ColorTexture { get; set; }
		[Category("Texture Maps")]
		public TextureMap SurfaceTexture { get; set; }
		[Category("Texture Maps")]
		public TextureMap NormalMapTexture { get; set; }
		[Category("Texture Maps")]
		public TextureMap EmissionTexture { get; set; }
		[Category("Texture Maps")]
		public TextureMap DirtTexture { get; set; }
		[Category("Texture Maps")]
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
			DirtLevel			=	0.0f;
			AlphaUsage			=	AlphaUsage.None;

			ColorTexture		=	new TextureMap("defaultColor"	, true );
			SurfaceTexture		=	new TextureMap("defaultMatte"	, false);
			NormalMapTexture	=	new TextureMap("defaultNormals"	, false);
			EmissionTexture		=	new TextureMap("defaultBlack"	, true );
			DirtTexture			=	new TextureMap("defaultDirt"	, true );
			DetailTexture		=	new TextureMap("defaultDetail"	, false);
		}



		/// <summary>
		/// Creates gpu material
		/// </summary>
		/// <param name="rs"></param>
		/// <returns></returns>
		internal MaterialInstance CreateMaterialInstance ( RenderSystem rs, ContentManager content )
		{
			var data = new MaterialData();
			
			GetMaterialData( ref data );

			var mtrl = new MaterialInstance( rs, content, data, GetTextureBindings(), GetSurfaceFlags() );

			return mtrl;
		}


		protected virtual SurfaceFlags GetSurfaceFlags ()
		{
			SurfaceFlags flags = SurfaceFlags.BASE_ILLUM;
			
			switch (AlphaUsage) {
				case AlphaUsage.EmissionMask : flags |= SurfaceFlags.ALPHA_EMISSION_MASK; break;
				case AlphaUsage.DetailMask   : flags |= SurfaceFlags.ALPHA_DETAIL_MASK; break;
			}	

			return flags;
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
			data.DirtLevel			=	DirtLevel;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		protected virtual TextureMapBind[] GetTextureBindings ()
		{
			return new TextureMapBind[] {
				new TextureMapBind( ColorTexture	, "defaultColor"	),
				new TextureMapBind( SurfaceTexture	, "defaultMatte"	),
				new TextureMapBind( NormalMapTexture, "defaultNormals"	),
				new TextureMapBind( EmissionTexture	, "defaultBlack"	),
				new TextureMapBind( DirtTexture		, "defaultDirt"		),
				new TextureMapBind( DetailTexture	, "defaultDetail"	),
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
			return Misc.SaveObjectToXml( material, material.GetType(), Misc.GetAllSubclassesOf( typeof(BaseIllum) ) );
		}



		/// <summary>
		/// Imports material from xml.
		/// </summary>
		/// <param name="xmlString"></param>
		/// <returns></returns>
		public static BaseIllum ImportFromXml ( string xmlString )
		{
			return (BaseIllum)Misc.LoadObjectFromXml( typeof(BaseIllum), xmlString, Misc.GetAllSubclassesOf( typeof(BaseIllum) ) );
		}

	}
}
