using System;

namespace GraphTest.BankModel.Model
{
    public class Bank
    {
        /// <summary>
        /// Structure: "b0".
        /// As different types of nodes to be hold in a network
        /// </summary>
        internal string ID; // todo event for fulfilling when edge is added to list
        // values of balance sheet are integral as edges' weights are integral
        /// <summary>
        /// Interbank Assets
        /// </summary>
        internal int IA;
        /// <summary>
        /// Interbank Liabilities
        /// </summary>
        internal int IL;
        /// <summary>
        /// External Assets
        /// </summary>
        internal int EA;
        /// <summary>
        /// External Liabilities
        /// </summary>
        internal int EL;

        /// <summary>
        /// Net Worth
        /// </summary>
        internal int NW
        {
            get { return EA + IA - EL - IL; }
        }

        internal const int DefaultValueNW = 0; // todo replace to bankPolicy parameters or to environment parameters
        internal int prevIA;
        internal int prevIL;
        internal int prevEA;
        internal int prevEL;
        internal int prevNW {get { return prevEA + prevIA - prevEL - prevIL; }}


        internal Bank(int id)
        {
            ID = "b"+id;
            IA = 0;
            IL = 0;
            EA = 0;
            EL = 0;
        }

        internal int GetA()
        {
            return IA + EA;
        }
        internal int GetL()
        {
            return IL + EL;
        }
        internal void IA_Plus(int value) { IA += value; }
        //(int value) {IA += value;// NW += value;}
        internal void IL_Plus(int value) { IL += value; }
        internal void EA_Plus(int value) { EA += value; }
        internal void EL_Plus(int value) { EL += value; }

        internal void IA_Minus(int value) { IA -= value; }
        internal void IL_Minus(int value) { IL -= value; }
        internal void EA_Minus(int value) { EA -= value; }
        internal void EL_Minus(int value) { EL -= value; }

        internal void UpdatePreviousBalanceSheetValues()
        {
            prevIA = IA;
            prevIL = IL;
            prevEA = EA;
            prevEL = EL;
        }

        #region Dynamics properties
        internal double Velocity{get { return NW - prevNW; }}

        internal int Remoteness
        {
            get
            {
                if (Velocity < 0)
                    return (int)Math.Ceiling((NW - DefaultValueNW)/Math.Abs(Velocity));
                return Int32.MaxValue;
            }
        }

        #endregion

    }
}
