using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TweetRecommender.Others;
// Personalized PageRank Algorithm
namespace TweetRecommender
{
    public class Model
    {
        /*************************** Properties **************************************/
        private ArrayList linkMatrix;
        private double dampingFactor;
        public double[] rank;
        public double[] nextRank;
        public int nNodes;
        public int egoNode;

        /****************************** Constructor **********************************/
        // Personalized PageRank Algorithm: All node do random jump to Ego node
        public Model(Graph graph, double dampingFactor, int targetNode)
        {
            this.linkMatrix = graph.matrix;
            this.nNodes = graph.getCntAllNodes();
            this.dampingFactor = dampingFactor;
            this.egoNode = targetNode;

            rank = new double[nNodes];
            nextRank = new double[nNodes];

            for (int i = 0; i < nNodes; i++)
            {
                // Initialize rank score of each node
                rank[i] = (i == targetNode) ? nNodes : 0;
                nextRank[i] = 0;
            }
        }

        public void run(int nIterations)
        {
            for (int n = 0; n < nIterations; n++)
            {
                deliverRanks();
                updateRanks();
            }
        }

        // Deliver ranks along with forward links
        public void deliverRanks()
        {
            ForwardLink[] linkList = null;
            ForwardLink link;
            int nLinks;
            double rank_randomWalk, rank_restart;

            for (int i = 0; i < nNodes; i++)
            {
                linkList = (ForwardLink[])this.linkMatrix[i]; // 'i'th node links
                nLinks = linkList.Length;

                // Random Walk
                rank_randomWalk = (1 - dampingFactor) * rank[i];
                for (int w = 0; w < nLinks; w++)
                {
                    link = linkList[w];
                    nextRank[link.targetNode] += rank_randomWalk * link.weight;
                }

                // Random Jump: Personalized PageRank
                rank_restart = dampingFactor * rank[i];
                nextRank[this.egoNode] += rank_restart;
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
