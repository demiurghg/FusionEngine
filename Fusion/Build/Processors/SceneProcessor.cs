using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Shell;
using Fusion.Drivers.Graphics;

namespace Fusion.Build.Processors {

	[AssetProcessor("Scenes", "Converts FBX files to scene")]
	public class SceneProcessor : AssetProcessor {

		/// <summary>
		/// Vertex merge tolerance
		/// </summary>
		[CommandLineParser.Name("merge", "merge tolerance (default=0)")]
		public float MergeTolerance { get; set; }

		/// <summary>
		/// Evaluates transform
		/// </summary>
		[CommandLineParser.Name("anim", "import animation")]
		public bool ImportAnimation { get; set; }

		/// <summary>
		/// Evaluates transform
		/// </summary>
		[CommandLineParser.Name("geom", "import geometry")]
		public bool ImportGeometry { get; set; }

		/// <summary>
		/// Evaluates transform
		/// </summary>
		[CommandLineParser.Name("report", "output html report")]
		public bool OutputReport { get; set; }

		/// <summary>
		/// Evaluates transform
		/// </summary>
		[CommandLineParser.Name("genmtrl", "generate missing materials")]
		public bool GenerateMissingMaterials { get; set; }


		
		/// <summary>
		/// 
		/// </summary>
		public SceneProcessor ()
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceStream"></param>
		/// <param name="targetStream"></param>
		public override void Process ( AssetSource assetFile, BuildContext context )
		{
			var resolvedPath	=	assetFile.FullSourcePath;
			var destPath		=	context.GetTempFileName( assetFile.KeyPath, ".scene" );

			var cmdLine			=	string.Format("\"{0}\" /out:\"{1}\" /base:\"{2}\" /merge:{3} {4} {5} {6} {7}", 
				resolvedPath, 
				destPath, 
				assetFile.BaseDirectory,
				MergeTolerance, 
				ImportAnimation ? "/anim":"", 
				ImportGeometry ? "/geom":"", 
				OutputReport ? "/report":"" ,
				GenerateMissingMaterials ? "/genmtrl":""
			);

			context.RunTool( "FScene.exe", cmdLine );

			using ( var target = assetFile.OpenTargetStream() ) {
				context.CopyFileTo( destPath, target );
			}
		}
	}
}
