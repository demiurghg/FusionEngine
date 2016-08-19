using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using GraphTest.BankModel.Classes;
using GraphTest.BankModel.Structs;

namespace GraphTest.BankModel.Model
{
    partial class BankingSystem
    {
        #region NODES STATES DYNAMICS

        double _negativeNwShare;
        void UpdateNegativeNwShare()
        {
            _negativeNwShare= (double)Banks.Count(x => x.NW < 0)/Banks.Count;
        }
        double _meanVelocity;
        /// <summary>
        /// N^{-1}*\sum{i}
        /// </summary>
        /// <returns></returns>
        void UpdateMeanVelocity()
        {
            _meanVelocity = Banks.Average(x => x.Velocity);
        }

        double _varianceVelocity;
        /// <summary>
        /// \sigma^2=N^{-1}*(\sum_{i}{(average-i)^2})
        /// </summary>
        /// <returns></returns>
        void UpdateVarianceVelocity()
        {
            _varianceVelocity= Banks.Average(x => Math.Pow(x.Velocity - _meanVelocity, 2));
        }
        double _meanRemoteness;
        void UpdateMeanRemoteness()
        {
            _meanRemoteness= Banks.Average(x => x.Remoteness);
        }
        double _varianceRemoteness;
        void UpdateVarianceRemoteness()
        {
            _varianceRemoteness= Math.Round(Banks.Average(x => Math.Pow(x.Remoteness - _meanRemoteness, 2)), 5);
        }
        double _negativeVelocityShare;
        void UpdateNegativeVelocityShare()
        {
            _negativeVelocityShare = (double)Banks.Count(x => x.Velocity < 0)/Banks.Count;
        }
        double T_ThreatenedSetCardinality(int T)
        {
            return Banks.Count(x => x.NW > Bank.DefaultValueNW && x.Velocity < 0 && x.Remoteness < T);
        }
        #endregion
        #region TOPOLOGY+NODE DYNAMICS

        //private double _edgePotential;
        /// <summary>
        /// Undirected edge feature.
        /// If edge expires before some bank go bankrupt,
        /// imply positive effect, otherwise, negative.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private double EdgePotential(Edge edge)
        {
            var m = edge.Maturity;
	        if (Regex.IsMatch(edge.Source, @"b\d+") && Regex.IsMatch(edge.Target, @"b\d+"))
	        {
		        var r1 = Banks.First(x => x.ID == edge.Source).Remoteness;
				var r2 = Banks.First(x => x.ID == edge.Target).Remoteness;
				if (m < r1 && m < r2)
					return 1;
	        }
            return - 1;
        }

        //private double WeightedEdgePotential;
        private double WeightedEdgePotential(Edge edge)
        {
            var m = edge.Maturity;
            var r1 = Banks.First(x => x.ID == edge.Source).Remoteness;
            var r2 = Banks.First(x => x.ID == edge.Target).Remoteness;
            var v1 = Banks.First(x => x.ID == edge.Source).Velocity;
            var v2 = Banks.First(x => x.ID == edge.Target).Velocity;
            var s1 = Banks.First(x => x.ID == edge.Source).NW;
            var s2 = Banks.First(x => x.ID == edge.Target).NW;

            if (m < r1 && m < r2)
                return RelativeGain(edge.Weight,s1, s2);
            if (r1 < r2)
                return RelativeLoss(v1, s2, edge.Weight);
            return  RelativeLoss(v2, s1, edge.Weight);
        }

        private double _meanWeightedPotential;
        /*internal double*/void UpdateMeanWeightedPotential()
        {
            if (IbNetwork.Count == 0) _meanWeightedPotential= 0;
            _meanWeightedPotential= IbNetwork.Average(x => WeightedEdgePotential(x));
        }
        internal double VarianceWeightedPotential()
        {
            if (IbNetwork.Count == 0) return 0;
            /*
            var sum = .0;//IbNetwork.Sum(x => Math.Sqrt(EdgePotential(x) - MeanWeightedPotential()));
            foreach (var edge in IbNetwork)
            {
                sum += Math.Pow(EdgePotential(edge) - MeanWeightedPotential(),2);
            }
            return (double)sum/IbNetwork.Count;
             */
            return IbNetwork.Average(x => Math.Pow(EdgePotential(x) - _meanWeightedPotential, 2)); // todo edge potential is evaled at least twice
        }

        private double _meanUnwaightedPotential;
        /// <summary>
        /// Summarize all potentials of edges and
        /// </summary>
        /// <returns></returns>
        internal void UpdateMeanUnwaightedPotential()
        {
            if (IbNetwork.Count == 0) {_meanUnwaightedPotential= 0;
                return;
            }
            _meanUnwaightedPotential= Math.Round(IbNetwork.Average(x => EdgePotential(x)), 5);
        }
        internal double VarianceUnweightedPotential()
        {
            if (IbNetwork.Count == 0) return 0;
            return Math.Round(IbNetwork.Average(x=>Math.Pow(EdgePotential(x)-_meanUnwaightedPotential, 2)), 5);
        }
        
