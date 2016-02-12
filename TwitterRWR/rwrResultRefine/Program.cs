using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rwrResultRefine
{
    class Program
    {
        // Command Line Argument: C:\Users\dilet\Desktop\TwitterDB C:\Users\dilet\Desktop\Twitter_RWR_Result 
        static void Main(string[] args)
        {
            string dbDirPath = args[0];
            string resutlDirPath = args[1];
            string[] dbCollection = Directory.GetFiles(dbDirPath, "*.sqlite");
            string[] resultCollection = Directory.GetFiles(resutlDirPath, "*.txt");
            SortedDictionary<long, RWRMetric> rwrResultList = new SortedDictionary<long, RWRMetric>();
            foreach (string dbPath in dbCollection)
            {
                long egoID = long.Parse(Path.GetFileNameWithoutExtension(dbPath));
                rwrResultList.Add(egoID, null);
            }

            foreach (string resultFilePath in resultCollection)
            {
                // Get Result from result file
                if (File.Exists(resultFilePath))
                {
                    long egoID;
                    int method, kFold, iteration, like, hit, friend;
                    string executionTime;
                    double MAP, recall;

                    using (StreamReader reader = new StreamReader(resultFilePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] tokens = line.Split('\t');
                            egoID = long.Parse(tokens[0]);
                            method = int.Parse(tokens[1]);
                            kFold = int.Parse(tokens[2]);
                            iteration = int.Parse(tokens[3]);
                            MAP = double.Parse(tokens[4]);
                            recall = double.Parse(tokens[5]);
                            like = int.Parse(tokens[6]);
                            hit = int.Parse(tokens[7]);
                            friend = int.Parse(tokens[8]);
                            executionTime = tokens[9];
                            if (rwrResultList.ContainsKey(egoID))
                                rwrResultList[egoID] =
                                    new RWRMetric(egoID, method, kFold, iteration, MAP, recall, like, hit, friend, executionTime);
                        }
                    }
                }

                // Replace File Result (In increasing ego ID order)
                File.Delete(resultFilePath);
                using (StreamWriter writer = new StreamWriter(resultFilePath, true))
                {
                    foreach (long egoID in rwrResultList.Keys)
                    {
                        if (rwrResultList[egoID] != null)
                            rwrResultList[egoID].logResultIntoFile(writer);
                    }
                }
            }
        }
    }
}
