using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Fusion;
using Fusion.Input;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;


namespace Fusion.Engine.Graphics.Graph
{

    public enum IntegratorType
    {
        EULER		= 0x8,
        RUNGE_KUTTA = 0x8 << 1
    }

    public enum GraphType
    {
        STATIC = 0,
        DYNAMIC
    }
	
	public enum State
	{
		RUN,
		PAUSE
	}

    public class GraphLayer : IDisposable
    {
	    private Game Game;

        public GraphConfig cfg { get; set; }

        //Node texture
        Texture2D texture;
        Texture2D stroke;
        Texture2D border;
        Texture2D gradLink;
        Ubershader shader;

        static public GraphType graphType;

        public State state;

        int injectionCount = 0;
		public Graph.Vertice[] injectionBufferCPU;

        StructuredBuffer simulationBufferSrc;
       // LinkId[] linksPtrBufferCPU;

        StructuredBuffer linksBuffer;
        Graph.Link[] linksBufferCPU;

 //       static int stupidcounter = 0; 
        ConstantBuffer paramsCB;
        List<List<int>> linkPtrLists;

        List<Graph.Link> linkList;
        public List<Graph.Vertice> ParticleList;

		LayoutSystem ls;
        public List<string> nodeText;


		public GreatCircleCamera Camera { set; get; }


		//[StructLayout(LayoutKind.Explicit)]
		//struct LinkId
		//{
		//	[FieldOffset(0)]
		//	public int id;
		//}

        enum Flags
        {
            // for compute shader: 
            REDUCTION	= 0x1,
            SIMULATION	= 0x1 << 1,
            MOVE	= 0x1 << 2,
            EULER	= 0x1 << 3,
            LOCAL	= 0x1 << 4,

            // for geometry shader:
            POINT	= 0x1 << 5,
            LINE	= 0x1 << 6,
            DRAW	= 0x1 << 7,

			//type of layout:
			STATIC	= 0x1 << 8,
			DYNAMIC = 0x1 << 9,
        }



        [StructLayout(LayoutKind.Explicit, Size=160)]
        struct Params
        {
            [FieldOffset(0)] public Matrix View;
            [FieldOffset(64)] public Matrix Projection;
            [FieldOffset(128)] public int MaxParticles;
            [FieldOffset(132)] public float DeltaTime;
            [FieldOffset(136)] public float LinkSize;
            [FieldOffset(140)] public float CalculationRadius;
            [FieldOffset(144)] public float Mass;
			[FieldOffset(148)] public int StartIndex;
			[FieldOffset(152)] public int EndIndex;
        }

        Params param = new Params();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        public GraphLayer(Game game)
        {
	        Game = game;

            cfg = new GraphConfig();
        }

		StateFactory	factory;
	    private Graph graph;

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            texture		= Game.Content.Load<Texture2D>("Graph/node");
            stroke		= Game.Content.Load<Texture2D>("Graph/palette");
            border		= Game.Content.Load<Texture2D>("Graph/big_circle_stroke");
			gradLink	= Game.Content.Load<Texture2D>("Graph/palette_invs");

			shader	= Game.Content.Load<Ubershader>("Graph/shaders");
			factory	= new StateFactory( shader, typeof(Flags), (ps,i) => Enum( ps, (Flags)i ) );

            paramsCB = new ConstantBuffer(Game.GraphicsDevice, typeof(Params));


            graphType = GraphType.DYNAMIC;
            linkList		= new List<Graph.Link>();
            ParticleList	= new List<Graph.Vertice>();
            linkPtrLists	= new List<List<int>>();
			ls = new LayoutSystem(Game, Game.Content.Load<Ubershader>(@"Graph/Compute"));
			//ls = new LayoutSystem(Game, Game.Content.Load<Ubershader>(@"Shaders/VKRepost"));
			//ls = new LayoutSystem(Game, Game.Content.Load<Ubershader>(@"Graph/Visheratin"));
			ls.UseGPU = false;
			ls.SpringTension = 0.1f;
			nodeText = new List<string>();
            //state = State.RUN;
			Log.Message("start graph");

	
			GraphReader gr = new GraphReader();
			//gr.ReadTamaraTGF(@".\Data\testedgeList.txt", cfg, out graph);
			gr.ReadMedicineTGF(@".\Data\edgeList.txt", cfg, out graph);

