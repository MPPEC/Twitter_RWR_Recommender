using TweetRecommender.Others;
using System;
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
        public static Semaphore semaphore;

        // To avoid file writer collision
        public static Object locker = new Object();

        // Path of directory that contains data files (*.sqlite)
        public static string dirData;

        // Methodologies: Experimental Codes List
        public static List<Methodology> methodologies;

        // Existing experimental result: To SKIP already performed experiments
        public static Dictionary<long, List<int>> existingResults = new Dictionary<long, List<int>>(); // (<ego ID>, <{Experiments Codes}>)

        // For output file
        public static StreamWriter logger;

        // Command line argument: C:\Users\M-PEC\Desktop\sample 0,1 5 10
        public static void Main(string[] args) 
        {
            Console.WriteLine("RWR-based Recommendation (" + DateTime.Now.ToString() + ")\n");
            Stopwatch programStopwatch = Stopwatch.StartNew(); // Stopwatch: C# Standard Library Class

            // Program arguments
            dirData = @args[0] + Path.DirectorySeparatorChar;           // Path of directory that containes SQLite DB files
            string outFilePath = args[0] + "\\" + args[1];
            Console.WriteLine(outFilePath);
            string[] methodologyList = args[2].Split(',');              // The list of experimental codes (csv format; for example: 0,1,8,9,10,11,12 )
            int nFolds = int.Parse(args[3]);                            // Number of folds
            int nIterations = int.Parse(args[4]);                       // Number of iterations for RWR

            // Load existing experimental results: SKIP already performed experiments
            if (File.Exists(outFilePath)) 
            {
                StreamReader reader = new StreamReader(outFilePath);
                string line;
                //reader.ReadLine(); // Discard First Line(Headline)
                while ((line = reader.ReadLine()) != null) 
                {
                    string[] tokens = line.Split('\t');
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

            // Methodology list
            methodologies = new List<Methodology>();
            foreach (string methodology in methodologyList) // 'methodologyList' == args[1]
                methodologies.Add((Methodology) int.Parse(methodology));

            // Result File Format
            Program.logger = new StreamWriter(outFilePath, true);
            //Program.logger.WriteLine("{0}\t\t{1}\t{2}\t{3}\t{4}\t\t\t{5}\t\t\t{6}\t{7}\t{8}", "EGO", "Method", "Kfold", "Iter", "MAP", "RECALL", "LIKE", "HIT", "FRIEND");

            // #Core Part: One .sqlite to One thread
            int cntSemaphore = 2;
            semaphore = new Semaphore(cntSemaphore, cntSemaphore);
            Stopwatch dbStopwatch = null; // Stopwatch: C# Standard Library Class
            foreach (string dbFile in sqliteDBs) 
            {
                Console.WriteLine("Start Ego: " + Path.GetFileNameWithoutExtension(dbFile));
                dbStopwatch = Stopwatch.StartNew();
                Experiment experiment = new Experiment(dbFile);
                experiment.startPersonalizedPageRank(nFolds, nIterations);
                dbStopwatch.Stop();
                Program.logger.WriteLine("\t" + Tools.getExecutionTime(dbStopwatch));
                Program.logger.Flush();
            }

            // Close Output file
            Program.logger.Close();

            // Execution Time
            programStopwatch.Stop();
            Console.WriteLine("Execution Time: " + Tools.getExecutionTime(programStopwatch));
            Console.WriteLine("Finished!");

        }
    }
}
