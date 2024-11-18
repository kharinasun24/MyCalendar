using MyCalendar.logger;
using System.Data;
using System.Data.SQLite;



namespace MyCalendar
{
    public class DateDao
    {


        public DateDao()
        {
        }


        public DataTable GetDatesFor(int day, int month, int year)
        {

            DateTime dateStart = new DateTime(year, month, day);
            DateTime dateEnd = new DateTime(year, month, day, 23, 59, 0);

            // Formatiere das Datum in den gewünschten String
            string start = dateStart.ToString("dd.MM.yyyy HH:mm");
            string end = dateEnd.ToString("dd.MM.yyyy HH:mm");

            //Dieses Datum ist nicht für die DB und es ist ja immer ein Tag, denn nur ein Tag kann im Kalender geklickt werden.
            Date clickedDate = new Date("-1", "", start, end, "1", "n");

            List<Date> datesForDayMonthYear = new List<Date>();

            List<Date> dates = ListOfDatesRelevantFor(day, month, year);

            foreach (Date d in dates)
            {

                if (d.IsBetween(clickedDate))
                {
                    datesForDayMonthYear.Add(d);

                }
            }

            // Erstellen eines DataTable
            DataTable dt = new DataTable();
            dt.Columns.Add("id", typeof(string));
            dt.Columns.Add("text", typeof(string));
            dt.Columns.Add("start", typeof(string));
            dt.Columns.Add("end", typeof(string));
            dt.Columns.Add("repeat", typeof(string));

            //List<Date> sortedDates = datesForDayMonthYear.OrderBy(d => DateTime.Parse(d.Start)).ToList();

            List<Date> sortedDates = datesForDayMonthYear
            .OrderBy(d => DateTime.Parse(d.Start).Date != DateTime.Now.Date) // Bringt Objekte mit dem heutigen Datum an den Anfang
            .ThenBy(d => DateTime.Parse(d.Start))  // Sortiert innerhalb der Gruppen nach Startzeitpunkt
            .ToList();



            foreach (Date date in sortedDates) //datesForDayMonthYear
            {

                DataRow dr = dt.NewRow();
                dr["id"] = date.Id;
                dr["text"] = date.Text;
                dr["start"] = date.Start;
                dr["end"] = date.End;
                dr["repeat"] = date.Repeat;
                dt.Rows.Add(dr);
            }

            // Erstellen eines DataSet
            //DataSet ds = new DataSet();
            //ds.Tables.Add(dt);


            return dt;
        }



