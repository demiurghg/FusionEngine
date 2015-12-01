using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Fusion;
using Fusion.Core.Shell;
using Fusion.Core.Content;
using Fusion.Core.IniParser;
using Fusion.Core.Utils;
using Native.Fbx;

namespace FScene {

	class FScene {

		static int Main ( string[] args )
		{
			Thread.CurrentThread.CurrentCulture	=	System.Globalization.CultureInfo.InvariantCulture;
			Log.AddListener( new StdLogListener() );

			var options = new Options();
			var parser = new CommandLineParser( options );

			try {

				//	parse arguments :
				if (!parser.ParseCommandLine( args )) {
					return 1;
				}

				//	change extension of output not set :
				if (options.Output==null) {
					options.Output = Path.ChangeExtension( options.Input, ".scene");
				}


				//	run fbx loader :
				var loader = new FbxLoader();
				using ( var scene  = loader.LoadScene( options.Input, options ) ) {
				
					//	prepare meshed :
					Log.Message("Preparation...");
					foreach ( var mesh in scene.Meshes ) {
						if (mesh!=null) {
							mesh.Prepare( scene, options.MergeTolerance );
						}
					}
					
					Log.Message("Writing binary file: {0}", options.Output);
					//	save scene :
					using ( var stream = File.OpenWrite( options.Output ) ) {
						scene.Save( stream );
					}

					if (options.Report) {
						File.WriteAllText( options.Input + ".html", scene.ExportHtmlReport());
					}
				}

				Log.Message("Done!");

			} catch ( Exception e ) {
				parser.ShowError( "{0}", e.Message );

				if (options.Wait) {
					Log.Message("Press any key to continue...");
					Console.ReadKey();
				}

				return 1;
			}

			if (options.Wait) {
				Log.Message("Press any key to continue...");
				Console.ReadKey();
			}

			return 0;
		}
	}
}
