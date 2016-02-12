using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderClassification
{
    class Program
    {
        // Commandline Argument: C:\Users\dilet\Desktop\TwitterDB\RWR_EGO_RESULT.txt C:\Users\dilet\Desktop\TwitterDB\EgoNetwork_Analysis.txt C:\Users\dilet\Desktop\TwitterDB\Classification_Result.txt 5 16
        static void Main(string[] args)
        {
            // Decision Attirbute List
            string[] columns = { "likeMention", "likeFriendPublish", "averageSimilarity", "friendRatio", "averageCofollow" };

            // Commandline Arguments
            string rwrResultFilePath = args[0];
            string egoNetworkAnalysisFilePath = args[1];
            string classificationResultFilePath = args[2];
            int nFold = int.Parse(args[3]);
            int classLabelCount = int.Parse(args[4]);

            // DataPreprocess: Split K-Fold DataSets
            DataPreprocess dataPreprocess = new DataPreprocess(nFold);
            dataPreprocess.dataSetConfiguration(rwrResultFilePath, egoNetworkAnalysisFilePath);

            // K-Fold Cross Validation
            double correctPredictRatio, sumOfCorrectPredictRatio = 0.0;
            double learningError, sumOfLearningError = 0.0;
            for (int k = 0; k < nFold; k++)
            {
                // Train & Test DataSet
                var dataSets = dataPreprocess.getTrainTestSet(k);
                DataSet trainSet, testSet;
                trainSet = (DataSet)dataSets.Item1;
                testSet = (DataSet)dataSets.Item2;

                // Decision Treee Configuration, Learning & Prediction
                Classification classification = new Classification(columns, classLabelCount);                
                learningError = classification.learnDecisionTreeModel(trainSet);
                sumOfLearningError += learningError;
                classification.prediction(testSet);

                // Correct Recommender Prdicted Label Ratio
                correctPredictRatio = testSet.validation();
                sumOfCorrectPredictRatio += correctPredictRatio;

                // Output Classification Result into File
                testSet.logClassificationResult(classificationResultFilePath);
            }
            double averageCorrectPredictRatio = sumOfCorrectPredictRatio / nFold;
            double averageLearningError = sumOfLearningError / nFold;

            Console.WriteLine("Average Learning Error: " + averageLearningError);
            Console.WriteLine("Correct Predict Ratio: " + averageCorrectPredictRatio);           
        }
    }
}
