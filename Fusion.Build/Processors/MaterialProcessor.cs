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
			var mtrl	=	Material.FromINI ( File.ReadAllText(assetFile.FullSourcePath) );

			//	get dependencies :
			var depList	=	new[]{
					mtrl.Layer0.ColorTexture, mtrl.Layer0.SurfaceTexture, mtrl.Layer0.NormalMapTexture, mtrl.Layer0.EmissionTexture,
					mtrl.Layer1.ColorTexture, mtrl.Layer1.SurfaceTexture, mtrl.Layer1.NormalMapTexture, mtrl.Layer1.EmissionTexture,
					mtrl.Layer2.ColorTexture, mtrl.Layer2.SurfaceTexture, mtrl.Layer2.NormalMapTexture, mtrl.Layer2.EmissionTexture,
					mtrl.Layer3.ColorTexture, mtrl.Layer3.SurfaceTexture, mtrl.Layer3.NormalMapTexture, mtrl.Layer3.EmissionTexture,
				};

			depList	=	depList
				.Where( s0 => !string.IsNullOrWhiteSpace(s0) )
				.Where( s1 => !s1.StartsWith("*") )
				.Distinct()
				.ToArray();



			var file	=	mtrl.ToINI();

			using ( var target = assetFile.OpenTargetStream(depList) ) {
				using ( var bw = new BinaryWriter(target) ) {
					bw.Write(file);
				}
			}
		}
	}
}
