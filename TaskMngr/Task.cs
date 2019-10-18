using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace TaskMngr
{
    class Task
    {
        public int Id;
        public string TaskName;
        public string Priority;
        public DateTime Date;
        public string Status;

        public Task(SqlDataReader dataReader)
        {
            this.Id = Int32.Parse(dataReader["Id"].ToString());
            this.TaskName = dataReader["TaskName"].ToString();
            this.Priority = dataReader["Priority"].ToString();
            this.Date = DateTime.Parse(dataReader["Date"].ToString());
            this.Status = dataReader["Status"].ToString();
        }
        public int PriorityValue()
        {
            if (this.Priority=="Wysoki")
            {
                return 0;
            }
            if (this.Priority == "Normalny")
            {
                return 1;
            }
            if (this.Priority == "Niski")
            {
                return 2;
            }
            return 1;
        }
        public int StatusValue()
        {
            if (this.Status == "Nowy")
            {
                return 0;
            }
            if (this.Status == "W realizacji")
            {
                return 1;
            }
            if (this.Status == "Zakończony")
            {
                return 2;
            }
            return 0;
        }
    }
}
