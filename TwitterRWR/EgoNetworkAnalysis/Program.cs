using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoNetworkAnalysis
{
    class Program
    {
        // Command line argument: C:\Users\dilet\Desktop\TwitterDB
        static void Main(string[] args)
        {
            Console.WriteLine("Ego-Network Analysis Start (" + DateTime.Now.ToString() + ")\n");
            Stopwatch programStopwatch = Stopwatch.StartNew();

            // Program arguments
            string dirPath = args[0] + Path.DirectorySeparatorChar;

            // DB(.sqlite) List
            string[] dbCollection = Directory.GetFiles(dirPath, "*.sqlite");

            // Outfile Setting
            string outFilePath = args[0] + Path.DirectorySeparatorChar + "EgoNetwork_Analysis.txt";

            // <Ego, dbPath> Sorted Order(Ascending)
            SortedDictionary<long, string> egoList = new SortedDictionary<long, string>();
            foreach(string dbPath in dbCollection)
            {
                long egoID = long.Parse(Path.GetFileNameWithoutExtension(dbPath));
                egoList.Add(egoID, dbPath);
            }

            // Ego-Network Analysis
            foreach(KeyValuePair<long, string> kvp in egoList)
            {               
                long egoID = kvp.Key;
                string dbPath = kvp.Value;
                SQLiteAdapter dbAdapter = new SQLiteAdapter(dbPath);
                Console.WriteLine(egoID + " Started");
                // Configure Ego-Network 
                EgoNetwork egoNetwork = new EgoNetwork(egoID);
                // Construct Ego-Network Information              
                egoNetwork.setNetworkInformation(dbAdapter);
                // Anaylize Ego-Network
                egoNetwork.startNetworkAnalysis();
                // Output Analysis Result
                egoNetwork.outputNetworkAnalysisResult(outFilePath);

                dbAdapter.closeDB(); 
            }
        }
    }
}
