using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Windows.Input;

namespace TaskMngr
{
    public partial class MainWindow : Window
    {
        const string HighPriorityParameterString="Wysoki";
        const string NormalPriorityParameterString = "Normalny";
        const string LowPriorityParameterString = "Niski";
        const string NewStatusParameterString = "Nowy";
        const string InProgressStatusParameterString = "W realizacji";
        const string FinishedStatusParameterString = "Zakończony";
        const string NullParameterString = "NULL";
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
        /* Metoda pobierająca dane z serwera w sposób określony przez użytkownika poprzez UI */
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
        /* Metoda ustalająca parametry w zapytaniu wysylanym przez FetchData() odpowiedzialne za filtrowanie wyników */
        private void AddFilterParametersTo(SqlCommand command)
        {
            if ((bool)filterHighPriorityCheckBox.IsChecked | (bool)filterNormalPriorityCheckBox.IsChecked | (bool)filterLowPriorityCheckBox.IsChecked)
            {
                if (filterHighPriorityCheckBox.IsChecked == true)
                {
                    command.Parameters.Add(new SqlParameter("@Wysoki", HighPriorityParameterString));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@Wysoki", NullParameterString));
                }
                if (filterNormalPriorityCheckBox.IsChecked == true)
                {
                    command.Parameters.Add(new SqlParameter("@Normalny", NormalPriorityParameterString));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@Normalny", NullParameterString));
                }
                if (filterLowPriorityCheckBox.IsChecked == true)
                {
                    command.Parameters.Add(new SqlParameter("@Niski", LowPriorityParameterString));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@Niski", NullParameterString));
                }
            }
            else
            {
                command.Parameters.Add(new SqlParameter("@Wysoki", HighPriorityParameterString));
                command.Parameters.Add(new SqlParameter("@Normalny", NormalPriorityParameterString));
                command.Parameters.Add(new SqlParameter("@Niski", LowPriorityParameterString));
            }
            if ((bool)filterNewTaskCheckBox.IsChecked | (bool)filterInProgressTaskCheckBox.IsChecked | (bool)filterFinishedTaskCheckBox.IsChecked)
            {
                if (filterNewTaskCheckBox.IsChecked == true)
                {
                    command.Parameters.Add(new SqlParameter("@Nowy", NewStatusParameterString));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@Nowy", NullParameterString));
                }
                if (filterInProgressTaskCheckBox.IsChecked == true)
                {
                    command.Parameters.Add(new SqlParameter("@W_realizacji", InProgressStatusParameterString));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@W_realizacji", NullParameterString));
                }
                if (filterFinishedTaskCheckBox.IsChecked == true)
                {
                    command.Parameters.Add(new SqlParameter("@Zakończony", FinishedStatusParameterString));
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@Zakończony", NullParameterString));
                }
            }
            else
            {
                command.Parameters.Add(new SqlParameter("@Nowy", NewStatusParameterString));
                command.Parameters.Add(new SqlParameter("@W_realizacji", InProgressStatusParameterString));
                command.Parameters.Add(new SqlParameter("@Zakończony", FinishedStatusParameterString));
            }
        }
        /* Metoda wyswietlająca panele odpowiedzialne za edycje i prezentacje zadań */
        private void ShowTasks()
        {
            StackPanelV.Children.Clear();
            foreach (Task task in Tasks)
            {
                CreateTaskControlPanel(task);
            }
        }
        /* Metoda tworząca panel kontrolny dla zadania */
        private void CreateTaskControlPanel(Task task)
        {
            StackPanelV.Width = scroll.MinWidth;
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
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
            nameTextBox.KeyDown += new KeyEventHandler(OnTextChangeEnter);
            nameTextBox.LostFocus += new RoutedEventHandler(OnTextChangeLostFocus);
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
            priorityComboBox.Items.Insert(0, HighPriorityParameterString);
            priorityComboBox.Items.Insert(1, NormalPriorityParameterString);
            priorityComboBox.Items.Insert(2, LowPriorityParameterString);
            priorityComboBox.SelectedIndex = task.PriorityValue();
            statusComboBox.Items.Insert(0, NewStatusParameterString);
            statusComboBox.Items.Insert(1, InProgressStatusParameterString);
            statusComboBox.Items.Insert(2, FinishedStatusParameterString);
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
        /* EventHandler edytujący nazwę odpowiedniego zadania w bazie danych i na ekranie po naciśnięciu Enter */
        private void OnTextChangeEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendName(sender);
                taskNameInputTextBox.Focus();
            }
        }
        /* EventHandler edytujący nazwę odpowiedniego zadania w bazie danych i na ekranie po utraceniu focusu prze pole tekstowe */
        private void OnTextChangeLostFocus(object sender, RoutedEventArgs e)
        {
            SendName(sender);
        }
        /* Metoda wysylajaca nazwe zadania do bazy danych wywoływana w EventHandlerach OnTextChangeLostFocus i OnTextChangeEnter */
        private void SendName(object sender)
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
                else
                {
                    CreateTaskControlPanel(task);
                }
            }
            foreach (StackPanel sp in TasksCopy)
            {
                StackPanelV.Children.Add(sp);
            }
            prioritySelectorComboBox.SelectedIndex = 1;
            statusSelectorComboBox.SelectedIndex = 0;
            datePicker.SelectedDate = DateTime.Now;
            taskNameInputTextBox.Text = "";
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
            prioritySelectorComboBox.Items.Insert(0, HighPriorityParameterString);
            prioritySelectorComboBox.Items.Insert(1, NormalPriorityParameterString);
            prioritySelectorComboBox.Items.Insert(2, LowPriorityParameterString);
            prioritySelectorComboBox.SelectedIndex = 1;
            statusSelectorComboBox.Items.Insert(0, NewStatusParameterString);
            statusSelectorComboBox.Items.Insert(1, InProgressStatusParameterString);
            statusSelectorComboBox.Items.Insert(2, FinishedStatusParameterString);
            statusSelectorComboBox.SelectedIndex = 0;
            orderSelector.Items.Insert(0, "Od najnowszego");
            orderSelector.Items.Insert(1, "Od najstarszego");
            orderSelector.Items.Insert(2, "Wg terminu rosnąco");
            orderSelector.Items.Insert(3, "Wg terminu malejąco");
            ShowTasks();
            orderSelector.SelectedIndex = 0;
        }
        /* EventHandler resetujący filtrowanie */
        private void FilterResetButtonClick(object sender, RoutedEventArgs e)
        {
            RemoveCheckBoxesEventHandlers();
            filterFinishedTaskCheckBox.IsChecked = false;
            filterInProgressTaskCheckBox.IsChecked = false;
            filterNewTaskCheckBox.IsChecked = false;
            filterHighPriorityCheckBox.IsChecked = false;
            filterNormalPriorityCheckBox.IsChecked = false;
            filterLowPriorityCheckBox.IsChecked = false;
            FetchData();
            StackPanel[] TasksCopy = new StackPanel[StackPanelV.Children.Count];
            StackPanelV.Children.CopyTo(TasksCopy, 0);
            StackPanelV.Children.Clear();
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
                else
                {
                    CreateTaskControlPanel(task);
                }
            }
            foreach (StackPanel sp in TasksCopy)
            {
                StackPanelV.Children.Add(sp);
            }
            AddCheckBoxesEventHandlers();
        }

        private void AddCheckBoxesEventHandlers()
        {
            filterFinishedTaskCheckBox.Unchecked += FilterCheckBoxChecked;
            filterInProgressTaskCheckBox.Unchecked += FilterCheckBoxChecked;
            filterNewTaskCheckBox.Unchecked += FilterCheckBoxChecked;
            filterHighPriorityCheckBox.Unchecked += FilterCheckBoxChecked;
            filterLowPriorityCheckBox.Unchecked += FilterCheckBoxChecked;
            filterNormalPriorityCheckBox.Unchecked += FilterCheckBoxChecked;
        }

        private void RemoveCheckBoxesEventHandlers()
        {
            filterFinishedTaskCheckBox.Unchecked -= FilterCheckBoxChecked;
            filterInProgressTaskCheckBox.Unchecked -= FilterCheckBoxChecked;
            filterNewTaskCheckBox.Unchecked -= FilterCheckBoxChecked;
            filterHighPriorityCheckBox.Unchecked -= FilterCheckBoxChecked;
            filterLowPriorityCheckBox.Unchecked -= FilterCheckBoxChecked;
            filterNormalPriorityCheckBox.Unchecked -= FilterCheckBoxChecked;
        }
    }
}
