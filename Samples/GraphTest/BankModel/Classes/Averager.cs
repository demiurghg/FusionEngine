using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GraphTest.BankModel.Classes
{
    static class Averager
    {
        static public void AverageAllFilesInDir(string pathToDir, string pathToResDir)
        {
            var filenames = Directory.GetFiles(pathToDir);//.Where(x => !Path.HasExtension(x));

            var processedFiles = new List<string>();
            foreach (var filename in filenames)
            {
                if(filename.Contains("edges")) continue;
                if (processedFiles.Contains(filename))  
                    continue;
                var expName = filename.Substring(0, filename.Length - 3);// name without run number
                List<string> curFilesToProcess = filenames.Where(x => x.Substring(0, x.Length - 3) == expName).ToList();
                WriteAveragesForFiles(curFilesToProcess, Path.Combine(pathToResDir, Path.GetFileNameWithoutExtension(expName)));
                processedFiles.AddRange(curFilesToProcess);
            }
        }

        static void WriteAveragesForFiles(List<string> fileNamesToProcess, string resFileName)
        {
            // get data to one structure
            var fileLinesList = new List<string[]>();
            foreach (var fileName in fileNamesToProcess)
            {
                var initialFileLines = File.ReadAllLines(fileName);
                // remove \t and too much spaces from strings
                var resultFileLines = (from s in initialFileLines select s.Replace("\t", ""))/*.Replace("\\s+", " "))*/.ToList();
                fileLinesList.Add(resultFileLines.Select(fileLine => new Regex("\\s+").Replace(fileLine, " ")).ToArray());
            }

            //  get number of network features
            int columnNum = Regex.Matches(fileLinesList[0][0], "(-?\\d+(\\.|,)?\\d*)|NaN").Count;
            // init result array
            var resArray = new List<double[]>();
            for (var i=0; i < fileLinesList.Min(x => x.Length); i++)
                resArray.Add(new double[columnNum]);
            
            // new double[fileLinesList.Min(x => x.Length),columnNum];
            for (var i = 0; i < fileLinesList.Min(x => x.Length); i++)
                resArray[i][0] = i;

            // process obtained data
            for (var iter = 0; iter < fileLinesList.Min(x => x.Length); iter++)
                for (var featNum = 1; featNum < columnNum; featNum++)
                    // foreach feature and iter form result strings
                {
                    var sum = 0.0;
                    foreach (var fileLines in fileLinesList)
                    {
                        if (fileLines[iter].Split(new[] { ';', ' ' })[featNum] == "NaN") continue;
                        string[] splitedString = fileLines[iter].Split(new[] {';', ' '});
                        var tmp = splitedString[featNum].Replace('.', ',');
                        double parsedValue;
                        try
                        {
                            parsedValue = Double.Parse(tmp);
                        }
                        catch (Exception)
                        {
                            parsedValue = 0;
                        }
                        
                        sum += parsedValue;
                    }
                   resArray[iter][featNum] = sum / fileLinesList.Count;
                }

            using (var writer = new StreamWriter(resFileName))
                for (var iter = 0; iter < fileLinesList.Min(x => x.Length); iter++)
                {
                    var stringArr = Array.ConvertAll<double, string>(resArray[iter], Convert.ToString);
                    writer.WriteLine(String.Join(";", stringArr));
                }
        }
    }
}
