using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics.Graph
{
	/// <summary>
	/// class for read data in different formats
	/// </summary>
	public class GraphReader
	{

		public GraphReader()
		{
			
		}


		// The reader for Trivial Graph Format
		public float ReadTGF(string path, float minParticleRadius, bool dynamic, out Dictionary<int, int> allVertices,
			out List<Tuple<int, int>> allEdges, out List<int> informationSpreadingByEdges, out List<int> edgesLife,
			out Dictionary<int, int> counterInGroup)
		{

			allVertices = new Dictionary<int, int>();
			allEdges = new List<Tuple<int, int>>();
			informationSpreadingByEdges = new List<int>();
			edgesLife = new List<int>();
			counterInGroup = new Dictionary<int, int>();

			//read all edges
			StreamReader fileEdges = new StreamReader(path);
			string line;

			while ((line = fileEdges.ReadLine()) != null)
			{
				var vert = line.Split(' ');
				var first = vert[0].Split('.');
				var second = vert[1].Split('.');
				int firstKey = int.Parse(first[1]);
				int secondKey = int.Parse(second[1]);
				if (!allVertices.ContainsKey(firstKey))
				{
					int groupId = int.Parse(first[0]) - 1;
					allVertices.Add(firstKey, groupId);
					int count;
					if (counterInGroup.TryGetValue(groupId, out count)) {
						counterInGroup.Remove(groupId);
						counterInGroup.Add(groupId, count + 1);
					}
					else {
						counterInGroup.Add(groupId, 1);
					}
				}
				if (!allVertices.ContainsKey(secondKey)) {
					int groupId = int.Parse(second[0]) - 1;
					allVertices.Add(secondKey, groupId);
					int count;
					if (counterInGroup.TryGetValue(groupId, out count)) {
						counterInGroup.Remove(groupId);
						counterInGroup.Add(groupId, count + 1);
					}
					else {
						counterInGroup.Add(groupId, 1);
					}
				}
				allEdges.Add(new Tuple<int, int>(firstKey, secondKey));
				edgesLife.Add(int.Parse(vert[2]));
				informationSpreadingByEdges.Add(int.Parse(vert[3]));
			}
			fileEdges.Close();
			counterInGroup = counterInGroup.OrderByDescending((x) => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
			int s;
			counterInGroup.TryGetValue(0, out s);
			return s*minParticleRadius*2/MathUtil.Pi;
		}

		public void ReadTamaraTGF(string path, GraphConfig gcConfig, out Graph graph)
		{
			Dictionary<int, Graph.Vertice> allVertices = new Dictionary<int, Graph.Vertice>();
			List<Graph.Link> allEdges = new List<Graph.Link>();
			Dictionary<int, int> counterInGroup = new Dictionary<int, int>();
			Dictionary<int, List<int>> neighboors = new Dictionary<int, List<int>>();
			graph = new Graph();

			//read all edges
			StreamReader fileEdges = new StreamReader(path);
			string line;

			while ((line = fileEdges.ReadLine()) != null)
			{
				var vert = line.Split(' ');
				var first = vert[0].Split('.');
				var second = vert[1].Split('.');
				int firstKey = int.Parse(first[1]);
				int secondKey = int.Parse(second[1]);
				if (!allVertices.ContainsKey(firstKey))
				{
					int groupId = int.Parse(first[0]) - 1;
					Graph.Vertice node = new Graph.Vertice()
					{
						Position = RadialRandomVector()* rand.Next(1000),//gcConfig.LinkSize,
						Velocity = Vector3.Zero,
						Color = ColorConstant.paletteWhite.ElementAt(groupId).ToVector4(),
						Size = gcConfig.MinParticleRadius,
						Acceleration = Vector3.Zero,
						Mass = 0,
						Information = (groupId == 2)? 1 : 0,
						Id = firstKey,
						Group = groupId,
						Charge = 0,
						Cluster = 0,
					};
					allVertices.Add( firstKey, node);
					neighboors.Add(firstKey, new List<int>());
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
					if (!allVertices.ContainsKey(secondKey))
					{
						int groupId = int.Parse(second[0]) - 1;
						Graph.Vertice node = new Graph.Vertice()
						{
							Position = RadialRandomVector() * rand.Next(1000),//gcConfig.LinkSize,
							Velocity = Vector3.Zero,
							Color = ColorConstant.paletteWhite.ElementAt(groupId).ToVector4(),
							Size = gcConfig.MinParticleRadius,
							Acceleration = Vector3.Zero,
							Mass = 0,
							Information = (groupId == 2)? 1 : 0,
							Id = secondKey,
							Group = groupId,
							Charge = 0,
							Cluster = 0,
						};
						allVertices.Add( secondKey, node);
						neighboors.Add(secondKey, new List<int>());

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
				Graph.Link link = new Graph.Link()
				{
					SourceID = firstKey,
					StockID = secondKey,
					Length = 50,
					Force = 0,
					Orientation = Vector3.Zero,
					Weight = gcConfig.MaxLinkWidth,
					LinkType = int.Parse(vert[3]),
					Color = ColorConstant.paletteByGroup.ElementAt(int.Parse(first[0]) - 1).ToVector4(),
					Width = gcConfig.MaxLinkWidth,
					LifeTime = int.Parse(vert[2]),
					TotalLifeTime = int.Parse(vert[2]),
				};
				allEdges.Add(link);
				List<int> list;
				if (!neighboors.TryGetValue(firstKey, out list))
				neighboors.Add(firstKey, list = new List<int>());
				list.Add(secondKey);

				if (!neighboors.TryGetValue(secondKey, out list))
				neighboors.Add(secondKey, list = new List<int>());
				list.Add(firstKey);
			}
			fileEdges.Close();
			counterInGroup = counterInGroup.OrderByDescending( (x) => x.Value ).ToDictionary( pair => pair.Key, pair => pair.Value );
			int s;
			counterInGroup.TryGetValue( 0, out s );
			float maxCircleRadius = s*gcConfig.MinParticleRadius/MathUtil.Pi;
			foreach (var pair in allVertices)
			{
				var node = pair.Value;
				node.ColorType = (counterInGroup.Keys.ToList().IndexOf(node.Group) == 0) ? maxCircleRadius : maxCircleRadius* ( 1 - (float) ( counterInGroup.Keys.ToList().IndexOf( node.Group )) / counterInGroup.Count) ;
				graph.nodes.Add( node);
			}
			// = allVertices.Values.ToList();
			graph.links = allEdges;
			graph.neighboors = neighboors;
			graph.NodesCount = graph.nodes.Count;
		}

		public void ReadMedicineTGF(string path, GraphConfig gcConfig, out Graph graph)
		{
			Dictionary<int, Graph.Vertice> allVertices = new Dictionary<int, Graph.Vertice>();
			List<Graph.Link> allEdges = new List<Graph.Link>();

			graph = new Graph();
			StreamReader fileEdges = new StreamReader(path);
			string line;
			Console.WriteLine("start");
			while ((line = fileEdges.ReadLine()) != null)
			{
				var vert = line.Split(' ');
				var first = vert[0].Split('.');
				var second = vert[1].Split('.');
				int firstKey = int.Parse(first[1]);
				int secondKey = int.Parse(second[1]);
				if (!allVertices.ContainsKey(firstKey))
				{
					int groupId = int.Parse(first[0]);
					Graph.Vertice node = new Graph.Vertice()
					{
						Position = RadialRandomVector3D() *  5000,
						Velocity = Vector3.Zero,
						Color = ColorConstant.paletteByGroup.ElementAt(groupId).ToVector4(),
						Size = gcConfig.MinParticleRadius,
						Acceleration = Vector3.Zero,
						Mass = 0.001f,
						Charge = 1,
						Id = firstKey,
						Group = groupId,
					};
					allVertices.Add( firstKey, node);
				}
					if (!allVertices.ContainsKey(secondKey))
					{
						int groupId = int.Parse(second[0]);
						Graph.Vertice node = new Graph.Vertice()
						{
							Position = RadialRandomVector3D() *  5000,
							Velocity = Vector3.Zero,
							Color = ColorConstant.paletteByGroup.ElementAt(groupId).ToVector4(),
							Size = gcConfig.MinParticleRadius,
							Acceleration = Vector3.Zero,
							Mass = 0.001f,
							Charge = 1,
							Id = secondKey,
							Group = groupId,
						};
						allVertices.Add( secondKey, node);
					}	
				Graph.Link link = new Graph.Link()
				{
					SourceID = firstKey,
					StockID = secondKey,
					Length = 1,
					Force = 0,
					Orientation = Vector3.Zero,
					Weight = 0.1f,//int.Parse(vert[4]),
					LinkType = int.Parse(vert[3]),
					Color = new Vector4(Color.White.ToVector3(), gcConfig.EdgeMaxOpacity),////ui.paletteByGroup.ElementAt(int.Parse(first[0])).ToVector4(),
					Width = gcConfig.MaxLinkWidth,
					LifeTime = int.Parse(vert[2]) * 10000,
					TotalLifeTime = int.Parse(vert[2]) * 10000,
				};
				allEdges.Add(link);
			}
			fileEdges.Close();
			
			foreach (var pair in allVertices)
			{
				var node = pair.Value;
				node.ColorType = 0;
				graph.nodes.Add( node);
			}
			// = allVertices.Values.ToList();
			graph.NodesCount = graph.nodes.Count;
			graph.links = allEdges;
		}

		// The reader for Trivial Graph Format
		public void ReadVKRepostsTGF(string path, GraphConfig gcConfig, out Graph graph)
		{

			Dictionary<int, Graph.Vertice> allVertices = new Dictionary<int, Graph.Vertice>();
			List<Graph.Link> allEdges = new List<Graph.Link>();
			Dictionary<int, int> counterInGroup = new Dictionary<int, int>();
			graph = new Graph();

			//read all edges
			StreamReader fileEdges = new StreamReader(path);
			string line;

			while ((line = fileEdges.ReadLine()) != null)
			{
				var vert = line.Split(' ');
				int firstKey = int.Parse(vert[0]);
				int secondKey = int.Parse(vert[1]);
				if (!allVertices.ContainsKey(firstKey))
				{
					int groupId = (firstKey > 0) ? 1 : 0;
					Graph.Vertice node = new Graph.Vertice()
					{
						Position = RadialRandomVector3D() * 100.0f,
						Velocity = Vector3.Zero,
						Color = ColorConstant.paletteByGroup.ElementAt(groupId).ToVector4(),
						Size = gcConfig.MinParticleRadius,
						Acceleration = Vector3.Zero,
						Mass = 0.001f,
						Charge = 0,
						Id = firstKey,
						Group = groupId,
					};
					allVertices.Add( firstKey, node);
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
					if (!allVertices.ContainsKey(secondKey))
					{
						int groupId = (secondKey > 0) ? 1 : 0;
						Graph.Vertice node = new Graph.Vertice()
						{
							Position = RadialRandomVector3D() * 100.0f,
							Velocity = Vector3.Zero,
							Color = ColorConstant.paletteByGroup.ElementAt(groupId).ToVector4(),
							Size = gcConfig.MinParticleRadius,
							Acceleration = Vector3.Zero,
							Mass = 0.001f,
							Charge = 0,
							Id = secondKey,
							Group = groupId,
						};
						allVertices.Add( secondKey, node);
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
				Graph.Link link = new Graph.Link()
				{
					SourceID = firstKey,
					StockID = secondKey,
					Length = 50,
					Force = 0,
					Orientation = Vector3.Zero,
					Weight = gcConfig.MaxLinkWidth,
					LinkType = 0,
					Color = ColorConstant.paletteByGroup.ElementAt((firstKey > 0) ? 1 : 0).ToVector4(),
					Width = gcConfig.MaxLinkWidth,
					LifeTime = 10000,
					TotalLifeTime = 10000,
				};
				allEdges.Add(link);
			}
			fileEdges.Close();
			
			foreach (var pair in allVertices)
			{
				var node = pair.Value;
				node.ColorType = 0;
				graph.nodes.Add( node);
			}
			graph.links = allEdges;
			graph.NodesCount = graph.nodes.Count;
		}

		public void ReadBankTGF(GraphConfig gcConfig, out Graph graph)
		{
			Dictionary<int, Graph.Vertice> allVertices = new Dictionary<int, Graph.Vertice>();
			List<Graph.Link> allEdges = new List<Graph.Link>();
			Dictionary<int, int> counterInGroup = new Dictionary<int, int>();
			Dictionary<int, List<int>> neighboors = new Dictionary<int, List<int>>();
			graph = new Graph();

			//read all edges
			StreamReader fileEdges = new StreamReader(gcConfig.LinkPath);
			string line;

			while ((line = fileEdges.ReadLine()) != null)
			{
				var vert = line.Split(';');
				int firstKey = int.Parse(vert[0]);
				int secondKey = int.Parse(vert[1]);
				if (!allVertices.ContainsKey(firstKey))
				{
					int groupId = (firstKey > 100) ? 1 : 2;
					Graph.Vertice node = new Graph.Vertice()
					{
						Position = RadialRandomVector()* rand.Next(1000),//gcConfig.LinkSize,
						Velocity = Vector3.Zero,
						Color = ColorConstant.paletteWhite.ElementAt(groupId).ToVector4(),
						Size = gcConfig.MinParticleRadius,
						Acceleration = Vector3.Zero,
						Mass = 0,
						Information = (groupId == 2)? 1 : 0,
						Id = firstKey,
						Group = groupId,
						Charge = 1,
						Cluster = 0,
					};
					allVertices.Add( firstKey, node);
					neighboors.Add(firstKey, new List<int>());
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
					if (!allVertices.ContainsKey(secondKey))
					{
						int groupId = (secondKey > 100) ? 1 : 2;
						Graph.Vertice node = new Graph.Vertice()
						{
							Position = RadialRandomVector() * rand.Next(1000),//gcConfig.LinkSize,
							Velocity = Vector3.Zero,
							Color = ColorConstant.paletteWhite.ElementAt(groupId).ToVector4(),
							Size = gcConfig.MinParticleRadius,
							Acceleration = Vector3.Zero,
							Mass = 0,
							Information = (groupId == 2)? 1 : 0,
							Id = secondKey,
							Group = groupId,
							Charge = 1,
							Cluster = 0,
						};
						allVertices.Add( secondKey, node);
						neighboors.Add(secondKey, new List<int>());

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
				int lifetime = int.Parse(vert[3]);
				Graph.Link link = new Graph.Link()
				{
					SourceID = firstKey,
					StockID = secondKey,
					Length = 50,
					Force = 0,
					Orientation = Vector3.Zero,
					Weight = float.Parse(vert[4], NumberStyles.Any, CultureInfo.InvariantCulture) / 1000,
					LinkType = 0,
					Color = ColorForBank(lifetime).ToVector4(), //ColorConstant.paletteByGroup.ElementAt(int.Parse(first[0]) - 1).ToVector4(),
					Width = gcConfig.MaxLinkWidth,
					LifeTime = lifetime,
					TotalLifeTime = lifetime,
				};
				allEdges.Add(link);
				List<int> list;
				if (!neighboors.TryGetValue(firstKey, out list))
				neighboors.Add(firstKey, list = new List<int>());
				list.Add(secondKey);

				if (!neighboors.TryGetValue(secondKey, out list))
				neighboors.Add(secondKey, list = new List<int>());
				list.Add(firstKey);
			}
			fileEdges.Close();
			counterInGroup = counterInGroup.OrderByDescending( (x) => x.Value ).ToDictionary( pair => pair.Key, pair => pair.Value );
			int s;
			counterInGroup.TryGetValue( 0, out s );
			float maxCircleRadius = s*gcConfig.MinParticleRadius/MathUtil.Pi;
			foreach (var pair in allVertices)
			{
				var node = pair.Value;
				node.ColorType = (counterInGroup.Keys.ToList().IndexOf(node.Group) == 0) ? maxCircleRadius : maxCircleRadius* ( 1 - (float) ( counterInGroup.Keys.ToList().IndexOf( node.Group )) / counterInGroup.Count) ;
				graph.nodes.Add( node);
			}
			// = allVertices.Values.ToList();
			graph.links = allEdges;
			graph.neighboors = neighboors;
			graph.NodesCount = graph.nodes.Count;
		}

		public Color ColorForBank(int lifetime)
		{
			if (lifetime <= 1)
				return ColorConstant.paletteByCluster[0];
			
			if (lifetime <= 30) return ColorConstant.paletteByCluster[2];
			return ColorConstant.paletteByCluster[3];
		}

		Random rand = new Random();
		/// <summary>
        /// Returns random radial vector
        /// </summary>
        /// <returns></returns>
       public Vector3 RadialRandomVector()
        {
			Vector2 r;
            do
            {
                r = rand.NextVector2(-Vector2.One, Vector2.One);
            } while (r.Length() > 1);

            r.Normalize();

            return new Vector3(0, r.X, r.Y); //
        }

		public Vector3 RadialRandomVector3D()
        {
			Vector3 r;
            do
            {
                r = rand.NextVector3(-Vector3.One, Vector3.One);
            } while (r.Length() > 1);
            r.Normalize();
            return r; //
        }


		public void ReadBankModel<T>(GraphConfig gcConfig, T model,  out Graph graph)
		{
			Dictionary<int, Graph.Vertice> allVertices = new Dictionary<int, Graph.Vertice>();
			List<Graph.Link> allEdges = new List<Graph.Link>();
			Dictionary<int, int> counterInGroup = new Dictionary<int, int>();
			Dictionary<int, List<int>> neighboors = new Dictionary<int, List<int>>();
			graph = new Graph();
			
			//read all edges
			StreamReader fileEdges = new StreamReader(gcConfig.LinkPath);
			string line;

			while ((line = fileEdges.ReadLine()) != null)
			{
				var vert = line.Split(';');
				int firstKey = int.Parse(vert[0]);
				int secondKey = int.Parse(vert[1]);
				if (!allVertices.ContainsKey(firstKey))
				{
					int groupId = (firstKey > 100) ? 1 : 2;
					Graph.Vertice node = new Graph.Vertice()
					{
						Position = RadialRandomVector()* rand.Next(1000),//gcConfig.LinkSize,
						Velocity = Vector3.Zero,
						Color = ColorConstant.paletteWhite.ElementAt(groupId).ToVector4(),
						Size = gcConfig.MinParticleRadius,
						Acceleration = Vector3.Zero,
						Mass = 0,
						Information = (groupId == 2)? 1 : 0,
						Id = firstKey,
						Group = groupId,
						Charge = 1,
						Cluster = 0,
					};
					allVertices.Add( firstKey, node);
					neighboors.Add(firstKey, new List<int>());
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
					if (!allVertices.ContainsKey(secondKey))
					{
						int groupId = (secondKey > 100) ? 1 : 2;
						Graph.Vertice node = new Graph.Vertice()
						{
							Position = RadialRandomVector() * rand.Next(1000),//gcConfig.LinkSize,
							Velocity = Vector3.Zero,
							Color = ColorConstant.paletteWhite.ElementAt(groupId).ToVector4(),
							Size = gcConfig.MinParticleRadius,
							Acceleration = Vector3.Zero,
							Mass = 0,
							Information = (groupId == 2)? 1 : 0,
							Id = secondKey,
							Group = groupId,
							Charge = 1,
							Cluster = 0,
						};
						allVertices.Add( secondKey, node);
						neighboors.Add(secondKey, new List<int>());

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
				int lifetime = int.Parse(vert[3]);
				Graph.Link link = new Graph.Link()
				{
					SourceID = firstKey,
					StockID = secondKey,
					Length = 50,
					Force = 0,
					Orientation = Vector3.Zero,
					Weight = float.Parse(vert[4], NumberStyles.Any, CultureInfo.InvariantCulture) / 1000,
					LinkType = 0,
					Color = ColorForBank(lifetime).ToVector4(), //ColorConstant.paletteByGroup.ElementAt(int.Parse(first[0]) - 1).ToVector4(),
					Width = gcConfig.MaxLinkWidth,
					LifeTime = lifetime,
					TotalLifeTime = lifetime,
				};
				allEdges.Add(link);
				List<int> list;
				if (!neighboors.TryGetValue(firstKey, out list))
				neighboors.Add(firstKey, list = new List<int>());
				list.Add(secondKey);

				if (!neighboors.TryGetValue(secondKey, out list))
				neighboors.Add(secondKey, list = new List<int>());
				list.Add(firstKey);
			}
			fileEdges.Close();
			counterInGroup = counterInGroup.OrderByDescending( (x) => x.Value ).ToDictionary( pair => pair.Key, pair => pair.Value );
			int s;
			counterInGroup.TryGetValue( 0, out s );
			float maxCircleRadius = s*gcConfig.MinParticleRadius/MathUtil.Pi;
			foreach (var pair in allVertices)
			{
				var node = pair.Value;
				node.ColorType = (counterInGroup.Keys.ToList().IndexOf(node.Group) == 0) ? maxCircleRadius : maxCircleRadius* ( 1 - (float) ( counterInGroup.Keys.ToList().IndexOf( node.Group )) / counterInGroup.Count) ;
				graph.nodes.Add( node);
			}
			// = allVertices.Values.ToList();
			graph.links = allEdges;
			graph.neighboors = neighboors;
			graph.NodesCount = graph.nodes.Count;
		}
	}
}
