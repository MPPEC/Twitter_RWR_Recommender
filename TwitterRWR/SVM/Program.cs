using Accord.Statistics.Kernels;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportVectorMachine
{
    class Program
    {
        // Features consisting of input vectors for 'SVM' learning
        public static string[] candidateColumns = { "AVERAGE_SIMILARITY", "FRIEND_RATIO", "AVERAGE_CO_FOLLOWEE", "CO_FOLLOWEE_COUNT",
                                                    "LIKE_FRIEND_PUBLISH_RATIO", "AVERAGE_MENTION_COUNT", "LIKE_MENTION_RATIO", "PUBLISH_MENTION_RATIO",
                                                    "LIKE_TO_PUBLISH_RATIO", "LIKE_RETWEET_QUOTE_RATIO", "LIKE_FAVORITE_RATIO", "CO_LIKE_WITH_FRIEND_RATIO",
                                                    "FOLLOWEE_COUNT"};

        // Commandline Argument: C:\Users\dilet\Desktop\TwitterDB C:\Users\dilet\Desktop\TwitterDB\EgoNetwork_Analysis.txt 8192 5 16
        [MTAThread]
        static void Main(string[] args)
        {
            string dirPath = args[0];
            string[] rwrFileCollection = Directory.GetFiles(dirPath, "RWR_EGO_RESULT*.txt");
            foreach (string rwrFilePath in rwrFileCollection)
            {
                // Experiment Environment Setting
                string egoNetworkAnalysisFilePath = args[1];
                int combinationCount = int.Parse(args[2]);
                int padding = (int)Math.Ceiling(Math.Log(combinationCount, 2.0));
                int nFold = int.Parse(args[3]);
                int classCount = int.Parse(args[4]);
                string classificationFilePath = dirPath + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(rwrFilePath) + "_CLASSIFICATION.txt";
                Console.WriteLine(classificationFilePath);
                if (File.Exists(classificationFilePath))
                    File.Delete(classificationFilePath);

                using (StreamWriter classificationLogger = new StreamWriter(classificationFilePath))
                {
                    // Experiment Argument Setting
                    for (int combination = 0; combination < combinationCount; combination++)
                    {
                        string s = Convert.ToString(combination, 2);
                        int[] combinationBitArray = s.PadLeft(padding, '0') // Add 0's from left
                                                .Select(c => int.Parse(c.ToString())) // convert each char to int
                                                .ToArray(); // Convert IEnumerable from select to Array

                        SortedList columnList = new SortedList();
                        for (int i = 0; i < combinationBitArray.Length; i++)
                        {
                            if (combinationBitArray[i] == 1)
                            {
                                columnList.Add(i, candidateColumns[i]);
                            }
                        }

                        // DataPreprocess: Split K-Fold DataSets
                        DataPreprocess dataPreprocess = new DataPreprocess(nFold);
                        dataPreprocess.dataSetConfiguration(columnList, rwrFilePath, egoNetworkAnalysisFilePath);

                        // K-Fold Cross Validation
                        double correctPredictRatio = 0.0, sumOfCorrectPredictRatio = 0.0;
                        double learningError = 0.0, sumOfLearningError = 0.0;
                        double sumOfMAP = 0.0;
                        for (int k = 0; k < nFold; k++)
                        {
                            // Train & Test DataSet
                            var dataSets = dataPreprocess.getTrainTestSet(k);
                            DataSet trainSet, testSet;
                            trainSet = (DataSet)dataSets.Item1;
                            testSet = (DataSet)dataSets.Item2;

                            // SVM(Support Vector Machine) Configuration: Gaussian Kernel
                            SVM classifier = new SVM(columnList, new Polynomial((int)Math.Ceiling(classCount * 1.5)), classCount);
                            // Learning & Prediction
                            learningError = classifier.learning(trainSet);                           
                            sumOfLearningError += learningError;
                            classifier.prediction(testSet);

                            // Correct Recommender Prdicted Label Ratio
                            correctPredictRatio = testSet.validation();
                            sumOfCorrectPredictRatio += correctPredictRatio;

                            // Get MAP of TrainSet
                            sumOfMAP += trainSet.MAP();
                            
                        }                       
                        double averageMAP = sumOfMAP / nFold;
                        double averageCorrectPredictRatio = sumOfCorrectPredictRatio / nFold;
                        double averageLearningError = sumOfLearningError / nFold;

                        Console.WriteLine("Combination: " + combination);
                        Console.WriteLine("MAP: " + averageMAP);
                        classificationLogger.WriteLine("{0}\t{1}\t{2:F15}\t{3:F15}\t{4:F15}",
                            combination, s.PadLeft(padding, '0'), averageMAP, averageCorrectPredictRatio, 1.0 - averageLearningError);                       
                    }
                }
            }
        }
    }
}
