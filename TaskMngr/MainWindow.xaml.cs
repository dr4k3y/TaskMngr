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
            comboBox.Items.Insert(0, "Wysoki");
            comboBox.Items.Insert(1, "Normalny");
            comboBox.Items.Insert(2, "Niski");
            comboBox.SelectedIndex = 1;
            comboBox1.Items.Insert(0, "Nowy");
            comboBox1.Items.Insert(1, "W realizacji");
            comboBox1.Items.Insert(2, "Zakończony");
            comboBox1.SelectedIndex = 0;
            comboBox2.Items.Insert(0, "Od najnowszego");
            comboBox2.Items.Insert(1, "Od najstarszego");
            comboBox2.Items.Insert(2, "Wg terminu rosnąco");
            comboBox2.Items.Insert(3, "Wg terminu malejąco");
            comboBox2.SelectedIndex = 0;
            checkBox.Unchecked += new RoutedEventHandler(CheckBox_Unchecked);
            checkBox1.Unchecked += new RoutedEventHandler(CheckBox_Unchecked);
            checkBox2.Unchecked += new RoutedEventHandler(CheckBox_Unchecked);
            checkBox3.Unchecked += new RoutedEventHandler(CheckBox_Unchecked);
            checkBox4.Unchecked += new RoutedEventHandler(CheckBox_Unchecked);
            checkBox5.Unchecked += new RoutedEventHandler(CheckBox_Unchecked);
            checkBox3.Checked += new RoutedEventHandler(CheckBox_Checked);
            checkBox4.Checked += new RoutedEventHandler(CheckBox_Checked);
            checkBox5.Checked += new RoutedEventHandler(CheckBox_Checked);
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }

        private void SQL_connect(int order)
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
                    command.CommandText = "select * from Tasks where (Priority in (@Wysoki, @Normalny, @Niski) and Status in (@Nowy, @W_realizacji, @Zakończony)) order by case when @OrderMode = 0 then [Id] end desc, case when @OrderMode = 1 then [Id] end, case when @OrderMode = 2 then [Date] end, case when @OrderMode = 3 then [Date] end desc";
                    command.Parameters.Add(new SqlParameter("@OrderMode", order));
                    if ((bool)checkBox.IsChecked|(bool)checkBox1.IsChecked|(bool)checkBox2.IsChecked)
                    {
                        if (checkBox.IsChecked==true)
                        {
                            command.Parameters.Add(new SqlParameter("@Wysoki", "Wysoki"));
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter("@Wysoki", "NULL"));
                        }
                        if (checkBox1.IsChecked == true)
                        {
                            command.Parameters.Add(new SqlParameter("@Normalny", "Normalny"));
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter("@Normalny", "NULL"));
                        }
                        if (checkBox2.IsChecked == true)
                        {
                            command.Parameters.Add(new SqlParameter("@Niski", "Niski"));
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter("@Niski", "NULL"));
                        }
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@Wysoki", "Wysoki"));
                        command.Parameters.Add(new SqlParameter("@Normalny", "Normalny"));
                        command.Parameters.Add(new SqlParameter("@Niski", "Niski"));
                    }
                    if ((bool)checkBox3.IsChecked | (bool)checkBox4.IsChecked | (bool)checkBox5.IsChecked)
                    {
                        if (checkBox3.IsChecked == true)
                        {
                            command.Parameters.Add(new SqlParameter("@Nowy", "Nowy"));
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter("@Nowy", "NULL"));
                        }
                        if (checkBox4.IsChecked == true)
                        {
                            command.Parameters.Add(new SqlParameter("@W_realizacji", "W realizacji"));
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter("@W_realizacji", "NULL"));
                        }
                        if (checkBox5.IsChecked == true)
                        {
                            command.Parameters.Add(new SqlParameter("@Zakończony", "Zakończony"));
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter("@Zakończony", "NULL"));
                        }
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@Nowy", "Nowy"));
                        command.Parameters.Add(new SqlParameter("@W_realizacji", "W realizacji"));
                        command.Parameters.Add(new SqlParameter("@Zakończony", "Zakończony"));
                    }
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
                nameTextBox.Name = "N" + task.Id.ToString();
                nameTextBox.LostFocus += new RoutedEventHandler(OnTextChange);
                ComboBox priorityComboBox = new ComboBox();
                priorityComboBox.Name = "P" + task.Id.ToString();
                priorityComboBox.DropDownClosed += new EventHandler(OnDropDownClose_Priority);
                ComboBox statusComboBox = new ComboBox();
                statusComboBox.Name = "S" + task.Id.ToString();
                statusComboBox.DropDownClosed += new EventHandler(OnDropDownClose_Status);
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
                datePickerBox.Name = "DP" + task.Id.ToString();
                datePickerBox.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(OnDateChange);
                Button deleteButton = new Button();
                deleteButton.Width = stackPanel.Width / 12;
                deleteButton.Content = "Usuń";
                deleteButton.Name = "D" + task.Id.ToString();
                deleteButton.Click += new RoutedEventHandler(OnButtonClick);
                stackPanel.Children.Add(deleteButton);
                stackPanel.Children.Add(nameTextBox);
                stackPanel.Children.Add(priorityComboBox);
                stackPanel.Children.Add(statusComboBox);
                stackPanel.Children.Add(datePickerBox);
                StackPanelV.Children.Add(stackPanel);
            }
        }

        private void OnTextChange(object sender, EventArgs e)
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
                    command.CommandText = "update Tasks set TaskName=@TaskName where id=@ID";
                    command.Parameters.Add(new SqlParameter("@ID", ((TextBox)sender).Name.Substring(1)));
                    command.Parameters.Add(new SqlParameter("@TaskName", ((TextBox)sender).Text));
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }

        private void OnDropDownClose_Priority(object sender, EventArgs e)
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
                    command.CommandText = "update Tasks set Priority=@Priority where id=@ID";
                    command.Parameters.Add(new SqlParameter("@ID", ((ComboBox)sender).Name.Substring(1)));
                    command.Parameters.Add(new SqlParameter("@Priority", ((ComboBox)sender).SelectedValue.ToString()));
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }

        private void OnDateChange(object sender, EventArgs e)
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
                    command.CommandText = "update Tasks set Date=@Date where id=@ID";
                    command.Parameters.Add(new SqlParameter("@ID", ((DatePicker)sender).Name.Substring(2)));
                    command.Parameters.Add(new SqlParameter("@Date", ((DatePicker)sender).SelectedDate.Value.ToString("yyyy-MM-dd")));
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }

        private void OnDropDownClose_Status(object sender, EventArgs e)
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
                    command.CommandText = "update Tasks set Status=@Status where id=@ID";
                    command.Parameters.Add(new SqlParameter("@ID", ((ComboBox)sender).Name.Substring(1)));
                    command.Parameters.Add(new SqlParameter("@Status", ((ComboBox)sender).SelectedValue.ToString()));
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
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
                    command.CommandText = "delete from Tasks where id=@ID";
                    command.Parameters.Add(new SqlParameter("@ID", ((Button)sender).Name.Substring(1)));
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
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
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }

        private void ComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }

        private void CheckBox1_Checked(object sender, RoutedEventArgs e)
        {
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }

        private void CheckBox2_Checked(object sender, RoutedEventArgs e)
        {
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SQL_connect(comboBox2.SelectedIndex);
            Show_Tasks(Tasks);
        }
    }
}
