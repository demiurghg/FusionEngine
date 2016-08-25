using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Mathematics;
using Fusion.Core.Shell;
using Fusion.Engine.Imaging;
using Fusion.Build.Processors;
using Fusion.Engine.Graphics;

namespace Fusion.Build.Mapping {

	[AssetProcessor("Map", "Performs map assembly")]
	public class MapProcessor : AssetProcessor {


		/// <summary>
		/// 
		/// </summary>
		public MapProcessor ()
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildContext"></param>
		public override void Process ( AssetSource assetFile, BuildContext context )
		{
			//	get list of scenes :
			/*var mapScenes	=	File.ReadAllLines(assetFile.FullSourcePath)
								.Select( f1 => f1.Trim() )
								.Where( f2 => !f2.StartsWith("#") && !string.IsNullOrWhiteSpace(f2) )
								.Select( f3 => new MapScene( f3, Path.Combine( fileDir, f3 ) ) )
								.ToArray();*/


			//	write master table :
			using ( var targetStream = assetFile.OpenTargetStream() ) {
			}

			using ( var zipArchive = assetFile.OpenAssetArchive() ) {
			}
		}
	}
}
