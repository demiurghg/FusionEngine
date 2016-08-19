using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Fusion;
using Fusion.Build;
using Fusion.Core.Mathematics;
using Fusion.Core.Shell;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Audio;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Graphics.Graph;
using Fusion.Engine.Input;
using Fusion.Framework;

namespace GraphTest {

	
	[Command("refreshServers", CommandAffinity.Default)]
	public class RefreshServerList : NoRollbackCommand {
		
		public RefreshServerList( Invoker invoker ) : base(invoker)
		{
		}

		public override void Execute ()
		{
			Invoker.Game.GameInterface.StartDiscovery(4, new TimeSpan(0,0,10));
		}

	}
	
	[Command("stopRefresh", CommandAffinity.Default)]
	public class StopRefreshServerList : NoRollbackCommand {
		
		public StopRefreshServerList( Invoker invoker ) : base(invoker)
		{
		}

		public override void Execute ()
		{
			Invoker.Game.GameInterface.StopDiscovery();
		}

	}




	class CustomGameInterface : Fusion.Engine.Common.UserInterface {

		[GameModule("Console", "con", InitOrder.Before)]
		public GameConsole Console { get { return console; } }
		public GameConsole console;


		[GameModule("GUI", "gui", InitOrder.Before)]
		public FrameProcessor FrameProcessor { get { return userInterface; } }
		FrameProcessor userInterface;

		[GameModule("Graph Configuration", "gr", InitOrder.Before)]
		public	GraphConfig GraphConfig { get { return graph.cfg; } }
		
		RenderWorld		masterView;
		RenderLayer		viewLayer;
		
		private Vector2 prevMousePos;
		private Vector2 mouseDelta;


		private GraphLayer graph;
		private BankModel.BankModel bankModel;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameInterface ( Game game ) : base(game)
		{
			console			=	new GameConsole( game, "conchars");
			userInterface	=	new FrameProcessor( game, @"Fonts\textFont" );
			graph = new GraphLayer(Game);

		}



		float angle = 0;

		private GraphReader gr;
		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{

			var bounds		=	Game.RenderSystem.DisplayBounds;
			masterView		=	Game.RenderSystem.RenderWorld;


			Game.RenderSystem.RemoveLayer(masterView);

			viewLayer = new RenderLayer(Game);
			viewLayer.SpriteLayers.Add(console.ConsoleSpriteLayer);
			Game.RenderSystem.AddLayer(viewLayer);

			//Game.RenderSystem.DisplayBoundsChanged += (s,e) => {
			//	masterView.Resize( Game.RenderSystem.DisplayBounds.Width, Game.RenderSystem.DisplayBounds.Height );
			//};
		

			Game.Keyboard.KeyDown += Keyboard_KeyDown;

			LoadContent();

			Game.Reloading += (s,e) => LoadContent();



			Game.Touch.Tap			+= args => System.Console.WriteLine("You just perform tap gesture at point: " + args.Position);
			Game.Touch.DoubleTap	+= args => System.Console.WriteLine("You just perform double tap gesture at point: " + args.Position);
			Game.Touch.SecondaryTap += args => System.Console.WriteLine("You just perform secondary tap gesture at point: " + args.Position);
			Game.Touch.Manipulate	+= args => System.Console.WriteLine("You just perform touch manipulation: " + args.Position + "	" + args.ScaleDelta + "	" + args.RotationDelta + " " + args.IsEventBegin + " " + args.IsEventEnd);


			graph.Camera = new GreatCircleCamera();
			graph.cfg.LinkPath = @".\Data\Banks\edges";
			bankModel = new BankModel.BankModel();

			Graph g;
			gr = new GraphReader();
			ReadBankModel( graph.cfg, bankModel, out g);

			graph.SetGraph(g);
			graph.Initialize();
			graph.staticMode = false;
			graph.AddMaxParticles();
			bankModel = new BankModel.BankModel();
			bankModel.Launch(0);
			//graph.state = State.RUN;
				
			viewLayer.GraphLayers.Add(graph);
		}

