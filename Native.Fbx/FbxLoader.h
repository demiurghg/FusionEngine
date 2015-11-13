// FbxLoader2.h

#pragma once

using namespace System;
using namespace Fusion;
using namespace msclr::interop;
using namespace Fusion::Engine::Scene;
using namespace Fusion::Core::Mathematics;

namespace Native {
	namespace Fbx {

		public ref class FbxLoader {
			public:
						FbxLoader	();
				Scene	^LoadScene	( string ^filename, Options ^options );

			private:

				Options					^options;
				FbxManager				*fbxManager	;	
				FbxImporter				*fbxImporter;		
				FbxScene				*fbxScene	;	
				FbxGeometryConverter	*fbxGConv	;	
				FbxTime::EMode			timeMode;

				void IterateChildren		( FbxNode *fbxNode, FbxScene *fbxScene, Fusion::Engine::Scene::Scene ^scene, int parentIndex, int depth );
				void HandleMesh				( Scene ^scene, Node ^node, FbxNode *fbxNode );
				void HandleSkinning			( Mesh ^nodeMesh, Scene ^scene, Node ^node, FbxNode *fbxNode, Matrix^ meshTransform, array<Int4> ^skinIndices, array<Vector4>	^skinWeights );
				void HandleCamera			( Scene ^scene, Node ^node, FbxNode *fbxNode );
				void HandleLight			( Scene ^scene, Node ^node, FbxNode *fbxNode );
				void HandleMaterial			( MeshSubset ^sg, FbxSurfaceMaterial *material );
				void GetNormalForVertex		( MeshVertex *vertex, FbxMesh *fbxMesh, int vertexIdCount, int ctrlPointId  );
				void GetTextureForVertex	( MeshVertex *vertex, FbxMesh *fbxMesh, int vertexIdCount, int vertexId );
				void GetColorForVertex		( MeshVertex *vertex, FbxMesh *fbxMesh, int vertexIdCount, int vertexId );

				void GetCustomProperties	( Fusion::Engine::Scene::Node ^node, FbxNode *fbxNode );
			
				// Animation stuff
		};
	}
}
