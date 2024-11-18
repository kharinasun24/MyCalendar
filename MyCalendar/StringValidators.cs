using MyCalendar.logger;
using MyCalendar;
using System;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;

namespace MyCalendar
{
    public class StringValidators
    {

        private static StringValidators _instance;



        private StringValidators()
        {

        }


        public static StringValidators Instance
        {
            get
            {

                if (_instance == null)
                {
                    _instance = new StringValidators();
                }
                return _instance;
            }
        }

        public bool IsNotInExceptionsMethod(List<KeyValuePair<string, DateTime>> appointmentsToIDsDict,

        DateTime givenAppointmentNotAdjusted, int year, int month, int day)
        {
            DateDao dateDao = new DateDao();

            // Liste der Ausnahmen abrufen
            List<Date> exceptions = dateDao.GetExceptions();

            // Überprüfung, ob eines der IDs in appointmentsToIDsDict einem Date-Objekt in exceptions entspricht
            bool found = false;
            foreach (var date in exceptions)
            {
                if (appointmentsToIDsDict.Any(kvp => kvp.Key == date.id))
                {
                    found = true;
                    break;  // Falls gefunden, können wir die Schleife beenden
                }
            }

            // Wenn keine ID gefunden wurde, die mit einem date.id übereinstimmt, dann gibt es auch keine exception.
            if (!found)
            {
                return true;
            }
            // Datum anpassen: givenAppointmentNotAdjusted soll das angepasste Jahr und den Monat erhalten
            DateTime givenAppointmentAdjusted = new DateTime(year, month, givenAppointmentNotAdjusted.Day);

            // Überprüfen, ob das angepasste Datum innerhalb von date.start und date.end liegt (nur Tag relevant)
            foreach (var date in exceptions)
            {
                // Vergleich nur auf Basis von Jahr, Monat, Tag (Zeit wird ignoriert)
                DateTime startDate = DateTime.ParseExact(date.start, "dd.MM.yyyy", CultureInfo.InvariantCulture).Date;
                DateTime endDate = DateTime.ParseExact(date.end, "dd.MM.yyyy", CultureInfo.InvariantCulture).Date;

                if (givenAppointmentAdjusted.Date >= startDate && givenAppointmentAdjusted.Date <= endDate)
                {
                    return true;  // Wenn das Datum innerhalb der Spanne liegt, true zurückgeben
                }
            }

            // Falls keine Übereinstimmung gefunden wurde, false zurückgeben
            return false;
        }


