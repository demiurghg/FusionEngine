using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics.Graph {
	/// <summary>
	/// base graph class
	/// contains list of edges and nodes
	/// </summary>
	public class Graph {

		// Node in 3d space:
        [StructLayout(LayoutKind.Explicit)]
        public struct Vertice
        {
			[FieldOffset(0)]	public Vector3 Position;
            [FieldOffset(12)]	public Vector3 Velocity;
            [FieldOffset(24)]	public Vector4 Color;
            [FieldOffset(40)]	public float Size;
            [FieldOffset(44)]	public float TotalLifeTime;
            [FieldOffset(48)]	public float LifeTime;
            [FieldOffset(52)]	public int		linksPtr;
            [FieldOffset(56)]	public int		OutDegree;
            [FieldOffset(60)]	public Vector3	Acceleration;
            [FieldOffset(72)]	public float	Mass;
            [FieldOffset(76)]	public int		Charge;
            [FieldOffset(80)]	public int		Id;
			[FieldOffset(84)]	public float	ColorType;
            [FieldOffset(88)]	public int		Degree;
			[FieldOffset(92)]	public int		Group;
			[FieldOffset(96)]	public int		Information;
	        [FieldOffset(100)]	public float	Energy;
			[FieldOffset(104)]	public Vector3	Force;
			[FieldOffset(116)]	public int		Cluster;
        }

		
        // Edge between 2 particles:
        [StructLayout(LayoutKind.Explicit)]
        public struct Link
        {
            [FieldOffset(0)] public int SourceID;
            [FieldOffset(4)] public int StockID;
            [FieldOffset(8)] public float Length;
            [FieldOffset(12)] public float Force;
            [FieldOffset(16)] public Vector3 Orientation;
			[FieldOffset(28)] public float Weight;
			[FieldOffset(32)] public int LinkType;
			[FieldOffset(36)] public Vector4 Color;
			[FieldOffset(52)] public float Width;
			[FieldOffset(56)] public float TimeOfAppearance;
			[FieldOffset(60)] public float TotalLifeTime;
			[FieldOffset(64)] public float LifeTime;
        }

		public List<Vertice>	nodes;
		public List<Link>		links;
		public int				NodesCount;
		public Dictionary<int, List<int>> neighboors;

		public Graph()
		{
			nodes = new List<Vertice>();
			links = new List<Link>();
			NodesCount = 0;
		}
	}
}
