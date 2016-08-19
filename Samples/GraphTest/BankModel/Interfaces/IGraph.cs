using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphTest.BankModel.Structs;
using IronPython.Hosting;
using IronPython.Runtime;
using log4net;
using Microsoft.Scripting.Hosting;

namespace GraphTest.BankModel.Interfaces
{
    interface IGraph
    {
        /// <summary>
        /// Generate list of interbank edges
        /// </summary>
        /// <returns></returns>
        IEnumerable<Edge> Generate();
    }

    public abstract class NxGraph// : IGraph
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(NxGraph));
        protected static ScriptRuntime Runtime;
        protected static string PathToNetworkX;
        //protected Graph(){}
        //public IList Generate(dynamic script)
        //{
        //    return new List();
        //}

        protected void InitEngine()
        {
            // prepare engine
            var info = new DirectoryInfo((Environment.CurrentDirectory));
            if (info.Parent != null)
	            if (info.Parent.Parent != null)
		            PathToNetworkX = Path.Combine(@"C:\Program Files (x86)\Python 3.5\Lib\site-packages", "networkx-1.9.1-py3.5.egg");
            //const string pathToNetworkX = @"D:\Valenitna\Downloads\STATIC\"; info.Parent.Parent.FullName

            // host python
            ScriptEngine engine = Python.CreateEngine();
            ICollection<string> searchingPaths = engine.GetSearchPaths();
            if (!Directory.Exists(@"C:\Program Files (x86)\IronPython 2.7"))
            {
                Log.Fatal("You need IronPython 2.7 to install!");
                throw new Exception("You must have IronPython being installed");
            }
            searchingPaths.Add(@"C:\Program Files (x86)\IronPython 2.7\Lib");
			searchingPaths.Add(@"C:\Program Files (x86)\IronPython 2.7.zip");
			searchingPaths.Add(@"C:\Program Files (x86)\IronPython 2.7\Lib");
			searchingPaths.Add(@"C:\Program Files (x86)\IronPython 2.7\DLLs");
			searchingPaths.Add(@"C:\Program Files (x86)\IronPython 2.7");
			searchingPaths.Add(@"C:\Program Files (x86)\IronPython 2.7\lib\site-packages");
			searchingPaths.Add(@"C:\Python34\Lib\site-packages");
			searchingPaths.Add(@"C:\Python27\Lib\site-packages");
			searchingPaths.Add(@"C:\Program Files (x86)\Python 3.5\Lib\site-packages");
			searchingPaths.Add(@"C:\Program Files (x86)\Python 3.5\Lib\site-packages\decorator-4.0.10-py3.5.egg");
           
            // add packages for script
            searchingPaths.Add(PathToNetworkX);
            searchingPaths.Add(Path.Combine(PathToNetworkX, "networkx"));
            searchingPaths.Add(@"D:\HPC\BANK\scipy-0.16.1\scipy");
            searchingPaths.Add(@"D:\HPC\BANK\numpy-1.11.1\numpy");
            // for further code: paths.Add(String.Concat(pathToNetworkX, @"networkx-1.9.1.tar\dist\networkx-1.9.1\networkx-1.9.1\networkx\generators"));
            engine.SetSearchPaths(searchingPaths);

            Runtime = engine.Runtime;
        }

    }

    class EmptyGraph : IGraph
    {
        public IEnumerable<Edge> Generate()
        {
            return new List<Edge>();
        }
    }


    class BarabasiAlbertGraph : NxGraph,IGraph
    {
        /// <summary>
        /// number of nodes in the network
        /// </summary>
        private readonly int _nodes;
        /// <summary>
        /// Number of edges to attach from a new node to existing nodes
        /// </summary>
        private readonly int _attached;

        public BarabasiAlbertGraph(int nodes, int attached)
        {
            InitEngine();
            _nodes = nodes;
            _attached = attached;
        }

        public IEnumerable<Edge> Generate()
        {
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\random_graphs.py")))
                Log.Error("Python script file does not exist!");
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\directed.py")))
                Log.Error("Python script file does not exist!");
            
            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, @"networkx\generators\random_graphs.py"));
            var tuples = (IList)script.barabasi_albert_graph(_nodes, _attached).edges();
            //var newlist = new List<Edge>();
            return (from PythonTuple tuple in tuples select new Edge("b" + (int) tuple[0], "b" + (int) tuple[1], 1, 3, 0))
                .ToList();
        }
    }
    
    class ConnectedWattsStrogatzGraph : NxGraph, IGraph
    {
        /// <summary>
        /// The number of nodes
        /// </summary>
        private readonly int _nodes;
        /// <summary>
        /// Each node is joined with its k nearest neighbors in a ring topology
        /// </summary>
        private readonly int _kNe;
        /// <summary>
        /// The probability of rewiring each edge
        /// </summary>
        private readonly double _p;
        /// <summary>
        /// Number of attempts to generate a connected graph 
        /// (default=100)
        /// </summary>
        private int _tries;
        /// <summary>
        /// The seed for random number generator.
        /// (Optional)
        /// </summary>
        private int _seed;

        public ConnectedWattsStrogatzGraph(int n, int k, double prob)
        {
            InitEngine();
            _nodes = n;
            _kNe = k;
            _p = prob;
            _tries = 100;
            _seed = 10;
        }

        public ConnectedWattsStrogatzGraph(int n, int k, double prob, int tries, int seed)
        {
            InitEngine();
            _nodes = n;
            _kNe = k;
            _p = prob;
            _tries = tries;
            _seed = seed;
        }
        /// <summary>
        /// Returns a connected Watts–Strogatz small-world graph.
        ///
        /// Attempts to generate a connected graph by repeated generation of Watts–Strogatz small-world graphs. 
        /// An exception is raised if the maximum number of tries is exceeded.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Edge> Generate()
        {
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\random_graphs.py")))
                Log.Error("Python script file does not exist!");
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\directed.py")))
                Log.Error("Python script file does not exist!");

            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, @"networkx\generators\random_graphs.py"));
            var tuples = (IList)script.connected_watts_strogatz_graph(_nodes, _kNe, _p).edges();
            return (from PythonTuple tuple in tuples select new Edge("b" + (int)tuple[0], "b" + (int)tuple[1], 1, 3, 0))
                .ToList();
        }
    }

    class ErdosRenyi : NxGraph, IGraph
    {
        /// <summary>
        /// The number of nodes.
        /// </summary>
        private readonly int _nodes;
        /// <summary>
        /// Probability for edge creation.
        /// </summary>
        private readonly double _prob;
        /// <summary>
        /// Seed for random number generator (default=None).
        /// (Optional)
        /// </summary>
        private int _seed;
        /// <summary>
        ///  If True, this function returns a directed graph.
        /// (Optional, default=false)
        /// </summary>
        private bool _directed;

        public ErdosRenyi(int nodes, double prob)
        {
            InitEngine();
            _nodes = nodes;
            _prob = prob;
        }
        public ErdosRenyi(int nodes, double prob, int seed, bool directed)
        {
            InitEngine();
            _nodes = nodes;
            _prob = prob;
            _seed = seed;
            _directed = directed;
        }

        /// <summary>
        /// Returns a G_{n,p} random graph, also known as an Erdős-Rényi graph or a binomial graph.
        ///
        /// The G_{n,p} model chooses each of the possible edges with probability p.
        /// The functions binomial_graph() and erdos_renyi_graph() are aliases of this function.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Edge> Generate()
        {
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\random_graphs.py")))
                Log.Error("Python script file does not exist!");
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\directed.py")))
                Log.Error("Python script file does not exist!");

            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, @"networkx\generators\random_graphs.py"));
            var tuples = (IList)script.erdos_renyi_graph(_nodes, _prob).edges();
            return (from PythonTuple tuple in tuples select new Edge("b" + (int)tuple[0], "b" + (int)tuple[1], 1, 3, 0))
                .ToList();
        }
    }

    class PowerlawClusterGraph : NxGraph, IGraph
    {
        /// <summary>
        /// the number of nodes
        /// </summary>
        private readonly int _n;
        /// <summary>
        ///  the number of random edges to add for each new node
        /// </summary>
        private readonly int _m;
        /// <summary>
        /// Probability of adding a triangle after adding a random edge
        /// </summary>
        private readonly double _p;
        /// <summary>
        /// Seed for random number generator (optional, default=None).
        /// </summary>
        private readonly int _seed;

        PowerlawClusterGraph(int n, int m, double p)
        {
            InitEngine();
            _n = n;
            _m = m;
            _p = p;
        }
        PowerlawClusterGraph(int n, int m, double p, int seed)
        {
            InitEngine();
            _n = n;
            _m = m;
            _p = p;
            _seed = seed;
        }

        public IEnumerable<Edge> Generate()
        {
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\random_graphs.py")))
                Log.Error("Python script file does not exist!");
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\directed.py")))
                Log.Error("Python script file does not exist!");

            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, @"networkx\generators\random_graphs.py"));
            var tuples = (IList)script.powerlaw_cluster_graph(_n, _m, _p).edges();
            return (from PythonTuple tuple in tuples select new Edge("b" + (int)tuple[0], "b" + (int)tuple[1], 1, 3, 0))
                .ToList();
        }
    }
    
    class RandomPowerlawTree : NxGraph, IGraph
    {
        /// <summary>
        /// The number of nodes.
        /// </summary>
        private readonly int _nodes;
        /// <summary>
        /// Exponent of the power law.
        /// Default value: 3
        /// </summary>
        private readonly float _gamma;
        /// <summary>
        /// Seed for random number generator (default=None).
        /// </summary>
        private readonly int _seed;// 10
        /// <summary>
        /// Number of attempts to adjust the sequence to make it a tree.
        /// </summary>
        private readonly int _tries; // 1000

        public RandomPowerlawTree(int nodes, float gamma)
        {
            InitEngine();
            _nodes = nodes;
            _gamma = gamma;
            _seed = 10;
            _tries = 1000;
        }
        public RandomPowerlawTree(int nodes, float gamma, int seed, int tries)
        {
            InitEngine();
            _nodes = nodes;
            _gamma = gamma;
            _seed = seed;
            _tries = tries;
        }
        /// <summary>
        /// A trial power law degree sequence is chosen 
        /// and then elements are swapped with new elements from a powerlaw distribution 
        /// until the sequence makes a tree (by checking, 
        /// for example, that the number of edges is one smaller than the number of nodes).
        /// Raises:	NetworkXError – If no valid sequence is found within the maximum number of attempts.
        /// </summary>
        /// <returns>Returns a tree with a power law degree distribution.</returns>
        public IEnumerable<Edge> Generate()
        {
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\random_graphs.py")))
                Log.Error("Python script file does not exist!");
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\directed.py")))
                Log.Error("Python script file does not exist!");

            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, @"networkx\generators\random_graphs.py"));
            var tuples = (IList)script.random_powerlaw_tree(_nodes, _gamma, _seed, _tries).edges();
            return (from PythonTuple tuple in tuples select new Edge("b" + (int)tuple[0], "b" + (int)tuple[1], 1, 3, 0))
               .ToList();
        }
    }

    class ScaleFree : NxGraph, IGraph
    {
        /// <summary>
        /// Number of nodes in graph
        /// </summary>
        private readonly int _n;
        /// <summary>
        /// Probability for adding a new node connected to an existing node chosen randomly 
        /// according to the in-degree distribution.
        /// (default=.41)
        /// </summary>
        private double _alpha=.41;
        /// <summary>
        /// Probability for adding an edge between two existing nodes. 
        /// One existing node is chosen randomly according the in-degree distribution 
        /// and the other chosen randomly according to the out-degree distribution.
        /// (default=.54)
        /// </summary>
        private double _beta=.54;
        /// <summary>
        /// Probability for adding a new node connected to an existing node chosen randomly 
        /// according to the out-degree distribution.
        /// (default=.05)
        /// </summary>
        private double _gamma=.05;
        /// <summary>
        ///  Bias for choosing ndoes from in-degree distribution.
        /// (default=.2)
        /// </summary>
        private double _delta_in=.2;
        /// <summary>
        /// Bias for choosing ndoes from out-degree distribution.
        /// (default=0)
        /// </summary>
        private double _delta_out = .0;

        //Use this graph instance to start the process (default=3-cycle).
        // private Graph _create_using = MultiDiGraph

        /// <summary>
        /// Seed for random number generator
        /// (optional)
        /// </summary>
        private int _seed;

        ScaleFree(int nodes)
        {
            InitEngine();
            _n = nodes;
        }
        
        /// <summary>
        /// The sum of alpha, beta, and gamma must be 1.
        /// 
        /// B. Bollobás, C. Borgs, J. Chayes, and O. Riordan, Directed scale-free graphs, 
        /// Proceedings of the fourteenth annual ACM-SIAM Symposium on Discrete Algorithms, 132–139, 2003.
        /// </summary>
        /// <returns>Returns a scale-free directed graph</returns>
        public IEnumerable<Edge> Generate()
        {
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\random_graphs.py")))
                Log.Error("Python script file does not exist!");
            if (!File.Exists(String.Concat(PathToNetworkX, @"\networkx\generators\directed.py")))
                Log.Error("Python script file does not exist!");

            dynamic script = Runtime.UseFile(Path.Combine(PathToNetworkX, @"networkx\generators\directed.py"));
            var tuples = (IList)script.scale_free_graph(_n).edges();
            return (from PythonTuple tuple in tuples select new Edge("b" + (int)tuple[0], "b" + (int)tuple[1], 1, 3, 0))
                .ToList();
        }
    }
}
