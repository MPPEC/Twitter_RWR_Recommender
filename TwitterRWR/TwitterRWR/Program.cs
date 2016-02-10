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
    struct personalizedPageRankThreadParamters
    {
        public int kFold;
        public int nIteration;
        public Methodology methodology;

        public personalizedPageRankThreadParamters(int kFold, int nIteration, Methodology methodology)
        {
            this.kFold = kFold;
            this.nIteration = nIteration;
            this.methodology = methodology;
        }
    }

    public class Program 
    {
        // To limit the number of multithreading concurrency
        public static Semaphore dbSemaphore;

        // To avoid file writer collision
        public static Object outFileLocker = new Object();

        // For output file
        public static StreamWriter logger;

        // Environment Setting
        public static double egoLikeThresholdRatioInTestSet;
        public static bool isValidTrainSet; // |Likes in TrainSet| >= constraintCntLikeOfEgoInTestSet
        public static bool isOnlyFriendInEgoNetwork;
        public static bool isGenericFriendship;

        // Command line argument: C:\Users\dilet\Desktop\TwitterDB 0 10 15 0.1 1 1
        public static void Main(string[] args) 
        {
            Console.WriteLine("RWR-based Recommendation (" + DateTime.Now.ToString() + ")\n");
            Stopwatch programStopwatch = Stopwatch.StartNew();

            // Program arguments
            string dirPath = args[0] + Path.DirectorySeparatorChar;     // Path of directory that containes SQLite DB files
            string[] methodologyList = args[1].Split(',');              // The list of experimental codes (csv format; for example: 0,1,8,9,10,11,12 )
            int kFolds = int.Parse(args[2]);                            // Number of folds
            int nIterations = int.Parse(args[3]);                       // Number of iterations for RWR
            egoLikeThresholdRatioInTestSet = double.Parse(args[4]);
            isOnlyFriendInEgoNetwork = (int.Parse(args[5]) == 1) ? true : false;
            isGenericFriendship = (int.Parse(args[6]) == 1) ? true : false;

            // DB(.sqlite) List
            string[] dbCollection = Directory.GetFiles(dirPath, "*.sqlite");

            // Methodology list(Experiment Codes)
            List<Methodology> methodologies = new List<Methodology>();
            foreach (string methodology in methodologyList)
                methodologies.Add((Methodology) int.Parse(methodology));

            // #Core Part: One .sqlite to One thread
            int cntSemaphore = 1;
            Program.dbSemaphore = new Semaphore(cntSemaphore, cntSemaphore);
            foreach (Methodology methodology in methodologies)
            {
                // Outfile Setting
                string outFilePath = args[0] + Path.DirectorySeparatorChar + "RWR_MAP_10Split_Friend_Domain1_" + (int)methodology + "_Friendship0.txt";
                
                // Load existing experimental results: SKIP already performed experiments
                HashSet<long> alreadyPerformedEgoList = new HashSet<long>(); // (<ego ID>, <{Experiments Codes}>)
                if (File.Exists(outFilePath))
                {
                    using (StreamReader reader = new StreamReader(outFilePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] tokens = line.Split('\t');
                            long egoUser = long.Parse(tokens[0]);
                            alreadyPerformedEgoList.Add(egoUser);
                        }
                    }
                }
                Program.logger = new StreamWriter(outFilePath, true);

                // Personalized PageRank: Multi-threading
                Experiment experiment;
                personalizedPageRankThreadParamters pagaRankParameters;
                List<Thread> threadList = new List<Thread>();
                foreach (string dbFile in dbCollection)
                {
                    long egoID = long.Parse(Path.GetFileNameWithoutExtension(dbFile));
                    if (alreadyPerformedEgoList.Contains(egoID) == true)
                    {
                        Console.WriteLine("Ego {0} Already Performend", egoID);
                        continue;
                    }                     
                    try
                    {
                        // one Thread to one DB
                        Program.dbSemaphore.WaitOne();
                        Console.WriteLine("Ego {0} Start", egoID);
                        experiment = new Experiment(dbFile);
                        Thread thread = new Thread(experiment.startPersonalizedPageRank);
                        pagaRankParameters = new personalizedPageRankThreadParamters(kFolds, nIterations, methodology);
                        thread.Start(pagaRankParameters);
                        threadList.Add(thread);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                foreach (Thread thread in threadList)
                    thread.Join();
                // Close Output file
                Program.logger.Close();
            }
            // Execution Time
            programStopwatch.Stop();
            Console.WriteLine("Execution Time: " + Tools.getExecutionTime(programStopwatch));
            Console.WriteLine("Finished!");
        }
    }
}
