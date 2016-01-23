using System.Collections.Generic;

namespace TweetRecommender
{
    // Below 'enum' part better move to 'graph.cs' file
    public enum NodeType { USER, TWEET, CANDIDATE, COFOLLOWEE }
    public enum EdgeType { UNDEFINED, LIKE, FRIENDSHIP, FOLLOW, MENTION, AUTHORSHIP} // Unnecessary: 'PURCHASE', 'ETC' 

    public class Recommender 
    {
        private Graph graph;

        public Recommender(Graph graph) 
        {
            this.graph = graph;
        }

        public List<KeyValuePair<long, double>> // <Tweet ID, Ranking Value>
        Recommendation(int idxTargetUser, float dampingFactor, int nIteration) // <Ego Nodex Index>, <Random Jump Factor: To Ego>, <# of PageRank Multiplication>
        {
            // Run Random Walk with Restart
            Model model = new Model(graph, dampingFactor, idxTargetUser);
            model.run(nIteration); // # Core Part
// Maybe Useless
            // Make exception list of items(tweets) already liked by ego user
            var linksOfTargetUser = new List<int>();
            foreach (ForwardLink link in graph.edges[idxTargetUser]) 
            {
                if (link.type == EdgeType.LIKE)
                    linksOfTargetUser.Add(link.targetNode);
            }

            // Sort candidate items(not alreday liked tweets) by their ranking score
            var recommendation = new List<KeyValuePair<long, double>>();
            for (int i = 0; i < model.nNodes; i++) 
            {
                if (graph.nodes[i].type == NodeType.CANDIDATE)
                    recommendation.Add(new KeyValuePair<long, double>(graph.nodes[i].id, model.rank[i])); // <Tweet ID, Ranking Score>
            }

            // Sort the candidate items (descending order)
            // Order by rank first, then by item id(time order; the latest one the higher order) -- Really??: 'Tweet ID' propotional 'Timestamp'
            recommendation.Sort((one, another) => 
            {
                int result = one.Value.CompareTo(another.Value) * -1;
                return result != 0 ? result : one.Key.CompareTo(another.Key) * -1;
            });
            return recommendation;
        }
    }
}