        private static List<Date> ListOfDatesRelevantFor(int day, int month, int year)
        {

            List<Date> dates = new List<Date>();

            string formattedDay = day.ToString("D2");
            string formattedMonth = month.ToString("D2");
            string formattedYear = year.ToString("D4");

            string formattedMonthPreceding;

            if (month == 1)
            {


                //year = --year;
                formattedYear = year.ToString("D4");
                formattedMonthPreceding = "12";


            }
            else
            {

                month = --month;
                formattedMonthPreceding = month.ToString("D2");
            }


            string connectionString = "Data Source=cal.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = $"SELECT * FROM dates WHERE (repeat = 'm' AND (SUBSTR(start, -4) >= {formattedYear} AND (SUBSTR(start, -4) = {formattedYear} AND SUBSTR(start, 7, 2) >= {formattedMonth}) OR SUBSTR(start, -4) > {formattedYear} OR SUBSTR(start, -4) >= {formattedYear} AND (SUBSTR(start, -4) = {formattedYear} AND SUBSTR(start, 7, 2) >= {formattedMonthPreceding}) OR SUBSTR(start, -4) > {formattedYear})) OR (repeat = 'n' AND (start LIKE '%.{formattedYear}%' AND start LIKE '%.{formattedMonth}.%' OR start LIKE '%.{formattedYear}%' AND start LIKE '%.{formattedMonthPreceding}.%')) OR (repeat = 'y' AND (start LIKE '%.{formattedMonth}.%' OR start LIKE '%.{formattedMonthPreceding}.%'))";
                //string sql = $"SELECT * FROM dates WHERE (repeat = 'm' AND SUBSTR(start, -4) >= {formattedYear} AND (SUBSTR(start, -4) = {formattedYear} AND SUBSTR(start, 7, 2) >= {formattedMonth}) OR SUBSTR(start, -4) > {formattedYear}) OR (repeat = 'n' AND start LIKE '%.{formattedYear}%' AND start LIKE '%.{formattedMonth}.%') OR (repeat = 'y' AND start LIKE '%.{formattedMonth}.%')";


                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["id"].ToString();
                            string text = reader["text"].ToString();
                            string start = reader["start"].ToString();
                            string end = reader["end"].ToString();
                            string duration = reader["duration"].ToString();
                            string repeat = reader["repeat"].ToString();

                            Date d = new Date(id, text, start, end, duration, repeat);

                            d.SetDayMonthYearClickedByUser(formattedDay, formattedMonth, formattedYear);
                            d.ConfigureDate();


                            dates.Add(d);
                        }
                    }
                }



                connection.Close();
            }



            return dates;
        }


        public List<Date> GetEntryById(string id)
        {
            var dates = new List<Date>();
            string connectionString = "Data Source=cal.db;Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM dates WHERE id = @id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);  // Sicherere Parameterbindung

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var d = new Date(
                                reader["id"].ToString(),
                                reader["text"].ToString(),
                                reader["start"].ToString(),
                                reader["end"].ToString(),
                                reader["duration"].ToString(),
                                reader["repeat"].ToString()
                            );
                            dates.Add(d);
                        }
                    }
                }
            }

            return dates;
        }


        public void DeleteEntryById(string id)
        {
            string connectionString = "Data Source=cal.db;Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Löschen aus der Tabelle "dates"
                string sqlDeleteDates = "DELETE FROM dates WHERE id = @id";
                using (SQLiteCommand command = new SQLiteCommand(sqlDeleteDates, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }

                // Löschen aus der Tabelle "exceptions", wo id_date = @id
                string sqlDeleteExceptions = "DELETE FROM exceptions WHERE id_date = @id";
                using (SQLiteCommand command = new SQLiteCommand(sqlDeleteExceptions, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }


        public void updateDate(string id, string text, string start, string end, string duration, string repeat)
        {
            Date date = new Date(id, text, start, end, duration, repeat);

            string connectionString = "Data Source=cal.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                //string sql = "INSERT INTO dates (text, start, end, duration, repeat) VALUES (@text, @start, @end, @duration, @repeat)";
                string sql = "UPDATE dates SET text = @text, start = @start, end = @end, duration = @duration, repeat = @repeat WHERE id = @id";


                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", date.Id);
                    command.Parameters.AddWithValue("@text", date.Text);
                    command.Parameters.AddWithValue("@start", date.Start);
                    command.Parameters.AddWithValue("@end", date.End);
                    command.Parameters.AddWithValue("@duration", date.Duration);
                    command.Parameters.AddWithValue("@repeat", date.Repeat);

                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void saveDate(string text, string start, string end, string duration, string repeat)
        {
            Date date = new Date("-1", text, start, end, duration, repeat);

            string connectionString = "Data Source=cal.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO dates (text, start, end, duration, repeat) VALUES (@text, @start, @end, @duration, @repeat)";


                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    //command.Parameters.AddWithValue("@id", date.Id);
                    command.Parameters.AddWithValue("@text", date.Text);
                    command.Parameters.AddWithValue("@start", date.Start);
                    command.Parameters.AddWithValue("@end", date.End);
                    command.Parameters.AddWithValue("@duration", date.Duration);
                    command.Parameters.AddWithValue("@repeat", date.Repeat);

                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void WriteExceptionIntoExceptionTBL(string idToDelete, string exceptionStart, string exceptionEnd)
        {
            string connectionString = "Data Source=cal.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO exceptions (id_date, startexception, endexception) VALUES (@idToDelete, @exceptionStart, @exceptionEnd)";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    //command.Parameters.AddWithValue("@id", date.Id); 
                    command.Parameters.AddWithValue("@idToDelete", idToDelete);
                    command.Parameters.AddWithValue("@exceptionStart", exceptionStart);
                    command.Parameters.AddWithValue("@exceptionEnd", exceptionEnd);


                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }


        public DateTime[] ListOfDateTimesRelevantFor(int day, int month, int year)
        {

            List<Date> dateList = ListOfDatesRelevantFor(day, month, year);

            List<DateTime> dateTimeList = new List<DateTime>();

            // Über die Date-Objekte iterieren und das DateTime-Objekt hinzufügen
            foreach (var date in dateList)
            {
                dateTimeList.Add(date.GetStartAsDateTime());
            }

            return dateTimeList.ToArray();

        }

        public List<Date> GetExceptions()
        {
            List<Date> dates = new List<Date>();


            string connectionString = "Data Source=cal.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = $"SELECT * FROM exceptions";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["id_date"].ToString();
                            string startex = reader["startexception"].ToString();
                            string endex = reader["endexception"].ToString();
                            Date d = new Date(id, "", startex, endex, "", "");

                            dates.Add(d);
                        }
                    }
                }


                connection.Close();
            }


            return dates;

        }


    }
}