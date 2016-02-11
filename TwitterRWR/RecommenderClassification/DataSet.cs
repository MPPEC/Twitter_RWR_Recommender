using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderClassification
{
    class DataSet
    {
        /***************************** Properties *********************************/
        public HashSet<EgoNetwork> egoNetworkList { get; }

        /***************************** Constructor *********************************/
        public DataSet()
        {
            egoNetworkList = new HashSet<EgoNetwork>();
        }

        /*************************** Primary Methods *******************************/
        public void addEgoNetwork(EgoNetwork egoNetwork)
        {
            this.egoNetworkList.Add(egoNetwork);
        }

        // Set Operation: Union
        public void unionWith(DataSet otherDataSet)
        {
            this.egoNetworkList.UnionWith(otherDataSet.egoNetworkList);
        }

        /*************************** Other Methods *******************************/
        public void display()
        {
            foreach (EgoNetwork egoNetwork in egoNetworkList)
            {
                Console.WriteLine(egoNetwork.egoID + "\t" + egoNetwork.optimalLabel + "\t" + egoNetwork.rwrResults[15] + "\t" + egoNetwork.attributes[4]);
            }
        }
    }
}
