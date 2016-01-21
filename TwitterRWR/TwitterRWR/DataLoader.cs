using System;
using System.Collections.Generic;
using System.IO;

namespace TweetRecommender {
    public class DataLoader {
        /*************************************** Properties ***************************************************/
        // Ego user's ID
        private long egoUserId;
        public int cntLikesOfEgoUser; // !!!: Necessary Variable???

        // Database adapter
        private SQLiteAdapter dbAdapter;

        // Graph information
        private int nNodes = 0; // # of all nodes(equal to last node index)
        private int nLinks = 0; // # of all links
        public Dictionary<int, Node> allNodes = new Dictionary<int, Node>(); // <Node Index, Node Object>, 'Node' defined in 'graph.cs'
        public Dictionary<int, List<ForwardLink>> allLinks = new Dictionary<int, List<ForwardLink>>(); // <Node Index, Out Links List>

        // Necessary for checking node dulpication: <Real ID, Node Index>
        public Dictionary<long, int> userIDs = new Dictionary<long, int>();
        public Dictionary<long, int> memberIDs = new Dictionary<long, int>();
        public Dictionary<long, int> tweetIDs = new Dictionary<long, int>();
        public Dictionary<long, int> thirdPartyIDs = new Dictionary<long, int>();

        // K-Fold Cross Validation
        private int nFolds;
        public HashSet<long> testSet;

        /*************************************** Constructor ***************************************************/
        public DataLoader(string dbPath, int nFolds) 
        {
            this.egoUserId = long.Parse(Path.GetFileNameWithoutExtension(dbPath));
            this.dbAdapter = new SQLiteAdapter(dbPath);
            this.nFolds = nFolds;
        }

        // Add user node including third party users
        public void addUserNode(long id, NodeType type) 
        {
            if (!userIDs.ContainsKey(id)) 
            {
                Node node = new Node(id, type);
                allNodes.Add(nNodes, node); // nNodes plays as 'node index'
                userIDs.Add(id, nNodes);
                if (type == NodeType.USER)
                    memberIDs.Add(id, nNodes); // member: egoUser & friends
                else
                    thirdPartyIDs.Add(id, nNodes); // thirdParty: user out of ego network
                nNodes += 1;
            }
        }

        public void addTweetNode(long id, NodeType type) 
        {
            if (!tweetIDs.ContainsKey(id)) 
            {
                Node node = new Node(id, type);
                allNodes.Add(nNodes, node);
                tweetIDs.Add(id, nNodes);
                nNodes += 1;
            }
        }

        public void addLink(int idxSourceNode, int idxTargetNode, EdgeType type, double weight) 
        {
            if (!allLinks.ContainsKey(idxSourceNode)) // !!! Name change: 'allLinks' --> 'linksFromNodes'
                allLinks.Add(idxSourceNode, new List<ForwardLink>());

            bool exist = false; // flag: already link(also equal type) between two nodes
            foreach (ForwardLink forwardLink in allLinks[idxSourceNode]) 
            {
                if (forwardLink.targetNode == idxTargetNode && forwardLink.type == type) 
                {
                    exist = true;
                    break;
                }
            }

            if (exist == false) 
            {
                ForwardLink link = new ForwardLink(idxTargetNode, type, weight);
                allLinks[idxSourceNode].Add(link);
                nLinks += 1;
            }
        }

        public int getLikeCountOfEgoUser() 
        {
            // Tweet IDs a member likes
            HashSet<long> likes = new HashSet<long>();
            HashSet<long> retweets = dbAdapter.getRetweets(egoUserId);
            foreach (long retweet in retweets)
                likes.Add(retweet);
            HashSet<long> quotes = dbAdapter.getQuotedTweets(egoUserId);
            foreach (long quote in quotes)
                likes.Add(quote);
            HashSet<long> favorites = dbAdapter.getFavoriteTweets(egoUserId);
            foreach (long favorite in favorites)
                likes.Add(favorite);
            cntLikesOfEgoUser = likes.Count;
            return likes.Count;
        }