        /// <summary>
        /// Sum all negative potentials, and divide by the number of edges.
        /// </summary>
        /// <returns>Share of negative potentials</returns>
        internal double NegativeUnweightedPotentialShare()
        {
            if (IbNetwork.Any(x => EdgePotential(x) < 0))
                return Math.Round((double)IbNetwork.Count(x => EdgePotential(x) < 0)/IbNetwork.Count, 5);
            return 0;
        }

        /// <summary>
        /// Possibly positive impact of an edge
        /// to the states of adjacent nodes.
        /// Combined from sum of profits of both nodes.
        /// If states are positive, relative gain is evaluated.
        /// If some state is negative, gain to be taken as an edge weight 
        /// </summary>
        /// <param name="edgeWeight"></param>
        /// <param name="node1State"></param>
        /// <param name="node2State"></param>
        /// <returns>edgeWeight*(1/node1State + 1/node2State)</returns>
        private double RelativeGain(int edgeWeight, int node1State, int node2State)
        {
            double addend1;
            double addend2;
            
            if (node1State <= 0)
                addend1 = edgeWeight;
            else addend1 = (double)edgeWeight/node1State;
            
            if (node2State <= 0)
                addend2 = edgeWeight;
            else addend2 = (double)edgeWeight/node2State;

            return addend1 + addend2;
        }
        /// <summary>
        /// Possibly negative impact of an edge
        /// to the states of adjacent nodes.
        /// It is combined edge weight and expected distance to be undergoed with velocity in 1 iteration.
        /// If state is negative then loss is taken as absolute value of sum of velosity abs and weight.
        /// </summary>
        /// <param name="worseNodeVelocity"></param>
        /// <param name="betterNodeState"></param>
        /// <param name="edgeWeight"></param>
        /// <returns>If node state is positive then (worseNodeVelocity + edgeWeight) / betterNodeState; 
        /// othewise, worseNodeVelocity + edgeWeight</returns>
        private double RelativeLoss(double worseNodeVelocity, double betterNodeState, int edgeWeight)
        {
            if (betterNodeState > 0)
                return (worseNodeVelocity + edgeWeight)/betterNodeState;
            return (worseNodeVelocity + edgeWeight);
        }
        #endregion
        #region TOPOLOGY
        // associated with Network
        #endregion

        internal string GetSystemState()
        {
            IEnumerable<string> features = new List<string>
            {
                /* 0*/_curIt.ToInt().                            ToString(CultureInfo.InvariantCulture),
                /* 1*/_meanVelocity.                             ToString(CultureInfo.InvariantCulture),
                /* 2*/_varianceVelocity.                         ToString(CultureInfo.InvariantCulture),
                /* 3*/_meanRemoteness.                           ToString(CultureInfo.InvariantCulture),
                /* 4*/_varianceRemoteness.                       ToString("f"),
                /* 5*/_negativeVelocityShare.                    ToString(CultureInfo.InvariantCulture),
                /* 6*/T_ThreatenedSetCardinality(50).            ToString(CultureInfo.InvariantCulture),
                /* 7*/T_ThreatenedSetCardinality(400).           ToString(CultureInfo.InvariantCulture),
                /* 8*/_meanUnwaightedPotential.                  ToString(CultureInfo.InvariantCulture),
                /* 9*/VarianceUnweightedPotential().             ToString(CultureInfo.InvariantCulture),
                /*10*/Math.Round(_meanWeightedPotential, 3).     ToString(CultureInfo.InvariantCulture),
                /*11*/Math.Round(VarianceWeightedPotential(), 3).ToString(CultureInfo.InvariantCulture),
                /*12*/NegativeUnweightedPotentialShare().        ToString(CultureInfo.InvariantCulture),
                /*13*/_negativeNwShare.                          ToString(CultureInfo.InvariantCulture),
                Network.AverageDegree(IbNetwork).                ToString("f"),
                Network.AverageClustering(IbNetwork).            ToString(CultureInfo.InvariantCulture),
                Network.AverageShortestPath(IbNetwork).          ToString(CultureInfo.InvariantCulture),
                Network.Energy(IbNetwork).                       ToString(CultureInfo.InvariantCulture),
                Network.PseudoEntropy(IbNetwork).                ToString(CultureInfo.InvariantCulture)
            };
            return String.Join(";", features);
        }

        /// <summary>
        /// Update time-dependent features: velocity and remoteness mean and variance, negative velocity share, neg NW share, mean unvated potential
        /// </summary>
        internal void UpdateProperties()
        {
            UpdateMeanVelocity();
            UpdateVarianceVelocity();
            UpdateMeanRemoteness();
            UpdateVarianceRemoteness();
            UpdateNegativeVelocityShare();
            UpdateMeanUnwaightedPotential();
            UpdateNegativeNwShare();
        }
    }
}