			int ed = 0;
			while (ed < graph.links.Count) {
			 numberOfEdgesPerIteration.Add(ed);
			 ed += 1000;
			}

			numberOfEdgesPerIteration.Add(graph.links.Count);
			injectionBufferCPU = new Graph.Vertice[graph.NodesCount];
            linksBufferCPU = new Graph.Link[graph.links.Count];
        }

	
		List<int> numberOfEdgesPerIteration = new List<int>();
		int edgeIterator = 0;
	    public bool informationSpreading = false;
		

		void Enum ( PipelineState ps, Flags flag )
		{
			ps.Primitive			=	Primitive.PointList;
			ps.RasterizerState		=	RasterizerState.CullNone;
			if (flag.HasFlag(Flags.LINE)) {
				ps.BlendState = BlendState.AlphaBlend;
				ps.DepthStencilState = DepthStencilState.Readonly;
			}
			if (flag.HasFlag(Flags.POINT)) {
				ps.BlendState = BlendState.AlphaBlend;
				ps.DepthStencilState = DepthStencilState.Default;
			}	
		}

        public void Pause()
        {
	        state = state == State.RUN ? State.PAUSE : State.RUN;
        }


	    public void AddMaxParticles()
        {
			ParticleList.Clear();
            linkList.Clear();
            linkPtrLists.Clear();
            addChain();
            setBuffers();
        }
		
		

        public void addLink(int end1, int end2, int reposter, Graph.Link link )
        {
			int linkNumber = linkList.Count;
			link.SourceID = end1;
			link.StockID = end2;
			
			linkList.Add( link);

			if (linkPtrLists.ElementAtOrDefault(end1) == null) {
				linkPtrLists.Insert(end1, new List<int>());
			}
			linkPtrLists[end1].Add(linkNumber);

			if (linkPtrLists.ElementAtOrDefault(end2) == null) {
				linkPtrLists.Insert(end1, new List<int>());
			}
			linkPtrLists[end2].Add(linkNumber);


			Graph.Vertice newPrt1 = ParticleList[end1];
			Graph.Vertice newPrt2 = ParticleList[end2];
			newPrt1.OutDegree += 1;
			//newPrt2.linksCount += 1;

			ParticleList[end1] = newPrt1;
			ParticleList[end2] = newPrt2;

			var element1 = ParticleList.ElementAt(end1);
			if ( (int) element1.Information == 0) element1.Information = link.LinkType ;
			element1.Degree++;
			//element1.Mass += link.Length;
			//element1.Size = (float) Math.Sqrt(element1.OutDegree + 1) * cfg.MinParticleRadius;// 
			ParticleList[end1] = element1;

			var element2 = ParticleList.ElementAt(end2);
			if ( (int) element2.Information == 0) element2.Information = link.LinkType ;//+ 0.002f * element2.linksCount; 
			//element2.Mass += link.Length;
			element2.Degree++;
			//element2.Size = (float) Math.Sqrt(element2.OutDegree + 1) * cfg.MinParticleRadius;//
			ParticleList[end2] = element2;
        }
		
        void addChain()
        {
			foreach (var node in graph.nodes)
			{
				ParticleList.Add(node);
				linkPtrLists.Add(new List<int>());
			}
        }

        public void createLinksFromFile(int iteration)
        {
	        int edgeAdd = (staticMode) ? numberOfEdgesPerIteration.Count - 1: iteration;
			if (edgeAdd < numberOfEdgesPerIteration.Count ) {
						for (int i = edgeIterator; i < numberOfEdgesPerIteration[edgeAdd]; i++) 
						{
							int end1 = graph.links[i].SourceID;
							int end2 = graph.links[i].StockID;
							int index = ParticleList.FindIndex(list => list.Id == end1);
							int index2 = ParticleList.FindIndex(list => list.Id == end2);
							
							if (index < 0) {
								index = ParticleList.Count;
								ParticleList.Add(graph.nodes[index]);
								linkPtrLists.Add(new List<int>());
							}
							
							if (index2 < 0) {
								index2 = ParticleList.Count;
								ParticleList.Add(graph.nodes[index2]);
								linkPtrLists.Add(new List<int>());
							}
							addLink(index, index2, end1,  graph.links[i]);
						}
						edgeIterator = numberOfEdgesPerIteration[edgeAdd];
			}
            setBuffers();
        }

