using System;	using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Graphics;
using BEPUphysics.BroadPhaseEntries;
using BEPUVector3 = BEPUutilities.Vector3;
using BEPUTransform = BEPUutilities.AffineTransform;

namespace ShooterDemo {
	partial class GameWorld {

		List<MeshInstance>	instances;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="scene"></param>
		void InitStaticModels ( GameClient client, Scene scene )
		{
			instances	=	new List<MeshInstance>();

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
		}


		void AddStaticModels( GameClient client )
		{
			var rw = client.Game.RenderSystem.RenderWorld;

			foreach ( var inst in instances ) {
				rw.Instances.Add( inst );
			}
		} 
	}
}
