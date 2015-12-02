using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Content;
using Fusion.Engine.Common;
using System.IO;

namespace Fusion.Engine.Graphics {
	
	/// <summary>
	/// 
	/// </summary>
	[ContentLoader(typeof(Material))]
	internal class MaterialLoader : ContentLoader {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		/// <param name="stream"></param>
		/// <param name="requestedType"></param>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
		{
			using ( var sr = new BinaryReader(stream) ) {
				var iniText = sr.ReadString();
				return Material.FromINI( iniText );
			}

		}

	}
}
