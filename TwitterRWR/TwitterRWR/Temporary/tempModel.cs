using System;
using System.Collections;
using System.Collections.Generic;
// Personalized PageRank Algorithm
namespace TweetRecommender
{
    public class Model
    {
        public Graph graph;
        public double[] rank;
        public double[] nextRank;
        public int nNodes;

        public double dampingFactor;
        public double[] restart;

        public Model(Graph graph, double dampingFactor, int targetNode)
        {
            this.graph = graph;
            this.nNodes = graph.getCntAllNodes();
            this.dampingFactor = dampingFactor;

            rank = new double[nNodes];
            nextRank = new double[nNodes];
            restart = new double[nNodes];

            for (int i = 0; i < nNodes; i++)
            {
                // Initialize rank score of each node
                rank[i] = (i == targetNode) ? nNodes : 0;
                nextRank[i] = 0;

                // Make restart weight
                restart[i] = (i == targetNode) ? 1d : 0; // [1, 0, 0, 0, 0, ... , 0, 0, 0]: Personalized PageRank
            }
        }

        public void run(int nIterations)
        {
            for (int n = 0; n < nIterations; n++)
            {
                deliverRanks();
                updateRanks();
                Console.WriteLine("{0}th Iteration Finished", n + 1);
            }
        }

        // Deliver ranks along with forward links
        public void deliverRanks()
        {
            ArrayList forwardLinks = graph.matrix; // <Node's Index, Outlinks>
            ForwardLink[] linkList = null;
            ForwardLink link;
            int nLinks;
            double rank_randomWalk, rank_restart;

            for (int i = 0; i < nNodes; i++)
            {
                linkList = (ForwardLink[])forwardLinks[i]; // 'i'th node links
                nLinks = linkList.Length;

                // Deliver rank score with Random Walk
                rank_randomWalk = (1 - dampingFactor) * rank[i];
                for (int w = 0; w < nLinks; w++)
                {
                    link = linkList[w];
                    nextRank[link.targetNode] += rank_randomWalk * link.weight;
                }

                // Deliver rank score with Restart(Random Jump)
                rank_restart = dampingFactor * rank[i];
                for (int r = 0; r < nNodes; r++)
                    nextRank[r] += rank_restart * restart[r];

                if ((i % 1000) == 0)
                {
                    Console.WriteLine("{0}", i);
                }
            }
        }

        // Replace current rank score with new one
        public void updateRanks()
        {
            for (int i = 0; i < nNodes; i++)
            {
                rank[i] = nextRank[i];
                nextRank[i] = 0;
            }
        }
    }
}