        public void GetMonthsAppointments(int selectedMonth, int selectedYear, DataTable appointments, List<Date> exceptions)
        {
            List<DataRow> rowsToRemove = new List<DataRow>();
            DateTime originalDateTimeStart;
            DateTime originalDateTimeEnd;
            DateTime adjustedDateTimeStart;
            DateTime adjustedDateTimeEnd;

            foreach (DataRow row in appointments.Rows)
            {

                // Parse the start field to a DateTime object
                originalDateTimeStart = DateTime.ParseExact(row.Field<string>("start"), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                originalDateTimeEnd = DateTime.ParseExact(row.Field<string>("end"), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                // Create a new DateTime object with the selected month and year
                adjustedDateTimeStart = new DateTime(selectedYear, selectedMonth, originalDateTimeStart.Day, originalDateTimeStart.Hour, originalDateTimeStart.Minute, originalDateTimeStart.Second);
                adjustedDateTimeEnd = new DateTime(selectedYear, selectedMonth, originalDateTimeEnd.Day, originalDateTimeEnd.Hour, originalDateTimeEnd.Minute, originalDateTimeEnd.Second);

                if (exceptions.Any(e => row.Field<string>("id") == e.id && DateTime.ParseExact(e.start, "dd.MM.yyyy", CultureInfo.InvariantCulture).Date == adjustedDateTimeStart.Date && DateTime.ParseExact(e.end, "dd.MM.yyyy", CultureInfo.InvariantCulture).Date == adjustedDateTimeEnd.Date))
                {
                    rowsToRemove.Add(row);
                }
            }

            foreach (DataRow row in rowsToRemove)
            {
                appointments.Rows.Remove(row);
            }
        }



        public string DayName(string dayName)
        {
            string res = dayName;

            LanguageDao languageDao = new LanguageDao();

            ResourceManager resourceManager = new ResourceManager("MyCalendar.Resources.ResXFile", typeof(Form1).Assembly);

            string language = languageDao.GetCurrentLanguage();



            string culture = "";
            switch (language)
            {
                case "English":
                    culture = "en-GB";
                    break;

                case "русский":
                    culture = "ru-RU";
                    break;

                case "magyar":
                    culture = "hu-HU";
                    break;

                default:
                    culture = "de-DE";
                    break;
            }

            if ("en-GB".Equals(culture))
            {
                return dayName;
            }

            else if ("de-DE".Equals(culture))
            {

                if ("Tu".Equals(dayName)) { dayName = "Di"; }
                if ("We".Equals(dayName)) { dayName = "Mi"; }
                if ("Th".Equals(dayName)) { dayName = "Do"; }
                if ("Su".Equals(dayName)) { dayName = "So"; }

                return dayName;

            }
            else if ("ru-RU".Equals(culture))
            {

                if ("Mo".Equals(dayName)) { dayName = "Пн"; }
                if ("Tu".Equals(dayName)) { dayName = "Вт"; }
                if ("We".Equals(dayName)) { dayName = "Ср"; }
                if ("Th".Equals(dayName)) { dayName = "Чт"; }
                if ("Fr".Equals(dayName)) { dayName = "Пт"; }
                if ("Sa".Equals(dayName)) { dayName = "Сб"; }
                if ("Su".Equals(dayName)) { dayName = "Вс"; }

                return dayName;
            }

            else if ("hu-HU".Equals(culture))
            {

                if ("Mo".Equals(dayName)) { dayName = "hé"; }
                if ("Tu".Equals(dayName)) { dayName = "ke"; }
                if ("We".Equals(dayName)) { dayName = "sze"; }
                if ("Th".Equals(dayName)) { dayName = "cs"; }
                if ("Fr".Equals(dayName)) { dayName = "pé"; }
                if ("Sa".Equals(dayName)) { dayName = "szo"; }
                if ("Su".Equals(dayName)) { dayName = "va"; }

                return dayName;
            }

            return res;
        }

        public List<KeyValuePair<string, DateTime>> AppointmentsToDateTimeDict(int day, int month, int year, DataTable appointments)
        {

            //Dictionary<string, DateTime> res = new Dictionary<string, DateTime>();

            List<KeyValuePair<string, DateTime>> res = new List<KeyValuePair<string, DateTime>>();

            foreach (DataRow row in appointments.Rows)
            {


                if ("n".Equals(row.Field<string>("repeat")))
                {

                    // Parse the start field to a DateTime object
                    DateTime originalDateTimeStart = DateTime.ParseExact(row.Field<string>("start"), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    DateTime originalDateTimeEnd = DateTime.ParseExact(row.Field<string>("end"), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);


                    // Setze die Uhrzeit beider DateTime-Objekte auf 00:00:00
                    DateTime dateStartWithMidnight = new DateTime(originalDateTimeStart.Year, originalDateTimeStart.Month, originalDateTimeStart.Day, 0, 0, 0);
                    DateTime dateEndWithMidnight = new DateTime(originalDateTimeEnd.Year, originalDateTimeEnd.Month, originalDateTimeEnd.Day, 0, 0, 0);

                    DateTime currentDate = dateStartWithMidnight;

                    // Füge alle Tage von dateStartWithMidnight bis dateEndWithMidnight zur Liste hinzu
                    //List<DateTime> dateList = new List<DateTime>();
                    while (currentDate <= dateEndWithMidnight)
                    {
                        //dateList.Add(currentDate);

                        res.Add(new KeyValuePair<string, DateTime>(row.Field<string>("id"), currentDate));


                        currentDate = currentDate.AddDays(1); // Zum nächsten Tag wechseln, Uhrzeit bleibt 00:00:00

                    }

                }
                else if ("y".Equals(row.Field<string>("repeat")))
                {

                    // Parse the start field to a DateTime object
                    DateTime originalDateTimeStart = DateTime.ParseExact(row.Field<string>("start"), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    DateTime originalDateTimeEnd = DateTime.ParseExact(row.Field<string>("end"), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                    DateTime adjustedDateTimeStart = new DateTime(year, originalDateTimeStart.Month, originalDateTimeStart.Day, originalDateTimeStart.Hour, originalDateTimeStart.Minute, originalDateTimeStart.Second);
                    DateTime adjustedDateTimeEnd = new DateTime(year, originalDateTimeEnd.Month, originalDateTimeEnd.Day, originalDateTimeEnd.Hour, originalDateTimeEnd.Minute, originalDateTimeEnd.Second);

                    // Setze die Uhrzeit beider DateTime-Objekte auf 00:00:00
                    DateTime dateStartWithMidnight = new DateTime(adjustedDateTimeStart.Year, adjustedDateTimeStart.Month, adjustedDateTimeStart.Day, 0, 0, 0);
                    DateTime dateEndWithMidnight = new DateTime(adjustedDateTimeEnd.Year, adjustedDateTimeEnd.Month, adjustedDateTimeEnd.Day, 0, 0, 0);

                    DateTime currentDate = dateStartWithMidnight;

                    // Füge alle Tage von dateStartWithMidnight bis dateEndWithMidnight zur Liste hinzu
                    //List<DateTime> dateList = new List<DateTime>();
                    while (currentDate <= dateEndWithMidnight)
                    {
                        //dateList.Add(currentDate);

                        //res.Add(row.Field<string>("id"), currentDate); //new KeyValuePair<string, DateTime>("NeuerSchlüssel", DateTime.Now);
                        res.Add(new KeyValuePair<string, DateTime>(row.Field<string>("id"), currentDate));


                        currentDate = currentDate.AddDays(1); // Zum nächsten Tag wechseln, Uhrzeit bleibt 00:00:00

                    }

                }
                else if ("m".Equals(row.Field<string>("repeat")))
                {

                    // Parse the start field to a DateTime object
                    DateTime originalDateTimeStart = DateTime.ParseExact(row.Field<string>("start"), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    DateTime originalDateTimeEnd = DateTime.ParseExact(row.Field<string>("end"), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                    DateTime adjustedDateTimeStart = new DateTime(year, month, originalDateTimeStart.Day, originalDateTimeStart.Hour, originalDateTimeStart.Minute, originalDateTimeStart.Second);
                    DateTime adjustedDateTimeEnd = new DateTime(year, month, originalDateTimeEnd.Day, originalDateTimeEnd.Hour, originalDateTimeEnd.Minute, originalDateTimeEnd.Second);


                    // Setze die Uhrzeit beider DateTime-Objekte auf 00:00:00
                    DateTime dateStartWithMidnight = new DateTime(adjustedDateTimeStart.Year, adjustedDateTimeStart.Month, adjustedDateTimeStart.Day, 0, 0, 0);
                    DateTime dateEndWithMidnight = new DateTime(adjustedDateTimeEnd.Year, adjustedDateTimeEnd.Month, adjustedDateTimeEnd.Day, 0, 0, 0);

                    DateTime currentDate = dateStartWithMidnight;

                    // Füge alle Tage von dateStartWithMidnight bis dateEndWithMidnight zur Liste hinzu
                    //List<DateTime> dateList = new List<DateTime>();
                    while (currentDate <= dateEndWithMidnight)
                    {

                        //dateList.Add(currentDate);

                        //res.Add(row.Field<string>("id"), currentDate);
                        res.Add(new KeyValuePair<string, DateTime>(row.Field<string>("id"), currentDate));

                        currentDate = currentDate.AddDays(1); // Zum nächsten Tag wechseln, Uhrzeit bleibt 00:00:00

                    }

                }


            }

            return res;
        }


        public bool IsValidPhoneNumber(string phone)
        {

            if (String.IsNullOrEmpty(phone)) { return true; }

            string pattern = @"\+?\d+$";

            bool isMatch = new Regex(pattern).IsMatch(phone);

            if (isMatch)
            {
                return true;
            }
            return false;
        }

        public bool IsValidEmail(string email)
        {
            if (String.IsNullOrEmpty(email)) { return true; }

            if (OccursMoreThanOnce(email, '@'))
            {
                return false;
            }

            Regex regex = new Regex("^\\S+@\\S+$", RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }

        public bool OccursMoreThanOnce(string str, char c)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                {
                    count++;
                    if (count > 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public string CheckDateI18N(string dateString)
        {

            string res = dateString;

            string pattern = @"^\d{2}/\d{2}/\d{4}$";

            if (Regex.IsMatch(dateString, pattern))
            {
                // Das Format stimmt, jetzt umwandeln
                DateTime date = DateTime.ParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                string formattedDate = date.ToString("dd.MM.yyyy");
                res = formattedDate;
            }

            return res;
        }


    }



}
