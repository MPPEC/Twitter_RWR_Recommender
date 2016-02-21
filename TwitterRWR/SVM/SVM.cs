using Accord.Statistics.Kernels;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportVectorMachine
{
    class SVM
    {
        /***************************** Properties **********************************/
        private double[][] trainInputArray;
        private int[] trainOutputVector;
        private MulticlassSupportVectorMachine machine;
        private MulticlassSupportVectorLearning teacher;
        private double[] mean;
        private double[] leftMost;
        private double[] rightMost;

        /***************************** Constructor *********************************/
        public SVM(SortedList columnList, IKernel kernel, int classCount)
        {
            // Create a new Multi-class Support Vector Machine with input vectors
            //  using the kernel and # of disjoint classes
            machine = new MulticlassSupportVectorMachine(columnList.Count, kernel, classCount);
        }

        /*************************** Primary Methods *******************************/

        public double learning(DataSet trainSet)
        {
            // Train Data Conversion
            var ingredient = convertToTrainIntputTable(trainSet);
            trainInputArray = (double[][])ingredient.Item1;
            trainOutputVector = (int[])ingredient.Item2;

            // Create the Multi-class learning algorithm for the SVM machine
            teacher = new MulticlassSupportVectorLearning(machine, trainInputArray, trainOutputVector);

            // Configure the learning algorithm to use SMO(Sequential Minimal Optimization)
            //  to train the underlying SVMs in each of the binary class subproblems
            teacher.Algorithm = (svm, classInputs, classOutputs, i, j) =>
                new SequentialMinimalOptimization(svm, classInputs, classOutputs);

            // Run the learning algorithm and return learning error            
            return teacher.Run();
        }

        public void prediction(DataSet testSet)
        {
            int predictedClass;
            foreach (EgoNetwork egoNetwork in testSet.egoNetworkList)
            {
                double[] featureVector = new double[egoNetwork.attributes.Length];
                egoNetwork.attributes.CopyTo(featureVector, 0);
                for (var i = 0; i < featureVector.Length; i++)
                {
                    double previous = featureVector[i];
                    if (previous <= this.mean[i])
                        featureVector[i] = (previous - this.mean[i]) / (this.mean[i] - this.leftMost[i]);
                    else
                        featureVector[i] = (previous - this.mean[i]) / (this.rightMost[i] - this.mean[i]);
                }
                predictedClass = machine.Compute(featureVector);
                egoNetwork.predictLabel = predictedClass;
            }
        }

        /*************************** Secondary Methods *******************************/
        private Tuple<Double[][], int[]> convertToTrainIntputTable(DataSet trainSet)
        {
            // Raw Data
            int egoCount = trainSet.egoNetworkList.Count;
            EgoNetwork[] egoList = new EgoNetwork[egoCount];
            trainSet.egoNetworkList.CopyTo(egoList);
            double[][] inputArray = new double[egoCount][];
            int[] outputVector = new int[egoCount];

            for (int i = 0; i < inputArray.Length; i++)
            {
                inputArray[i] = egoList[i].attributes;
                outputVector[i] = egoList[i].optimalLabel;
            }

            // Normalization: Data Preprocess
            int height = inputArray.Length, width = inputArray[0].Length;
            mean = new double[width];
            leftMost = new double[width];
            rightMost = new double[width];

            for (var j = 0; j < width; j++)
            {
                double average = 0.0;
                double min = double.PositiveInfinity;
                double max = double.NegativeInfinity;

                for (var i = 0; i < height; i++)
                {
                    // Left Most Value
                    if (inputArray[i][j] < min)
                        min = inputArray[i][j];
                    // Right Most Value
                    if (inputArray[i][j] > max)
                        max = inputArray[i][j];
                    // Average
                    average += inputArray[i][j];
                }
                this.leftMost[j] = min;
                this.rightMost[j] = max;
                this.mean[j] = average / height;
            }

            // Normalization: Scaling --> [-1, +1]
            for (var j = 0; j < width; j++)
            {
                for (var i = 0; i < height; i++)
                {
                    double previous = inputArray[i][j];
                    if (previous <= this.mean[j])
                        inputArray[i][j] = (previous - this.mean[j]) / (this.mean[j] - this.leftMost[j]);
                    else
                        inputArray[i][j] = (previous - this.mean[j]) / (this.rightMost[j] - this.mean[j]);
                }
            }

            return new Tuple<Double[][], int[]>(inputArray, outputVector);
        }
    }
}
