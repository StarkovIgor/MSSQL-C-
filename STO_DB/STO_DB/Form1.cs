using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace STO_DB
{
    public partial class Form1 : Form
    {
        //объявляем экземпляры для подключения и работой с БД
        private SqlConnection sqlConnection;
        private SqlCommandBuilder commandBuilder;
        private SqlDataAdapter sqlDataAdapter;
        private DataSet dataSet;
        bool newRowAdding = false;//состояние добавления строки


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["STO_DB"].ConnectionString);
            sqlConnection.Open();
            LoadData();
        }

        private void LoadData()//метод для первоначальной загрузки данных
        {
            try
            {
                sqlDataAdapter = new SqlDataAdapter("SELECT *, N'Удалить' AS [Действие]  FROM Owner", sqlConnection);
                SqlDataAdapter sqlDataAdapterCar = new SqlDataAdapter( "SELECT Model, VIN, Year FROM Car", sqlConnection);
                SqlDataAdapter sqlDataAdapterWorks = new SqlDataAdapter("SELECT Work, Price FROM Works", sqlConnection);
                SqlDataAdapter sqlDataAdapterOrder = new SqlDataAdapter("SELECT Owner.Name, Car.Model FROM Owner, Car WHERE Owner.Id = Car.OwnerId", sqlConnection);
                commandBuilder = new SqlCommandBuilder(sqlDataAdapter);

                commandBuilder.GetDeleteCommand();
                commandBuilder.GetInsertCommand();
                commandBuilder.GetUpdateCommand();

                dataSet = new DataSet();

                sqlDataAdapter.Fill(dataSet, "Owner");
                sqlDataAdapterCar.Fill(dataSet, "Car");
                sqlDataAdapterWorks.Fill(dataSet, "Works");
                sqlDataAdapterOrder.Fill(dataSet, "Order");
                dataGridViewOwner.DataSource = dataSet.Tables["Owner"];
                for (int i = 0; i < dataGridViewOwner.Rows.Count; i++)
                {
                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                    dataGridViewOwner[6, i] = linkCell;
                }
                dataGridViewCar.DataSource = dataSet.Tables["Car"];
                dataGridViewWorks.DataSource = dataSet.Tables["Works"];
                dataGridViewOrder.DataSource = dataSet.Tables["Order"];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,"Ошибка",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReloadData()//метод для обновления данных
        {
            try
            {

                dataSet.Tables["Owner"].Clear();
                sqlDataAdapter.Fill(dataSet, "Owner");
                dataGridViewOwner.DataSource = dataSet.Tables["Owner"];
                for (int i = 0; i < dataGridViewOwner.Rows.Count; i++)
                {
                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                    dataGridViewOwner[6, i] = linkCell;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void обновитьToolStripMenuItem_Click(object sender, EventArgs e)//метод подписанный на событие кнопкки обновить
        {
            ReloadData();
        }

        private void dataGridViewOwner_CellContentClick(object sender, DataGridViewCellEventArgs e)//метод подписанный на события клика по ячейке
        {
            try
            {
                if (e.ColumnIndex == 6)
                {
                    string task = dataGridViewOwner.Rows[e.RowIndex].Cells[6].Value.ToString();
                    if (task == "Удалить")
                    {
                        if (MessageBox.Show("Удалить владельца из БД?", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            int rowIndex = e.RowIndex;
                            dataGridViewOwner.Rows.RemoveAt(rowIndex);
                            dataSet.Tables["Owner"].Rows[rowIndex].Delete();
                            sqlDataAdapter.Update(dataSet, "Owner");
                        }
                    }
                    else if (task == "Добавить")
                    {
                        int rowIndex = e.RowIndex;
                        DataRow row = dataSet.Tables["Owner"].NewRow();
                        row["Name"] = dataGridViewOwner.Rows[rowIndex].Cells["Name"].Value;
                        row["Surname"] = dataGridViewOwner.Rows[rowIndex].Cells["Surname"].Value;
                        row["Phone"] = dataGridViewOwner.Rows[rowIndex].Cells["Phone"].Value;
                        row["Email"] = dataGridViewOwner.Rows[rowIndex].Cells["Email"].Value;
                        row["Birthday"] = dataGridViewOwner.Rows[rowIndex].Cells["Birthday"].Value;

                        dataSet.Tables["Owner"].Rows.Add(row);
                        dataSet.Tables["Owner"].Rows.RemoveAt(dataSet.Tables["Owner"].Rows.Count - 1);
                        dataGridViewOwner.Rows.RemoveAt(rowIndex);
                        dataGridViewOwner.Rows[rowIndex].Cells[6].Value = "Удалить";

                        sqlDataAdapter.Update(dataSet, "Owner");
                        newRowAdding = false;
                    }
                    else if (task == "Обновить")
                    {
                        int rowIndex = e.RowIndex;
                        dataSet.Tables["Owner"].Rows[rowIndex]["Name"] = dataGridViewOwner.Rows[rowIndex].Cells["Name"].Value;
                        dataSet.Tables["Owner"].Rows[rowIndex]["Surname"] = dataGridViewOwner.Rows[rowIndex].Cells["Surname"].Value;
                        dataSet.Tables["Owner"].Rows[rowIndex]["Phone"] = dataGridViewOwner.Rows[rowIndex].Cells["Phone"].Value;
                        dataSet.Tables["Owner"].Rows[rowIndex]["Email"] = dataGridViewOwner.Rows[rowIndex].Cells["Email"].Value;
                        dataSet.Tables["Owner"].Rows[rowIndex]["Birthday"] = dataGridViewOwner.Rows[rowIndex].Cells["Birthday"].Value;

                        sqlDataAdapter.Update(dataSet, "Owner");
                        dataGridViewOwner.Rows[rowIndex].Cells[6].Value = "Удалить";
                    }
                    ReloadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridViewOwner_UserAddedRow(object sender, DataGridViewRowEventArgs e)//метод обрабатывающий событие добавление строки
        {
            try
            {
                if (newRowAdding == false)
                {
                    newRowAdding = true;//как только внесли данные в ячейку перключаем состояние 
                    int lastRow = dataGridViewOwner.Rows.Count - 2;//находим индекс последней строки
                    DataGridViewRow row = dataGridViewOwner.Rows[lastRow];
                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                    dataGridViewOwner[6, lastRow] = linkCell;
                    row.Cells["Действие"].Value = "Добавить";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridViewOwner_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (newRowAdding == false)
                {
                    int rowIndex = dataGridViewOwner.SelectedCells[0].RowIndex;

                    DataGridViewRow editingRow = dataGridViewOwner.Rows[rowIndex];
                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                    dataGridViewOwner[6, rowIndex] = linkCell;
                    editingRow.Cells["Действие"].Value = "Обновить";

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void добавитьНарядЗаказToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OrderForm form2 = new OrderForm();
            form2.Show();
        }
    }
}
