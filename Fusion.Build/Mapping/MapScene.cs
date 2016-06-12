using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Extensions;
using Fusion.Engine.Graphics;

namespace Fusion.Build.Mapping {


	public class MapScene {
		
		readonly string sceneDirectory;

		/// <summary>
		/// Gets full source file path on disk.
		/// </summary>
		public string SourceFullPath {
			get; private set;
		}


		/// <summary>
		/// Gets logic scene path
		/// </summary>
		public string KeyPath {
			get; private set;
		}


		/// <summary>
		/// Gets built scene path.
		/// If scene is not built throws exception.
		/// </summary>
		public string BuiltScenePath {
			get {
				if (builtScenePath==null) {
					throw new InvalidOperationException("Scene is not built");
				}
				return builtScenePath;
			}
		}

		string builtScenePath = null;

		
		/// <summary>
		/// 
		/// </summary>
		public ICollection<MapTexture> Textures {
			get; private set;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyPath"></param>
		/// <param name="fullPath"></param>
		public MapScene ( string keyPath, string sourceFullPath )
		{
			sceneDirectory	=	Path.GetDirectoryName( sourceFullPath );
			SourceFullPath	=	sourceFullPath;
			KeyPath			=	keyPath;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public void BuildScene ( BuildContext context )
		{
			builtScenePath		=	context.GetTempFileName( KeyPath, ".vtscene" );

			var cmdLine			=	string.Format("\"{0}\" /out:\"{1}\" /merge:0 /anim /geom /report", 
				SourceFullPath, 
				builtScenePath
			);

			if (!context.IsUpToDate( SourceFullPath, builtScenePath )) {
				Log.Message("...scene build  : {0}", KeyPath );
				context.RunTool( "FScene.exe", cmdLine );
			} else {
				Log.Message("...scene is utd : {0}", KeyPath );
			}

			var scene = Scene.Load( File.OpenRead( builtScenePath ) );

			Textures	=	scene.Materials
						.Select( mtrl => new MapTexture( Path.Combine( sceneDirectory, Path.ChangeExtension( mtrl.Texture, ".tga" ) ) ) )
						.ToList();
		}
		
	}
}
