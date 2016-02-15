using System;
using System.IO;
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

        public double validation()
        {
            double egoCount = (double)this.egoNetworkList.Count;
            double correctCount = 0.0;

            foreach(EgoNetwork egoNetwork in this.egoNetworkList)
            {
                if (egoNetwork.optimalLabel == egoNetwork.predictLabel)
                    correctCount += 1.0;
            }

            return correctCount / egoCount;
        }

        public double MAP()
        {
            double sumOfAP = 0.0;
            foreach (EgoNetwork egoNetwork in this.egoNetworkList)
            {
                sumOfAP += egoNetwork.rwrResults[egoNetwork.predictLabel];
            }

            return sumOfAP / this.egoNetworkList.Count;
        }
        /*************************** Other Methods *******************************/
        public void logClassificationResult(string classificationFilePath)
        {
            using (StreamWriter logger = new StreamWriter(classificationFilePath, true))
            {
                foreach(EgoNetwork egoNetwork in this.egoNetworkList)
                {
                    logger.WriteLine("{0}\t{1}\t{2:F15}\t{3}\t{4:F15}", egoNetwork.egoID,
                        egoNetwork.optimalLabel, egoNetwork.rwrResults[egoNetwork.optimalLabel],
                        egoNetwork.predictLabel, egoNetwork.rwrResults[egoNetwork.predictLabel]);
                }          
            }
        }

        public void display()
        {
            foreach (EgoNetwork egoNetwork in egoNetworkList)
            {
                Console.WriteLine(egoNetwork.egoID + "\t" + egoNetwork.optimalLabel + "\t" + egoNetwork.rwrResults[15] + "\t" + egoNetwork.attributes[4]);
            }
        }
    }
}
