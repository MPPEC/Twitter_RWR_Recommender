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
            string[] intPutColumns = { "likeMention", "likeFriendPublish", "averageSimilarity", "friendRatio", "averageCofollow" };

            // Commandline arguments
            string rwrResultFilePath = args[0];
            string egoNetworkAnalysisFilePath = args[1];
            string classificationResultFilePath = args[2];
            int nFold = int.Parse(args[3]);
            int classLabelCount = int.Parse(args[4]);

            // Initialize Classification Model
            DataPreprocess dataPreprocess = new DataPreprocess(nFold);
            dataPreprocess.dataSetConfiguration(rwrResultFilePath, egoNetworkAnalysisFilePath);

            // K-Fold Cross Validation
            double sumOfCorrectPredictRatio = 0.0;
            for (int k = 0; k < nFold; k++)
            {
                var dataSets = dataPreprocess.getTrainTestSet(k);
                DataSet trainSet, testSet;
                trainSet = (DataSet)dataSets.Item1;
                testSet = (DataSet)dataSets.Item2;

                Classification classification = new Classification(intPutColumns, classLabelCount);
                classification.learnDecisionTreeModel(trainSet);
                classification.prediction(testSet);

                double correctPredictRatio = 0.0;
                correctPredictRatio = testSet.validation();
                sumOfCorrectPredictRatio += correctPredictRatio;

                // Log Classification Result
                testSet.logClassificationResult(classificationResultFilePath);
            }
            double averageCorrectPredictRatio = sumOfCorrectPredictRatio / nFold;

            Console.WriteLine("Error Predict Raio: " + (1 - averageCorrectPredictRatio));
            
        }
    }
}
