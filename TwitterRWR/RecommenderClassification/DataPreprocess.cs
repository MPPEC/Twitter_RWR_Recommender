using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderClassification
{
    class DataPreprocess
    {
        /***************************** Properties **********************************/
        private DataSet[] dataSets;
        private int nFold;

        /***************************** Constructor *********************************/
        public DataPreprocess(int numOfFold)
        {
            this.nFold = numOfFold;
            this.dataSets = new DataSet[this.nFold];
        }

        /*************************** Primary Methods *******************************/
        public void dataSetConfiguration(string rwrFilePath, string networkFilePath)
        {
            ArrayList egoIDList = new ArrayList();
            SortedDictionary<long, int> egoOptimalLabelDictionary = new SortedDictionary<long, int>();
            SortedDictionary<long, double[]> egoRwrResultsDictionary = new SortedDictionary<long, double[]>();
            SortedDictionary<long, double[]> egoAttributesDictionary = new SortedDictionary<long, double[]>();

            using (StreamReader rwrReader = new StreamReader(rwrFilePath))
            {
                string line = null;
                while ((line = rwrReader.ReadLine()) != null)
                {
                    long egoID;
                    int egoOptimalLabel;
                    string[] rwrResult = line.Split('\t');
                    double[] MAP = new double[rwrResult.Length - 2];

                    egoID = long.Parse(rwrResult[0]);
                    egoOptimalLabel = int.Parse(rwrResult[1]);
                    for (int i = 2; i < rwrResult.Length; i++)
                        MAP[i - 2] = double.Parse(rwrResult[i]);

                    egoOptimalLabelDictionary.Add(egoID, egoOptimalLabel);
                    egoRwrResultsDictionary.Add(egoID, MAP);
                    egoIDList.Add(egoID);
                }
            }

            using (StreamReader networkReader = new StreamReader(networkFilePath))
            {
                string line = null;
                while ((line = networkReader.ReadLine()) != null)
                {
                    long egoID;
                    string[] networkResults = line.Split('\t');
                    double[] attributes = new double[networkResults.Length - 1];

                    egoID = long.Parse(networkResults[0]);
                    for (int i = 1; i < networkResults.Length; i++)
                        attributes[i - 1] = double.Parse(networkResults[i]);

                    egoAttributesDictionary.Add(egoID, attributes);
                }
            }

            // K-Fold Split DataSets
            SortedList<long, EgoNetwork> egoNetworkList = new SortedList<long, EgoNetwork>();
            egoIDList.Sort(); // Ascending Order
            foreach (long egoID in egoIDList)
            {
                egoNetworkList.Add(egoID, new EgoNetwork(egoID, egoOptimalLabelDictionary[egoID],
                    egoRwrResultsDictionary[egoID], egoAttributesDictionary[egoID]));
            }

            int boundary = (int)egoNetworkList.Count / nFold;
            for (int i = 0; i < nFold; i++)
            {
                this.dataSets[i] = new DataSet();
                for (int j = i * boundary; j < (i + 1) * boundary; j++) // Each sub-dataset boundary
                {
                    EgoNetwork egoNetWork = egoNetworkList[(long)egoIDList[j]];
                    dataSets[i].addEgoNetwork(egoNetWork);
                }
            }
        }

        /************************** Secondary Methods ******************************/
        public Tuple<DataSet, DataSet> getTrainTestSet(int index)
        {
            DataSet trainSet = new DataSet();
            DataSet testSet = dataSets[index]; // TestSet Setting

            for (int i = 0; i < this.dataSets.Length; i++)
            {
                if (i != index)
                    trainSet.unionWith(dataSets[i]);
            }

            return new Tuple<DataSet, DataSet>((DataSet)trainSet, (DataSet)testSet);
        }
    }
}
