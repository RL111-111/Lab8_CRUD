using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Lab8_CRUD
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Server=PC238-2;Database=StudentDB;Integrated Security=True;";
        private int selectedStudentId = -1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadStudents();
        }



        private void LoadStudents()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT Id, FullName, Age, GroupName, AverageGrade FROM Students";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridViewStudents.DataSource = dt;


                    UpdateStatus($"Загружено записей. Всего студентов: {dt.Rows.Count}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void UpdateStatus(string message)
        {
            int count = dataGridViewStudents.Rows.Count;
            labelStatus.Text = $"{message} | Всего студентов в базе: {count}";
        }

        private void dataGridViewStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridViewStudents.SelectedRows[0];
                if (row.Cells["Id"].Value != null && row.Cells["Id"].Value != DBNull.Value)
                {
                    selectedStudentId = Convert.ToInt32(row.Cells["Id"].Value);
                    textBoxName.Text = row.Cells["FullName"].Value.ToString();
                    numericAge.Value = Convert.ToDecimal(row.Cells["Age"].Value);
                    textBoxGroup.Text = row.Cells["GroupName"].Value.ToString();
                    numericGrade.Value = Convert.ToDecimal(row.Cells["AverageGrade"].Value);
                }
            }
        }

 
        private void buttonAdd_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(textBoxName.Text) || string.IsNullOrWhiteSpace(textBoxGroup.Text))
            {
                MessageBox.Show("Пожалуйста, заполните ФИО и Группу!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();


                    string checkSql = "SELECT COUNT(*) FROM Students WHERE FullName = @name";
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@name", textBoxName.Text.Trim());
                        int existingCount = (int)checkCmd.ExecuteScalar();

                        if (existingCount > 0)
                        {
                            MessageBox.Show("Студент с таким ФИО уже существует в базе данных!", "Дубликат записи", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string insertSql = "INSERT INTO Students (FullName, Age, GroupName, AverageGrade) VALUES (@name, @age, @group, @grade)";
                    using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", textBoxName.Text.Trim());
                        cmd.Parameters.AddWithValue("@age", (int)numericAge.Value);
                        cmd.Parameters.AddWithValue("@group", textBoxGroup.Text.Trim());
                        cmd.Parameters.AddWithValue("@grade", numericGrade.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                ClearFields();
                LoadStudents();
                UpdateStatus("Студент успешно добавлен");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении: " + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (selectedStudentId == -1)
            {
                MessageBox.Show("Выберите студента из таблицы для редактирования!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxName.Text) || string.IsNullOrWhiteSpace(textBoxGroup.Text))
            {
                MessageBox.Show("Поля ФИО и Группа не могут быть пустыми!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE Students SET FullName = @name, Age = @age, GroupName = @group, AverageGrade = @grade WHERE Id = @id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", textBoxName.Text.Trim());
                        cmd.Parameters.AddWithValue("@age", (int)numericAge.Value);
                        cmd.Parameters.AddWithValue("@group", textBoxGroup.Text.Trim());
                        cmd.Parameters.AddWithValue("@grade", numericGrade.Value);
                        cmd.Parameters.AddWithValue("@id", selectedStudentId);

                        cmd.ExecuteNonQuery();
                    }
                }

                LoadStudents();
                UpdateStatus("Данные студента обновлены");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении: " + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (selectedStudentId == -1)
            {
                MessageBox.Show("Выберите студента из таблицы для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Вы действительно хотите удалить студента «{textBoxName.Text}»?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);


            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM Students WHERE Id = @id";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", selectedStudentId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    ClearFields();
                    LoadStudents();
                    UpdateStatus("Запись удалена");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void buttonClear_Click(object sender, EventArgs e)
        {
            ClearFields();
            UpdateStatus("Поля очищены");
        }

        private void ClearFields()
        {
            selectedStudentId = -1;
            textBoxName.Clear();
            numericAge.Value = numericAge.Minimum;
            textBoxGroup.Clear();
            numericGrade.Value = numericGrade.Minimum;
            dataGridViewStudents.ClearSelection();
        }

        private void buttonExportCsv_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта!", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV файлы (*.csv)|*.csv";
                sfd.FileName = "Students_Export.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();

                        
                        int colCount = dataGridViewStudents.Columns.Count;
                        for (int i = 0; i < colCount; i++)
                        {
                            sb.Append(dataGridViewStudents.Columns[i].HeaderText);
                            if (i < colCount - 1)
                            {
                                sb.Append(";");
                            }
                        }
                        sb.AppendLine();

                  
                        foreach (DataGridViewRow row in dataGridViewStudents.Rows)
                        {
                            for (int i = 0; i < colCount; i++)
                            {
                                object cellValue = row.Cells[i].Value;
                                sb.Append(cellValue != null ? cellValue.ToString() : "");
                                if (i < colCount - 1)
                                {
                                    sb.Append(";");
                                }
                            }
                            sb.AppendLine();
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        UpdateStatus("Данные экспортированы в CSV");

                        MessageBox.Show("Файл успешно сохранён!", "Экспорт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при экспорте: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

   
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadStudents();
            UpdateStatus("Изменения отменены, таблица обновлена.");
        }
    }
}
