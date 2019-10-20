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
    public partial class MainWindow : Window
    {
        List<Task> Tasks;
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
        {
            DataSource = @"localhost\SQLEXPRESS",   //do zmiany
            UserID = "Adam_DB",                     //do zmiany
            Password = "Password_1",                //do zmiany
            InitialCatalog = "TasksDb"              //do zmiany
        };
        public MainWindow()
        {
            WindowInitialization();
        }
        /* Funkcja pobierająca dane z serwera w sposób określony przez użytkownika poprzez UI */
        private void FetchData()
        {
            Tasks = new List<Task>();
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand
                    {
                        Connection = connection,
                        CommandText = "select * from Tasks where (Priority in (@Wysoki, @Normalny, @Niski) and Status in (@Nowy, @W_realizacji, @Zakończony)) order by case when @OrderMode = 0 then [Id] end desc, case when @OrderMode = 1 then [Id] end, case when @OrderMode = 2 then [Date] end, case when @OrderMode = 3 then [Date] end desc"
                    };
                    command.Parameters.Add(new SqlParameter("@OrderMode", orderSelector.SelectedIndex));
                    AddFilterParametersTo(command);
                    SqlDataReader read = command.ExecuteReader();
                    while (read.Read())
                    {
                        Tasks.Add(new Task(read));
                    }
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        /* Funkcja ustalająca parametry w zapytaniu wysylanym przez FetchData() odpowiedzialne za filtrowanie wyników */
        private void AddFilterParametersTo(SqlCommand command)
        {
            if ((bool)filterHighPriorityCheckBox.IsChecked | (bool)filterNormalPriorityCheckBox.IsChecked | (bool)filterLowPriorityCheckBox.IsChecked)
            {
                if (filterHighPriorityCheckBox.IsChecked == true)
                {
                    command.Parameters.Add(new SqlParameter("@Wysoki", "Wysoki"));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@Wysoki", "NULL"));
                }
                if (filterNormalPriorityCheckBox.IsChecked == true)
                {
                    command.Parameters.Add(new SqlParameter("@Normalny", "Normalny"));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@Normalny", "NULL"));
                }
                if (filterLowPriorityCheckBox.IsChecked == true)
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
            if ((bool)filterNewTaskCheckBox.IsChecked | (bool)filterInProgressTaskCheckBox.IsChecked | (bool)filterFinishedTaskCheckBox.IsChecked)
            {
                if (filterNewTaskCheckBox.IsChecked == true)
                {
                    command.Parameters.Add(new SqlParameter("@Nowy", "Nowy"));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@Nowy", "NULL"));
                }
                if (filterInProgressTaskCheckBox.IsChecked == true)
                {
                    command.Parameters.Add(new SqlParameter("@W_realizacji", "W realizacji"));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@W_realizacji", "NULL"));
                }
                if (filterFinishedTaskCheckBox.IsChecked == true)
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
        }
        /* Funkcja wyswietlająca panele odpowiedzialne za edycje i prezentacje zadań */
        private void ShowTasks()
        {
            StackPanelV.Children.Clear();
            foreach (Task task in Tasks)
            {
                CreateTaskControlPanel(task);
            }
        }
        /* Funkcja tworząca panel kontrolny dla każdego zadania */
        private void CreateTaskControlPanel(Task task)
        {
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = StackPanelV.Width,
                Name = "SN" + task.Id.ToString()
            };
            RegisterName(stackPanel.Name, stackPanel);
            TextBox nameTextBox = new TextBox
            {
                Width = stackPanel.Width / 5,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Text = task.TaskName,
                Name = "N" + task.Id.ToString()
            };
            nameTextBox.LostFocus += new RoutedEventHandler(OnTextChange);
            ComboBox priorityComboBox = new ComboBox
            {
                Name = "P" + task.Id.ToString()
            };
            priorityComboBox.DropDownClosed += new EventHandler(OnDropDownClose_Priority);
            ComboBox statusComboBox = new ComboBox
            {
                Name = "S" + task.Id.ToString()
            };
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
            DatePicker datePickerBox = new DatePicker
            {
                Width = stackPanel.Width / 5,
                SelectedDate = task.Date,
                Name = "DP" + task.Id.ToString()
            };
            datePickerBox.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(OnDateChange);
            Button deleteButton = new Button
            {
                Width = stackPanel.Width / 12,
                Content = "Usuń",
                Name = "D" + task.Id.ToString()
            };
            deleteButton.Click += new RoutedEventHandler(OnDeleteButtonClick);
            stackPanel.Children.Add(deleteButton);
            stackPanel.Children.Add(nameTextBox);
            stackPanel.Children.Add(priorityComboBox);
            stackPanel.Children.Add(statusComboBox);
            stackPanel.Children.Add(datePickerBox);
            StackPanelV.Children.Add(stackPanel);
        }
        /* EventHandler edytujący nazwę odpowiedniego zadania w bazie danych i na ekranie */
        private void OnTextChange(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand
                    {
                        Connection = connection,
                        CommandText = "update Tasks set TaskName=@TaskName where id=@ID"
                    };
                    command.Parameters.Add(new SqlParameter("@ID", ((TextBox)sender).Name.Substring(1)));
                    command.Parameters.Add(new SqlParameter("@TaskName", ((TextBox)sender).Text));
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
        }
        /* EventHandler edytujący priorytet odpowiedniego zadania w bazie danych i na ekranie */
        private void OnDropDownClose_Priority(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand
                    {
                        Connection = connection,
                        CommandText = "update Tasks set Priority=@Priority where id=@ID"
                    };
                    command.Parameters.Add(new SqlParameter("@ID", ((ComboBox)sender).Name.Substring(1)));
                    command.Parameters.Add(new SqlParameter("@Priority", ((ComboBox)sender).SelectedValue.ToString()));
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
        }
        /* EventHandler edytujący termin odpowiedniego zadania w bazie danych i na ekranie */
        private void OnDateChange(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand
                    {
                        Connection = connection,
                        CommandText = "update Tasks set Date=@Date where id=@ID"
                    };
                    command.Parameters.Add(new SqlParameter("@ID", ((DatePicker)sender).Name.Substring(2)));
                    command.Parameters.Add(new SqlParameter("@Date", ((DatePicker)sender).SelectedDate.Value.ToString("yyyy-MM-dd")));
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
        }
        /* EventHandler edytujący status odpowiedniego zadania w bazie danych i na ekranie */
        private void OnDropDownClose_Status(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand
                    {
                        Connection = connection,
                        CommandText = "update Tasks set Status=@Status where id=@ID"
                    };
                    command.Parameters.Add(new SqlParameter("@ID", ((ComboBox)sender).Name.Substring(1)));
                    command.Parameters.Add(new SqlParameter("@Status", ((ComboBox)sender).SelectedValue.ToString()));
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
        }
        /* EventHandler usuwający odpowiednie zadanie z bazy i z ekranu */
        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand
                    {
                        Connection = connection,
                        CommandText = "delete from Tasks where id=@ID"
                    };
                    command.Parameters.Add(new SqlParameter("@ID", ((Button)sender).Name.Substring(1)));
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
            string TargetName = "SN" + ((Button)sender).Name.Substring(1);
            StackPanelV.Children.Remove((StackPanel)FindName(TargetName));
            UnregisterName(TargetName);
        }
        /* EventHandler dodający zadanie do bazy danych i wyświetlający je na ekranie po wciśnieciu przycisku saveTaskButton */
        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand
                    {
                        Connection = connection,
                        CommandText = "insert into Tasks values (@TaskName, @Priority, @Date, @Status)"
                    };
                    command.Parameters.Add(new SqlParameter("@TaskName", taskNameInputTextBox.Text));
                    command.Parameters.Add(new SqlParameter("@Priority", prioritySelectorComboBox.SelectedValue.ToString()));
                    command.Parameters.Add(new SqlParameter("@Date", datePicker.SelectedDate.Value.ToString("yyyy-MM-dd")));
                    command.Parameters.Add(new SqlParameter("@Status", statusSelectorComboBox.SelectedValue.ToString()));
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException er)
            {
                MessageBox.Show(er.ToString());
            }
            FetchData();
            StackPanel[] TasksCopy = new StackPanel[StackPanelV.Children.Count];
            StackPanelV.Children.CopyTo(TasksCopy, 0);
            StackPanelV.Children.Clear();
            foreach (Task task in Tasks)
            {
                string TargetName = "SN" + task.Id;
                if (Array.Find(TasksCopy, Task => Task.Name == TargetName) != null)
                {
                    StackPanelV.Children.Add(Array.Find(TasksCopy, Task => Task.Name == TargetName));
                }
                else
                {
                    CreateTaskControlPanel(task);
                }
            }
        }
        /* EventHandler wymuszający pobranie danych posortowanych w sposob okreslony przez użytkownika */
        private void OrderSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FetchData();
            StackPanel[] TasksCopy = new StackPanel[StackPanelV.Children.Count];
            StackPanelV.Children.CopyTo(TasksCopy, 0);
            StackPanelV.Children.Clear();
            foreach (Task task in Tasks)
            {
                string TargetName = "SN" + task.Id;
                StackPanelV.Children.Add(Array.Find(TasksCopy, Task => Task.Name == TargetName));
            }
        }
        /* EventHandler wymuszający pobranie danych przefiltrowanych w sposob okreslony przez użytkownika */
        private void FilterCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            FetchData();
            StackPanel[] TasksCopy = new StackPanel[StackPanelV.Children.Count];
            StackPanelV.Children.CopyTo(TasksCopy, 0);
            for (int i = 0; i < TasksCopy.Length; i++)
            {
                TasksCopy[i].Visibility = Visibility.Collapsed;
            }
            foreach (Task task in Tasks)
            {
                string TargetName = "SN" + task.Id;
                if (Array.Find(TasksCopy, Task => Task.Name == TargetName) != null)
                {
                    (Array.Find(TasksCopy, Task => Task.Name == TargetName)).Visibility = Visibility.Visible;
                }
            }
            StackPanelV.Children.Clear();
            foreach (StackPanel sp in TasksCopy)
            {
                StackPanelV.Children.Add(sp);
            }
        }
        /* Funkcja inicjalizująca statyczne kontrolki i wyświetlająca zadania w domyślny sposób po otworzeniu aplikacji */
        private void WindowInitialization()
        {
            InitializeComponent();
            FetchData();
            datePicker.SelectedDate = DateTime.Now;
            prioritySelectorComboBox.Items.Insert(0, "Wysoki");
            prioritySelectorComboBox.Items.Insert(1, "Normalny");
            prioritySelectorComboBox.Items.Insert(2, "Niski");
            prioritySelectorComboBox.SelectedIndex = 1;
            statusSelectorComboBox.Items.Insert(0, "Nowy");
            statusSelectorComboBox.Items.Insert(1, "W realizacji");
            statusSelectorComboBox.Items.Insert(2, "Zakończony");
            statusSelectorComboBox.SelectedIndex = 0;
            orderSelector.Items.Insert(0, "Od najnowszego");
            orderSelector.Items.Insert(1, "Od najstarszego");
            orderSelector.Items.Insert(2, "Wg terminu rosnąco");
            orderSelector.Items.Insert(3, "Wg terminu malejąco");
            ShowTasks();
            orderSelector.SelectedIndex = 0;
        }
    }
}
