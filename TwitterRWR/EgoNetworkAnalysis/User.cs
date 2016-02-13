using System;
using System.Collections;
using System.Collections.Generic;

namespace EgoNetworkAnalysis
{
    public class User
    {
        /***************************** Properties *********************************/
        public long ID { get; }
        private HashSet<long> followeeList;
        private HashSet<Tweet> publishedTweets;
        private HashSet<Tweet> retweets;
        private HashSet<Tweet> quotes;
        private HashSet<Tweet> favorites;
        private HashSet<Tweet> likedTweets; // retweets U quotes U favorites

        /***************************** Constructor *********************************/
        public User(long newID)
        {
            this.ID = newID;
            this.likedTweets = new HashSet<Tweet>();
        }
        public User(User newUser)
        {
            this.ID = newUser.ID;
            this.followeeList = new HashSet<long>(followeeList);
            this.publishedTweets = deepCloneHashSetTweet(newUser.publishedTweets);
            this.retweets = deepCloneHashSetTweet(newUser.retweets);
            this.quotes = deepCloneHashSetTweet(newUser.quotes);
            this.favorites = deepCloneHashSetTweet(newUser.favorites);
            this.updateLikedTweets();
        }
        /***************************** Core Methods *********************************/        
        // Similarity Measure: 'Ego user' to 'Followee'
        public double jaccardSimilarity(User otherUser)
        {
            // A: user liked tweets, B: other user liked tweets
            HashSet<Tweet> A = new HashSet<Tweet>(this.likedTweets); // Shallow Copy
            HashSet<Tweet> B = new HashSet<Tweet>(otherUser.getLikedTweets()); // Shallow Copy
            double cardinalityA, cardinalityB, intersection = 0;
            cardinalityA = (double)A.Count;
            cardinalityB = (double)B.Count;

            // Intersection
            A.IntersectWith(B);
            intersection = (double)A.Count;
            // Jaccard Similarity
            double similarScore =
                (intersection / (cardinalityA + cardinalityB - intersection));

            return similarScore;
        }

        public bool isLike(Tweet tweet)
        {
            return likedTweets.Contains(tweet);
        }
        public bool isFollow(User otherUser)
        {
            return this.followeeList.Contains(otherUser.ID);
        }
        public bool isFriend(User otherUser)
        {
            return (this.followeeList.Contains(otherUser.ID) && otherUser.followeeList.Contains(this.ID));
        }

        /**************************** Accessor Methods ****************************/
        public long getID() { return this.ID; }
        public HashSet<long> getFolloweeList() { return new HashSet<long>(this.followeeList); }
        public HashSet<Tweet> getPublishedTweets() { return deepCloneHashSetTweet(this.publishedTweets); }
        public HashSet<Tweet> getRetweets() { return deepCloneHashSetTweet(this.retweets); }
        public HashSet<Tweet> getQuotes() { return deepCloneHashSetTweet(this.quotes); }
        public HashSet<Tweet> getFavorites() { return deepCloneHashSetTweet(this.favorites); }
        public HashSet<Tweet> getLikedTweets() { return deepCloneHashSetTweet(this.likedTweets); }

        /**************************** Setter Methods ****************************/
        public void setFolloweeList(HashSet<long> newFolloweeList) { this.followeeList = newFolloweeList; }
        public void setLikedTweets(HashSet<Tweet> newLikedTweets) { this.likedTweets = newLikedTweets; }
        public void setPublishedTweets(HashSet<Tweet> newPublishedTweets) { this.publishedTweets = newPublishedTweets; }
        public void setRetweets(HashSet<Tweet> newRetweets) { this.retweets = newRetweets; }
        public void setQuotes(HashSet<Tweet> newQuotes) { this.quotes = newQuotes; }
        public void setFavorites(HashSet<Tweet> newFavorites) { this.favorites = newFavorites; }       
        // Liked Tweets = retweet U quote U favorite
        public void updateLikedTweets()
        {
            likedTweets.Clear();
            foreach (Tweet tweet in retweets)
                likedTweets.Add(tweet);
            foreach (Tweet tweet in quotes)
                likedTweets.Add(tweet);
            foreach (Tweet tweet in favorites)
                likedTweets.Add(tweet);
        }
        public void deleteLikedTweet(Tweet tweet)
        {
            if (this.likedTweets.Contains(tweet))
                this.likedTweets.Remove(tweet);
        }

        /***************************** Other Methods *********************************/
        // Deep Copy: tweetSet0 --> tweetSet1
        public HashSet<Tweet> deepCloneHashSetTweet(HashSet<Tweet> tweetSet0)
        {
            HashSet<Tweet> tweetSet1 = new HashSet<Tweet>();
            foreach(Tweet tweet in tweetSet0)
            {
                tweetSet1.Add(new Tweet(tweet));
            }

            return tweetSet1;
        }
    }
}