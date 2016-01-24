using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TweetRecommender.Others
{ 
    public class Tools
    {
        public static string getExecutionTime(Stopwatch stopwatch)
        {
            var timespan = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            return timespan.ToString();
        }
    }

    /*************** IComparer Interface for 'SortedSet<Tweet>' *****************/
    public class TweetIDComparer : IComparer<long>
    {
        public int Compare(long xID, long yID)
        {
            // From 'lattest' tweet to 'oldest' tweet
            if (xID < yID)
                return 1;
            else if (xID == yID)
                return 0;
            else
                return -1;
        }
    }
}
