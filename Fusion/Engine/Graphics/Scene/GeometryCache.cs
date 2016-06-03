using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Core;
using Fusion.Core.Content;
using Fusion.Core.Extensions;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;


namespace Fusion.Engine.Graphics {
	
	public class GeometryCache : DisposableBase {

		/// <summary>
		/// Gets total geometry clip length
		/// </summary>
		public TimeSpan	Length { 
			get;
			private set;
		}

		/// <summary>
		/// Gets total number of vertices
		/// </summary>
		public int VertexCount {
			get;
			private set;
		}


		/// <summary>
		/// Gets total number of indices
		/// </summary>
		public int IndexCount {
			get;
			private set;
		}


		/// <summary>
		/// Gets material references
		/// </summary>
		public IEnumerable<MaterialRef> Materials {
			get;
			private set;
		}


		internal VertexBuffer VertexBuffer { 
			get { 
				return vertexBuffer; 
			} 
		}

		internal IndexBuffer IndexBuffer { 
			get { 
				return indexBuffer; 
			}
		}

		public bool					IsSkinned		{ get; private set; }

		VertexBuffer vertexBuffer;
		IndexBuffer	 indexBuffer;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="stream"></param>
		public GeometryCache ( RenderSystem rs, Stream stream )
		{
            ReadStream(stream);
		}



        private void ReadStream(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                var fileHeader = reader.ReadFourCC();
                var formarVersion = reader.Read<int>();
                /*
                    FourCC        texture section ("TEX0")
                    int           number of textures
                    (string,string)[]  material & texture names
                 */
                var textureSection = reader.ReadFourCC();
                var countTexture = reader.Read<int>();
                List<MaterialRef> materialTexture = new List<MaterialRef>();
                for (int i = 0; i < countTexture; i++)
                {
                    var Name = reader.ReadString();
                    var texture = reader.ReadString();
                    materialTexture.Add(new MaterialRef(){Name =  Name, Texture =  texture});
                }
                Materials = materialTexture;

                /*
                    FourCC        indices section ("IDX0")
                    int           number of indices
                    int           number of vertices
                    int[]         indices data
                */
                var indicesSection = reader.ReadFourCC();
                IndexCount = reader.Read<int>();
                VertexCount = reader.Read<int>();
                List<int> indicesData = new List<int>();
                for (int i = 0; i < IndexCount; i++)
                {
                    indicesData.Add(reader.Read<int>());
                }

                /*
                    FourCC        subset section ("SBST")
                    int           number of subsets
                    (int,int)[]   start index, index count
                */
                var subsetSection = reader.ReadFourCC();
                var countSubsets = reader.Read<int>();
                List<Tuple<int, int>> startCountIndex = new List<Tuple<int, int>>();
                for (int i = 0; i < countSubsets; i++)
                {
                    startCountIndex.Add(new Tuple<int, int>(reader.Read<int>(), reader.Read<int>()));
                }
                /*
                    FourCC        animated data section ("ANIM")
                    int           number of frames
                    float         frame rate (frames per second)
                 * */
                var animDataSection = reader.ReadFourCC();
                var countFrames = reader.Read<int>();
                var frameRate = reader.Read<float>();

                /*
                    FourCC        frame data section ("FRM0")
                    int           reserved (must be 0)
                    vertex[]      vertex data: 
                                    - Vector3 - position
                                    - Vector2 - texture coordinates
                                    - Vector3 - normal
                                    - Vector3 - tangent
                                    - Vector3 - binormal
                                    - Half4   - color
                 */

                List<List<VertexData>> framesList = new List<List<VertexData>>();
                for (int i = 0; i < countFrames; i++)
                {
                    var frameDataSection = reader.ReadFourCC();
                    var reserved = reader.Read<int>();
                    List<VertexData> vertexDataList = new List<VertexData>();
                    for (int j = 0; j < VertexCount; j++)
                    {
                        VertexData vertexData = new VertexData();
                        vertexData.position = reader.Read<Vector3>();
                        vertexData.texCoord = reader.Read<Vector2>();
                        vertexData.normal = reader.Read<Vector3>();
                        vertexData.tangent = reader.Read<Vector3>();
                        vertexData.binormal = reader.Read<Vector3>();
                        vertexData.color = reader.Read<Half4>();
                        vertexDataList.Add(vertexData);
                    }
                    framesList.Add(vertexDataList);
                }
            }
        }

        struct VertexData
        {
            public Vector3 position;
            public Vector2 texCoord;
            public Vector3 normal;
            public Vector3 tangent;
            public Vector3 binormal;
            public Half4 color;
        }
	}
}
