using TwitterRWR.Data;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace TweetRecommender {
    public class SQLiteAdapter
    {
        /***************************** Properties *********************************/
        private SQLiteConnection conn = null;

        /***************************** Constructor *********************************/
        public SQLiteAdapter(string dbPath)
        {
            try
            {
                SQLiteConnectionStringBuilder connBuilder = new SQLiteConnectionStringBuilder();
                connBuilder.DataSource = dbPath;
                connBuilder.Version = 3;
                connBuilder.JournalMode = SQLiteJournalModeEnum.Wal;
                this.conn = new SQLiteConnection(connBuilder.ToString());
                this.conn.Open();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void closeDB()
        {
            if (conn != null)
                conn.Close();
        }

        /*******************************************************************************/
        /***************************** Primary Methods *********************************/
        /*******************************************************************************/
        // Followee list of 'userId'
        public HashSet<long> getFolloweeList(User user)
        {
            HashSet<long> followeeList = new HashSet<long>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT target FROM follow WHERE source = " + user.ID;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long followee = (long)reader.GetValue(0);
                        followeeList.Add(followee);
                    }
                }
            }
            return followeeList;
        }

        // Tweets published by 'user'
        public HashSet<long> getPublishedTweets(User user)
        {
            HashSet<long> publishedTweetList = new HashSet<long>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT id FROM tweet WHERE author = " + user.ID;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long tweet = (long)reader.GetValue(0);
                        publishedTweetList.Add(tweet);
                    }
                }
            }
            return publishedTweetList;
        }

        // Tweets retweeted by 'userId'
        public HashSet<long> getRetweetList(User user)
        {
            HashSet<long> retweetList = new HashSet<long>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT tweet FROM retweet WHERE user = " + user.ID;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long tweet = (long)reader.GetValue(0);
                        retweetList.Add(tweet);
                    }
                }
            }
            return retweetList;
        }

        // Tweets quoted by 'userId'
        public HashSet<long> getQuoteList(User user)
        {
            HashSet<long> quoteList = new HashSet<long>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT tweet FROM quote WHERE user = " + user.ID;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long tweet = (long)reader.GetValue(0);
                        quoteList.Add(tweet);
                    }
                }
            }
            return quoteList;
        }

        // Tweets favorited by 'userId'
        public HashSet<long> getFavoriteList(User user)
        {
            HashSet<long> favoriteList = new HashSet<long>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT tweet FROM favorite WHERE user = " + user.ID;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long tweet = (long)reader.GetValue(0);
                        favoriteList.Add(tweet);
                    }
                }
            }
            return favoriteList;
        }

        // Tweets published by 'userId'
        public HashSet<long> getAuthorship(long userId)
        {
            HashSet<long> tweetList = new HashSet<long>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT id FROM tweet WHERE author = " + userId;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long tweet = (long)reader.GetValue(0);
                        tweetList.Add(tweet);
                    }
                }
            }
            return tweetList;
        }

        // Total mention count between 'userID1' & 'userID2'
        public int getMentionCount(long userID1, long userID2)
        {
            int mentionCount = 0;
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT COUNT(*) FROM mention WHERE source = " + userID1 + " AND target = " + userID2;
                Int64 count = (Int64)cmd.ExecuteScalar();
                mentionCount += (int)count;
                cmd.CommandText = "SELECT COUNT(*) FROM mention WHERE source = " + userID2 + " AND target = " + userID1;
                count = (Int64)cmd.ExecuteScalar();
                mentionCount += (int)count;
            }
            return mentionCount;
        }
    }
/*
        // Total count of tweets which mention 'userId'
        // Return: <Other User ID, Mention Count>s
        public Dictionary<long, int> getMentionCounts(long userID) 
        {
            Dictionary<long, int> mentionCounts = new Dictionary<long, int>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn)) 
            {
                cmd.CommandText = "SELECT target FROM mention WHERE source = " + userID;
                using (SQLiteDataReader reader = cmd.ExecuteReader()) 
                {
                    while (reader.Read()) 
                    {
                        long target = (long)reader.GetValue(0);
                        if (!mentionCounts.ContainsKey(target))
                            mentionCounts.Add(target, 1);
                        else
                            mentionCounts[target] += 1;
                    }
                }
            }
            return mentionCounts;
        }
*/
}
