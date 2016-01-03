﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Shell;

namespace Fusion.Build.Processors {

	[AssetProcessor("Scripts", "No description")]
	public class ScriptProcessor : AssetProcessor {
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceStream"></param>
		/// <param name="targetStream"></param>
		public override void Process ( AssetSource assetFile, BuildContext context )
		{
			using ( var sourceStream = assetFile.OpenSourceStream() ) {
				using ( var targetStream = assetFile.OpenTargetStream() ) {
					sourceStream.CopyTo( targetStream );
				}
			}
		}
	}
}