		Random rand = new Random();
		public void ReadBankModel(GraphConfig gcConfig, BankModel.BankModel model,  out Graph graph)
		{
			Dictionary<int, Graph.Vertice> allVertices = new Dictionary<int, Graph.Vertice>();
			List<Graph.Link> allEdges = new List<Graph.Link>();
			Dictionary<int, int> counterInGroup = new Dictionary<int, int>();
			Dictionary<int, List<int>> neighboors = new Dictionary<int, List<int>>();
			graph = new Graph();
			
			//read all edges

			int id;// = 0;
			foreach (var b in model.bSystem.Banks)
			{
				id = int.Parse(b.ID.Substring(1));
					int groupId = (id >= 100) ? 1 : 2;
					Graph.Vertice node = new Graph.Vertice()
					{
						Position = gr.RadialRandomVector3D()* 1000,//* rand.Next(1000),//gcConfig.LinkSize,
						Velocity = Vector3.Zero,
						Color = ColorConstant.paletteWhite.ElementAt(groupId).ToVector4(),
						Size = gcConfig.MinParticleRadius * 2,
						Acceleration = Vector3.Zero,
						Mass = 0,
						Information = (groupId == 2)? 1 : 0,
						Id = id,
						Group = groupId,
						Charge = 1,
						Cluster = 0,
					};
					allVertices.Add( id, node);
					neighboors.Add(id, new List<int>());
					int count;
					if (counterInGroup.TryGetValue(groupId, out count))
					{
						counterInGroup.Remove(groupId);
						counterInGroup.Add(groupId, count + 1);
					}
					else
					{
						counterInGroup.Add(groupId, 1);
					}
				//id ++;
			}
				
			foreach (var c in model.bSystem.Customers)
			{
				id = int.Parse(c.ID.Substring(1)) + 100;
					int groupId = (id >= 100) ? 1 : 2;
					Graph.Vertice node = new Graph.Vertice()
					{
						Position = gr.RadialRandomVector3D()* 1000,//rand.Next(1000),//gcConfig.LinkSize,
						Velocity = Vector3.Zero,
						Color = ColorConstant.paletteWhite.ElementAt(groupId).ToVector4(),
						Size = gcConfig.MinParticleRadius,
						Acceleration = Vector3.Zero,
						Mass = 0,
						Information = (groupId == 2)? 1 : 0,
						Id = id,
						Group = groupId,
						Charge = 1,
						Cluster = 0,
					};
					allVertices.Add( id, node);
					neighboors.Add(id, new List<int>());
					int count;
					if (counterInGroup.TryGetValue(groupId, out count))
					{
						counterInGroup.Remove(groupId);
						counterInGroup.Add(groupId, count + 1);
					}
					else
					{
						counterInGroup.Add(groupId, 1);
					}
			}

			foreach (var edge in model.bSystem.AllEdgesOverSimulation)
			{
				Graph.Link link = new Graph.Link()
				{
					SourceID = int.Parse(edge.Source.Substring(1)) + (edge.Source[0].Equals('c') ? 100 : 0),
					StockID = int.Parse(edge.Target.Substring(1)) + (edge.Target[0].Equals('c') ? 100 : 0),
					Length = 50,
					Force = 0,
					Orientation = Vector3.Zero,
					Weight = edge.Weight,
					LinkType = 0,
					Color = gr.ColorForBank(edge.Maturity).ToVector4(), //ColorConstant.paletteByGroup.ElementAt(int.Parse(first[0]) - 1).ToVector4(),
					Width = gcConfig.MaxLinkWidth,
					LifeTime = edge.Maturity,
					TotalLifeTime = edge.Maturity,
				};
				if (edge.Source[0].Equals('c')) link.Color.W = 0;
				allEdges.Add(link);
				List<int> list;
				if (!neighboors.TryGetValue(link.SourceID, out list))
				neighboors.Add(link.SourceID, list = new List<int>());
				list.Add(link.StockID);

				if (!neighboors.TryGetValue(link.StockID, out list))
				neighboors.Add(link.StockID, list = new List<int>());
				list.Add(link.SourceID);
			}
				
			
			counterInGroup = counterInGroup.OrderByDescending( (x) => x.Value ).ToDictionary( pair => pair.Key, pair => pair.Value );
			int s = counterInGroup.Values.Max();
			//counterInGroup.TryGetValue( , out s );
			float maxCircleRadius = s*gcConfig.MinParticleRadius/(MathUtil.Pi * 2);
			foreach (var pair in allVertices)
			{
				var node = pair.Value;
				node.ColorType = (counterInGroup.Keys.ToList().IndexOf(node.Group) == 0) ? maxCircleRadius : maxCircleRadius* ( 1 - (float) ( counterInGroup.Keys.ToList().IndexOf( node.Group )) / counterInGroup.Count) ;
				graph.nodes.Add( node);
			}

			//Log.Message(counterInGroup.Keys.ToString());
			graph.links = allEdges;
			graph.neighboors = neighboors;
			graph.NodesCount = graph.nodes.Count;
		}


		void LoadContent ()
		{
			
		}


		void Keyboard_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.F5) {

				Builder.SafeBuild();
				Game.Reload();
			}
			
