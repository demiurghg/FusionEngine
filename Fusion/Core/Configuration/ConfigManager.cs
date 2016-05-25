using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.IniParser;
using Fusion.Core.IniParser.Model;
using Fusion.Engine.Common;

namespace Fusion.Core.Configuration {
	public class ConfigManager {


		/// <summary>
		/// Gets dictionary of all configuration variables.
		/// </summary>
		internal IDictionary<string,ConfigVariable> Variables {
			get {
				return configVariables;
			}
		}


		Dictionary<string,ConfigVariable> configVariables;
		readonly Game game;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public ConfigManager ( Game game )
		{
			this.game		=	game;
			configVariables	=	new Dictionary<string,ConfigVariable>();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="?"></param>
		public void ExposeProperties ( object targetObject, string niceName, string shortName )
		{
			var props = targetObject
				.GetType()
				.GetProperties()
				.Where( p => p.HasAttribute<ConfigAttribute>() )
				.ToList();

			foreach ( var prop in props ) {
				
				var key  = shortName + "." + prop.Name;
				var cvar = new ConfigVariable( niceName, shortName, prop.Name, prop, targetObject );

				try {

					configVariables.Add( key, cvar );

				} catch ( ArgumentException ) {	
					Log.Warning("Can not expose property {0}. Skipped.", key);
				}

			}
		}


		
		/// <summary>
		/// Saves configuration to file in user storage.
		/// </summary>
		/// <param name="path"></param>
		public void Save ( string filename )
		{
			Log.Message("Saving configuration...");

			var storage = game.UserStorage;

			storage.DeleteFile(filename);
			Save( storage.OpenFile(filename, FileMode.Create, FileAccess.Write) );
		}



		/// <summary>
		/// Saves configuration to stream.
		/// </summary>
		/// <param name="stream"></param>
		public void Save ( Stream stream )
		{
			try {
		
				//	prepare ini data :			
				IniData iniData = new IniData();
				iniData.Configuration.CommentString	=	"# ";

				var sortedList = configVariables
								.Select( cv1 => cv1.Value )
								.OrderBy( cv2 => cv2.ComponentName )
								.ThenBy( cv3 => cv3.Name )
								.ToList();


				foreach ( var cvar in sortedList ) {
					
					if (!iniData.Sections.ContainsSection( cvar.ComponentName )) {
						iniData.Sections.AddSection( cvar.ComponentName );
					}

					var section = iniData.Sections[ cvar.ComponentName ];

					section.AddKey( new KeyData( cvar.Name, cvar.Get() ) );
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
		/// Loads configuration from file in user storage.
		/// </summary>
		/// <param name="path"></param>
		public void Load ( string filename )
		{
			Log.Message("Loading configuration...");

			var storage = game.UserStorage;

			if (storage.FileExists(filename)) {
				
				Load( storage.OpenFile(filename, FileMode.Open, FileAccess.Read) );

			} else {
				Log.Warning("Can not load configuration from {0}", filename);
			}
		}



		/// <summary>
		/// Loads configuration from stream.
		/// </summary>
		/// <param name="stream"></param>
		public void Load ( Stream stream )
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

					var cvarDict = configVariables
									.Where( cv1 => cv1.Value.ComponentName==section.SectionName )
									.Select( cv2 => cv2.Value )
									.ToDictionary( cv3 => cv3.Name );

					ConfigVariable cvar;

					foreach ( var keyData in section.Keys ) {
						if (cvarDict.TryGetValue( keyData.KeyName, out cvar )) {
							cvar.Set( keyData.Value );
						} else {
							Log.Warning("Key {0}.{1} ignored.", section.SectionName, keyData.KeyName );
						}
					}
				}

			} catch (IniParser.Exceptions.ParsingException e) {
				Log.Warning("INI parser error: {0}", e.Message);
			}
			
		}
	}
}
