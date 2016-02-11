using Accord.MachineLearning.DecisionTrees;
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
        private string[] columnNames;


        /***************************** Constructor *********************************/
        public Classification(string[] newColumnNames, int classLabeCount)
        {
            this.columnNames = newColumnNames;
            // Initialize Schema of trainDataTable
            this.trainDataTable = new DataTable("Train Table");
            this.dataColumns = new DataColumn[columnNames.Length];
            for (int i = 0; i < dataColumns.Length; i++)
            {
                DataColumn column = new DataColumn();
                column.DataType = System.Type.GetType("System.Double");
                column.ColumnName = columnNames[i];
                trainDataTable.Columns.Add(column);
            }
            
            // Initialize DecisionTree
            DecisionVariable[] attributeList = new DecisionVariable[columnNames.Length];
            for (int i = 0; i < columnNames.Length; i++)
            {
                attributeList[i] = new DecisionVariable(columnNames[i], DecisionVariableKind.Continuous);
            }

            int classCount = classLabeCount;
            tree = new DecisionTree(attributeList, classCount);
        }

        /*************************** Primary Methods *******************************/
        public void learnDecisionTreeModel(DataSet trainSet)
        {
            this.setTrainDataTable(trainSet);
        }

        /*************************** Secondary Methods *******************************/
        public void setTrainDataTable(DataSet trainSet)
        {
            foreach (EgoNetwork egoNetwork in trainSet.egoNetworkList)
            {
                DataRow row = trainDataTable.NewRow();
                for (int i = 0; i < columnNames.Length; i++)
                {
                    row[columnNames[i]] = egoNetwork.attributes[i];
                }
                trainDataTable.Rows.Add(row);
            }
        }
    }
}
