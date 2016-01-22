using System;
using System.Collections;
using System.Collections.Generic;

namespace TwitterRWR.Data
{
    public class User
    {
        /***************************** Properties *********************************/
        public long ID;
        private HashSet<long> followeeList;
        private HashSet<long> publishedTweets;
        private HashSet<long> retweets;
        private HashSet<long> quotes;
        private HashSet<long> favorites;
        private HashSet<long> likedTweets; // retweets U quotes U favorites

        /***************************** Constructor *********************************/
        public User(long newID)
        {
            this.ID = newID;
            this.likedTweets = new HashSet<long>();
        }

        /***************************** Core Methods *********************************/
        public bool isLike(long tweet)
        {
            return likedTweets.Contains(tweet);
        }
        public bool isFriend(User otherUser)
        {
            return (this.followeeList.Contains(otherUser.ID) && otherUser.followeeList.Contains(this.ID));
        }

        /**************************** Accessor Methods ****************************/
        public long getID() { return this.ID; }
        public HashSet<long> getFolloweeList() { return this.followeeList; }
        public HashSet<long> getPublishedTweets() { return this.publishedTweets; }
        public HashSet<long> getRetweets() { return this.retweets; }
        public HashSet<long> getQuotes() { return this.quotes; }
        public HashSet<long> getFavorites() { return this.favorites; }
        public HashSet<long> getLikedTweets() { return new HashSet<long>(likedTweets); }

        /**************************** Setter Methods ****************************/
        public void setFolloweeList(HashSet<long> newFolloweeList) { this.followeeList = newFolloweeList; }
        public void setLikedTweets(HashSet<long> newLikedTweets) { this.likedTweets = newLikedTweets; }
        public void setPublishedTweets(HashSet<long> newPublishedTweets) { this.publishedTweets = newPublishedTweets; }
        public void setRetweets(HashSet<long> newRetweets) { this.retweets = newRetweets; }
        public void setQuotes(HashSet<long> newQuotes) { this.quotes = newQuotes; }
        public void setFavorites(HashSet<long> newFavorites) { this.favorites = newFavorites; }
        // Liked Tweets = retweet U quote U favorite
        public void updateLikedTweets()
        {
            likedTweets.Clear();
            foreach (long tweet in retweets)
                likedTweets.Add(tweet);
            foreach (long tweet in quotes)
                likedTweets.Add(tweet);
            foreach (long tweet in favorites)
                likedTweets.Add(tweet);
        }
        public void deleteLikedTweet(long tweet)
        {
            if (this.likedTweets.Contains(tweet))
                this.likedTweets.Remove(tweet);
            if (this.publishedTweets.Contains(tweet))
                this.publishedTweets.Remove(tweet);
            if (this.retweets.Contains(tweet))
                this.retweets.Remove(tweet);
            if (this.quotes.Contains(tweet))
                this.quotes.Remove(tweet);
            if (this.favorites.Contains(tweet))
                this.favorites.Remove(tweet);
        }
    }
}