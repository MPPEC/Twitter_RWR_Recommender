using System.Collections.Generic;

namespace TweetRecommender
{
    public struct Node 
    {
        public long id;
        public NodeType type;

        public Node(long id, NodeType type) 
        {
            this.id = id;
            this.type = type;
        }
    }
// Maybe Useless
    public struct ForwardLink 
    {
        public int targetNode;
        public EdgeType type;
        public double weight;

        public ForwardLink(int targetNode, double weight) 
        {
            this.targetNode = targetNode;
            this.type = EdgeType.UNDEFINED;
            this.weight = weight;
        }

        public ForwardLink(int targetNode, EdgeType type, double weight) 
        {
            this.targetNode = targetNode;
            this.type = type;
            this.weight = weight;
        }
    }

    public class Graph 
    {
        // Graph information
        public Dictionary<int, Node> nodes;
        public Dictionary<int, List<ForwardLink>> edges;

        // Weighted and directed graph (the weights are adjusted)
        public Dictionary<int, ForwardLink[]> graph;

        public Graph(Dictionary<int, Node> nodes, Dictionary<int, List<ForwardLink>> edges) 
        {
            this.nodes = nodes;
            this.edges = edges;
            this.graph = new Dictionary<int, ForwardLink[]>();
        }

        public void buildGraph() 
        {
            for (int i = 0; i < nodes.Count; i++) 
            {
                ForwardLink[] forwardLinks = null;

                if (edges.ContainsKey(i)) 
                {
                    // Count the number of explicit(defined) forwardlinks
                    int nExplicitLinks = 0;
                    foreach (ForwardLink forwardLink in edges[i]) // edges[i]: all forward links of 'i'th node
                    {
                        if (forwardLink.type != EdgeType.UNDEFINED)
                            nExplicitLinks += 1;
                    }
                    
                    // The graph should consider only explicit forward links
                    if (nExplicitLinks > 0) 
                    { 
                        // Make ragged array of forward links for a node: To avoid 'Sparse Matrix'
                        forwardLinks = new ForwardLink[nExplicitLinks];

                        // Calculate sum of weights of the given source node (entry.Key)
                        int idx = 0;
                        double sumWeights = 0;
                        foreach (ForwardLink link in edges[i]) // edges[i]: all forward links of 'i'th node
                        {
                            if (link.type != EdgeType.UNDEFINED) 
                            {
                                // Add link to array and its weight to summation
                                forwardLinks[idx++] = link;
                                sumWeights += link.weight;
                            }
                        }
                        // Adjust weights whose sum is 1
                        for (int f = 0; f < nExplicitLinks; f++)
                            forwardLinks[f].weight /= sumWeights;
                    }
                }
                // Add filterd forward links of 'i'th node
                graph.Add(i, forwardLinks);
            }
        }

        // Get the number of nodes in the graph
        public int size() 
        {
            return nodes.Count;
        }
    }
}
