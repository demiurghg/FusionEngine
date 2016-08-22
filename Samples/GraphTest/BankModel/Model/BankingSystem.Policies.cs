using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphTest.BankModel.Model
{
    partial class BankingSystem
    {
        private static readonly Random ForBankChoice = new Random();
        private static readonly Random ForWeightChoice = new Random();
        private static readonly Random ForMaturityChoice = new Random();

        private const int mean = 10;
        private const int dev = 2;

        /// <summary>
        /// Returns the int of bank id.
        /// Bank must exist in the network
        /// </summary>
        /// <returns></returns>
        internal string ChooseBank()
        {
            return Banks[ForBankChoice.Next(0, Banks.Count)].ID;
            /*
            var tarBank = ForBankChoose.Next(0, Banks.Count);
            while (tarBank == curBank)
                tarBank = ForBankChoose.Next(0, Banks.Count);
            return tarBank;
             */
        }

        /// <summary>
        /// Split segment [0:1] in accordance to assets, 
        /// so probability of choing 'i' is proportional to A_i/A
        /// </summary>
        /// <param name="curBank">The id of a bank we are choosing a partner for</param>
        /// <returns>index of bank, -1 if random wasn't in range</returns>
        internal string ChooseBank_PreferentiallyAssets(string curBank)
        {
            var random = ForBankChoice.NextDouble();
            if (random < 0 || random > 1) throw new Exception("ChooseBankPreferentiallyAssets: input value is oput of range");
            var ratingArray = new double[Banks.Count];
            var allBanksAssets = Banks.Where(x=>x.ID != curBank).Sum(x => x.GetA());
            if (!(allBanksAssets > 0)) return ChooseBank();
            
            foreach (var bank in Banks)
                ratingArray[Banks.IndexOf(bank)] = bank.ID != curBank ? (double)bank.GetA() / allBanksAssets : 0;
            var rightBound = ratingArray[0];
            for (var i = 0; i < ratingArray.Length; i++)
                if (random < rightBound)
                    return Banks[i].ID;
                else if (i + 1 < ratingArray.Length) rightBound += ratingArray[i + 1];
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curBank">"b"+int</param>
        /// <returns></returns>
        internal string ChooseBank_AssortativeAssets(string curBank)
        {
            var ratingArray = new double[Banks.Count];
            var maxAsset = Banks.Max(x => x.GetA());
                // var allBanksAssets = Banks.Where(x => x.ID != curBank).Sum(x => x.GetA());
                // if (!(allBanksAssets > 0)) return ChooseBank();
            foreach (var bank in Banks)
                ratingArray[Banks.IndexOf(bank)] = bank.ID != curBank ? maxAsset - Math.Abs((double)bank.GetA() - Banks.First(x=>x.ID==curBank).GetA()) : 0;
            var sumRatArr = ratingArray.Sum(x => x);
            var normArray = new double[Banks.Count];
            for (var i = 0; i < ratingArray.Length; i++)
                normArray[i] = ratingArray[i]/sumRatArr;

            var random = ForBankChoice.NextDouble();
            if (random < 0 || random > 1) throw new Exception("ChooseBankPreferentiallyAssets: input random is out of range");
            
            var rightBound = normArray[0];
            for (var i = 0; i < normArray.Length; i++)
                if (random < rightBound)
                    return Banks[i].ID;
                else if (i + 1 < normArray.Length) rightBound += normArray[i + 1];
            return "";
        }

        /// <summary>
        /// Possible wrong assignment when bounds of intervals coincide
        /// </summary>
        /// <param name="curBank"></param>
        /// <returns></returns>
        internal string ChooseBank_PreferentiallyNW(string curBank)
        {
            var random = ForBankChoice.NextDouble();
            if (random < 0 || random > 1) throw new Exception("ChooseBankPreferentiallyNWs: input random is out of range");
            var ratingArray = new double[Banks.Count];
            var allBanksNWs = Banks.Where(x=>x.NW>0 && x.ID != curBank).Sum(x => x.NW);
            if (!(allBanksNWs > 0)) return ChooseBank();
            
            foreach (var bank in Banks)
                ratingArray[Banks.IndexOf(bank)] = (double)bank.NW>0 && bank.ID != curBank ? (double)bank.NW / allBanksNWs : 0;
            var rightBound = ratingArray[0];
            for (var i = 0; i < ratingArray.Length; i++)
                if (random < rightBound)
                    return Banks[i].ID;
                else if (i + 1 < ratingArray.Length) rightBound += ratingArray[i + 1];
            return "";
        }

        int ChooseWeight(/*int mean, int dev*/)
        {
            //Random rand = new Random(); //reuse this if you are generating many
            double u1 = ForWeightChoice.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = ForWeightChoice.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            var randNormal = (int) Math.Round(mean + dev * randStdNormal); //random normal(mean,stdDev^2)
            
            return Math.Abs(randNormal);// EdgeWeight;
        }
        int ChooseMaturity()
        {
            int tmpTerm;
            var termKind = ForMaturityChoice.Next(1, 4);
            if (termKind == 1)
                tmpTerm = 1;
            else if (termKind == 2)
                tmpTerm = ForMaturityChoice.Next(2, 30);
            else tmpTerm = ForMaturityChoice.Next(31, 360);
            return tmpTerm;
            return Maturity;
        }

        int ChooseMaturity(int defaultMaturity)
        {
            
            return defaultMaturity;
        }
        

        #region REWIRING POLICY (COMPARISON)
        /*
        public class CompareEdgesByBankAssetsAscendingHelper:IComparer<Edge>
        {
            
            public int Compare(Edge x, Edge y)
            {
                throw new NotImplementedException();
            }
        }
        */
        #endregion
    }
}// todo encapsuate bank policy as a single parameter; encapsulate the balance sheet as a class either


/*        public static int ReturnTerm()
        {
            // eval term
            int tmpTerm;
            var termKind = _forKindOfTerm.Next(1, 4);
            if (termKind == 1)
                tmpTerm = 1;
            else if (termKind == 2)
                tmpTerm = _forTerm.Next(2, 30);
            else tmpTerm = _forTerm.Next(31, 360);
            return tmpTerm;
        }
         */