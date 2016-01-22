using TwitterRWR.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading; // Multi-threading Library
using System.Threading.Tasks;

namespace TweetRecommender 
{
    public enum Methodology // Total: 16
    { 
        BASELINE,
        INCL_FRIENDSHIP, INCL_FOLLOWSHIP_ON_THIRDPARTY, INCL_AUTHORSHIP, INCL_MENTIONCOUNT,
        INCL_ALLFOLLOWSHIP, INCL_FRIENDSHIP_AUTHORSHIP, INCL_FRIENDSHIP_MENTIONCOUNT,
        ALL,
        EXCL_FRIENDSHIP, EXCL_FOLLOWSHIP_ON_THIRDPARTY, EXCL_AUTHORSHIP, EXCL_MENTIONCOUNT,
        INCL_FOLLOWSHIP_ON_THIRDPARTY_AND_AUTHORSHIP, INCL_FOLLOWSHIP_ON_THIRDPARTY_AND_MENTIONCOUNT, INCL_AUTHORSHIP_AND_MENTIONCOUNT
    }
    public enum Feature { FRIENDSHIP, FOLLOWSHIP_ON_THIRDPARTY, AUTHORSHIP, MENTIONCOUNT } 
    public enum EvaluationMetric { HIT, AVGPRECISION }

    public struct ThreadParams 
    {
        public DataLoader loader;
        public Methodology methodology;
        public int fold;

        public ThreadParams(DataLoader loader, Methodology methodology, int fold) 
        {
            this.loader = loader;
            this.methodology = methodology;
            this.fold = fold;
        }
    }

    public class Experiment 
    {
        /*************************** Properties **************************************/
        private string dbPath;
        private int nFold;
        private int nIteration;
        private Dictionary<EvaluationMetric, double> finalResult;
        private List<EvaluationMetric> metrics;

        /****************************** Constructor **********************************/
        public Experiment(string dbPath, int nFolds, int nIterations)
        {
            this.dbPath = dbPath;
            this.nFold = nFolds;
            this.nIteration = nIterations;
        }

