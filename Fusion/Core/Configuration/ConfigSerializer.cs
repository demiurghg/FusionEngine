using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Fusion.Core.IniParser;
using Fusion.Core.IniParser.Model;
using System.ComponentModel;
using Fusion.Engine.Common;

namespace Fusion.Core.Configuration {

	/// <summary>
	/// Saves and loads configuration to file.
	/// 
	/// Note on multi-threading:
	///		Be sure that all structure properties 
	///		larger than 4 (32-bit) or 8 (64-bit) bytes in config classes 
	///		have lock on set and get.
	/// </summary>
	internal static class ConfigSerializer {

		static IniData globalIniData = null;


		public static void SaveToStream ( IEnumerable<GameModule.ModuleBinding> bindings, Stream stream )
		{
			try {
		
				//	prepare ini data :			
				IniData iniData = globalIniData ?? new IniData();
				iniData.Configuration.CommentString	=	"# ";

				foreach ( var bind in bindings ) {

					var sectionName		=	bind.NiceName;
					var config			=	bind.Module.GetConfiguration();

					iniData.Sections.AddSection( sectionName );

					var sectionData	=	iniData.Sections.GetSectionData( sectionName );

					foreach ( var key in config ) { 
						if (sectionData.Keys.ContainsKey(key.KeyName)) {
							sectionData.Keys.RemoveKey(key.KeyName);
						}
						sectionData.Keys.AddKey( key );
					}
				}


				//	write file :
				var parser = new StreamIniDataParser();

				using ( var sw = new StreamWriter(stream) ) {
					parser.WriteData( sw, iniData );
				}

			} catch (IniParser.Exceptions.ParsingException e) {
				Log.Warning("INI parser error: {0}", e.Message);
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="bindings"></param>
		/// <param name="path"></param>
		public static void LoadFromStream	( IEnumerable<GameModule.ModuleBinding> bindings, Stream stream )
		{
			try {
		
				var iniData = new IniData();
				var parser = new StreamIniDataParser();

				parser.Parser.Configuration.CommentString	=	"# ";

				using ( var sw = new StreamReader(stream) ) {
					iniData	= parser.ReadData( sw );
				}
			

				//	read data :
				foreach ( var section in iniData.Sections ) {

					var bind	=	bindings
								.Where( b => b.NiceName == section.SectionName )
								.SingleOrDefault();

					if (bind==null) {
						Log.Warning("Module {0} does not exist. Section ignored.", section.SectionName );
					}

					bind.Module.SetConfiguration( section.Keys );
				}

				globalIniData = iniData;

			} catch (IniParser.Exceptions.ParsingException e) {
				Log.Warning("INI parser error: {0}", e.Message);
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string GetConfigPath ( string fileName )
		{
			string myDocs	=	Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string appName	=	Path.GetFileNameWithoutExtension( AppDomain.CurrentDomain.FriendlyName.Replace(".vshost","") );
			return Path.Combine( myDocs, appName, fileName );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static IEnumerable<ConfigVariable> GetConfigVariables ( IEnumerable<GameModule.ModuleBinding> bindings )
		{
			var list = new List<ConfigVariable>();

			foreach ( var bind in bindings ) {

				var prefix			=	bind.ShortName;
				var configObject	=	bind.Module;

				if (configObject==null) {
					continue;
				}

				foreach ( var prop in configObject.GetConfigurationProperties() ) {

					var cfgVar	=	new ConfigVariable( prefix, prop.Name, prop, configObject );
					
					list.Add( cfgVar ); 
				}
			}

			return list;
		}
	}
}
