using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUVector3 = BEPUutilities.Vector3;
using BEPUTransform = BEPUutilities.AffineTransform;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Client;
using Fusion.Engine.Common;
using Fusion.Engine.Server;
using Fusion.Core.Content;
using Fusion.Engine.Graphics;


namespace ShooterDemo {
	partial class ShooterServer : GameServer {

		Space physSpace;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scene"></param>
		void InitializeStaticPhysWorld ( Scene scene )
		{
			physSpace	=	new Space();

			var transforms = new Matrix[ scene.Nodes.Count ];
			scene.ComputeAbsoluteTransforms( transforms );

			for ( int i=0; i<scene.Nodes.Count; i++ ) {

				var node = scene.Nodes[i];

				if (node.MeshIndex<0) {
					continue;
				}

				var physMesh   = new PhysMeshData( scene.Meshes[node.MeshIndex], transforms[i] );

				var staticMesh = new StaticMesh( physMesh.Vertices, physMesh.Indices );
				physSpace.Add( staticMesh );
			}
		}



		class PhysMeshData {
			public int[]			Indices;
			public BEPUVector3[]	Vertices;

			public PhysMeshData( Mesh mesh, Matrix transform )
			{
				Indices		=	mesh.GetIndices();
				Vertices	=	mesh.Vertices
								.Select( v => Convert( v.Position, transform ) )
								.ToArray();
			}


			BEPUVector3 Convert ( Vector3 position, Matrix transform )
			{
				var tpos = Vector3.TransformCoordinate( position, transform );
				return new BEPUVector3( tpos.X, tpos.Y, tpos.X );
			}
		}
	}
}
