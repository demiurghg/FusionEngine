using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GraphTest.BankModel.Classes
{
    static class Plotter
    {
        public const  string pathToFiles = "";
        public static Dictionary<string, int>  NullFeaturesMetaData = new Dictionary<string, int>()
        {
            {"CurIt", 0},
            {"MeanVelocity",1},
            {"VarianceVelocity",2},
            {"MeanRemoteness", 3},
            {"VarianceRemoteness", 4},
            {"NegativeVelocityShare", 5},
            {"T_ThreatenedSetCardinality(50)", 6},
            {"T_ThreatenedSetCardinality(400)", 7},
            {"MeanUnwaightedPotential", 8},
            {"VarianceUnweightedPotential", 9},
            {"MeanWeightedPotential", 10},
            {"VarianceWeightedPotential", 11},
            {"NegativeUnweightedPotentialShare", 12},
            {"NegativeNwShare", 13},
            {"AverageDegree", 14},
            {"AverageClustering", 15},
            {"AverageShortestPath", 16},
            {"Energy", 17},
            {"PseudoEntropy", 18}
        };
        public static Dictionary<string, int>  FullFeaturesMetaData = new Dictionary<string, int>()
        {
            {"CurIt", 0},
            {"Energy12", 1},
            {"NegativeDangerousVelocityShare(400)", 2},
            {"NegativeDangerousVelocityShare(300)", 3},
            {"NegativeDangerousVelocityShare(200)", 4},
            {"NegativeDangerousVelocityShare(100)", 5},
            {"NegativeDangerousVelocityShare(50)", 6},
            {"SumDynamicsAF_v", 7},
            {"SumDynamicsAF_h", 8},
            {"SumDynamicsNW_v", 9},
            {"SumDynamicsNW_h", 10},
            {"DynamicOfNodeStatesAF_v", 11},
            {"DynamicOfNodeStatesAF_h", 12},
            {"DynamicOfNodeStatesNW_v", 13},
            {"DynamicOfNodeStatesNW_h", 14},
            {"DynamicsOfStateNodeInteractionAF", 15},
            {"DynamicsOfStateNodeInteractionNW", 16},
            {"DynamicsOfStateNodeInteractionAF_n", 17},
            {"DynamicsOfStateNodeInteractionNW_n", 18},
            {"Energy", 19},
            {"Entropy", 20},
            {"Temperature", 21},
            {"MaxNetworth", 22},
            {"MaxAssets", 23},
            {"MinNetworth", 24},
            {"MinAssets", 25},
            {"AverageNetworth", 26},
            {"AverageAssets", 27},
            {"ReborrowCounter", 28},
            {"Bankrupts", 29},
            //{"negNW", 29}
        };
        public static List<string>  FileNameList = new List<string>()
        {
            "systemStates_m03RR_01",
            "systemStates_m03PR_01",
            "systemStates_m03RP_01",
            "systemStates_m03PP_01",
            "systemStates_m03AR_01",
            "systemStates_m03AP_01",
        };

        public static List<string> FileNameList2 = new List<string>()
        {
            "systemStates_m03PPR100_01",
            "systemStates_m03PPR050_01",
            "systemStates_m03PPR025_01",
            
            "systemStates_m03PPA100_01",
            "systemStates_m03PPA050_01",
            "systemStates_m03PPA025_01",
            
            "systemStates_m03APR100_01",
            "systemStates_m03APR050_01",
            "systemStates_m03APR025_01"
        };
        public static List<string> FileNameList3 = new List<string>()
        {
            "systemStates_m03PPR100_01",
            "systemStates_m03PPR050_01",
            "systemStates_m03PPR025_01"
        };
        
        /// <summary>
        /// create dat file using data from other file
        /// </summary>
        /// <param name="dataFileName"></param>
        static public void CreateDat(string dataFileName)
        {
            var lines = File.ReadAllLines(dataFileName);
            using (var writer = new StreamWriter(String.Concat(dataFileName, ".dat")))
                foreach (var line in lines)
                    writer.WriteLine(line.Replace(",", ".").Replace(";","\t"));
        }
        /// <summary>
        /// create dat files from all data files in the directory
        /// </summary>
        /// <param name="pathToDir"></param>
        static public void CreateAllDat(string pathToDir)
        {
            var fileNames = Directory.GetFiles(pathToDir);
            foreach (var fileName in fileNames/*.Where(x => x != "readme" && (!Path.HasExtension(x)||Path.GetExtension(x)=="binned"))*/)
                CreateDat(fileName);
        }
        /// <summary>
        /// plot all graphs from one dat file
        /// </summary>
        /// <param name="datFile"></param>
        /// <param name="metaData"></param>
        static public void PlotAllFromDat(string datFile, Dictionary<string, int> metaData)
        {
            foreach (var feature in metaData)
            {
                string pltFWrite = Path.Combine(Path.GetDirectoryName(datFile), Path.GetFileNameWithoutExtension(datFile)+ ".plt");
                using (var file = new StreamWriter(pltFWrite /*pltFile*/))
                {
                    file.WriteLine();
                    file.Write(String.Concat("set xlabel \"Iteration\""));
                    file.WriteLine();
                    file.Write(String.Concat("set ylabel \"Value\""));
                    file.WriteLine();
                    file.WriteLine("set grid xtics ytics"); //  y2tics
                    file.WriteLine("set term png \n set output \'" + feature.Key+"_"+Path.GetFileNameWithoutExtension(datFile) + ".png\'");
                    file.Write("plot ");

                    file.WriteLine(String.Concat("\'", Path.GetFileNameWithoutExtension(datFile), ".dat\' using 1:",
                        feature.Value + 1, " with lines lw 2",
                        " title \"", feature.Key, "\",\\"));
                }
                Process.Start(pltFWrite);
                ProcessStartInfo PSI = new ProcessStartInfo();
                PSI.FileName = pltFWrite;
                PSI.WorkingDirectory = Directory.GetParent(pltFWrite).ToString();
                using (Process exeProcess = Process.Start(PSI))
                {
                    exeProcess.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Plots graphs for different parameters in one plot, 
        /// according to fileNamesList, 
        /// which is list of exp param description
        /// </summary>
        /// <param name="dirToDatFile"></param>
        static public void PlotRunsForFeatures(string dirToDatFile)
        {
            foreach (var feature in NullFeaturesMetaData)
            {
                string pltFWrite = Path.Combine(dirToDatFile,
                    feature.Key + ".plt");
                using (var file = new StreamWriter(pltFWrite /*pltFile*/))
                {
                    file.WriteLine();
                    file.Write(String.Concat("set ylabel \"" + feature.Key + "\""));
                    file.WriteLine();
                    file.Write(String.Concat("set xlabel \"Iteration\""));
                    file.WriteLine();
                    file.WriteLine("set grid xtics ytics"); //  y2tics
                    file.WriteLine("set term png \n set output \'" + feature.Key + ".png\'");
                    file.Write("plot ");
                    foreach (var datFileName in FileNameList3)
                    {
                        file.WriteLine(String.Concat("\'", datFileName/*Path.GetFileNameWithoutExtension(dirToDatFile)*/,
                            ".dat\' using 1:",
                            feature.Value + 1, " with lines lw 2",
                            " title \"", datFileName, "\",\\"));
                    }
                }
                Process.Start(pltFWrite);
                ProcessStartInfo PSI = new ProcessStartInfo();
                PSI.FileName = pltFWrite;
                PSI.WorkingDirectory = Directory.GetParent(pltFWrite).ToString();
                using (Process exeProcess = Process.Start(PSI))
                {
                    exeProcess.WaitForExit();
                }
            }
        }
        /// <summary>
        /// Plot graphs for data in all dat files in the directory.
        /// One graph for one feature and launch
        /// </summary>
        /// <param name="pathToDir"></param>
        static public void PlotAllFromDir(string pathToDir)
        {
            var fileNames = Directory.GetFiles(pathToDir).Where(x => Path.GetExtension(x) == ".dat");
            foreach (var fileName in fileNames)
            {
                PlotAllFromDat(fileName, FullFeaturesMetaData);
            }
        }
    }
}
