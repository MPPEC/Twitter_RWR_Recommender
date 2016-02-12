using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rwrResultRefine
{
    class RWRMetric
    {
        // Properties
        public long egoID;
        private int method;
        private int kFold;
        private int iteration;
        private double MAP;
        private double recall;
        private int like;
        private int hit;
        private int friend;
        private string executionTime;

        // Constructor
        public RWRMetric
        (long egoID, int method, int kFold, int iteration, double MAP, double recall, int like, int hit, int friend, string executionTime)
        {
            this.egoID = egoID;
            this.method = method;
            this.kFold = kFold;
            this.iteration = iteration;
            this.MAP = MAP;
            this.recall = recall;
            this.like = like;
            this.hit = hit;
            this.friend = friend;
            this.executionTime = executionTime;
        }

        // Output RWR Result
        public void logResultIntoFile(StreamWriter writer)
        {
            writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4:F15}\t{5:F15}\t{6}\t{7}\t{8}\t{9}",
            egoID, method, kFold, iteration, MAP, recall, like, hit, friend, executionTime);
            writer.Flush();
        }
    }
}