			if (e.Key==Keys.Q)
			{
				graph.Pause();
			}
			if (e.Key == Keys.C)
			{
				whereTimeGoes = !whereTimeGoes;
				graph.Camera.TargetCenterOfOrbit = Vector3.Zero;
				graph.Rewind = true;
				counter = graph.cfg.StartRewind;
			}
		}

		public List<Graph.Link> getNewEdges(GraphConfig gcConfig, BankModel.BankModel model)
		{
			List<Graph.Link> allEdges = new List<Graph.Link>();

			foreach (var edge in model.bSystem.ENetwork)
			{
				var link = new Graph.Link()
				{
					SourceID = int.Parse(edge.Source.Substring(1)) + (edge.Source[0].Equals('c') ? 100 : 0),
					StockID = int.Parse(edge.Target.Substring(1)) + (edge.Target[0].Equals('c') ? 100 : 0),
					Length = 50,
					Force = 0,
					Orientation = Vector3.Zero,
					Weight = edge.Weight,
					LinkType = 0,
					Color = ColorConstant.paletteWhite.ElementAt((int.Parse(edge.Source.Substring(1)) > 100) ? 1 : 2).ToVector4(),
					//ColorConstant.paletteByGroup.ElementAt(int.Parse(first[0]) - 1).ToVector4(),
					Width = gcConfig.MaxLinkWidth,
					LifeTime = edge.Maturity,
					TotalLifeTime = edge.Maturity,
				};
				if (edge.Source[0].Equals('c')) link.Color.W = 0;
				allEdges.Add(link);
			}

			foreach (var edge in model.bSystem.IbNetwork)
			{
				var link = new Graph.Link()
				{
					SourceID = int.Parse(edge.Source.Substring(1)) + (edge.Source[0].Equals('c') ? 100 : 0),
					StockID = int.Parse(edge.Target.Substring(1)) + (edge.Target[0].Equals('c') ? 100 : 0),
					Length = 50,
					Force = 0,
					Orientation = Vector3.Zero,
					Weight = edge.Weight,
					LinkType = 0,
					Color = ColorConstant.paletteWhite.ElementAt((int.Parse(edge.Source.Substring(1)) > 100) ? 1 : 2).ToVector4(),
					//ColorConstant.paletteByGroup.ElementAt(int.Parse(first[0]) - 1).ToVector4(),
					Width = gcConfig.MaxLinkWidth,
					LifeTime = edge.Maturity,
					TotalLifeTime = edge.Maturity,
				};
				if (edge.Source[0].Equals('c')) link.Color.W = 0;
				allEdges.Add(link);
			}

			return allEdges;
		}

		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				masterView.Dispose();
				viewLayer.Dispose();
			}
			base.Dispose( disposing );
		}


		public override void RequestToExit ()
		{
			Game.Exit();
		}




		float frame = 0;
		private bool whereTimeGoes = true;
		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			console.Update( gameTime );

			graph.Camera.Update(gameTime);


			mouseDelta		= Game.Mouse.Position - prevMousePos;
			prevMousePos	= Game.Mouse.Position;


			
			tt += gameTime.ElapsedSec;
			if (tt > seconds)
			{
				tt = tt - seconds;
				if ( counter < 1000)
				{
					if (graph.state == State.RUN && whereTimeGoes)
					{
						counter++;
						bankModel.Launch(counter);
						//graph.numberOfEdgesPerIteration = new List<int>();
						graph.edgeIterator = 0;
						graph.graph.links = getNewEdges(graph.cfg, bankModel);
						foreach (var bank in bankModel.bSystem.Banks)
						{
							var node = graph.ParticleList.Find((x) => x.Id == int.Parse(bank.ID.Substring(1)));
							int id = graph.ParticleList.IndexOf(node);
							node.Size = GraphConfig.MinParticleRadius * (bank.NW + 1);
							node.Charge = bank.EL + bank.IL;
							Log.Message(node.Size+"");
							if (bank.NW < 0)
							{
								node.Color = ColorConstant.Orange.ToVector4();
							}
							graph.ParticleList.RemoveAt(id);
							graph.ParticleList.Insert(id, node);
						}
						graph.numberOfEdgesPerIteration.Add(graph.graph.links.Count);
					}
					else {if (counter > graph.cfg.EndRewind && graph.Rewind ) counter--;}
					Log.Message(counter + "");
					graph.createLinksFromFile(counter);
				}
			}
			
		}

		private int counter = 0;
		private float tt = 1;
		private float seconds = 5;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="endPoint"></param>
		/// <param name="serverInfo"></param>
		public override void DiscoveryResponse ( System.Net.IPEndPoint endPoint, string serverInfo )
		{
			Log.Message("DISCOVERY : {0} - {1}", endPoint.ToString(), serverInfo );
		}
	}
}