        /*******************************************************************************/
        /***************************** Primary Methods *********************************/
        /*******************************************************************************/
        public void startPersonalizedPageRank() 
        {
            try
            {
                // Do experiments for each methodology
                foreach (Methodology methodology in Program.methodologies)
                {
                    // Get ego user's ID and his like count
                    long egoUser = long.Parse(Path.GetFileNameWithoutExtension(this.dbPath));

                    // Check if this experiment has ever been performed earlier
                    int m = (int)methodology;
                    if (Program.existingResults.ContainsKey(egoUser) && Program.existingResults[egoUser].Contains(m))
                    {
                        lock (Program.locker)
                        {
                            Console.WriteLine("Ego network(" + egoUser + "): done on experiment #" + m);
                        }
                        continue;
                    }

                    // Final result to put the experimental result per fold together
                    this.finalResult = new Dictionary<EvaluationMetric, double>(); // <'HIT(0)', double> or <'AVGPRECISION(1)', double>
                    foreach (EvaluationMetric metric in Enum.GetValues(typeof(EvaluationMetric)))
                        this.finalResult.Add(metric, 0d); // Initialization

                    // Need to avoid the following error: "Collection was modified; enumeration operation may not execute"
                    metrics = new List<EvaluationMetric>(this.finalResult.Keys);

                    // K-Fold Cross Validation
                    List<Thread> threadList = new List<Thread>(); // 'Thread': Standard Library Class
                    for (int fold = 0; fold < nFold; fold++) // // One fold to One thread
                    {
                        // Load graph information from database and then configurate the graph
                        DataLoader loader = new DataLoader(this.dbPath);
                        loader.setEgoNetwork();
                        loader.setEgoTimeline();
                        loader.splitTimelineToKfolds(this.nFold);
                        Thread thread = new Thread(new ParameterizedThreadStart(runKfoldCrossValidation)); // Core Part
                        ThreadParams parameters = new ThreadParams(loader, methodology, fold); // 'ThreadParams': defined in 'Experiment.cs'
                        thread.Start(parameters);
                        threadList.Add(thread);
                    }
                    // Synchronization: Wait until threads be terminated
                    foreach (Thread thread in threadList)
                        thread.Join();

                    // Output file(.dat) format: <Ego ID>\t<Experi Code>\t<N-fold>\t<iteration>\t<hit>\t<|prefers tweets|>\t<MAP>
                    lock (Program.locker)
                    {
                        // Write the result of this ego network to file
                        StreamWriter logger = new StreamWriter(Program.dirData + "result.txt", true);
                        logger.Write(egoUser + "\t" + (int)methodology + "\t" + this.nFold + "\t" + this.nIteration);
                        foreach (EvaluationMetric metric in metrics)
                        {
                            switch (metric)
                            {
                                case EvaluationMetric.HIT:
                                    logger.Write("\t" + (int)finalResult[metric] + "\t");
                                    break;
                                case EvaluationMetric.AVGPRECISION:
                                    logger.Write("\t" + (finalResult[metric] / this.nFold));
                                    break;
                            }
                        }
                        logger.WriteLine();
                        logger.Close();
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
        }

        private void runKfoldCrossValidation(object parameters)
        {
            try
            {
                Program.semaphore.WaitOne(); // Wait until Semaphore released

                // Setting environment for experiments
                ThreadParams p = (ThreadParams)parameters;
                DataLoader loader = p.loader;
                Methodology methodology = p.methodology;
                int fold = p.fold;

                // #1 Core Part: 'EgoNetwork DB' --> 'Graph Strctures(Node, Edge)'
                loader.setTrainTestSet(fold);
                List<Feature> features = loader.getFeaturesOnMethodology(methodology);
                loader.setGraphConfiguration(features);

                // Graph Elements: Nodes and edges
                Dictionary<int, Node> nodes = loader.allNodes;
                Dictionary<int, List<ForwardLink>> edges = loader.allLinksFromNodes;

                // Incase: Mention Count(O), Friendship(X)
                if (methodology == Methodology.INCL_MENTIONCOUNT
                    || methodology == Methodology.INCL_AUTHORSHIP_AND_MENTIONCOUNT
                    || methodology == Methodology.INCL_FOLLOWSHIP_ON_THIRDPARTY_AND_MENTIONCOUNT
                    || methodology == Methodology.EXCL_FRIENDSHIP)
                {
                    foreach (List<ForwardLink> forwardLinks in edges.Values)
                    {
                        for (int i = 0; i < forwardLinks.Count; i++)
                        {
                            if (forwardLinks[i].type == EdgeType.FRIENDSHIP)
                            {
                                ForwardLink revisedForwardLink = forwardLinks[i];
                                revisedForwardLink.type = EdgeType.UNDEFINED; // FRIENDSHIP --> UNDEFINED
                                forwardLinks[i] = revisedForwardLink;
                            }
                        }
                    }
                }

                // #2 Core Part: 'Graph Strcture(Nodes, Edges)' --> 'PageRank Matrix(2D-ragged array)'
                Graph graph = new Graph(nodes, edges);
                graph.buildGraph();

                // #3 Core Part: Recommendation list(Personalized PageRanking Algorithm)
                Recommender recommender = new Recommender(graph);
                var recommendation = recommender.Recommendation(0, 0.15f, this.nIteration); // '0': Ego Node's Index, '0.15f': Damping Factor

                // #4 Core Part: Validation - AP(Average Precision)
                DataSet testSet = loader.getTestSet();
                HashSet<long> egoLikedTweets = testSet.getEgoLikedTweets();  
                int nHits = 0;
                double AP = 0.0, sumPrecision = 0.0; // Average Precision
                for (int i = 0; i < recommendation.Count; i++)
                {
                    if (egoLikedTweets.Contains(recommendation[i].Key))
                    {
                        nHits += 1;
                        sumPrecision += (double)nHits / (i + 1);
                    }
                }
                if (nHits != 0)
                    AP = sumPrecision / nHits;
                else
                    AP = 0.0;
                Console.WriteLine("Average Precision: " + AP);

                // Add current result to final one
                foreach (EvaluationMetric metric in this.metrics)
                {
                    switch (metric)
                    {
                        case EvaluationMetric.HIT:
                            this.finalResult[metric] += nHits;
                            break;
                        case EvaluationMetric.AVGPRECISION:
                            this.finalResult[metric] += AP;
                            break;
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Program.semaphore.Release();
            }
        }
    }
}
