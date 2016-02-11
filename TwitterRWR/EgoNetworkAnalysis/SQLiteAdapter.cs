using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace EgoNetworkAnalysis
{
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
        // Followee list of 'userID'
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
        public HashSet<Tweet> getPublishedTweets(User user)
        {
            HashSet<Tweet> publishedTweetList = new HashSet<Tweet>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT id, author, isMention FROM tweet WHERE author = " + user.ID;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long tweetID = (long)reader.GetValue(0);
                        long author = (long)reader.GetValue(1);
                        bool isMention = false;
                        if (reader.GetInt32(2) == 1)
                            isMention = true;
                        publishedTweetList.Add(new Tweet(tweetID, author, isMention));
                    }
                }
            }
            return publishedTweetList;
        }

        // Tweets retweeted by 'userId'
        public HashSet<Tweet> getRetweetList(User user)
        {
            HashSet<Tweet> retweetList = new HashSet<Tweet>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT tweet.id, tweet.author, tweet.isMention FROM retweet, tweet WHERE retweet.user = " + user.ID + " and retweet.tweet = tweet.id";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long tweetID = (long)reader.GetValue(0);
                        long author = (long)reader.GetValue(1);
                        bool isMention = false;
                        if (reader.GetInt32(2) == 1)
                            isMention = true;
                        retweetList.Add(new Tweet(tweetID, author, isMention));
                    }
                }
            }
            return retweetList;
        }

        // Tweets quoted by 'userId'
        public HashSet<Tweet> getQuoteList(User user)
        {
            HashSet<Tweet> quoteList = new HashSet<Tweet>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT tweet.id, tweet.author, tweet.isMention FROM quote, tweet WHERE quote.user = " + user.ID + " and quote.tweet = tweet.id";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long tweetID = (long)reader.GetValue(0);
                        long author = (long)reader.GetValue(1);
                        bool isMention = false;
                        if (reader.GetInt32(2) == 1)
                            isMention = true;
                        quoteList.Add(new Tweet(tweetID, author, isMention));
                    }
                }
            }
            return quoteList;
        }

        // Tweets favorited by 'userId'
        public HashSet<Tweet> getFavoriteList(User user)
        {
            HashSet<Tweet> favoriteList = new HashSet<Tweet>();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT tweet.id, tweet.author, tweet.isMention FROM favorite, tweet WHERE favorite.user = " + user.ID + " and favorite.tweet = tweet.id";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long tweetID = (long)reader.GetValue(0);
                        long author = (long)reader.GetValue(1);
                        bool isMention = false;
                        if (reader.GetInt32(2) == 1)
                            isMention = true;
                        favoriteList.Add(new Tweet(tweetID, author, isMention));
                    }
                }
            }
            return favoriteList;
        }

        // Total mention count between 'user1' & 'user2'
        public int getMentionCount(User user1, User user2)
        {
            int mentionCount = 0;
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "SELECT COUNT(*) FROM mention WHERE source = " + user1.ID + " AND target = " + user2.ID;
                Int64 count = (Int64)cmd.ExecuteScalar();
                mentionCount += (int)count;
                cmd.CommandText = "SELECT COUNT(*) FROM mention WHERE source = " + user2.ID + " AND target = " + user1.ID;
                count = (Int64)cmd.ExecuteScalar();
                mentionCount += (int)count;
            }
            return mentionCount;
        }
    }
}
