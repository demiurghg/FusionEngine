using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Shell;
using Fusion.Engine.Graphics;

namespace Fusion.Build.Processors {

	[AssetProcessor("Materials", "Process material files.")]
	public class MaterialProcessor : AssetProcessor {

		
		/// <summary>
		/// 
		/// </summary>
		public MaterialProcessor ()
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceStream"></param>
		/// <param name="targetStream"></param>
		public override void Process ( AssetSource assetFile, BuildContext context )
		{
			var mtrl	=	BaseIllum.ImportFromXml ( File.ReadAllText(assetFile.FullSourcePath) );

			//	get dependencies :
			var deps	=	mtrl.GetDependencies().ToArray();

			var file	=	BaseIllum.ExportToXml(mtrl);

			using ( var target = assetFile.OpenTargetStream(deps) ) {
				using ( var bw = new BinaryWriter(target) ) {
					bw.Write(file);
				}
			}
		}
	}
}
