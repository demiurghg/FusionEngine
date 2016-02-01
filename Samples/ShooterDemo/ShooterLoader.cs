using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Server;
using Fusion.Engine.Graphics;


namespace ShooterDemo {
	class ShooterLoader : GameLoader {

		Task loadingTask;


		public Scene Scene {
			get; private set;
		}


		public IEnumerable<MeshInstance> StaticInstances {
			get; private set;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="serverInfo"></param>
		public ShooterLoader ( ShooterClient client, string serverInfo )
		{
			loadingTask	=	new Task( ()=>LoadingTask(client, serverInfo) );
			loadingTask.Start();
		}


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			//	do nothing.
		}



		/// <summary>
		/// 
		/// </summary>
		public override bool IsCompleted {
			get { 
				return loadingTask.IsCompleted; 
			}
		}


		/// <summary>
		/// 
		/// </summary>
		void LoadingTask ( ShooterClient client, string serverInfo )
		{
			var instances	=	new List<MeshInstance>();

			var scene	=	client.Content.Load<Scene>( serverInfo );
			Scene		=	scene;

			//	load textures and materials :
			var transforms = new Matrix[ scene.Nodes.Count ];
			scene.ComputeAbsoluteTransforms( transforms );

			var defMtrl		=	client.Game.RenderSystem.DefaultMaterial;
			var materials	=	scene.Materials.Select( m => client.Content.Load<MaterialInstance>( m.Name, defMtrl ) ).ToArray();
			
			for ( int i=0; i<scene.Nodes.Count; i++ ) {
			
				var meshIndex  = scene.Nodes[i].MeshIndex;
			
				if (meshIndex<0) {
					continue;
				}
				
				var inst   = new MeshInstance( client.Game.RenderSystem, scene, scene.Meshes[meshIndex], materials );
				inst.World = transforms[ i ];
			
				instances.Add( inst );
			}


			StaticInstances	=	instances;
		}
	}
}
