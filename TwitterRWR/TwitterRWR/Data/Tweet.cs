using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterRWR.Data
{
    public class Tweet
    {
        /***************************** Properties *********************************/
        public long ID;
        public double rankingScore;

        /***************************** constructor *********************************/
        public Tweet(long newID, double newRankingScore)
        {
            this.ID = newID;
            this.rankingScore = newRankingScore;
        }
    }

    /*************** IComparer Interface for 'ArrayList(Recommendation List)' *****************/
    public class TweetDateComparer : IComparer
    {
        public int Compare(Object x, Object y)
        {
            Tweet tweetX = (Tweet)x;
            Tweet tweetY = (Tweet)y;
            double xID = tweetX.ID;
            double yID = tweetY.ID;
            // From 'lattest' tweet to 'oldest' tweet
            if (xID < yID)
                return 1;
            else if (xID == yID)
                return 0;
            else
                return -1;
        }
    }

    public class TweetScoreComparer : IComparer
    {
        public int Compare(Object x, Object y)
        {
            Tweet tweetX = (Tweet)x;
            Tweet tweetY = (Tweet)y;
            double xScore = tweetX.rankingScore;
            double yScore = tweetY.rankingScore;
            // From 'lattest' tweet to 'oldest' tweet
            if (xScore < yScore)
                return 1;
            else if (xScore == yScore)
                return 0;
            else
                return -1;
        }
    }
}