	    public void countingStars()
	    {
		    for (int i = 0; i < ParticleList.Count; i++)
		    {
			    var elem = ParticleList[i];
			    float force = elem.Mass;
				List<int> edgesList = new List<int>();
			    graph.neighboors.TryGetValue(elem.Id, out edgesList);
			    foreach (var v in ParticleList)
			    {
				    List<int> otherEdge = new List<int>();
					graph.neighboors.TryGetValue(v.Id, out edgesList);
				    List<int> intersection = edgesList.Intersect(otherEdge).ToList();
				    if (intersection.Count > 0) {
					    foreach (var e in intersection)
					    {
						    List<Graph.Link> corr = linkList.FindAll(list => (list.SourceID == elem.Id && list.StockID == v.Id) || (list.SourceID == v.Id && list.StockID == elem.Id) );
						    foreach (var corWeight in corr)
						    {
							    force -= corWeight.TotalLifeTime;
						    }
					    }
				    }
			    }
			    elem.Mass = force;
				ParticleList[i] = elem;
		    }
	    }

	    public bool Clustering = false;
		
        void setBuffers()
        {
            if (ParticleList.Count == 0) return;
            int iter = 0;
            if (simulationBufferSrc != null) {
                simulationBufferSrc.GetData(injectionBufferCPU);
                simulationBufferSrc.Dispose();
				foreach (var p in ParticleList)
				{
					injectionBufferCPU[iter].OutDegree = linkPtrLists.Count;//p.OutDegree;
					injectionBufferCPU[iter].Charge = (injectionBufferCPU[iter].Charge > 0) ? injectionBufferCPU[iter].Charge : p.Charge;
					injectionBufferCPU[iter].ColorType = p.ColorType ;
					injectionBufferCPU[iter].Color = p.Color; 
					injectionBufferCPU[iter].Degree = p.Degree;
					injectionBufferCPU[iter].Size =  p.Size * (float) Math.Sqrt(cfg.NodeScale);
					injectionBufferCPU[iter].Id = p.Id;
					injectionBufferCPU[iter].Information = p.Information;
					injectionBufferCPU[iter].Group = p.Group;
					if (informationSpreading)
					{
						//injectionBufferCPU[iter] .Color = ( (int) p.Information == 0) 
						//	? ColorConstant.palleteByInformation.ElementAt(0).ToVector4() 
						//	: ColorConstant.palleteByInformation.ElementAt(1).ToVector4();
						//injectionBufferCPU[iter].Size = cfg.MinParticleRadius * 2;
						injectionBufferCPU[iter].Color = ColorConstant.paletteByCluster.ElementAt(injectionBufferCPU[iter].Cluster).ToVector4();
					}
					++iter;
				}
            } else {
	            ParticleList.CopyTo(injectionBufferCPU);
            }
			if (Clustering) readAwesomeClusters();
			
			for(int j = 0; j < linkList.Count; j++)
            {
                var l = linkList.ElementAt(j);
				if( l.LifeTime > 0) {
					//l.LifeTime--;
				} else {
					l.Color.W -= l.Color.W < 0.02f ? 0 : 0.01f;
				}
				linkList[j] = l;
            }

			iter = 0;
			foreach (var l in linkList)
			{
				linksBufferCPU[iter] = l;
				linksBufferCPU[iter].Weight = l.Weight;
				linksBufferCPU[iter].LinkType = l.LinkType;
	            if (informationSpreading) {
					//linksBufferCPU[iter].Color = new Vector4( (l.LinkType == 1) 
					//	? ColorConstant.palleteByInformation.ElementAt(1).ToVector3() 
					//	: ColorConstant.palleteByInformation.ElementAt(0).ToVector3(),  l.Color.W );
		            linksBufferCPU[iter].Color = ColorConstant.paletteByCluster[10].ToVector4();
	            }
				++iter;	
			}

	        if (selectedVertice != -1) {
		        List<int> coloredId = new List<int>();
				iter = 0;
				coloredId.Add(selectedVertice);
				foreach (var l in linkList)
				{
					linksBufferCPU[iter].Color.W = 0.01f;
					if (l.StockID == selectedVertice || l.SourceID == selectedVertice) {
						linksBufferCPU[iter].Color.W = cfg.EdgeMaxOpacity;
						linksBufferCPU[iter].Width = cfg.MaxLinkWidth * 2;
						coloredId.Add(l.StockID);
						coloredId.Add(l.SourceID);
					}
					++iter;	
				}
		        coloredId = coloredId.Distinct().ToList();

				for (int i = 0; i < ParticleList.Count;  i++)
				{
					injectionBufferCPU[i] .Color.W =  0.5f;
				}
				foreach (var p in coloredId)
                {
					injectionBufferCPU[p] .Color += new Vector4(new Vector3(0.3f), 1f);
                }
	        }
            
            if (injectionBufferCPU.Length != 0) {
                simulationBufferSrc = new StructuredBuffer(Game.GraphicsDevice, typeof(Graph.Vertice), injectionBufferCPU.Length, StructuredBufferFlags.Counter);
                simulationBufferSrc.SetData(injectionBufferCPU);
            }
            if (linksBufferCPU.Length != 0) {
                linksBuffer = new StructuredBuffer(Game.GraphicsDevice, typeof(Graph.Link), linksBufferCPU.Length, StructuredBufferFlags.Counter);
                linksBuffer.SetData(linksBufferCPU);
            }

	        
			ls.ResetState();
				List<Particle3D> list = new List<Particle3D>();
				foreach (var elem in injectionBufferCPU)
				{
					var p = new Particle3D {
						Position	= elem.Position,
						Velocity	= elem.Velocity,
						Force		= elem.Force,
						Energy		= elem.Energy,
						Mass		= elem.Mass,
						Charge		= elem.Charge,

						Color			= elem.Color,
						Size			= elem.Size,
						linksPtr		= elem.linksPtr,
						linksCount		= elem.Degree, //elem.Degree,
						DesiredRadius	= elem.ColorType,
						Information		= elem.Information,
						Group			= elem.Group,
						Cluster			= elem.Cluster,
					};
					list.Add(p);
				}
				List<Link> listLink = new List<Link>();
				foreach (var elem in linksBufferCPU)
				{
					var l = new Link {
						par1 = (uint) elem.SourceID,
						par2 = (uint) elem.StockID,
						length		= elem.Length,
						strength	= elem.TotalLifeTime,
						//strength = elem.LifeTime / elem.TotalLifeTime,
					};
					listLink.Add(l);
				}

				ls.SetData(list, listLink, linkPtrLists);	
	        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose()
        {
			paramsCB.Dispose();
			simulationBufferSrc.Dispose();
			linksBuffer.Dispose();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
        }

		Vector2 PixelsToProj(Point point)
        {
            Vector2 proj = new Vector2(
                (float)point.X / (float)Game.GraphicsDevice.DisplayBounds.Width,
                (float)point.Y / (float)Game.GraphicsDevice.DisplayBounds.Height
            );
            proj.X = proj.X * 2 - 1;
            proj.Y = -proj.Y * 2 + 1;
            return proj;
        }

		public void SelectNode(Point cursor, StereoEye eye, float threshold, out int VerticeIndex, out Vector3 VerticePosition)
        {
            VerticeIndex = -1;
			var cam = Camera;
            var viewMatrix = cam.GetViewMatrix(eye);
            var projMatrix = cam.GetProjectionMatrix(eye);
            Vector2 cursorProj = PixelsToProj(cursor);
			Dictionary<int, float> candidatesToSelect = new Dictionary<int, float>();
			VerticePosition = new Vector3();
            float minZ = 99999;
            if (simulationBufferSrc != null) {
	            selectedVertice = -1;
				Graph.Vertice[] particleArray = new Graph.Vertice[ParticleList.Count];
                simulationBufferSrc.GetData(particleArray);
                foreach (var p in particleArray) {
                    Vector4 posWorld	= new Vector4(p.Position, 1.0f);
                    Vector4 posView		= Vector4.Transform(posWorld, viewMatrix);
                    Vector4 posProj		= Vector4.Transform(posView, projMatrix);
					posProj /= posProj.W;

                    Vector2 diff = new Vector2(posProj.X - cursorProj.X, posProj.Y - cursorProj.Y);
					
                    if (diff.Length() < threshold) {
						//if (minZ > posProj.Z) {
                           // minZ			= posProj.Z;
                            
							candidatesToSelect.Add(p.Id, diff.Length());
							//Console.WriteLine(p.Id + " " + diff.Length());
						//}
                    }
                }
	            if (candidatesToSelect.Count != 0) {
		            float min = candidatesToSelect.Min((x) => x.Value);
					int index = candidatesToSelect.First( (y) => y.Value <= min).Key;
					var sVertice = ParticleList.First( (x) => x.Id == index);
					selectedVertice = ParticleList.IndexOf(sVertice);
					VerticeIndex	= index;
		            VerticePosition = sVertice.Position;
		            //Console.WriteLine(index);
		            //Console.WriteLine(VerticePosition);
	            }
            }
        }

	    private int selectedVertice = -1;
	    public bool staticMode;

	    public void GoToStatic()
	    {
		    staticMode = true;
	    }

	    public void GoToDynamic()
	    {
		    staticMode = false;
		    edgeIterator = 0;
			AddMaxParticles();
	    }

	    public void writeMyName()
	    {
		    System.IO.StreamWriter file = new System.IO.StreamWriter("d:\\outCall.txt");
		    int id = 0;
		    foreach (var elem in injectionBufferCPU)
		    {
			    string line = id + ";" + elem.Id + ";" + elem.Position.X + ";" + elem.Position.Y + ";" + elem.Position.Z + ";" + elem.Group; 
				file.WriteLine(line);
			    id++;
		    }
			

			file.Close();
	    }

	    public void readAwesomeClusters()
	    {
		    StreamReader fileClusters = new StreamReader(@"d:\TestData\testClusters.txt");
			//StreamReader fileEdgesByIteration = new StreamReader(@"../../../Data/VKRepost/Viborg/links");

			string str;
			int counter = 0;
			while ( ( str = fileClusters.ReadLine() ) != null )
			{
				var vert = str.Split(';');
				int id = int.Parse(vert[0]);
				injectionBufferCPU[id].Charge = int.Parse(vert[1]);
				injectionBufferCPU[id].Position = new Vector3(float.Parse(vert[2]), float.Parse(vert[3]), float.Parse(vert[4]));
				counter++;
			}
			fileClusters.Close();

			Dictionary<int, List<int>> clustering = new Dictionary<int, List<int>>();

		    foreach (var elem in injectionBufferCPU)
		    {
			    int group = elem.Group;
				List<int> list;
				if (!clustering.TryGetValue(group, out list))
				clustering.Add(group, list = new List<int>());
			    
				list.Add(elem.Charge);
			    
		    }

		    int i = 0;
		    foreach (var p in ParticleList) {
			    List<int> list;
			    clustering.TryGetValue(p.Group, out list);
				list = list.Distinct().ToList();
			    int cluster = list.FindIndex(x => x == injectionBufferCPU[i].Charge);
			    
			    if (injectionBufferCPU[i].Group == 0) {
				    switch (cluster) {
					    case 0:
						    cluster = 6;
							break;
						case 4: 
						    cluster = 6;
							break;
						case 1:
						    cluster = 5;
							break;
				    }
			    }
				injectionBufferCPU[i].Cluster =  cluster;

			    injectionBufferCPU[i].Cluster = (injectionBufferCPU[i].Group == 2 && cluster == 0) ? 4 : cluster;
			  // if (injectionBufferCPU[i].Charge == 9) injectionBufferCPU[i].Charge = 2;
				injectionBufferCPU[i].Color = ColorConstant.paletteByCluster.ElementAt(injectionBufferCPU[i].Cluster).ToVector4();
				i++;
		    }
			Clustering = false;
		    state = State.PAUSE;
		    informationSpreading = true;
			//int j = 0;
			//foreach (var p in ParticleList)
			//{
			    
			//}
	    }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="stereoEye"></param>
        public void Draw(GameTime gameTime, StereoEye stereoEye)
        {
			var device	= Game.GraphicsDevice;
			var cam		= Camera;

            param.View			= cam.GetViewMatrix(stereoEye);
            param.Projection	= cam.GetProjectionMatrix(stereoEye);
            param.MaxParticles	= 0;
            param.DeltaTime		= gameTime.ElapsedSec;
            param.CalculationRadius		= cfg.MinParticleRadius * 100;
	        param.Mass			= cfg.Min_mass;
            param.LinkSize		= cfg.LinkSize;
	        param.StartIndex	= 0;
	        param.EndIndex		= graph.NodesCount;
			//device.SetTargets(device.BackbufferDepth, device.BackbufferColor);


            //device.ComputeShaderConstants[0]	= paramsCB;
            device.VertexShaderConstants[0]		= paramsCB;
            device.GeometryShaderConstants[0]	= paramsCB;
            device.PixelShaderConstants[0]		= paramsCB;


            device.PixelShaderSamplers[0]		= SamplerState.LinearWrap;

            //	Simulate : ------------------------------------------------------------------------
            //

            param.MaxParticles = injectionCount;
            paramsCB.SetData(param);


            device.ComputeShaderConstants[0] = paramsCB;


				
            // ------------------------------------------------------------------------------------


            //	Render: ---------------------------------------------------------------------------
            //
			

			device.SetCSRWBuffer(0, null);

			// draw lines: --------------------------------------------------------------------------

				device.PipelineState = factory[(int)Flags.DRAW | (int)Flags.LINE];
				device.GeometryShaderResources[1] = simulationBufferSrc;
				device.GeometryShaderResources[3] = linksBuffer;
				device.PixelShaderSamplers[0] = SamplerState.AnisotropicWrap;
				device.PixelShaderResources[4] = stroke;
				device.PixelShaderResources[5] = gradLink;
				device.Draw(linkList.Count, 0);


            // draw points: ------------------------------------------------------------------------


            device.PipelineState = factory[(int)Flags.DRAW | (int)Flags.POINT];
			device.PixelShaderSamplers[0] = SamplerState.LinearWrap;
            device.PixelShaderResources[0] = texture;
			//if(finish)
			{
				device.PixelShaderResources[4] = stroke;
				device.PixelShaderResources[5] = border;
			}

			device.GeometryShaderResources[1] = simulationBufferSrc;

            device.Draw(ParticleList.Count, 0);

			

			// --------------------------------------------------------------------------------------
			if ( state == State.RUN ) {
				if (ls.ParticleCount > 0)
				{

					ls.Update( 0 ); //(int) LayoutSystem.StepMethod.Fixed 
					Particle3D[] particleArray = new Particle3D[ls.ParticleCount];
					Graph.Vertice[] vertArr = new Graph.Vertice[injectionBufferCPU.Length];
					ls.CurrentStateBuffer.GetData( particleArray );
					for ( int i = 0; i < vertArr.Length; i++ ) {
						var elem = injectionBufferCPU[i];
						elem.Position = particleArray[i].Position;
						elem.Velocity = particleArray[i].Velocity;
						elem.Force = particleArray[i].Force;
						elem.Energy = particleArray[i].Energy;
						elem.Mass = particleArray[i].Mass;
						elem.Charge = (int) particleArray[i].Charge;
						vertArr[i] = elem;
					}
					simulationBufferSrc.Dispose();
					simulationBufferSrc = new StructuredBuffer( Game.GraphicsDevice, typeof( Graph.Vertice ), vertArr.Length, StructuredBufferFlags.Counter );
					simulationBufferSrc.SetData( vertArr );
				}
				
			}
        }
    }
}

