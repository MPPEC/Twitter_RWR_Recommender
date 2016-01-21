using System;
using System.Collections.Generic;
using System.IO;

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
        public string dbFile;
        public int nFolds;
        public int nIterations;

        public ThreadParams(string dbFile, int nFolds, int nIterations) 
        {
            this.dbFile = dbFile;
            this.nFolds = nFolds;
            this.nIterations = nIterations;
        }
    }

    public class Experiment 
    {
        public static void personalizedPageRank(object parameters) 
        {
            try 
            {
                Program.semaphore.WaitOne(); // Wait until Semaphore released

                // Setting environment for experiments
                ThreadParams p = (ThreadParams)parameters;
                string dbFile = p.dbFile;
                int nFolds = p.nFolds;
                int nIterations = p.nIterations;

                // Do experiments for each methodology
                foreach (Methodology methodology in Program.methodologies) 
                {
                    // Get ego user's ID and his like count
                    long egoUser = long.Parse(Path.GetFileNameWithoutExtension(dbFile));

                    // Check if this experiment has ever been performed earlier
                    int m = (int) methodology;
                    if (Program.existingResults.ContainsKey(egoUser) && Program.existingResults[egoUser].Contains(m)) 
                    {
                        lock (Program.locker) 
                        {
                            Console.WriteLine("Ego network(" + egoUser + "): done on experiment #" + m);
                        }
                        continue;
                    }

                    // Final result to put the experimental result per fold together
                    var finalResult = new Dictionary<EvaluationMetric, double>(); // <'HIT(0)', double> or <'AVGPRECISION(1)', double>
                    foreach (EvaluationMetric metric in Enum.GetValues(typeof(EvaluationMetric)))
                        finalResult.Add(metric, 0d); // Initialization

                    // Need to avoid the following error: "Collection was modified; enumeration operation may not execute"
                    List<EvaluationMetric> metrics = new List<EvaluationMetric>(finalResult.Keys);

                    // K-Fold Cross Validation
                    for (int fold = 0; fold < nFolds; fold++) 
                    {
                        // Load graph information from database and then configurate the graph
                        DataLoader loader = new DataLoader(dbFile, nFolds);

                        // #1 Core Part: 'EgoNetwork DB' --> 'Graph Structure(Node, Edge)'
                        loader.graphConfiguration(methodology, fold); 

                        // Nodes and edges of graph
                        Dictionary<int, Node> nodes = loader.allNodes;
                        Dictionary<int, List<ForwardLink>> edges = loader.allLinks;
                        // Mention Count(O), Friendship(X)
                        // Exception: for the case that mention count is included when the friendship is none
                        if (methodology == Methodology.INCL_MENTIONCOUNT
                            || methodology == Methodology.EXCL_FRIENDSHIP
                            || methodology == Methodology.INCL_FOLLOWSHIP_ON_THIRDPARTY_AND_MENTIONCOUNT) 
                        {
                            foreach (List<ForwardLink> forwardLinks in edges.Values) 
                            {
                                List<int> indFriendshipLinks = new List<int>();
                                for (int i = 0; i < forwardLinks.Count; i++) 
                                {
                                    if (forwardLinks[i].type == EdgeType.FRIENDSHIP) // First Check
                                        indFriendshipLinks.Add(i);
                                }
                                foreach (int i in indFriendshipLinks) 
                                {
                                    ForwardLink forwardLink = forwardLinks[i];
                                    if (forwardLink.type == EdgeType.FRIENDSHIP) // Re-check
                                    {
                                        forwardLink.type = EdgeType.UNDEFINED; // FRIENDSHIP --> UNDEFINED
                                        forwardLinks[i] = forwardLink; // Reassignment
                                    }
                                }
                            }
                        }

                        // #2 Core Part: 'Graph Strcture' --> 'PageRank Matrix(but, ragged array)'
                        Graph graph = new Graph(nodes, edges);
                        graph.buildGraph();

                        // #3 Core Part: Recommendation list(Personalized PageRanking Algorithm)
                        Recommender recommender = new Recommender(graph);
                        var recommendation = recommender.Recommendation(0, 0.15f, nIterations); // '0': Ego Node's Index, '0.15f': Damping Factor

                        // #4 Core Part: Validation - AP(Average Precision)
                        int nHits = 0;
                        double sumPrecision = 0;
                        for (int i = 0; i < recommendation.Count; i++) 
                        {
                            if (loader.testSet.Contains(recommendation[i].Key)) 
                            {
                                nHits += 1;
                                sumPrecision += (double) nHits / (i + 1);
                            }
                        }

                        // Add current result to final one
                        foreach (EvaluationMetric metric in metrics) 
                        {
                            switch (metric) 
                            {
                                case EvaluationMetric.HIT:
                                    finalResult[metric] += nHits; 
                                    break;
                                case EvaluationMetric.AVGPRECISION:
                                    finalResult[metric] += (nHits == 0) ? 0 : sumPrecision / nHits; 
                                    break;
                            }
                        }
                    }
                    // Output file(.dat) format: <Ego ID>\t<Experi Code>\t<N-fold>\t<iteration>\t<hit>\t<|prefers tweets|>\t<MAP>
                    lock (Program.locker) 
                    {
                        // Write the result of this ego network to file
                        StreamWriter logger = new StreamWriter(Program.dirData + "result.txt", true);
                        logger.Write(egoUser + "\t" + (int)methodology + "\t" + nFolds + "\t" + nIterations);
                        foreach (EvaluationMetric metric in metrics) 
                        {
                            switch (metric) 
                            {
                                case EvaluationMetric.HIT:
                                    logger.Write("\t" + (int)finalResult[metric] + "\t"); 
                                    break;
                                case EvaluationMetric.AVGPRECISION:
                                    logger.Write("\t" + (finalResult[metric] / nFolds)); 
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
            finally 
            {
                Program.semaphore.Release();
            }
        }
    }
}
