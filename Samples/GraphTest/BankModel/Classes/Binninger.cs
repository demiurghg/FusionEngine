using System;
using System.Collections.Generic;
using System.IO;

namespace GraphTest.BankModel.Classes
{
    static class Binninger
    {
        static public void DataBinning(int binSize, string dataFilePath)
        {
            // get data from file
            var inputData = new List<double[]>();
            var fileLines = File.ReadAllLines(dataFilePath);
            foreach (var line in fileLines)
            {
                var splittedLine = line.Split(';');
                double[] doubleLine = new double[splittedLine.Length];
                for (var i = 0; i < splittedLine.Length; i++)
                    doubleLine[i] = Double.Parse(splittedLine[i]);
                inputData.Add(doubleLine);
            }

            // bin it
            var binnedData = new List<double[]>();
            for (var i = 0; i < inputData.Count; i++)
            {
                binnedData.Add(new double[inputData[1].Length]);
                binnedData[i][0] = i;
            }


            for (var j = 1; j < inputData[1].Length; j++) // column num
                for (var i = 0; i < inputData.Count; i++) // row num
                {
                    var halfBinWidth = Math.Min((int)(binSize/2), Math.Min(i, inputData.Count-i));
                    var binSum = 0.0;
                    //if (i < 10)
                    //    for (var k = 0; k < 3; k++)
                    //        binSum += inputData[i + k][j];
                    //else if (i < 50)
                    //    for (var k = 0; k < 5; k++)
                    //        binSum += inputData[i + k][j];
                    //else
                        for (var k = 0; k < halfBinWidth; k++)
                            binSum += inputData[i + k][j] + inputData[i - k][j];
                    binnedData[i][j] = binSum/(halfBinWidth*2);
                }

            // write result to another file
            using (var writer = new StreamWriter(dataFilePath + ".binned"))
                foreach (var line in binnedData)
                {
                    writer.WriteLine(String.Join(";", line));
                    // writer.WriteLine(line);
                }
        }

        static public void BinningAllFilesInDir(int binSize, string dirPath)
        {
            var files = Directory.GetFiles(dirPath);
            foreach (var file in files)
                DataBinning(binSize, file);
        }
    }
}
