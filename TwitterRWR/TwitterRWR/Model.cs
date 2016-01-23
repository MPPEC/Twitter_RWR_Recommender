﻿using System;
using System.Collections.Generic;
// Personalized PageRank Algorithm
namespace TweetRecommender
{ 
    public class Model {
        public Graph graph;
        public double[] rank;
        public double[] nextRank;
        public int nNodes;

        public double dampingFactor;
        public double[] restart;

        public Model(Graph graph, double dampingFactor, int targetNode) 
        {
            this.graph = graph;
            this.nNodes = graph.size();
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
                restart[i] = (i == targetNode) ? 1d : 0;
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
            Dictionary<int, ForwardLink[]> forwardLinks = graph.graph; // <Node's Index, Outlinks>
            for (int i = 0; i < nNodes; i++) 
            {
                ForwardLink[] links = forwardLinks[i]; // All out links of 'i'th node
                if (links != null && links.Length > 0) // ???: Equal Expression('links != null', 'links.Length > 0')
                {
                    int nLinks = links.Length;

                    // Deliver rank score with Random Walk
                    double rank_randomWalk = (1 - dampingFactor) * rank[i];
                    for (int w = 0; w < nLinks; w++) 
                    {
                        ForwardLink link = links[w];
                        nextRank[link.targetNode] += rank_randomWalk * link.weight;
                    }

                    // Deliver rank score with Restart
                    double rank_restart = rank[i] - rank_randomWalk;
                    for (int r = 0; r < nNodes; r++)
                        nextRank[r] += rank_restart * restart[r];
                } 
                else 
                {
                    // !!! Suspicious Part: How about virtual links in 'Graph.cs' part 
                    // Dangling node: the rank score is delivered along with only virtual links
                    for (int r = 0; r < nNodes; r++)
                        nextRank[r] += rank[i] * restart[r];
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
