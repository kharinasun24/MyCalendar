using Microsoft.Extensions.Configuration;
using System.Data.SQLite;

namespace MyCalendar
{
    public class LanguageDao
    {
        private IConfiguration configuration;

        public LanguageDao() {

            configuration = new ConfigurationBuilder()
           .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) 
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // JSON-Datei laden
           .Build();

        }    

        public string GetCurrentLanguage()
        {

            string currentLanguage = "";

            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            //string connectionString = configuration.GetConnectionString("SQLiteConnection");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = $"SELECT lang FROM language";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            currentLanguage = reader["lang"].ToString();

                        }



                    }
                }
                connection.Close();
            }

            if (String.IsNullOrEmpty(currentLanguage))
            {
                currentLanguage = "Deutsch";

                SetLanguage(currentLanguage);
            }


            return currentLanguage;

        }
        public void SetLanguage(string language)
        {
            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Überprüfen, ob bereits ein Eintrag existiert
                string checkSql = "SELECT COUNT(*) FROM language";
                using (SQLiteCommand checkCommand = new SQLiteCommand(checkSql, connection))
                {
                    long count = (long)checkCommand.ExecuteScalar(); // Anzahl der vorhandenen Einträge
                    if (count > 0)
                    {
                        // Wenn Eintrag vorhanden ist, dann ein Update durchführen
                        string updateSql = "UPDATE language SET lang = @language";
                        using (SQLiteCommand updateCommand = new SQLiteCommand(updateSql, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@language", language);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Wenn kein Eintrag vorhanden ist, ein Insert durchführen
                        string insertSql = "INSERT INTO language (lang) VALUES (@language)";
                        using (SQLiteCommand insertCommand = new SQLiteCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@language", language);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

    }
}
