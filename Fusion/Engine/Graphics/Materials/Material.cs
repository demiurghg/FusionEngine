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

		/// <summary>
		/// Indicates that material is tranparent.
		/// </summary>
		public bool Transparent;

		/// <summary>
		/// Indicates that object with this material should cast shadow.
		/// </summary>
		public bool CastShadow;
		
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

			this.Layer1	=	null;
			this.Layer2	=	null;
			this.Layer3	=	null;

			this.Transparent	=	false;
			this.CastShadow		=	true;
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




		/// <summary>
		/// Creates non-transparent material that casts shadow from color texture.
		/// Method search for existing textures with postfixes like "_s", "_n", "_e" 
		/// and substitutes them into material.
		/// </summary>
		/// <param name="path"></param>
		public static Material CreateFromTexture ( string path )
		{
			var mtrl = new Material();
			mtrl.Layer0	=	new MaterialLayer();
			mtrl.Layer0.ColorTexture		=	path + "|srgb";
			mtrl.Layer0.SurfaceTexture		=	path + "_s";
			mtrl.Layer0.NormalMapTexture	=	path + "_n";
			mtrl.Layer0.EmissionTexture		=	path + "_e|srgb";

			mtrl.Layer1	=	null;
			mtrl.Layer2	=	null;
			mtrl.Layer3	=	null;

			mtrl.Transparent			=	false;
			mtrl.CastShadow				=	true;

			return mtrl;
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
				obj = default(T);
				return;
			}

			var type = typeof(T);

			foreach ( var field in type.GetFields() ) {

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
																	    