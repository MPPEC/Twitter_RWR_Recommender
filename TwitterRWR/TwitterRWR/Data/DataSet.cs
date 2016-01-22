using TweetRecommender.Others;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace TwitterRWR.Data
{
    public class DataSet
    {
        /***************************** Properties *********************************/
        private HashSet<long> egoLikedTweets; // Total Ego liked tweets within timebound of the timeline
        private HashSet<long> egoLikedTweetsInTimeline;
        private HashSet<long> egoUnLikedTweetsInTimeline;
        private SortedSet<long> timeline;

        /***************************** Constructor *********************************/
        public DataSet()
        {
            egoLikedTweets = new HashSet<long>();
            egoLikedTweetsInTimeline = new HashSet<long>();
            egoUnLikedTweetsInTimeline = new HashSet<long>();
            timeline = new SortedSet<long>(new TweetIDComparer());
            egoLikedTweets.Clear();
            egoLikedTweetsInTimeline.Clear();
            egoUnLikedTweetsInTimeline.Clear();
            timeline.Clear();
        }

        /**************************** Accessor Methods ****************************/
        public HashSet<long> getEgoLikedTweets() { return egoLikedTweets; }
        public HashSet<long> getEgoLikedTweetsInTimeline() { return egoLikedTweetsInTimeline; }
        public HashSet<long> getEgoUnLikedTweetsInTimeline() { return egoUnLikedTweetsInTimeline; }
        public SortedSet<long> getTimeline() { return timeline; }

        /**************************** Setter Methods ****************************/
        public void addEgoLikedTweet(long newTweet)
        {
            egoLikedTweets.Add(newTweet);
        }
        public void addEgoLikedTweetInTimeline(long newTweet)
        {
            egoLikedTweetsInTimeline.Add(newTweet);
            timeline.Add(newTweet);
        }
        public void addEgoUnLikedTweetInTimeline(long newTweet)
        {
            egoUnLikedTweetsInTimeline.Add(newTweet);
            timeline.Add(newTweet);
        }
        // Set Operation: Union
        public void unionWith(DataSet otherDataSet)
        {
            this.egoLikedTweetsInTimeline.UnionWith(otherDataSet.egoLikedTweetsInTimeline);
            this.egoUnLikedTweetsInTimeline.UnionWith(otherDataSet.egoUnLikedTweetsInTimeline);
            this.timeline.UnionWith(otherDataSet.timeline);
        }

        // Check the tweet is in timebound of timeline
        public bool isInTimebound(long tweet)
        {
            TweetIDComparer comparer = new TweetIDComparer();
            // Notice: Timeline is 'reverse chronological order' == minTweet ID > maxTweet ID
            long minTweet = (long)this.timeline.Min;
            long maxTweet = (long)this.timeline.Max;
            // minTweet ID <= tweet ID <= maxTweet ID
            if (comparer.Compare(tweet, minTweet) >= 0 && comparer.Compare(maxTweet, tweet) >= 0)
                return true;
            else
                return false;
        }
        /**************************** Other Methods ****************************/
        public bool contain(long tweet) { return this.timeline.Contains(tweet); }
        public void displaySubTimeline()
        {
            foreach (long tweet in this.timeline)
                Console.WriteLine(tweet);
        }
    }
}