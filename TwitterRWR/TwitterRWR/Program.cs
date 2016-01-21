﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading; // Multi-threading Library
using System.Threading.Tasks;

namespace TweetRecommender 
{
    public class Program 
    {
        // To limit the number of multithreading concurrency
        public static Semaphore semaphore = new Semaphore(10, 10);

        // To avoid file writer collision
        public static Object locker = new Object();

        // Path of directory that contains data files (*.sqlite)
        public static string dirData;

        // Methodologies: Experimental Codes List
        public static List<Methodology> methodologies;

        // Existing experimental result: To SKIP already performed experiments
        public static Dictionary<long, List<int>> existingResults = new Dictionary<long, List<int>>(); // (<ego ID>, <{Experiments Codes}>)

        // Command line argument: C:\Users\M-PEC\Desktop\sample 0,1 5 10
        public static void Main(string[] args) 
        {
            Console.WriteLine("RWR-based Recommendation (" + DateTime.Now.ToString() + ")\n");
            Stopwatch stopwatch = Stopwatch.StartNew(); // Stopwatch: C# Standard Library Class

            // Program arguments
            dirData = @args[0] + Path.DirectorySeparatorChar;           // Path of directory that containes SQLite DB files
            string[] methodologyList = args[1].Split(',');              // The list of experimental codes (csv format; for example: 0,1,8,9,10,11,12 )
            int nFolds = int.Parse(args[2]);                            // Number of folds
            int nIterations = int.Parse(args[3]);                       // Number of iterations for RWR

            // Load existing experimental results: SKIP already performed experiments
            if (File.Exists(dirData + "result.txt")) 
            {
                StreamReader reader = new StreamReader(dirData + "result.txt");
                string line;
                while ((line = reader.ReadLine()) != null) 
                {
                    string[] tokens = line.Split('\t');
                    if (tokens.Length != 7)
                        continue;
                    long egouser = long.Parse(tokens[0]);
                    int experiment = int.Parse(tokens[1]);
                    if (!existingResults.ContainsKey(egouser))
                        existingResults.Add(egouser, new List<int>());
                    existingResults[egouser].Add(experiment);
                }
                reader.Close();
            }

            // Run experiments using multi-threading
            string[] sqliteDBs = Directory.GetFiles(dirData, "*.sqlite");
            List<Thread> threadList = new List<Thread>(); // 'Thread': Standard Library Class

            // Methodology list
            methodologies = new List<Methodology>();
            foreach (string methodology in methodologyList) // 'methodologyList' == args[1]
                methodologies.Add((Methodology) int.Parse(methodology));
            
            // One .sqlite to One thread
            foreach (string dbFile in sqliteDBs) 
            {
                Thread thread = new Thread(new ParameterizedThreadStart(Experiment.personalizedPageRank)); // Core Part
                ThreadParams parameters = new ThreadParams(dbFile, nFolds, nIterations); // 'ThreadParams': defined in 'Experiment.cs'
                thread.Start(parameters);
                threadList.Add(thread);
            }
            // Synchronization: Wait until threads be terminated
            foreach (Thread thread in threadList)
                thread.Join();

            stopwatch.Stop();
            Tools.printExecutionTime(stopwatch);
            Console.WriteLine("Finished!");
        }
    }
}
