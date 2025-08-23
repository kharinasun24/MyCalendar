using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using MyCalendar;
using System.Data;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

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

        public void ReadVcf()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "VCF files (*.vcf)|*.vcf|All files (*.*)|*.*";
            openFileDialog.Title = "Open VCF File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;

                try
                {
                    string[] lines = File.ReadAllLines(selectedFilePath);
                    string name = "", nameGiven = "", phone = "", email = "", address = "", addressstreet = "", birthday = "", notes = "";

                    foreach (string line in lines)
                    {
                        if (line.StartsWith("BEGIN:VCARD"))
                        {
                            // reset values for new contact
                            name = nameGiven = phone = email = address = addressstreet = birthday = notes = "";
                        }
                        else if (line.StartsWith("N:"))
                        {
                            // Format: N:Last;First;;;
                            var parts = line.Substring(2).Split(';');
                            if (parts.Length > 1)
                            {
                                name = parts[0];
                                nameGiven = parts[1];
                            }
                        }
                        else if (line.StartsWith("FN:"))
                        {
                            // Full name – optional
                        }
                        else if (line.StartsWith("TEL"))
                        {
                            int index = line.IndexOf(':');
                            if (index != -1)
                                phone = line.Substring(index + 1);
                        }
                        else if (line.StartsWith("EMAIL"))
                        {
                            int index = line.IndexOf(':');
                            if (index != -1)
                                email = line.Substring(index + 1);
                        }
                        else if (line.StartsWith("ADR"))
                        {
                            // Format: ADR;TYPE=HOME:;;street;city;state;zip;country
                            int index = line.IndexOf(':');
                            if (index != -1)
                            {
                                var adrParts = line.Substring(index + 1).Split(';');
                                if (adrParts.Length > 2)
                                {
                                    addressstreet = adrParts[2]; // Straße
                                }
                                if (adrParts.Length > 3)
                                {
                                    address = adrParts[3]; // Ort (z. B. Stadt mit PLZ)
                                }
                            }
                        }
                        else if (line.StartsWith("BDAY:"))
                        {
                            birthday = line.Substring(5); // falls vCard Geburtstag enthält
                        }
                        else if (line.StartsWith("NOTE:"))
                        {
                            notes = line.Substring(5);
                        }
                        else if (line.StartsWith("END:VCARD"))
                        {
                            contactDao.CreateNewContact(name, nameGiven, phone, email, birthday, notes, address, addressstreet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Einlesen der VCF-Datei: " + ex.Message);
                }
            }
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

                    //Generell gehe ich an dieser Stelle davon aus, dass der Termin keine Wdh. hat.
                    dateDao.SaveDate(text, start, end, duration, "n");

                }
            }
        }

        public void WriteICS()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "ICS files (*.ics)|*.ics|All files (*.*)|*.*";
            saveFileDialog.Title = "Save ICS File";
            saveFileDialog.FileName = "export.ics";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Neues Kalender-Objekt anlegen
                var calendar = new Ical.Net.Calendar();

                // Alle Termine aus der Datenbank holen
                List<Date> allDates = dateDao.GetAllDates(); // <-- anpassen an deine Methode

                foreach (var date in allDates)
                {
                    var calendarEvent = new CalendarEvent
                    {
                        Summary = date.Text,  // Titel
                        DtStart = new CalDateTime(DateTime.ParseExact(date.Start, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)),
                        DtEnd = new CalDateTime(DateTime.ParseExact(date.End, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)),
                        Description = "Exported from MyCalendar"
                    };

                    calendar.Events.Add(calendarEvent);
                }

                // Kalender in String serialisieren
                var serializer = new CalendarSerializer();
                string serializedCalendar = serializer.SerializeToString(calendar);

                // Datei schreiben
                File.WriteAllText(saveFileDialog.FileName, serializedCalendar, Encoding.UTF8);
            }
        }

    }
}
