using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderClassification
{
    class Classification
    {
        /***************************** Properties **********************************/
        private double[][] trainInputArray;
        private int[] trainOutputVector;
        private DecisionVariable[] decisionAttributes;
        private DecisionTree descisionTree;

        /***************************** Constructor *********************************/
        public Classification(string[] columns, int classLabeCount)
        {
            // Initialize DecisionTree
            decisionAttributes = new DecisionVariable[columns.Length];
            for (int i = 0; i < decisionAttributes.Length; i++)
            {
                decisionAttributes[i] = new DecisionVariable(columns[i], DecisionVariableKind.Continuous);
            }

            int classCount = classLabeCount;
            descisionTree = new DecisionTree(decisionAttributes, classCount);
        }

        /*************************** Primary Methods *******************************/
        public double learnDecisionTreeModel(DataSet trainSet)
        {           
            // Convert TrainSet --> TrainDataTable
            this.convertToTrainIntputTable(trainSet);
            // C4.5 Decision Tree Algorithm
            double learningError;
            C45Learning c45 = new C45Learning(this.descisionTree);
            learningError = c45.Run(this.trainInputArray, this.trainOutputVector);

            return learningError;
        }

        public void prediction(DataSet testSet)
        {
            int newPredictLabel;
            foreach(EgoNetwork egoNetwork in testSet.egoNetworkList)
            {
                newPredictLabel = this.descisionTree.Compute(egoNetwork.attributes);
                egoNetwork.predictLabel = newPredictLabel;
            }
        }

        /*************************** Secondary Methods *******************************/
        public void convertToTrainIntputTable(DataSet trainSet)
        {
            int egoCount = trainSet.egoNetworkList.Count;
            EgoNetwork[] egoList = new EgoNetwork[egoCount];
            trainSet.egoNetworkList.CopyTo(egoList);
            this.trainInputArray = new double[egoCount][];
            this.trainOutputVector = new int[egoCount];
            
            for (int i = 0; i < this.trainInputArray.Length; i++)
            {
                this.trainInputArray[i] = egoList[i].attributes;
                this.trainOutputVector[i] = egoList[i].optimalLabel;
            }
        }
    }
}
