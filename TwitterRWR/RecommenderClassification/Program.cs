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
            string[] columnNames = { "likeMention", "likeFriendPublish", "averageSimilarity", "friendRatio", "averageCofollow" };

            // Commandline arguments
            string rwrResultFilePath = args[0];
            string egoNetworkAnalysisFilePath = args[1];
            int nFold = int.Parse(args[2]);
            int classLabelCount = int.Parse(args[3]);

            // Initialize Classification Model
            DataPreprocess dataPreprocess = new DataPreprocess(nFold);
            dataPreprocess.dataSetConfiguration(rwrResultFilePath, egoNetworkAnalysisFilePath);

            // K-Fold Cross Validation
            for (int k = 0; k < 1; k++)
            {
                var dataSets = dataPreprocess.getTrainTestSet(k);
                DataSet trainSet, testSet;
                trainSet = (DataSet)dataSets.Item1;
                testSet = (DataSet)dataSets.Item2;

                Classification classification = new Classification(columnNames, classLabelCount);
                classification.learnDecisionTreeModel(trainSet);
            }
        }
    }
}
