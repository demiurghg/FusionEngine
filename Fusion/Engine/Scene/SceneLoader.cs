using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Scene;
using Fusion.Core.Content;
using Fusion.Engine.Common;


namespace Fusion.Engine.Scene {

	/// <summary>
	/// Scene loader
	/// </summary>
	[ContentLoader(typeof(Scene))]
	public class Loader : ContentLoader {


		/// <summary>
		/// Loads scene
		/// </summary>
		/// <param name="game"></param>
		/// <param name="stream"></param>
		/// <param name="requestedType"></param>
		/// <returns></returns>
		public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
		{																			
			var scene = Scene.Load( stream );

			foreach ( var mesh in scene.Meshes ) {	
				mesh.CreateVertexAndIndexBuffers( game.GraphicsDevice );
			}

			return scene;
		}
	}
}
