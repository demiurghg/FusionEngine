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


	    private readonly RenderSystem rs;
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

        public List<MeshSubset> Subsets { get; private set; }
	    public List<List<MeshVertex>> framesList { get; private set; }

	    private int currentFrame = 0;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="stream"></param>
		public GeometryCache ( RenderSystem rs, Stream stream )
		{
		    this.rs = rs;
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
                var indicesData = new int[IndexCount];
                for (int i = 0; i < IndexCount; i++)
                {
                    indicesData[i]=reader.Read<int>();
                }
                indexBuffer = IndexBuffer.Create(rs.Game.GraphicsDevice, indicesData);
                /*
                    FourCC        subset section ("SBST")
                    int           number of subsets
                    (int,int,int)[]   start index, index count, Material Index 
                */
                var subsetSection = reader.ReadFourCC();
                var countSubsets = reader.Read<int>();
                Subsets = new List<MeshSubset>();
                for (int i = 0; i < countSubsets; i++)
                {
                    var startIndex = reader.Read<int>();
                    var indexCount = reader.Read<int>();
                    var materialIndex = reader.Read<int>();

                    Subsets.Add(new MeshSubset()
                    {
                        StartPrimitive = startIndex,
                        PrimitiveCount = indexCount,
                        MaterialIndex = materialIndex,
                    });
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


                framesList = new List<List<MeshVertex>>();
                for (int i = 0; i < countFrames; i++)
                {
                    var frameDataSection = reader.ReadFourCC();
                    var reserved = reader.Read<int>();
                    List<MeshVertex> vertexDataList = new List<MeshVertex>();
                    for (int j = 0; j < VertexCount; j++)
                    {
                        MeshVertex vertexData = new MeshVertex();
                        vertexData.Position = reader.Read<Vector3>();
                        vertexData.TexCoord0 = reader.Read<Vector2>();
                        vertexData.Normal = reader.Read<Vector3>();
                        vertexData.Tangent = reader.Read<Vector3>();
                        vertexData.Binormal = reader.Read<Vector3>();
                        reader.Read<Half4>();
                        vertexData.Color0 = Color.Red;
                        vertexDataList.Add(vertexData);
                    }
                    framesList.Add(vertexDataList);
                }
                // fill vertex buffer for first iteration
                Update();
            }
        }

        public void Update()
        {
            if (framesList == null || framesList.Count <= 0)
                return;

            IsSkinned = framesList[currentFrame].Any(v => v.SkinIndices != Int4.Zero);
            vertexBuffer = new VertexBuffer(rs.Game.GraphicsDevice, typeof(MeshVertex), VertexCount);
            vertexBuffer.SetData(framesList[currentFrame].ToArray());
            currentFrame++;
            if (currentFrame >= framesList.Count)
                currentFrame = 0;
        }
	}
}
