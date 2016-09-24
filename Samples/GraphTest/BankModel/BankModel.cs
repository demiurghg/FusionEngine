using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphTest.BankModel.Classes;
using GraphTest.BankModel.Interfaces;
using GraphTest.BankModel.Model;
using log4net;
using GraphTest.BankModel.Structs;

namespace GraphTest.BankModel {

    internal enum Policy
    {
        [Description("r")]
        R/*andom*/,
        [Description("pa")]
        Pa/*referential*/,
        [Description("pnw")]
        Pnw,
        [Description("a")]
        A/*ssortative*/
    }
	class BankModel{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        public const  int                 BankNum = 100;
        public const  int                 CustNum = 300;
        public static Policy           BankPolicy = Policy.R;
        public static Policy       CustomerPolicy = Policy.R;
        public static IGraph InitialNetworkConfig = new BarabasiAlbertGraph(BankNum, 5);
        public static bool       WithNodeDeletion = true;
        public static IComparer<Edge> RewiringComparisonA = new CompareEdgesExpiresDescending();
        public static IComparer<Edge> RewiringComparisonL = new CompareEdgesExpiresDescending();
        // public Comparison<Edge> RewiringComparisonA = CompareBanksByAssets;
        // public Comparison<Edge> RewiringComparisonL;
        const int             MaxIter = 30;
        
        private const string EdgesOverSimulationFile = "edges";
        private const string BankBalanceSheetsDir = "balance";
		public  BankingSystem bSystem;

        public BankModel()
        {
            log4net.Config.XmlConfigurator.Configure();
            if (!Directory.Exists(BankBalanceSheetsDir))
                Directory.CreateDirectory(BankBalanceSheetsDir);
            bSystem = new BankingSystem(BankNum, CustNum, InitialNetworkConfig);// todo make a graph type be a Launch() parameter
             // , BankNum, CustNum, InitialNetworkConfig, RewiringComparisonA, RewiringComparisonL);
            //
        }

		/// <summary>
		/// Launch with the elimination of negative net worth nodes
		/// </summary>
		/// <param name="runNumber"></param>
		/// <param name="bankNum"></param>
		/// <param name="custNum"></param>
		/// <param name="graphType">The initial network configuration</param>
		/// <param name="rewiringPolicyA">Parameter 1 for edge rewiring after node elimination</param>
		/// <param name="rewiringPolicyL">Parameter 2 for edge rewiring after node elimination</param>
		public void Launch(int runNumber)
			//, int bankNum, int custNum, IGraph graphType, Comparison<Edge> rewiringPolicyA, Comparison<Edge> rewiringPolicyL )
		{
			Console.WriteLine("Bank Iteration Number " + runNumber);
			if (bSystem.Banks.Count > 2)
			{
				Log.Info("T=" + runNumber + "; bank-nodes=" + (BankNum - bSystem.DeletedNodeIDs.Count));
			if (WithNodeDeletion)
				bSystem.Iteration(BankPolicy, CustomerPolicy, RewiringComparisonA, RewiringComparisonL);
			else
				bSystem.Iteration(BankPolicy, CustomerPolicy);

			bSystem.UpdateProperties(); // update time-dependent network features, save results for previous  iteration
			bSystem.CommitSysState(); // write topologycal features for the current iteration to the corresponding fields
			// OutputDataPerIter(bSystem, i); // update output files
			}
		}

        /// <summary>
        /// Network edges and bank balance sheets output
        /// </summary>
        /// <param name="bSystem"></param>
        /// <param name="i"></param>
        static void OutputDataPerIter(BankingSystem bSystem, int i)
        {
            //   a) edges over simulation
            using (var writer = new StreamWriter(EdgesOverSimulationFile, true))
            {
                foreach (var edge in bSystem.AllEdgesOverSimulation.Where(x => x.Created == i))
                    //TODO DEBUG CONDITION (indexes)
                    writer.WriteLine(edge.ToStringKsenia());
            }
            //    b) bank balance sheets
            using (var writer = new StreamWriter(Path.Combine(BankBalanceSheetsDir, i.ToString("D4"))))
            {
                foreach (var bank in bSystem.Banks)
                    writer.WriteLine(bank.ID + ";" + bank.NW
                                     + ";" + bank.IA
                                     + ";" + bank.EA
                                     + ";" + bank.IL
                                     + ";" + bank.EL);
                //+ ";" + bank.Cash);
            }

            //    c) net features todo
        }
	}
}
