using Ical.Net.CalendarComponents;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml;
using System.Resources;
using MyCalendar;

namespace MyCalendar
{
    public class Reader
    {
        private DateDao dateDao;
        private ContactDao contactDao;
        private ResourceManager resourceManager;

        public Reader()
        {

            dateDao = new DateDao();
            contactDao = new ContactDao();
        }

        public void ReadXML()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog.Title = "Open XML File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the file path of the selected file
                string selectedFilePath = openFileDialog.FileName;

                try
                {
                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.Load(selectedFilePath);

                    XmlNodeList contactNodes = xmlDoc.GetElementsByTagName("contact");

                    List<Contact> readContactsToSave = new List<Contact>();

                    string nameGiven = "";
                    string name = "";
                    string address = "";
                    string addressstreet = "";
                    string phone = "";
                    string email = "";
                    string birthday = "";
                    string notes = "";

                    foreach (XmlNode contactNode in contactNodes)
                    {

                        XmlNode nameGivenNode = contactNode["nameGiven"];
                        XmlNode nameNode = contactNode["name"];
                        XmlNode addressNode = contactNode["address"];
                        XmlNode addressStreetNode = contactNode["addressstreet"];
                        XmlNode phoneNode = contactNode["phone"];
                        XmlNode emailNode = contactNode["email"];
                        XmlNode birthdayNode = contactNode["birthday"];
                        XmlNode notesNode = contactNode["notes"];


                        if (nameGivenNode != null)
                        {
                            nameGiven = nameGivenNode.InnerText;
                        }

                        if (nameNode != null)
                        {
                            name = nameNode.InnerText;
                        }

                        if (addressNode != null)
                        {
                            address = addressNode.InnerText;
                        }

                        if (addressStreetNode != null)
                        {
                            addressstreet = addressStreetNode.InnerText;
                        }


                        if (phoneNode != null)
                        {
                            phone = phoneNode.InnerText;
                        }

                        if (emailNode != null)
                        {
                            email = emailNode.InnerText;
                        }

                        if (birthdayNode != null)
                        {
                            birthday = birthdayNode.InnerText;
                        }

                        if (notesNode != null)
                        {
                            notes = notesNode.InnerText;
                        }


                        contactDao.CreateNewContact(name, nameGiven, phone, email, birthday, notes, address, addressstreet);

                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., file not found, incorrect XML format)
                    MessageBox.Show("Error reading XML file: " + ex.Message);
                }
            }
        }

        public void ReadICS()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ICS files (*.ics)|*.ics|All files (*.*)|*.*";
            openFileDialog.Title = "Open ICS File";

            // Show the dialog and check if the user clicked OK
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the file path of the selected file
                string selectedFilePath = openFileDialog.FileName;

                // Lade den Inhalt der ICS-Datei
                string icsContent = File.ReadAllText(selectedFilePath);

                // Erstelle ein neues Calendar-Objekt und parse den Inhalt
                Ical.Net.Calendar calendar = Ical.Net.Calendar.Load(icsContent);

                // Greife auf die Ereignisse zu
                foreach (CalendarEvent calendarEvent in calendar.Events)
                {
                    string input = calendarEvent.Start.ToString();

                    // Regex für Datum und Zeit
                    string pattern = @"(\d{2}\.\d{2}\.\d{4})\s+(\d{2}:\d{2}:\d{2})";

                    // Verwende Regex.Match, um die Teile zu extrahieren
                    Match match = Regex.Match(input, pattern);

                    string start = "";
                    if (match.Success)
                    {
                        string datePartStart = match.Groups[1].Value; // 02.01.2014
                        string timePartStart = match.Groups[2].Value.Substring(0, 5); // 11:00 :00

                        start = datePartStart + " " + timePartStart;

                    }
                    else
                    {
                        MessageBox.Show(resourceManager.GetString("Could not read ICS file!"));
                    }

                    input = calendarEvent.End.ToString();

                    string end = "";
                    if (match.Success)
                    {
                        string datePartEnd = match.Groups[1].Value; // 02.01.2014
                        string timePartEnd = match.Groups[2].Value.Substring(0, 5); // 11:00 :00

                        end = datePartEnd + " " + timePartEnd;

                    }
                    else
                    {
                        MessageBox.Show("Could not read ICS file!", "Failure", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    }


                    string text = calendarEvent.Summary;


                    TimeSpan d = DateTime.ParseExact(end, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(start, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture); ;
                    int durationInt = d.Days;

                    durationInt++;

                    string duration = durationInt.ToString();

                    // Generell gehe ich an dieser Stelle davon aus, dass der Termin keine Wdh. hat.
                    dateDao.saveDate(text, start, end, duration, "n");

                    // ... weitere Eigenschaften können ausgeben werden.

                }
            }
        }
    }
}
