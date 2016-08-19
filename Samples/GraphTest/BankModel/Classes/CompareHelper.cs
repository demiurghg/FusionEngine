using System.Collections.Generic;
using GraphTest.BankModel.Structs;

namespace GraphTest.BankModel.Classes
{
    public class CompareEdgesExpiresAscending : IComparer<Edge>
    {
        public int Compare(Edge x, Edge y)
        {
            if (x.Expires > y.Expires)
                return 1;
            if (x.Expires < y.Expires)
                return -1;
            return 0;
        }
    }
    public class CompareEdgesExpiresDescending : IComparer<Edge>
    {
        public int Compare(Edge x, Edge y)
        {
            if (x.Expires < y.Expires)
                return 1;
            if (x.Expires > y.Expires)
                return -1;
            return 0;
        }
    }
    /* CompareEdgesByBankAssetsAscending 
    public class CompareEdgesByBankAssetsAscendingHelper : IComparer<Edge>
    {
        private List<Bank> _banks;

        public void InitBanks(List<Bank> banks)
        {
            _banks = new List<Bank>();
            _banks.AddRange(banks);
        }

        public int Compare(Edge x, Edge y)
        {
            if (!_banks.Exists(b => b.ID == x.Source || b.ID == x.Target) ||
                (!_banks.Exists(b => b.ID == y.Source || b.ID == y.Target)))
                throw new Exception("At least one of the edges does not exist in current network");

            if (x.Source == y.Source)
            {
                var xTarA = _banks[0].
            }

            if (x == null || x.IntTarget() >= Banks.Count.Count)
            {
                if (y == null || y.BankAssignment >= Banks.Count)
                    return 0;
                // If x is null and y is not null, y
                // is greater. 
                return -1;
            }
            // If x is not null...
            //
            if (y == null || y.BankAssignment >= Banks.Count)
                // ...and y is null, x is greater.
                return 1;


            // ...and y is not null, compare the 
            // lengths of the two strings.
            //
            // eval total assets of x
            var xAssets = Banks[x.BankAssignment].IntAssList.Sum(t => t.InvestmentSize) +
                          Banks[x.BankAssignment].ExtAssList.Sum(t => t.InvestmentSize);
            // eval total assets of y
            var yAssets = Banks[y.BankAssignment].IntAssList.Sum(t => t.InvestmentSize) +
                          Banks[y.BankAssignment].ExtAssList.Sum(t => t.InvestmentSize);
            int retval = xAssets.CompareTo(yAssets);

            if (retval != 0)
                // If the strings are not of equal length,
                // the longer string is greater.
                //
                return retval;
            // If the strings are of equal length,
            // sort them with ordinary string comparison.
            //
            return x.CompareTo(y);
            throw new NotImplementedException();
        }
    }
    */
}