        // #1 Main Part
        public void graphConfiguration(Methodology type, int fold) 
        {
            List<Feature> features = new List<Feature>();
            switch (type) {
                case Methodology.BASELINE:                          // 0
                    break;
                case Methodology.INCL_FRIENDSHIP:                   // 1
                    features.Add(Feature.FRIENDSHIP);
                    break;
                case Methodology.INCL_FOLLOWSHIP_ON_THIRDPARTY:     // 2
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    break;
                case Methodology.INCL_AUTHORSHIP:                   // 3
                    features.Add(Feature.AUTHORSHIP);
                    break;
                case Methodology.INCL_MENTIONCOUNT:                 // 4
                    features.Add(Feature.FRIENDSHIP);               // temporarily included
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.INCL_ALLFOLLOWSHIP:                // 5
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    break;
                case Methodology.INCL_FRIENDSHIP_AUTHORSHIP:        // 6
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.AUTHORSHIP);
                    break;
                case Methodology.INCL_FRIENDSHIP_MENTIONCOUNT:      // 7
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.ALL:                               // 8
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.AUTHORSHIP);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.EXCL_FRIENDSHIP:                   // 9
                    features.Add(Feature.FRIENDSHIP);               // temporarily included
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.AUTHORSHIP);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.EXCL_FOLLOWSHIP_ON_THIRDPARTY:     // 10
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.AUTHORSHIP);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.EXCL_AUTHORSHIP:                   // 11
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.EXCL_MENTIONCOUNT:                 // 12
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.AUTHORSHIP);
                    break;
                case Methodology.INCL_FOLLOWSHIP_ON_THIRDPARTY_AND_AUTHORSHIP:      // 13
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.AUTHORSHIP);
                    break;
                case Methodology.INCL_FOLLOWSHIP_ON_THIRDPARTY_AND_MENTIONCOUNT:    // 14
                    features.Add(Feature.FRIENDSHIP);                               // temporarily included
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.INCL_AUTHORSHIP_AND_MENTIONCOUNT:                  // 15
                    features.Add(Feature.AUTHORSHIP);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
            }

            // Run graph configuration
            graphConfiguration(features, fold);

            // Close database connection
            dbAdapter.closeDB();
        }
        
        // #2 Main Part
        public void graphConfiguration(List<Feature> features, int fold) 
        {
            // Display all features used in graph
            lock (Program.locker) // Synchronization: Mutex(locker)
            {
                Console.Write("Graph Configuration(" + egoUserId + ": Fold #" + (fold + 1) + "/" + nFolds + ") - ");
                foreach (Feature feature in features)
                    Console.Write(feature + " ");
                Console.WriteLine();
            }

            // Makeup user and tweet nodes and their relations
            addMemberNodes();
            addTweetNodesAndLikeEdges(fold);

            // Add link between users
            if (features.Contains(Feature.FRIENDSHIP)) 
            {
                if (features.Contains(Feature.FOLLOWSHIP_ON_THIRDPARTY))
                    addAllFollowship();
                else
                    addFriendship();
            } 
            else 
            {
                if (features.Contains(Feature.FOLLOWSHIP_ON_THIRDPARTY))
                    addFollowshipOnThirdParty();
            }

            // Add authorship of members
            if (features.Contains(Feature.AUTHORSHIP))
                addAuthorship();

            // Add mention counts among members
            if (features.Contains(Feature.MENTIONCOUNT))
                addMentionCount();

            // Print out the graph information
            printGraphInfo();
        }
        
        // Member = ego U ego's friends
        public void addMemberNodes() 
        {
            // Add ego user's node
            addUserNode(egoUserId, NodeType.USER);

            // Get all friends(also follow ego) in ego-network
            HashSet<long> followeesOfEgoUser = dbAdapter.getFollowingUsers(egoUserId);
            foreach (long followee in followeesOfEgoUser) 
            {
                // Only 'Friends'
                HashSet<long> followeesOfFollowee = dbAdapter.getFollowingUsers(followee);
                if (followeesOfFollowee.Contains(egoUserId))
                    addUserNode(followee, NodeType.USER);
            }
        }
// !!! Need Revision
        public void addTweetNodesAndLikeEdges(int fold) 
        {
            // Tweets that members(ego U friends) like: retweet, quote, favorite
            foreach (long memberId in memberIDs.Keys) 
            {
                // Node index of given member
                int idxMember = userIDs[memberId];

                // Tweet IDs a member likes
                HashSet<long> likes = new HashSet<long>();
                HashSet<long> retweets = dbAdapter.getRetweets(memberId);
                foreach (long retweet in retweets)
                    likes.Add(retweet);
                HashSet<long> quotes = dbAdapter.getQuotedTweets(memberId);
                foreach (long quote in quotes)
                    likes.Add(quote);
                HashSet<long> favorites = dbAdapter.getFavoriteTweets(memberId);
                foreach (long favorite in favorites)
                    likes.Add(favorite);

                // If the user is ego user, his like history is divided into training set and test set.
                if (idxMember == 0) 
                {
                    // Split ego user's like history into two
                    var data = splitLikeHistory(likes, fold);
                    foreach (long tweet in data.Key) // data.Key: train set of like history
                    {               
                        addTweetNode(tweet, NodeType.TWEET);
                        addLink(idxMember, tweetIDs[tweet], EdgeType.LIKE, 1);
                        addLink(tweetIDs[tweet], idxMember, EdgeType.LIKE, 1);
                    }
                    
                    // Set test set(like history not in train set)
                    testSet = data.Value;
                } 
                else 
                {
                    foreach (long tweet in likes) // Maek 'edges' between user and liked tweets
                    {
                        addTweetNode(tweet, NodeType.TWEET);
                        addLink(idxMember, tweetIDs[tweet], EdgeType.LIKE, 1);
                        addLink(tweetIDs[tweet], idxMember, EdgeType.LIKE, 1);
                    }
                }
            }
        }
