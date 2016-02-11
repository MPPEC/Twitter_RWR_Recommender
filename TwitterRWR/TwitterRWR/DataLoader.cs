using TwitterRWR.Data;
using TweetRecommender.Others;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace TweetRecommender {
    public class DataLoader {
        /*************************************** Properties ***************************************************/
        // Database adapter
        private SQLiteAdapter dbAdapter;

        // Ego Network Information
        private User egoUser;
        private Hashtable followeeTable;
        private Hashtable memberTable; // egoUser U followeeTable
        private SortedSet<long> egoTimeline;
        private DataSet[] dataSets;
        private DataSet trainSet, testSet;
        private int numOfFriend;

        // Graph information
        private int nNodes = 0; // # of all nodes(equal to last node index)
        private int nLinks = 0; // # of all links
        public Dictionary<int, Node> allNodes = new Dictionary<int, Node>(); // <Node Index, Node Object>, 'Node' defined in 'graph.cs'
        public Dictionary<int, List<ForwardLink>> allLinksFromNodes = new Dictionary<int, List<ForwardLink>>(); // <Node Index, Nodes' Out Links List>

        // Necessary for checking node dulpication: <Real ID, Node Index>
        public Dictionary<long, int> userIDtoIndex = new Dictionary<long, int>();
        public Dictionary<long, int> memberIDtoIndex = new Dictionary<long, int>();
        public Dictionary<long, int> tweetIDtoIndex = new Dictionary<long, int>();
        public Dictionary<long, int> thirdPartyIDtoIndex = new Dictionary<long, int>();

        /****************************** Constructor **********************************/
        public DataLoader(string dbPath) 
        {
            this.dbAdapter = new SQLiteAdapter(dbPath);
            long egoID = long.Parse(Path.GetFileNameWithoutExtension(dbPath));
            egoUser = new User(egoID);
            numOfFriend = 0;
        }

        /*******************************************************************************/
        /***************************** Primary Methods *********************************/
        /*******************************************************************************/
        // Ego User and Followee Inforamtion
        public void setEgoNetwork()
        {
            // Ego User Information
            egoUser.setFolloweeList(dbAdapter.getFolloweeList(egoUser));
            egoUser.setPublishedTweets(dbAdapter.getPublishedTweets(egoUser));
            egoUser.setRetweets(dbAdapter.getRetweetList(egoUser));
            egoUser.setQuotes(dbAdapter.getQuoteList(egoUser));
            egoUser.setFavorites(dbAdapter.getFavoriteList(egoUser));
            egoUser.updateLikedTweets();

            // Followee Information
            followeeTable = new Hashtable();
            HashSet<long> followeeList = egoUser.getFolloweeList();
            foreach (long followeeID in followeeList)
            {
                User followee = new User(followeeID);
                followee.setFolloweeList(dbAdapter.getFolloweeList(followee));
                followee.setPublishedTweets(dbAdapter.getPublishedTweets(followee));
                followee.setRetweets(dbAdapter.getRetweetList(followee));
                followee.setQuotes(dbAdapter.getQuoteList(followee));
                followee.setFavorites(dbAdapter.getFavoriteList(followee));
                followee.updateLikedTweets();

                if (Program.isOnlyFriendInEgoNetwork == true)
                {
                    // CASE 1: Only Friends
                    if (egoUser.isFriend(followee))
                    {
                        followeeTable.Add(followee.ID, followee);
                        this.numOfFriend++;
                    }
                }
                else
                {
                    // CASE 2: All of followees including friends
                    followeeTable.Add(followee.ID, followee);
                    if (egoUser.isFriend(followee))
                        this.numOfFriend++;
                }
            }

            // Member table: egoUser U followee
            memberTable = new Hashtable();
            memberTable.Add(egoUser.ID, egoUser);
            ICollection followees = followeeTable.Values;
            foreach(User followee in followees)
            {
                memberTable.Add(followee.ID, followee);
            }
        }

        // Timeline(reverse chronological order) 
        public void setEgoTimeline()
        {
            // Ego Timeline(chronological order)
            this.egoTimeline = new SortedSet<long>(new TweetIDComparer());
            ICollection followeeList = this.followeeTable.Values;
            foreach (User followee in followeeList)
            {
                // Domain 1 Version: timeline = retweet(f) U quote(f) U favorite(f)
                HashSet<long> likedTweets = followee.getLikedTweets();
                foreach (long tweet in likedTweets)
                {
                    this.egoTimeline.Add(tweet);
                }
            }
        }

        // Return: Ego Timeline --> Boundary of K sub-timelines --> K sub-datasets
        public void splitTimelineToKfolds(int K)
        {
            this.dataSets = new DataSet[K];
            int boundary = (int)this.egoTimeline.Count / K;
            // Timeline Tranforamtion: SortedSet --> Sorted Array
            long[] timeline = new long[this.egoTimeline.Count];
            this.egoTimeline.CopyTo(timeline);
            // K sub-datasets
            int timelineLikeCount = 0;
            for (int i = 0; i < K; i++)
            {
                this.dataSets[i] = new DataSet();
                for (int j = i * boundary; j < (i + 1) * boundary; j++) // Each sub-dataset boundary
                {
                    long tweet = timeline[j];
                    if (this.egoUser.isLike(tweet))
                    {
                        this.dataSets[i].addEgoLikedTweetInTimeline(tweet);
                        timelineLikeCount++;
                    }
                    else
                        this.dataSets[i].addEgoUnLikedTweetInTimeline(tweet);
                }
            }
        }

        // Return: K-fold 'trainset' and 'testset'
        public void setTrainTestSet(int index)
        {
            this.trainSet = new DataSet();
            this.testSet = dataSets[index]; // TestSet Setting
            this.testSet.clear();

            for (int i = 0; i < this.dataSets.Length; i++)
            {
                if (i != index)
                    trainSet.unionWith(dataSets[i]);
            }
            // Total Ego liked tweets within timebound of the timeline
            egoUser.updateLikedTweets(); // Recharge Liked Tweets: for another kfold validation
            foreach (long tweet in egoUser.getLikedTweets())
            {
                if (testSet.isInTimebound(tweet))
                {
                    testSet.addEgoLikedTweet(tweet);
                    egoUser.deleteLikedTweet(tweet); // Assure candidate tweets are not liked yet by Ego User
                }
                else
                {
                    trainSet.addEgoLikedTweet(tweet);
                }
            }
            // Only Recommend on Ego: |Likes of Ego in TrainSet| >= threshold
            double egoLikeThresholdInTestSet = Program.egoLikeThresholdRatioInTestSet * (trainSet.getCntEgoLikedTweets() + testSet.getCntEgoLikedTweets());
            if (testSet.getCntEgoLikedTweets() < Math.Floor(egoLikeThresholdInTestSet))
                Program.isValidTrainSet = false;
            else
                Program.isValidTrainSet = true;
        }

        // #1 Main Part
        public List<Feature> getFeaturesOnMethodology(Methodology type)
        {
            List<Feature> features = new List<Feature>();
            switch (type)
            {
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
                case Methodology.INCL_FOLLOWSHIP_ON_THIRDPARTY_AND_AUTHORSHIP:      // 8
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.AUTHORSHIP);
                    break;
                case Methodology.INCL_FOLLOWSHIP_ON_THIRDPARTY_AND_MENTIONCOUNT:    // 9
                    features.Add(Feature.FRIENDSHIP);                               // temporarily included
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.INCL_AUTHORSHIP_AND_MENTIONCOUNT:                  // 10
                    features.Add(Feature.FRIENDSHIP);               // temporarily included
                    features.Add(Feature.AUTHORSHIP);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.EXCL_FRIENDSHIP:                   // 11
                    features.Add(Feature.FRIENDSHIP);               // temporarily included
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.AUTHORSHIP);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.EXCL_FOLLOWSHIP_ON_THIRDPARTY:     // 12
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.AUTHORSHIP);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.EXCL_AUTHORSHIP:                   // 13
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
                case Methodology.EXCL_MENTIONCOUNT:                 // 14
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.AUTHORSHIP);
                    break;
                case Methodology.ALL:                               // 15
                    features.Add(Feature.FRIENDSHIP);
                    features.Add(Feature.FOLLOWSHIP_ON_THIRDPARTY);
                    features.Add(Feature.AUTHORSHIP);
                    features.Add(Feature.MENTIONCOUNT);
                    break;
            }

            return features;
        }

        // #2 Main Part
        public void setGraphConfiguration(List<Feature> features)
        {
            // Makeup user and tweet nodes and their relations
            addMemberNodes();
            addTweetNodesAndLikeEdges();

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

            // Close DB connection
            this.dbAdapter.closeDB();
        }

        /*********************************************************************************/
        /***************************** Secondary Methods *********************************/
        /*********************************************************************************/
        // Member = ego U ego's friends
        public void addMemberNodes()
        {
            // Add Ego user node
            addUserNode(egoUser.ID, NodeType.USER);

            // Get all friends(also follow ego) in ego-network
            ICollection followeeList = this.followeeTable.Values;
            foreach(User followee in followeeList)
            {
                addUserNode(followee.ID, NodeType.USER);
            }
        }

        // Add user node(ego, followee, thirdParty(co-followee))
        public void addUserNode(long ID, NodeType type) // <User's ID>
        {
            int newNodeIndex = nNodes; // 'nNodes' plays as 'node index'
            if (!userIDtoIndex.ContainsKey(ID)) // Check already exist
            {
                Node node = new Node(ID, type);
                allNodes.Add(newNodeIndex, node); 
                userIDtoIndex.Add(ID, newNodeIndex);
                if (type == NodeType.USER)
                    memberIDtoIndex.Add(ID, newNodeIndex); // member: egoUser & friends
                else
                    thirdPartyIDtoIndex.Add(ID, newNodeIndex); // thirdParty: (1) !ego && !followee (2) co-followee of ego and followee
                nNodes += 1;
            }
        }

        // Link 'edges' between user and liked tweets
        public void addTweetNodesAndLikeEdges()
        {
            // Tweets that members(ego U friends) like: retweet, quote, favorite
            foreach (long memberID in memberIDtoIndex.Keys)
            {
                // Node index of given member
                int memberIndex = userIDtoIndex[memberID]; // Posssible?? userIDtoIndex --> memberIDtoIndex
                
                // Frist Case(Ego User): Log his like history
                if (memberIndex == 0) // Ego User
                {
                    foreach (long tweet in egoUser.getLikedTweets())
                    {
                        addTweetNode(tweet, NodeType.TWEET);
                        addLink(memberIndex, tweetIDtoIndex[tweet], EdgeType.LIKE, 1);
                        addLink(tweetIDtoIndex[tweet], memberIndex, EdgeType.LIKE, 1);
                    }
                }
                else
                {
                    User followee = (User)followeeTable[memberID];
                    foreach (long tweet in followee.getLikedTweets()) 
                    {
                        if (!this.testSet.contain(tweet)) // Tweets(!Candidate Tweet) liked by followee
                        {
                            addTweetNode(tweet, NodeType.TWEET);
                            addLink(memberIndex, tweetIDtoIndex[tweet], EdgeType.LIKE, 1);
                            addLink(tweetIDtoIndex[tweet], memberIndex, EdgeType.LIKE, 1);
                        }
                        else // Candidate Tweet to recommend fro ego user
                        {
                            addTweetNode(tweet, NodeType.CANDIDATE);
                            addLink(memberIndex, tweetIDtoIndex[tweet], EdgeType.LIKE, 1);
                            addLink(tweetIDtoIndex[tweet], memberIndex, EdgeType.LIKE, 1);
                        }
                    }
                }
            }
        }

        public void addTweetNode(long ID, NodeType type)
        {
            int newNodeIndex = nNodes;
            if (!tweetIDtoIndex.ContainsKey(ID))
            {
                Node node = new Node(ID, type);
                allNodes.Add(newNodeIndex, node);
                tweetIDtoIndex.Add(ID, nNodes);
                nNodes += 1;
            }
        }

        public void addLink(int indexSourceNode, int indexNewTargetNode, EdgeType type, double weight)
        {
            // Create new link structure for source node
            if (!allLinksFromNodes.ContainsKey(indexSourceNode))
                allLinksFromNodes.Add(indexSourceNode, new List<ForwardLink>());
            
            // Check already a link(equal type) between source and target nodes 
            foreach (ForwardLink forwardLink in allLinksFromNodes[indexSourceNode])
            {
                if (forwardLink.targetNode == indexNewTargetNode && forwardLink.type == type)
                    return;
            }
            // Create a new link between source and target nodes
            ForwardLink link = new ForwardLink(indexNewTargetNode, type, weight);
            allLinksFromNodes[indexSourceNode].Add(link);
            nLinks += 1;
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

        public void addFollowship(bool includeFriendShip, bool includeFollowshipOnThirdparty)
        {
            
            if (includeFriendShip)
            {
                if (Program.isGenericFriendship == true)
                {
                    // CASE 1: Friendship Featrue (ego - ego's followee, ego's followee - ego's followee)
                    ICollection memberList1 = memberTable.Values;
                    foreach (User member1 in memberList1)
                    {
                        ICollection memberList2 = memberTable.Values;
                        foreach (User member2 in memberList2)
                        {
                            if (member1.ID != member2.ID)
                            {
                                if (member1.isFriend(member2))
                                {
                                    // Add links between members; the member nodes are already included in graph
                                    addLink(memberIDtoIndex[member1.ID], memberIDtoIndex[member2.ID], EdgeType.FRIENDSHIP, 1);
                                    addLink(memberIDtoIndex[member2.ID], memberIDtoIndex[member1.ID], EdgeType.FRIENDSHIP, 1);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // CASE 2: Friendship Featrue (Only ego - ego's followee)
                    ICollection followeeList = followeeTable.Values;
                    foreach (User followee in followeeList)
                    {
                        if (this.egoUser.isFriend(followee))
                        {
                            // Add links between members; the member nodes are already included in graph
                            addLink(memberIDtoIndex[this.egoUser.ID], memberIDtoIndex[followee.ID], EdgeType.FRIENDSHIP, 1);
                            addLink(memberIDtoIndex[followee.ID], memberIDtoIndex[this.egoUser.ID], EdgeType.FRIENDSHIP, 1);
                        }
                    }
                }
            }
            if (includeFollowshipOnThirdparty)
            {
                ICollection memberList = memberTable.Values;
                foreach (User member in memberList)
                {
                    HashSet<long> followeeList = member.getFolloweeList();
                    
                    foreach (long followee in followeeList)
                    {
                        // Check third party user
                        if (!memberIDtoIndex.ContainsKey(followee))
                        {
                            // Add third party user
                            if (!userIDtoIndex.ContainsKey(followee))
                                addUserNode(followee, NodeType.COFOLLOWEE);
                            // Add links between member and third party user
                            addLink(userIDtoIndex[member.ID], userIDtoIndex[followee], EdgeType.FOLLOW, 1);
                            addLink(userIDtoIndex[followee], userIDtoIndex[member.ID], EdgeType.FOLLOW, 1);
                        }
                    }
                }
            }
        }

        public void addAuthorship()
        {
            foreach (long memberID in memberIDtoIndex.Keys)
            {
                // Node index of given member
                int memberIndex = userIDtoIndex[memberID];

                User member = (User)memberTable[memberID]; // egoUsr U followee
                foreach (long tweet in member.getPublishedTweets())
                {
                    if (!tweetIDtoIndex.ContainsKey(tweet)) // Only existing tweet node can be linked
                        continue;
                    else
                    {
                        // Add links between ego network member and tweet written by himself/herself
                        addLink(memberIndex, tweetIDtoIndex[tweet], EdgeType.AUTHORSHIP, 1);
                        addLink(tweetIDtoIndex[tweet], memberIndex, EdgeType.AUTHORSHIP, 1);
                    }
                }
            }
        }

        // Add 'mention edge' between each pair of users in ego-network
        public void addMentionCount()
        {
            foreach (long memberID1 in memberIDtoIndex.Keys)
            {
                // Node index of member 1
                int memberIndex1 = userIDtoIndex[memberID1];

                // For checking 'Friendshp' potential links
                if (!allLinksFromNodes.ContainsKey(memberIndex1))
                    continue;

                // Get mention count
                var mentionCounts = new Dictionary<int, int>();
                double sumLogMentionCount = 0;
                foreach (long memberID2 in memberIDtoIndex.Keys)
                {
                    if (memberID1 == memberID2)
                        continue;

                    int mentionCount = dbAdapter.getMentionCount(memberID1, memberID2);
                    if (mentionCount > 1)
                    {
                        mentionCounts.Add(userIDtoIndex[memberID2], mentionCount);
                        sumLogMentionCount += Math.Log(mentionCount);
                    }
                }

                // Get the number of friendship links
                int nFriendhips = 0;
                foreach (ForwardLink friendship in allLinksFromNodes[memberIndex1])
                {
                    if (friendship.type == EdgeType.FRIENDSHIP)
                        nFriendhips += 1;
                }
                // Normalize weight of 'Mention type edge'
                // Add link with the weight as much as mention frequency
                if (sumLogMentionCount > 1)
                {
                    foreach (int memberIndex2 in mentionCounts.Keys)
                    {
                        double weight = nFriendhips * Math.Log(mentionCounts[memberIndex2]) / sumLogMentionCount; // Mention Edge Weight
                        addLink(memberIndex1, memberIndex2, EdgeType.MENTION, weight);
                    }
                }
            }
        }

        public void printGraphInfo()
        {
            lock (Program.outFileLocker)
            {
                Console.WriteLine("\t* Graph information");
                Console.WriteLine("\t\t- # of nodes: " + nNodes
                    + " - User(" + userIDtoIndex.Count + "), Tweet(" + tweetIDtoIndex.Count + "), ThirdParty(" + thirdPartyIDtoIndex.Count + ")");
                Console.WriteLine("\t\t- # of links: " + nLinks);
            }
        }

        public DataSet getTestSet() { return this.testSet; }
        public int getNumOfFriend() { return this.numOfFriend; }
    }
}
