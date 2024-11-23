using Microsoft.Extensions.Configuration;
using MyCalendar;
using System.Data;
using System.Data.SQLite;

namespace MyCalendar
{
    public class ContactDao
    {

        IConfiguration configuration;

        public ContactDao()
        {

            configuration = new ConfigurationBuilder()
           .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // JSON-Datei laden
           .Build();

        }
        public DataTable GetContactsOrderByName()
        {

            string connectionString = configuration.GetConnectionString("SQLiteConnection");
            DataTable dt = new DataTable();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT id, name FROM contacts ORDER BY name";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }

            return dt;

        }

        public DataTable GetContactsOrderByIsCouple(string dateid)
        {
            string connectionString = configuration.GetConnectionString("SQLiteConnection");
            DataTable dt = new DataTable();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = @"SELECT c.id, c.name FROM contacts c LEFT JOIN couples cp ON c.id = cp.id_contact AND cp.id_date = @dateid ORDER BY CASE WHEN cp.id_date IS NOT NULL THEN 0 ELSE 1 END, c.name";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dateid", dateid);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }

            return dt;
        }

        /*
              public DataTable GetContactsOrderByIsCouple(string dateid)
              {
                  string connectionString = configuration.GetConnectionString("SQLiteConnection");
                  DataTable dt = new DataTable();

                  using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                  {
                      connection.Open();


                      string sql = "SELECT contacts.id, contacts.name FROM contacts LEFT JOIN couples ON contacts.id = couples.id_contact ORDER BY couples.id_contact DESC";

                      using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                      {
                          using (SQLiteDataReader reader = command.ExecuteReader())
                          {
                              dt.Load(reader); // Direktes Laden der Daten in den DataTable
                          }
                      }
                  }

                  return dt;
              }
        */


