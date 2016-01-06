using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.IniParser;
using System.IO;
using Fusion.Core;
using Fusion.Core.IniParser.Model;
using Fusion.Core.IniParser.Model.Formatting;
using Fusion.Core.Content;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Reprsents material.
	/// </summary>
	public partial class Material : DisposableBase {

		public const int MaxTextures = 16;
										
		/// <summary>
		/// Defines the way how input textures and parameters will be combined.
		/// </summary>
		public ShaderCombiner Combiner { get; set; }

		/// <summary>
		/// Indicates that object with this material should cast shadow.
		/// </summary>
		public bool CastShadow { get; set; }


		/// <summary>
		/// Color level
		/// </summary>
		public float ColorLevel { get; set; }

		/// <summary>
		/// Gloss level
		/// </summary>
		public float SpecularLevel { get; set; }

		/// <summary>
		/// Color level
		/// </summary>
		public float EmissionLevel { get; set; }

		/// <summary>
		/// Minimum roughness level.
		/// </summary>
		public float RoughnessMinimum { get; set; }

		/// <summary>
		/// Maximum roughness level.
		/// </summary>
		public float RoughnessMaximum { get; set; }



		/// <summary>
		/// Array of texture paths:
		/// </summary>
		readonly MaterialTexture[] textures = new MaterialTexture[MaxTextures];


		/// <summary>
		/// Color texture.
		/// Alpha contains emission, detail or alpha-kill mask.
		/// </summary>
		public MaterialTexture ColorTexture { 
			get { return textures[0]; }				
			set { textures[0] = value; }				
		}


		/// <summary>
		/// Surface texture. Red channel contains specular level.
		/// Green channel contains roughness.
		/// Blue channel contains metallicity.
		/// </summary>
		public MaterialTexture SurfaceTexture { 
			get { return textures[1]; }				
			set { textures[1] = value; }				
		}


		/// <summary>
		/// Normal map texture.
		/// </summary>
		public MaterialTexture NormalMapTexture { 
			get { return textures[2]; }				
			set { textures[2] = value; }				
		}

		/// <summary>
		/// Color texture.
		/// </summary>
		public MaterialTexture EmissionTexture { 
			get { return textures[3]; }				
			set { textures[3] = value; }				
		}


		/// <summary>
		/// Material options for mapping and layer blending.
		/// Default value is SingleLayer.
		/// </summary>
		public MaterialOptions Options;

		/// <summary>
		/// Material layer #0.
		/// </summary>
		public MaterialLayer	Layer0 { 
			get { return layers[0]; } 
			set { layers[0] = value; }
		}

		/// <summary>
		/// Material layer #1
		/// </summary>
		public MaterialLayer Layer1 {
			get { return layers[1]; } 
			set { layers[1] = value; }
		}

		/// <summary>
		/// Material layer #2
		/// </summary>
		public MaterialLayer Layer2 {
			get { return layers[2]; } 
			set { layers[2] = value; }
		}

		/// <summary>
		/// Material layer #2
		/// </summary>
		public MaterialLayer Layer3 {
			get { return layers[3]; } 
			set { layers[3] = value; }
		}


		MaterialLayer[] layers = new MaterialLayer[4];



		/// <summary>
		/// Creates instance of material.
		/// </summary>
		public Material ()
		{
			this.Layer0	=	new MaterialLayer();
			this.Layer0.ColorTexture		=	"";
			this.Layer0.SurfaceTexture		=	"";
			this.Layer0.NormalMapTexture	=	"";
			this.Layer0.EmissionTexture		=	"";

			this.Layer1	=	new MaterialLayer();
			this.Layer1.ColorTexture		=	"";
			this.Layer1.SurfaceTexture		=	"";
			this.Layer1.NormalMapTexture	=	"";
			this.Layer1.EmissionTexture		=	"";

			this.Layer2	=	new MaterialLayer();
			this.Layer2.ColorTexture		=	"";
			this.Layer2.SurfaceTexture		=	"";
			this.Layer2.NormalMapTexture	=	"";
			this.Layer2.EmissionTexture		=	"";

			this.Layer3	=	new MaterialLayer();
			this.Layer3.ColorTexture		=	"";
			this.Layer3.SurfaceTexture		=	"";
			this.Layer3.NormalMapTexture	=	"";
			this.Layer3.EmissionTexture		=	"";
		}



		/// <summary>
		/// Disposes material.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				DisposeGpuResources();
			}
			base.Dispose( disposing );
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Serialization :
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Converts section to object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="keyDataCollection"></param>
		static void SectionToObject<T>( ref T obj, KeyDataCollection keyDataCollection )
		{
			if (keyDataCollection==null) {
				//obj = default(T);
				return;
			}

			var type = typeof(T);

			foreach ( var field in type.GetFields() ) {

				if (field.IsLiteral) {
					continue;
				}

				if (field.FieldType==typeof(MaterialLayer)) {
					continue;
				}

				var keyValue = keyDataCollection[ field.Name ];

				if (keyValue!=null) {
					field.SetValue( obj, StringConverter.FromString( field.FieldType, keyValue ) );
				}
			}
		}


		/// <summary>
		/// Converts object to section
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="sectionName"></param>
		/// <returns></returns>
		static SectionData ObjectToSection<T>( T obj, string sectionName )
		{
			var sectionData = new SectionData(sectionName);
			
			var type = typeof(T);

			foreach ( var field in type.GetFields() ) {
				
				if (field.IsLiteral) {
					continue;
				}

				if (field.FieldType==typeof(MaterialLayer)) {
					continue;
				}

				var name	=	field.Name;
				var value	=	StringConverter.ToString( field.GetValue(obj) );

				var key		=	new KeyData( name, value );
				
				if (field.FieldType.IsEnum) {
					key.Comments.Add("Possible values:");
					foreach ( var enumName in Enum.GetNames( field.FieldType )) {
						key.Comments.Add("  " + enumName);
					}
				}

				sectionData.Keys.AddKey( key );
			}

			return sectionData;
		}


		/// <summary>
		/// Creates material from INI data.
		/// </summary>
		/// <param name="iniData"></param>
		public static Material FromINI ( string iniDataString )
		{
			var parser = new StreamIniDataParser();

			parser.Parser.Configuration.AllowDuplicateSections	=	true;
			parser.Parser.Configuration.AllowDuplicateKeys		=	true;
			parser.Parser.Configuration.CommentString			=	"#";
			parser.Parser.Configuration.OverrideDuplicateKeys	=	true;
			parser.Parser.Configuration.KeyValueAssigmentChar	=	'=';
			parser.Parser.Configuration.AllowKeysWithoutValues	=	false;

			var iniData = parser.ReadData( new StreamReader( new MemoryStream( Encoding.UTF8.GetBytes(iniDataString) ) ) );

			var mtrlSection = iniData.Sections["Material"];//*/
			
			var material	=	new Material();
			material.Layer0	=	new MaterialLayer();
			material.Layer1	=	new MaterialLayer();
			material.Layer2	=	new MaterialLayer();
			material.Layer3	=	new MaterialLayer();
			
			SectionToObject( ref material, iniData.Sections["Material"] );
			SectionToObject( ref material.layers[0], iniData.Sections["Layer0"] );
			SectionToObject( ref material.layers[1], iniData.Sections["Layer1"] );
			SectionToObject( ref material.layers[2], iniData.Sections["Layer2"] );
			SectionToObject( ref material.layers[3], iniData.Sections["Layer3"] );

			return material;
		}



		/// <summary>
		/// Create INI-description for material.
		/// </summary>
		/// <param name="material"></param>
		/// <returns></returns>
		public string ToINI ()
		{
			var parser = new StreamIniDataParser();

			parser.Parser.Configuration.AllowDuplicateSections	=	true;
			parser.Parser.Configuration.AllowDuplicateKeys		=	true;
			parser.Parser.Configuration.CommentString			=	"#";
			parser.Parser.Configuration.OverrideDuplicateKeys	=	true;
			parser.Parser.Configuration.KeyValueAssigmentChar	=	'=';
			parser.Parser.Configuration.AllowKeysWithoutValues	=	false;

			var iniData = new IniData();

			var mtrlSection = ObjectToSection( this, "Material" );
			iniData.Sections.Add( mtrlSection );

			if (Layer0!=null) {
				iniData.Sections.Add( ObjectToSection( Layer0, "Layer0" ) );
			}
			if (Layer1!=null) {
				iniData.Sections.Add( ObjectToSection( Layer1, "Layer1" ) );
			}
			if (Layer2!=null) {
				iniData.Sections.Add( ObjectToSection( Layer2, "Layer2" ) );
			}
			if (Layer3!=null) {
				iniData.Sections.Add( ObjectToSection( Layer3, "Layer3" ) );
			}

			iniData.Configuration.AssigmentSpacer = " ";
			iniData.Configuration.CommentString   = "# ";
			return iniData.ToString( new AlignedIniDataFormatter(iniData.Configuration) );
		}
	}
}
																	    