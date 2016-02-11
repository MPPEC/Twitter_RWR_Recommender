using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderClassification
{
    class EgoNetwork
    {
        /***************************** Properties *********************************/
        public long egoID { get; }
        public int optimalLabel { get; }
        public int predictLabel { get; set; }
        public double[] attributes;
        public double[] rwrResults;

        /***************************** Constructor *********************************/
        public EgoNetwork(long newEgoID, int newOptimalLabel, double[] newRwrResults, double[] newAttributes)
        {
            this.egoID = newEgoID;
            this.optimalLabel = newOptimalLabel;
            this.rwrResults = newRwrResults;
            this.attributes = newAttributes;           
        }

        /***************************** Override *********************************/
        public override bool Equals(Object obj)
        {
            if (obj is EgoNetwork)
            {
                EgoNetwork otherTweet = (EgoNetwork)obj;
                return (this.egoID == otherTweet.egoID);
            }
            else
                throw new ArgumentException("Object is not EgoNetwork");
        }
        public override int GetHashCode() { return (int)((this.egoID >> 32) ^ this.egoID); }
    }
}
