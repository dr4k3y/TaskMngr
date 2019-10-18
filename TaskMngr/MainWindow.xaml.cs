using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;

namespace TaskMngr
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string DataSource = @"localhost\SQLEXPRESS";
        string Login = "Adam_DB";
        string Password = "Password_1";
        string Database = "TasksDb";
        List<Task> Tasks = new List<Task>();
        public MainWindow()
        {
            InitializeComponent();
            datePicker.SelectedDate = DateTime.Now;
            SQL_connect();
            Show_Tasks(Tasks);
            comboBox.Items.Insert(0, "Wysoki");
            comboBox.Items.Insert(1, "Normalny");
            comboBox.Items.Insert(2, "Niski");
            comboBox.SelectedIndex = 1;
            comboBox1.Items.Insert(0, "Nowy");
            comboBox1.Items.Insert(1, "W realizacji");
            comboBox1.Items.Insert(2, "Zakończony");
            comboBox1.SelectedIndex = 0;
        }

        private void SQL_connect()
        {
            Tasks = new List<Task>();
            try
            {

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = DataSource;
                builder.UserID = Login;
                builder.Password = Password;
                builder.InitialCatalog = Database;


                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "select * from Tasks";
                    SqlDataReader read = command.ExecuteReader();
                    while (read.Read())
                    {
                        Tasks.Add(new Task(read));
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void Show_Tasks(List<Task> tasks)
        {
            StackPanelV.Children.Clear();
            foreach (Task task in Tasks)
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                stackPanel.Width = StackPanelV.Width;
                TextBox nameTextBox = new TextBox();
                nameTextBox.Width = stackPanel.Width / 5;
                nameTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                nameTextBox.Text = task.TaskName;
                ComboBox priorityComboBox = new ComboBox();
                ComboBox statusComboBox = new ComboBox();
                priorityComboBox.Width = stackPanel.Width / 5;
                statusComboBox.Width = stackPanel.Width / 5;
                priorityComboBox.Items.Insert(0, "Wysoki");
                priorityComboBox.Items.Insert(1, "Normalny");
                priorityComboBox.Items.Insert(2, "Niski");
                priorityComboBox.SelectedIndex = task.PriorityValue();
                statusComboBox.Items.Insert(0, "Nowy");
                statusComboBox.Items.Insert(1, "W realizacji");
                statusComboBox.Items.Insert(2, "Zakończony");
                statusComboBox.SelectedIndex = task.StatusValue();
                DatePicker datePickerBox = new DatePicker();
                datePickerBox.Width = stackPanel.Width / 5;
                datePickerBox.SelectedDate = task.Date;
                Button deleteButton = new Button();
                deleteButton.Width = stackPanel.Width / 12;
                deleteButton.Content = "Usuń";
                stackPanel.Children.Add(deleteButton);
                stackPanel.Children.Add(nameTextBox);
                stackPanel.Children.Add(priorityComboBox);
                stackPanel.Children.Add(statusComboBox);
                stackPanel.Children.Add(datePickerBox);
                StackPanelV.Children.Add(stackPanel);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = DataSource;
                builder.UserID = Login;
                builder.Password = Password;
                builder.InitialCatalog = Database;


                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "insert into Tasks values (@TaskName, @Priority, @Date, @Status)";
                    command.Parameters.Add(new SqlParameter("@TaskName", textBox2.Text));
                    command.Parameters.Add(new SqlParameter("@Priority", comboBox.SelectedValue.ToString()));
                    command.Parameters.Add(new SqlParameter("@Date", datePicker.SelectedDate.Value.ToString("yyyy-MM-dd")));
                    command.Parameters.Add(new SqlParameter("@Status", comboBox1.SelectedValue.ToString()));
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
            SQL_connect();
            Show_Tasks(Tasks);
        }
    }
}
