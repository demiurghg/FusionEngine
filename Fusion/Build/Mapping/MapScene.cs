using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Extensions;
using Fusion.Engine.Graphics;

namespace Fusion.Build.Mapping {


	internal class MapScene {
		
		readonly string fullSceneDirectory;
		readonly string keySceneDirectory;

		/// <summary>
		/// Gets full source file path on disk.
		/// </summary>
		public string SourceFullPath {
			get; private set;
		}


		/// <summary>
		/// Gets logic scene path
		/// </summary>
		public string KeyPath {
			get; private set;
		}


		/// <summary>
		/// Gets built scene path.
		/// If scene is not built throws exception.
		/// </summary>
		public string BuiltScenePath {
			get {
				if (builtScenePath==null) {
					throw new InvalidOperationException("Scene is not built");
				}
				return builtScenePath;
			}
		}

		string builtScenePath = null;
		Scene scene;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyPath"></param>
		/// <param name="fullPath"></param>
		public MapScene ( string keyPath, string sourceFullPath )
		{
			keySceneDirectory	=	Path.GetDirectoryName( keyPath );
			fullSceneDirectory	=	Path.GetDirectoryName( sourceFullPath );
			SourceFullPath		=	sourceFullPath;
			KeyPath				=	keyPath;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public void BuildScene ( BuildContext context, VTPageTable pageTable )
		{
			builtScenePath		=	context.GetTempFileName( KeyPath, ".vtscene" );

			//	do not merge!
			var cmdLine			=	string.Format("\"{0}\" /out:\"{1}\" /merge:-1 /anim /geom /report", 
				SourceFullPath, 
				builtScenePath
			);

			if (!context.IsUpToDate( SourceFullPath, builtScenePath )) {
				Log.Message("...scene build  : {0}", KeyPath );
				context.RunTool( "FScene.exe", cmdLine );
			} else {
				Log.Message("...scene is utd : {0}", KeyPath );
			}


			scene		=	Scene.Load( File.OpenRead( builtScenePath ) );


			foreach ( var mtrl in scene.Materials ) {

				string keyTexPath;
				string fullTexPath;
				
				if (string.IsNullOrWhiteSpace(mtrl.Texture)) {
					mtrl.Texture	=	"defaultColor.tga";
					keyTexPath		=	mtrl.Texture;
					fullTexPath		=	context.ResolveContentPath( mtrl.Texture );
				} else {
					mtrl.Texture	=	Path.Combine( keySceneDirectory,  Path.ChangeExtension( mtrl.Texture, ".tga" ) );
					keyTexPath		=	mtrl.Texture;
					fullTexPath		=	context.ResolveContentPath( mtrl.Texture );
				}

				pageTable.AddTexture( keyTexPath, fullTexPath, this );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetPath"></param>
		public void SaveScene ( Stream stream )
		{
			scene.Save( stream );
		} 



		/// <summary>
		/// TODO : split vertices that share triangles with diferrent materials!
		/// </summary>
		public void RemapTexCoords ( VTPageTable pageTable )
		{
			foreach ( var mesh in scene.Meshes ) {

				foreach ( var subset in mesh.Subsets ) {

					var material	=	scene.Materials[ subset.MaterialIndex ];
					var texPath		=	material.Texture;
					var texture		=	pageTable.GetSourceTextureByKeyPath( texPath );

					var startPrimitive	=	subset.StartPrimitive;
					var endPrimitive	=	subset.StartPrimitive + subset.PrimitiveCount;

					for ( int tri = startPrimitive; tri < endPrimitive; tri++ ) {

						var triangle = mesh.Triangles[ tri ];

						var v0	=	mesh.Vertices[ triangle.Index0 ];
						var v1	=	mesh.Vertices[ triangle.Index1 ];
						var v2	=	mesh.Vertices[ triangle.Index2 ];

						v0.TexCoord0 = texture.RemapTexCoords( v0.TexCoord0 );
						v1.TexCoord0 = texture.RemapTexCoords( v1.TexCoord0 );
						v2.TexCoord0 = texture.RemapTexCoords( v2.TexCoord0 );

						mesh.Vertices[ triangle.Index0 ]	=	v0;
						mesh.Vertices[ triangle.Index1 ]	=	v1;
						mesh.Vertices[ triangle.Index2 ]	=	v2;
					}
				}

				mesh.MergeVertices(0);
				mesh.DefragmentSubsets(scene, true);
				mesh.ComputeTangentFrame();
				mesh.ComputeBoundingBox();
                CheckUVLayoutIntersection(mesh);
			}
		}

        public void CheckUVLayoutIntersection(Mesh mesh)
        {
            var trianglePoints = mesh.Vertices.Select(t => t.TexCoord0).ToList();
            if (Geometry.TrianglesIntersection(trianglePoints))
            {
                Log.Warning("Mesh has intersects in UV layout");
            }
        }
	}
}