        public bool GetLinkedContact(string dateID, string contactID)
        {

            bool boolValue = false;
            string connectionString = configuration.GetConnectionString("SQLiteConnection");
            string iscouple = "";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = $"SELECT iscouple FROM couples WHERE id_date = '{dateID}' AND id_contact = '{contactID}' AND iscouple = '1'";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            iscouple = reader["iscouple"].ToString();

                        }



                    }
                }
                connection.Close();
            }


            if ("1".Equals(iscouple))
            {
                boolValue = true;
            }
            else
            {
                boolValue = false;
            }

            return boolValue;

        }


        public void ToggleCouple(string id, string clickedCellValue)
        {
            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            SQLiteConnection connection = new SQLiteConnection(connectionString);

            connection.Open();

            // SQL-Befehle für Update und Insert
            string sqlUpdate = $"UPDATE couples SET iscouple = CASE WHEN iscouple = '0' THEN '1' ELSE '0' END WHERE id_date = @id AND id_contact = @clickedCellValue;";
            string sqlInsert = $"INSERT INTO couples (id_date, id_contact, iscouple) VALUES (@id, @clickedCellValue, '1');";
            string sqlDelete = "DELETE FROM couples WHERE iscouple = '0';";


            // Beginne die SQL-Transaktion
            using (SQLiteCommand command = new SQLiteCommand(sqlUpdate, connection))
            {
                // Parameter zum SQL-Befehl hinzufügen
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@clickedCellValue", clickedCellValue);

                // Führt das Update aus
                int rowsAffected = command.ExecuteNonQuery();

                // Wenn keine Zeilen betroffen sind, füge einen neuen Datensatz ein
                if (rowsAffected == 0)
                {
                    using (SQLiteCommand insertCommand = new SQLiteCommand(sqlInsert, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@id", id);
                        insertCommand.Parameters.AddWithValue("@clickedCellValue", clickedCellValue);

                        insertCommand.ExecuteNonQuery();
                    }
                }

                using (SQLiteCommand deleteCommand = new SQLiteCommand(sqlDelete, connection))
                {
                    deleteCommand.ExecuteNonQuery();
                }

                connection.Close();
            }

        }


        public void DeleteEntryById(string id)
        {


            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = $"DELETE FROM contacts WHERE id = @id";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }


                string sql2 = $"DELETE FROM couples WHERE id_contact = @id";
                using (SQLiteCommand command = new SQLiteCommand(sql2, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }


        public void CreateNewContact(string name, string nameGiven, string phone, string email, string birthday, string notes, string address, string addressstreet)
        {
            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO contacts (name, namegiven, phone, email, birthday, notes, address, address_street) VALUES (@name, @nameGiven, @phone, @email, @birthday, @notes, @address, @addressstreet)";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    //command.Parameters.AddWithValue("@id", date.Id);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@nameGiven", nameGiven);
                    command.Parameters.AddWithValue("@phone", phone);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@birthday", birthday);
                    command.Parameters.AddWithValue("@notes", notes);
                    command.Parameters.AddWithValue("@address", address);
                    command.Parameters.AddWithValue("@addressstreet", addressstreet);

                    command.ExecuteNonQuery();
                }


                connection.Close();
            }
        }

        public void UpdateContactWithId(string id, string name, string nameGiven, string phone, string email, string birthday, string notes, string address, string addressstreet)
        {
            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            SQLiteConnection connection = new SQLiteConnection(connectionString);

            connection.Open();

            string sql = $"UPDATE contacts SET name = @name, nameGiven = @nameGiven, phone = @phone, email = @email, birthday = @birthday, notes = @notes, address = @address, address_street = @address_street WHERE id = @id;";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@nameGiven", nameGiven);
                command.Parameters.AddWithValue("@phone", phone);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@birthday", birthday);
                command.Parameters.AddWithValue("@notes", notes);
                command.Parameters.AddWithValue("@address", address);
                command.Parameters.AddWithValue("@address_street", addressstreet);

                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        public List<Contact> GetContactsOrderByIsCoupleId(string id)
        {
            List<Contact> ctcs = new List<Contact>();

            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = $"SELECT * FROM contacts WHERE id = '{id}'";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string idd = reader["id"].ToString();
                            string name = reader["name"].ToString();
                            string nameGiven = reader["namegiven"].ToString();
                            string phone = reader["phone"].ToString();
                            string email = reader["email"].ToString();
                            string birthday = reader["birthday"].ToString();
                            string notes = reader["notes"].ToString();
                            string address = reader["address"].ToString();
                            string addressstreet = reader["address_street"].ToString();

                            Contact c = new Contact(idd, name, nameGiven, phone, email, birthday, notes, address, addressstreet);

                            ctcs.Add(c);
                        }
                    }
                }

                connection.Close();
            }

            return ctcs;
        }

        public List<Contact> GetAllContacts()
        {
            List<Contact> ctcs = new List<Contact>();

            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = $"SELECT * FROM contacts";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string idd = reader["id"].ToString();
                            string name = reader["name"].ToString();
                            string nameGiven = reader["namegiven"].ToString();
                            string phone = reader["phone"].ToString();
                            string email = reader["email"].ToString();
                            string birthday = reader["birthday"].ToString();
                            string notes = reader["notes"].ToString();
                            string address = reader["address"].ToString();
                            string addressstreet = reader["address_street"].ToString();

                            Contact c = new Contact(idd, name, nameGiven, phone, email, birthday, notes, address, addressstreet);

                            ctcs.Add(c);
                        }
                    }
                }

                connection.Close();
            }

            return ctcs;
        }

        public List<Contact> GetContactByContactId(string contid)
        {
            var contacts = new List<Contact>();
            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM contacts WHERE id = @contid";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@contid", contid);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var c = new Contact(
                                reader["id"].ToString(),
                                reader["name"].ToString(),
                                reader["namegiven"].ToString(),
                                reader["phone"].ToString(),
                                reader["email"].ToString(),
                                reader["birthday"].ToString(),
                                reader["notes"].ToString(),
                                reader["address"].ToString(),
                                reader["address_street"].ToString()
                            );
                            contacts.Add(c);
                        }
                    }
                }
            }

            return contacts;
        }

        public void DeleteAllDates()
        {
            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Beginne die SQL-Transaktion
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Lösche alle Einträge aus der Tabelle 'dates'
                        string sqlDeleteDates = "DELETE FROM dates";
                        using (SQLiteCommand command = new SQLiteCommand(sqlDeleteDates, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Lösche alle Einträge aus der Tabelle 'couples'
                        string sqlDeleteCouples = "DELETE FROM couples";
                        using (SQLiteCommand command = new SQLiteCommand(sqlDeleteCouples, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Transaktion abschließen
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Bei einem Fehler die Transaktion zurückrollen
                        transaction.Rollback();
                        throw new Exception("Fehler beim Löschen der Einträge", ex);
                    }
                }

                connection.Close();
            }
        }

        public void DeleteAllContacts()
        {
            string connectionString = configuration.GetConnectionString("SQLiteConnection");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Beginne die SQL-Transaktion
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Lösche alle Einträge aus der Tabelle 'dates'
                        string sqlDeleteDates = "DELETE FROM contacts";
                        using (SQLiteCommand command = new SQLiteCommand(sqlDeleteDates, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Lösche alle Einträge aus der Tabelle 'couples'
                        string sqlDeleteCouples = "DELETE FROM couples";
                        using (SQLiteCommand command = new SQLiteCommand(sqlDeleteCouples, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Transaktion abschließen
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Bei einem Fehler die Transaktion zurückrollen
                        transaction.Rollback();
                        throw new Exception("Fehler beim Löschen der Einträge", ex);
                    }
                }

                connection.Close();
            }
        }
    }
}
