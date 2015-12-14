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
using Fusion.Engine.Graphics;
using Fusion.Core.Content;
using Fusion.Engine.Common;


namespace Fusion.Engine.Graphics {

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
		public override object Load ( ContentManager content, Stream stream, Type requestedType, string assetPath )
		{																			
			var scene = Scene.Load( stream );

			foreach ( var mesh in scene.Meshes ) {	
				mesh.CreateVertexAndIndexBuffers( content.GameEngine.GraphicsDevice );
			}

			foreach ( var mtrlRef in scene.Materials ) {
				mtrlRef.Material	=	content.Load<Material>( mtrlRef.Name );
				mtrlRef.Material.LoadGpuResources( content );
			}

			return scene;
		}
	}
}
