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
        private DataTable trainDataTable;
        private DataColumn[] dataColumns;
        private DecisionVariable[] attributeList;
        private DecisionTree tree;
        private string[] inputColumns;
        private int[] outputVector;


        /***************************** Constructor *********************************/
        public Classification(string[] newInputColumns, int classLabeCount)
        {
            this.inputColumns = newInputColumns;

            // Initialize Schema of trainDataTable
            this.trainDataTable = new DataTable("Train Table");
            for (int i = 0; i < inputColumns.Length; i++)
            {
                DataColumn inputColumn = new DataColumn();
                inputColumn.DataType = System.Type.GetType("System.Double");
                inputColumn.ColumnName = inputColumns[i];
                trainDataTable.Columns.Add(inputColumn);
            }
            
            // Initialize DecisionTree
            DecisionVariable[] attributeList = new DecisionVariable[inputColumns.Length];
            for (int i = 0; i < inputColumns.Length; i++)
            {
                attributeList[i] = new DecisionVariable(inputColumns[i], DecisionVariableKind.Continuous);
            }

            int classCount = classLabeCount;
            tree = new DecisionTree(attributeList, classCount);
        }

        /*************************** Primary Methods *******************************/
        public void learnDecisionTreeModel(DataSet trainSet)
        {
            this.setTrainDataTable(trainSet);
            C45Learning c45 = new C45Learning(this.tree);

            double[][] inputs = new double[trainDataTable.Rows.Count][];
            int i = 0;
            foreach (DataRow row in trainDataTable.Rows)
            {               
                inputs[i] = new double[trainDataTable.Columns.Count];
                int j = 0;
                foreach(DataColumn column in trainDataTable.Columns)
                {
                    inputs[i][j] = (double)row[column];
                    j++;
                }
                i++;
            }

            double error = c45.Run(inputs, outputVector);
            Console.WriteLine("error: " + error);
        }

        public void prediction(DataSet testSet)
        {

        }

        /*************************** Secondary Methods *******************************/
        public void setTrainDataTable(DataSet trainSet)
        {
            outputVector = new int[trainSet.egoNetworkList.Count];
            int i = 0;
            foreach (EgoNetwork egoNetwork in trainSet.egoNetworkList)
            {
                DataRow row = trainDataTable.NewRow();
                for (int j = 0; j < inputColumns.Length; j++)
                {
                    row[inputColumns[j]] = egoNetwork.attributes[j];
                }
                trainDataTable.Rows.Add(row);
                outputVector[i++] = egoNetwork.optimalLabel;
            }
        }
    }
}
