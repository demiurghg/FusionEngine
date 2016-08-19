using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphTest.BankModel.Structs;
using IronPython.Hosting;
using log4net;
using Microsoft.Scripting.Hosting;

namespace GraphTest.BankModel.Classes
{
    public static partial class Network
    {
        #region CONSTRUCTOR
        private static readonly ILog Log = LogManager.GetLogger(typeof(Network));
        public static readonly string PathToNetworkX;
    
        static readonly ScriptRuntime Runtime;
        

        static Network()
        {
            // prepare engine
            var info = new DirectoryInfo((Environment.CurrentDirectory));
            if (info.Parent != null)
                if (info.Parent.Parent != null)
                    PathToNetworkX = Path.Combine(info.Parent.Parent.FullName, "networkx-1.9.1");
            //const string pathToNetworkX = @"D:\Valenitna\Downloads\STATIC\";

            // host python
            ScriptEngine engine = Python.CreateEngine();
            ICollection<string> searchingPaths = engine.GetSearchPaths();
            if (!Directory.Exists(@"C:\Program Files (x86)\IronPython 2.7"))
            {
                Log.Fatal("You need IronPython 2.7 to install!");
                throw new Exception("You must have IronPython being installed");
            }
            searchingPaths.Add(@"C:\Program Files (x86)\IronPython 2.7\Lib");

            // add packages for script
            searchingPaths.Add(PathToNetworkX);
            searchingPaths.Add(Path.Combine(PathToNetworkX, "networkx"));
            searchingPaths.Add(@"d:\Valenitna\Documents\Projects VS2010\Financial Network Simulation\Financial Network Simulation\scipy-0.16.0\scipy");
            searchingPaths.Add(@"d:\Valenitna\Documents\Projects VS2010\Financial Network Simulation\Financial Network Simulation\numpy-1.9.2\numpy");

            // for further code: paths.Add(String.Concat(pathToNetworkX, @"networkx-1.9.1.tar\dist\networkx-1.9.1\networkx-1.9.1\networkx\generators"));
            engine.SetSearchPaths(searchingPaths);

            Runtime = engine.Runtime;
        }
        #endregion
        
        public static double AverageDegree(IList<Edge> edges)
        {
            var stringEdges = new string[edges.Count];
            for (var i = 0; i < edges.Count; i++)
                stringEdges[i] = edges[i].ToStringNX();
            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, "networkx/topology.py"));
            if (!stringEdges.Any()) return 0;
            return script.average_degree(stringEdges);
        }

        public static double AverageClustering(IList<Edge> edges)
        {
            var stringEdges = new string[edges.Count];
            for (var i = 0; i < edges.Count; i++)
                stringEdges[i] = edges[i].ToStringNX();
            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, "networkx/topology.py"));
            if (!stringEdges.Any()) return 0;
            double res = script.average_clustering(stringEdges);
            return Math.Round(res, 5);
        }
        public static double AverageShortestPath(IList<Edge> edges)
        {
            var stringEdges = new string[edges.Count];
            for (var i = 0; i < edges.Count; i++)
                stringEdges[i] = edges[i].ToStringNX();
            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, "networkx/topology.py"));
            if (!stringEdges.Any()) return 0;
            double res =  script.average_shortest_path(stringEdges);
            return Math.Round(res, 5);
        }
        public static double[] LaplacianSpectrum(IList<Edge> edges)
        {
            throw new NotImplementedException();
            var stringEdges = new string[edges.Count];
            for (var i = 0; i < edges.Count; i++)
                stringEdges[i] = edges[i].ToStringNX();
            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, "networkx/topology.py"));
            return script.l_spectrum(stringEdges);
        }

        public static double Entropy(IEnumerable<double> serie)
        {
            return -serie.Sum(x => x * Math.Log(x));
        }

        #region THERMODYNAMIC FEATURES
        // TODO

        public static int Energy(ICollection<Edge> edges)
        {
            return edges.Count;
        }

        /// <summary>
        /// 1-1/N-(1/N*N)*sum{1/d_i*d_j}
        /// Strongly depends on a network size
        /// </summary>
        internal static double PseudoEntropy(ICollection<Edge> edges)
        {
            var degrees = new SortedDictionary<int, int>();// key is bank id, value -- its degree
            foreach (var edge in edges)
            {
                if (degrees.ContainsKey(Int32.Parse(edge.IntSource())))
                    degrees[Int32.Parse(edge.IntSource())]++;
                else degrees.Add(Int32.Parse(edge.IntSource()), 1);
                if (degrees.ContainsKey(Int32.Parse(edge.IntTarget())))
                    degrees[Int32.Parse(edge.IntTarget())]++;
                else degrees.Add(Int32.Parse(edge.IntTarget()), 1);
            }
            var sumWithDegrees = 0.0;
            for(var i =0; i < degrees.Count-1;i++)
                for(var j=i+1; j<degrees.Count; j++)
                    if (ContainsNodes(edges, i, j))
                        sumWithDegrees += 1.0/(degrees[i]*degrees[j]);
                            
            return Math.Round(1 - 1.0/degrees.Count - (double)sumWithDegrees/Math.Pow(degrees.Count,2), 6);
        }

        private static bool ContainsNodes(IEnumerable<Edge> edges, int id1, int id2)
        {
            string stringId1 = "b" + id1;
            string stringId2 = "b" + id2;
            return edges.Any(
                item =>
                    item.Source == stringId1 && item.Target == stringId2 ||
                    item.Source == stringId2 && item.Target == stringId1);
        }

        #endregion

        
    }
}
