using TwitterRWR.Data;
using System.Collections.Generic;
using System.Collections;
using System;

namespace TweetRecommender
{ 
    public class Recommender 
    {
        private Graph graph;

        public Recommender(Graph graph) 
        {
            this.graph = graph;
        }

        public ArrayList // <Tweet ID, Ranking Value>
        Recommendation(int idxTargetUser, float dampingFactor, int nIteration) // <Ego Nodex Index>, <Random Jump Factor: To Ego>, <# of PageRank Multiplication>
        {
            Console.WriteLine("PageRank Started");
            // Run Random Walk with Restart
            Model model = new Model(graph, dampingFactor, idxTargetUser);
            model.run(nIteration); // # Core Part

            // Sort candidate items(not alreday liked tweets) by their ranking score
            ArrayList recommendation = new ArrayList();
            for (int i = 0; i < model.nNodes; i++) 
            {
                // Suspicious Execution Time
                if (graph.nodes[i].type == NodeType.CANDIDATE)
                    recommendation.Add(new Tweet(graph.nodes[i].id, model.rank[i])); // <Tweet ID, Ranking Score>
            }

            Console.WriteLine("PageRank Finished");
            // Sort the candidate items (descending order)
            // Order by rank first, then by item id(time order; the latest one the higher order) -- Really??: 'Tweet ID' propotional 'Timestamp'
            recommendation.Sort(new TweetDateComparer());
            recommendation.Sort(new TweetScoreComparer());
            Console.WriteLine("Sroting Finished");

            return recommendation;
        }
    }
}
