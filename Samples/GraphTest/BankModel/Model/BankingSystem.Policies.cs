using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphTest.BankModel.Model
{
    partial class BankingSystem
    {
        private static readonly Random ForBankChoose = new Random();
        internal int ChooseBank()
        {
            return ForBankChoose.Next(0, Banks.Count);
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
        /// <param name="random">Double in [0:1]</param>
        /// <returns>index of bank, -1 if random wasn't in range</returns>
        internal int ChooseBank_PreferentiallyAssets()
        {
            var random = ForBankChoose.NextDouble();
            if (random < 0 || random > 1) throw new Exception("ChooseBankPreferentiallyAssets: input value is oput of range");
            var ratingArray = new double[Banks.Count];
            var allBanksAssets = Banks.Sum(x => x.GetA());
            if (!(allBanksAssets > 0)) return ChooseBank();
            foreach (var bank in Banks)
                ratingArray[Int32.Parse(bank.ID.Substring(1))] = (double)bank.GetA() / allBanksAssets;
            var rightBound = ratingArray[0];
            for (var i = 0; i < ratingArray.Length; i++)
                if (random < rightBound)
                    return i;
                else if (i + 1 < ratingArray.Length) rightBound += ratingArray[i + 1];
            return -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curBank">"b"+int</param>
        /// <returns></returns>
        internal int ChooseBank_AssortativeAssets(string curBank)
        {
            if(curBank==" ") throw new Exception("The method probably was called by a customer");
            var curBankA = Banks.First(x => x.ID == curBank).GetA();
            var minDist = Int32.MaxValue;
            var minIndex = "";
            foreach (var bank in Banks.Where(x=>x.ID != curBank).Where(bank => Math.Abs(bank.GetA() - curBankA) < minDist))
            {
                minDist = Math.Abs(bank.GetA() - curBankA);
                minIndex = bank.ID;
            }
            return Int32.Parse(minIndex.Substring(1));
        }

        int ChooseWeight()
        {
            return EdgeWeight;
        }
        int ChooseMaturity()
        {
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