// !!! Need Revision        
        // K-fold 'TrainSet', 'TestSet'
        public KeyValuePair<HashSet<long>, HashSet<long>> // !!!: 'KeyValuePair' is used for multiple returns(TrainSet, TestSet), so can be replcaed another 'datastructure'
        splitLikeHistory(HashSet<long> likes, int fold) // <Ego liked tweet IDs, Kth fold in Nfolds>
        {
            List<long> likesList = new List<long>();
            foreach (long like in likes)
                likesList.Add(like);
            likesList.Sort(); // Tweet ID propotional to Timestamp(Snowflake Algorithm in Twitter)

            HashSet<long> trainSet = new HashSet<long>();
            HashSet<long> testSet = new HashSet<long>();
            int unitSize = likes.Count / nFolds;
            int idxLowerbound = unitSize * fold;
            int idxUpperbound = (fold < nFolds - 1) ? unitSize * (fold + 1) : likes.Count;
            for (int idx = 0; idx < likesList.Count; idx++) 
            {
                if (idxLowerbound <= idx && idx < idxUpperbound)
                    testSet.Add(likesList[idx]);
                else
                    trainSet.Add(likesList[idx]);
            }
            return new KeyValuePair<HashSet<long>, HashSet<long>>(trainSet, testSet);
        }
        
        public void addFriendship() 
        {
            addFollowship(true, false);
        }

        public void addFollowshipOnThirdParty() 
        {
            addFollowship(false, true);
        }

        public void addAllFollowship() 
        {
            addFollowship(true, true);
        }

        public void addFollowship(bool inclFriendShip, bool inclFollowshipOnThirdparty) 
        {
            foreach (long memberId in memberIDs.Keys) 
            {
                // Node index of given member
                int idxMember = userIDs[memberId];
                // Followees of given member
                HashSet<long> followees = dbAdapter.getFollowingUsers(memberId);
                foreach (long followee in followees) 
                {
                    // !!! Suspicious Part: No guarantee 'friend' relation betweetn a given member and his followee
                    if (memberIDs.ContainsKey(followee)) 
                    {
                        if (inclFriendShip) 
                        {
                            // Add links between members; the member nodes are already included in graph
                            addLink(idxMember, userIDs[followee], EdgeType.FRIENDSHIP, 1);
                            addLink(userIDs[followee], idxMember, EdgeType.FRIENDSHIP, 1);
                        }
                    } 
                    else 
                    {
                        if (inclFollowshipOnThirdparty) 
                        {
                            // Add third part user
                            addUserNode(followee, NodeType.COFOLLOWEE);

                            // Add links between member and third party user
                            addLink(idxMember, userIDs[followee], EdgeType.FOLLOW, 1);
                            addLink(userIDs[followee], idxMember, EdgeType.FOLLOW, 1);
                        }
                    }
                }
            }
        }

        public void addAuthorship() 
        {
            foreach (long memberId in memberIDs.Keys) 
            {
                // Node index of given member
                int idxMember = userIDs[memberId];

                HashSet<long> timeline = dbAdapter.getAuthorship(memberId);
                foreach (long tweet in timeline) 
                {
                    if (!tweetIDs.ContainsKey(tweet))
                        continue;

                    // Add links between ego network member and tweet written by himself/herself
                    addLink(idxMember, tweetIDs[tweet], EdgeType.AUTHORSHIP, 1);
                    addLink(tweetIDs[tweet], idxMember, EdgeType.AUTHORSHIP, 1);
                }
            }
        }
        // Add 'mention edge' between each pair of users in ego-network
        public void addMentionCount() 
        {
            foreach (long memberId1 in memberIDs.Keys) 
            {
                // Node index of member 1
                int idxMember = userIDs[memberId1];

                // Validation check
                if (!allLinks.ContainsKey(idxMember))
                    continue;

                // Get mention count
                var mentionCounts = new Dictionary<int, int>();
                double sumLogMentionCount = 0;
                foreach (long memberId2 in memberIDs.Keys) 
                {
                    if (memberId1 == memberId2)
                        continue;

                    int mentionCount = dbAdapter.getMentionCount(memberId1, memberId2);
                    if (mentionCount > 1) 
                    {
                        mentionCounts.Add(userIDs[memberId2], mentionCount);
                        sumLogMentionCount += Math.Log(mentionCount);
                    }
                }

                // Get the number of friendship links
                int nFriendhips = 0;
                foreach (ForwardLink friendship in allLinks[idxMember]) 
                {
                    if (friendship.type == EdgeType.FRIENDSHIP)
                        nFriendhips += 1;
                }
                // Normalize weight of 'Mention type edge'
                // Add link with the weight as much as mention frequency
                if (sumLogMentionCount > 1) 
                {
                    foreach (int idx in mentionCounts.Keys) 
                    {
                        double weight = nFriendhips * Math.Log(mentionCounts[idx]) / sumLogMentionCount; // Mention Edge Weight
                        addLink(idxMember, idx, EdgeType.MENTION, weight);
                    }
                }
            }
        }

        public void printGraphInfo() 
        {
            lock (Program.locker) 
            {
                Console.WriteLine("\t* Graph information");
                Console.WriteLine("\t\t- # of nodes: " + nNodes
                    + " - User(" + userIDs.Count + "), Tweet(" + tweetIDs.Count + "), ThirdParty(" + thirdPartyIDs.Count + ")");
                Console.WriteLine("\t\t- # of links: " + nLinks);
            }
        }
    }
}
