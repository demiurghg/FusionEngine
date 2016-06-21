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
using Fusion.Drivers.Graphics;
using Fusion.Engine.Graphics;

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
				

				//
				//	run fbx loader :
				//
				Log.Message("Reading FBX: {0}", options.Input);

				var loader = new FbxLoader();
				using ( var scene  = loader.LoadScene( options.Input, options ) ) {
				
					//
					//	Save scene :
					//					
					Log.Message("Preparation...");
					foreach ( var mesh in scene.Meshes ) {
						if (mesh!=null) {
							mesh.MergeVertices( options.MergeTolerance );
							mesh.DefragmentSubsets(scene, true);
							mesh.ComputeTangentFrame();
							mesh.ComputeBoundingBox();
						}
					}

					Log.Message("Merging instances...");
					scene.DetectAndMergeInstances();
					
					if (options.BaseDirectory!=null) {

						Log.Message("Resolving assets path...");
						
						var relativePath = ContentUtils.MakeRelativePath( options.BaseDirectory + @"\", options.Input );
						var relativeDir  = Path.GetDirectoryName( relativePath );
						var sceneDir	 = Path.GetDirectoryName( options.Input );
						Log.Message("...scene directory      : {0}", sceneDir);
						Log.Message("...scene base directory : {0}", options.BaseDirectory);
						Log.Message("...scene relative path  : {0}", relativePath);
						Log.Message("...scene relative dir   : {0}", relativeDir);

						foreach ( var mtrl in scene.Materials ) {
							ResolveMaterial( mtrl, relativeDir, sceneDir );
						}
					}

					//
					//	Save scene :
					//					
					Log.Message("Writing binary file: {0}", options.Output);
					using ( var stream = File.OpenWrite( options.Output ) ) {
						scene.Save( stream );
					}


					if (options.Report) {
						var reportPath = options.Input + ".html";
						Log.Message("Writing report: {0}", reportPath);
						File.WriteAllText( reportPath, SceneReport.CreateHtmlReport(scene));
					}
				}

				Log.Message("Done.");

			} catch ( Exception e ) {
				parser.ShowError( "{0}", e.ToString() );

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






		static void ResolveMaterial ( MaterialRef material, string relativeSceneDir, string fullSceneDir )
		{
			var mtrlName		=	ContentUtils.CreateSafeName( material.Name );
			var texPath			=	material.Texture ?? "";

			material.Name		=	Path.Combine( relativeSceneDir, mtrlName );
			material.Texture	=	Path.Combine( relativeSceneDir, texPath );
			var mtrlFileName	=	Path.Combine( fullSceneDir, mtrlName + ".material" );


			if (!File.Exists(mtrlFileName)) {

				var newMtrl =	new BaseIllum();

				newMtrl.ColorTexture.Path		=	ResolveTexture( relativeSceneDir, fullSceneDir, texPath, ""					, newMtrl.ColorTexture.Path		);  
				newMtrl.SurfaceTexture.Path		=	ResolveTexture( relativeSceneDir, fullSceneDir, texPath, "_surf"			, newMtrl.SurfaceTexture.Path	);  
				newMtrl.NormalMapTexture.Path	=	ResolveTexture( relativeSceneDir, fullSceneDir, texPath, "_local", "_bump"	, newMtrl.NormalMapTexture.Path	);  
				newMtrl.EmissionTexture.Path	=	ResolveTexture( relativeSceneDir, fullSceneDir, texPath, "_glow"			, newMtrl.EmissionTexture.Path	);  

				File.WriteAllText( mtrlFileName, BaseIllum.ExportToXml(newMtrl) );
			}

		}



		static string ResolveTexture ( string relativeSceneDir, string fullSceneDir, string textureName, string postfix, string fallback )
		{	
			if (string.IsNullOrWhiteSpace(textureName)) {
				return "";
			}

			var ext		=	Path.GetExtension( textureName );
			var noExt	=	Path.Combine( Path.GetDirectoryName(textureName), Path.GetFileNameWithoutExtension( textureName ) );

			var fileName	=	Path.Combine( fullSceneDir, noExt + postfix + ext );
			
			if ( File.Exists(fileName) ) {
				return Path.Combine( relativeSceneDir, noExt + postfix + ext );
			} else {
				return fallback;
			}
		}


		static string ResolveTexture ( string relativeSceneDir, string fullSceneDir, string textureName, string postfix, string altPostfix, string fallback )
		{	
			if (string.IsNullOrWhiteSpace(textureName)) {
				return "";
			}

			var ext		=	Path.GetExtension( textureName );
			var noExt	=	Path.Combine( Path.GetDirectoryName(textureName), Path.GetFileNameWithoutExtension( textureName ) );

			var fileName	=	Path.Combine( fullSceneDir, noExt + postfix + ext );
			var fileNameAlt	=	Path.Combine( fullSceneDir, noExt + altPostfix + ext );
			
			if ( File.Exists(fileName) ) {
				return Path.Combine( relativeSceneDir, noExt + postfix + ext );

			} else if ( File.Exists(fileNameAlt) ) {
				return Path.Combine( relativeSceneDir, noExt + altPostfix + ext );

			}else {
				return fallback;
			}
		}
	}
}
