using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderClassification
{
    class Program
    {
        static void Main(string[] args)
        {
            // Attirbute Column List
            string[] column = { "likeMention", "likeFriendPublish", "averageSimilarity", "friendRatio", "averageCofollow" };

            // Commandline arguments
            string rwrResultFilePath = args[0];
            string egoNetworkAnalysisFilePath = args[1];
            int nFold = int.Parse(args[2]);

            // Initialize Classification Model
            DataPreprocess classification = new DataPreprocess(nFold);
            classification.dataSetConfiguration(rwrResultFilePath, egoNetworkAnalysisFilePath);

            // K-Fold Cross Validation
            for (int k = 0; k < 1; k++)
            {
                var dataSets = classification.getTrainTestSet(k);
                DataSet trainSet, testSet;
                trainSet = (DataSet)dataSets.Item1;
                testSet = (DataSet)dataSets.Item2;

            }
        }
    }
}
