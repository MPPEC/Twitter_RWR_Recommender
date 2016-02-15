using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderClassification
{
    class Program
    {
        // Decision Attirbute List
        public static string[] candidateColumns = { "AVERAGE_SIMILARITY", "FRIEND_RATIO", "AVERAGE_CO_FOLLOWEE", "CO_FOLLOWEE_COUNT",
                                                    "LIKE_FRIEND_PUBLISH_RATIO", "AVERAGE_MENTION_COUNT", "LIKE_MENTION_RATIO", "PUBLISH_MENTION_RATIO",
                                                    "LIKE_TO_PUBLISH_RATIO", "LIKE_RETWEET_QUOTE_RATIO", "LIKE_FAVORITE_RATIO", "CO_LIKE_WITH_FRIEND_RATIO",
                                                    "FOLLOWEE_COUNT"};

        // Commandline Argument: 0,3,4,5,6 5 16 C:\Users\dilet\Desktop\TwitterDB\RWR_EGO_RESULT.txt C:\Users\dilet\Desktop\TwitterDB\EgoNetwork_Analysis.txt C:\Users\dilet\Desktop\TwitterDB\EgoNetwork_Classification.txt
        static void Main(string[] args)
        {
            // Select Columns for decision attributes
            int[] columnNumberList = Array.ConvertAll<string, int>(args[0].Split(','), new Converter<string, int>(int.Parse));
            SortedList columnList = new SortedList(); // <Column Nmber, Column Name>
            for (int i = 0; i < columnNumberList.Length; i++)
            {
                int columnNumber = columnNumberList[i];
                columnList.Add(columnNumber, candidateColumns[columnNumber]);
            }
            int nFold = int.Parse(args[1]);
            int classLabelCount = int.Parse(args[2]);
            string rwrResultFilePath = args[3];
            string egoNetworkAnalysisFilePath = args[4];
            string classificationResultFilePath = args[5];
            if (File.Exists(classificationResultFilePath))
                File.Delete(classificationResultFilePath);

            // DataPreprocess: Split K-Fold DataSets
            DataPreprocess dataPreprocess = new DataPreprocess(nFold);
            dataPreprocess.dataSetConfiguration(columnList, rwrResultFilePath, egoNetworkAnalysisFilePath);

            // K-Fold Cross Validation
            double correctPredictRatio, sumOfCorrectPredictRatio = 0.0;
            double learningError, sumOfLearningError = 0.0;
            double sumOfMAP = 0.0;
            for (int k = 0; k < nFold; k++)
            {
                // Train & Test DataSet
                var dataSets = dataPreprocess.getTrainTestSet(k);
                DataSet trainSet, testSet;
                trainSet = (DataSet)dataSets.Item1;
                testSet = (DataSet)dataSets.Item2;

                // Decision Treee Configuration, Learning & Prediction
                Classification classification = new Classification(columnList, classLabelCount);
                learningError = classification.learnDecisionTreeModel(trainSet);
                sumOfLearningError += learningError;
                classification.prediction(testSet);

                // Correct Recommender Prdicted Label Ratio
                correctPredictRatio = testSet.validation();
                sumOfCorrectPredictRatio += correctPredictRatio;

                // Output Classification Result into File
                testSet.logClassificationResult(classificationResultFilePath);

                // Get MAP of TrainSet
                sumOfMAP += trainSet.MAP();
            }
            double averageCorrectPredictRatio = sumOfCorrectPredictRatio / nFold;
            double averageLearningError = sumOfLearningError / nFold;
            double averageMAP = sumOfMAP / nFold;

            Console.WriteLine("Average Learning Error: {0:F15}", averageLearningError);
            Console.WriteLine("Correct Predict Ratio: {0:F15}", averageCorrectPredictRatio);
            Console.WriteLine("Average MAP: {0:F15}", averageMAP);
        }
    }
}
