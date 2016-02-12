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
        private DecisionVariable[] decisionTreeAttributes;
        private DecisionTree tree;

        /***************************** Constructor *********************************/
        public Classification(string[] columns, int classLabeCount)
        {
            // Initialize DecisionTree
            decisionTreeAttributes = new DecisionVariable[columns.Length];
            for (int i = 0; i < decisionTreeAttributes.Length; i++)
            {
                decisionTreeAttributes[i] = new DecisionVariable(columns[i], DecisionVariableKind.Continuous);
            }

            int classCount = classLabeCount;
            tree = new DecisionTree(decisionTreeAttributes, classCount);
        }

        /*************************** Primary Methods *******************************/
        public void learnDecisionTreeModel(DataSet trainSet)
        {
            this.setTrainDataTable(trainSet);
            C45Learning c45 = new C45Learning(this.tree);

            double error = c45.Run(this.trainInputArray, this.trainOutputVector);
            Console.WriteLine("error: " + error);
        }

        public void prediction(DataSet testSet)
        {
            int newPredictLabel;
            foreach(EgoNetwork egoNetwork in testSet.egoNetworkList)
            {
                newPredictLabel = this.tree.Compute(egoNetwork.attributes);
                egoNetwork.predictLabel = newPredictLabel;
            }
        }

        /*************************** Secondary Methods *******************************/
        public void setTrainDataTable(DataSet trainSet)
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
