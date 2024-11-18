using MyCalendar.logger;
using MyCalendar;
using NodaTime;
using System.Globalization;
using System.Resources;


namespace MyCalendar
{
    public class Date : IComparable<Date>
    {

        public string id;
        private string text;
        public string start;
        public string end;
        private string duration;
        private string repeat;

        private string formattedDay; private string formattedMonth; private string formattedYear;

        List<DateTime> startDateTimes;

        ResourceManager resourceManager;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }


        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public string Start
        {
            get { return start; }
            set { start = value; }
        }

        public string End
        {
            get { return end; }
            set { end = value; }
        }

        public string Duration
        {
            get { return duration; }
            set { duration = value; }
        }


        public string Repeat
        {
            get { return repeat; }
            set { repeat = value; }
        }





        public Date(string id, string text, string start, string end, string duration, string repeat)
        {

            resourceManager = new ResourceManager("MyCalendar.Resources.ResXFile", typeof(Form1).Assembly);
            CultureInfo ci = new CultureInfo("de-DE");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            this.id = id;

            if (String.IsNullOrEmpty(text))
            {
                text = resourceManager.GetString("My Event");
            }

            this.text = text;
            this.start = start;
            this.end = end;
            this.duration = duration;
            this.repeat = repeat;


            startDateTimes = new List<DateTime>();

        }



        public int CompareTo(Date other)
        {
            if (other == null)
            {
                return 1;
            }

            return DateTime.ParseExact(start, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture).CompareTo(DateTime.ParseExact(other.Start, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture));
        }

        public List<DateTime> GetStartDatTimes()
        {

            return this.startDateTimes;
        }

        public string GetDateClickedByUser()
        {

            return this.formattedDay + "." + this.formattedMonth + "." + this.formattedYear;
        }

        public void SetDayMonthYearClickedByUser(string formattedDay, string formattedMonth, string formattedYear)
        {
            this.formattedDay = formattedDay;
            this.formattedMonth = formattedMonth;
            this.formattedYear = formattedYear;


        }

        public void ConfigureDate()
        {
            if ("y".Equals(this.repeat))
            {

                string startHHmm = start.Split(' ')[1];

                DateTime startDate = DateTime.Parse(start);
                DateTime clickedDate = DateTime.Parse(GetDateClickedByUser());

                // Berechne die Differenz in Jahren
                int yearsDifference = clickedDate.Year - startDate.Year;

                // Berücksichtige die Monate, falls das Jahr gleich ist und der Monat des zweiten Datums kleiner ist
                if (clickedDate.Year == startDate.Year && clickedDate.Month < startDate.Month)
                {
                    yearsDifference--;
                }

                LocalDate startDateLD = new LocalDate(startDate.Year, startDate.Month, startDate.Day);

                // Verwende PlusYears, um die Jahresdifferenz hinzuzufügen
                LocalDate recurringDate = startDateLD.PlusYears(yearsDifference);

                DateTime startDateTimeToWorkWith = recurringDate.ToDateTimeUnspecified();


                startDateTimeToWorkWith = startDateTimeToWorkWith.AddHours(Convert.ToInt32(startHHmm.Split(':')[0])).AddMinutes(Convert.ToInt32(startHHmm.Split(':')[0]));

                FillInTheLists(startDateTimeToWorkWith);

            }
            else if ("m".Equals(this.repeat))
            {
                string startHHmm = start.Split(' ')[1];

                DateTime startDate = DateTime.Parse(start);
                DateTime clickedDate = DateTime.Parse(GetDateClickedByUser());

                int monthsDifference = ((clickedDate.Year - startDate.Year) * 12) + clickedDate.Month - startDate.Month;

                // Falls der Tag des zweiten Datums kleiner ist als der Tag des ersten Datums, einen Monat abziehen
                if (clickedDate.Day < startDate.Day)
                {
                    monthsDifference--;
                }

                LocalDate startDateLD = new LocalDate(startDate.Year, startDate.Month, startDate.Day);

                LocalDate recurringDate = startDateLD.PlusMonths(monthsDifference);

                DateTime startDateTimeToWorkWith = recurringDate.ToDateTimeUnspecified();

                startDateTimeToWorkWith = startDateTimeToWorkWith.AddHours(Convert.ToInt32(startHHmm.Split(':')[0])).AddMinutes(Convert.ToInt32(startHHmm.Split(':')[0]));


                FillInTheLists(startDateTimeToWorkWith);
            }

            else
            {

                FillInTheLists(DateTime.Parse(start));
            }
        }

        //Jeder EndTermin am ende einer Reihenfolge ist quasi auch ein startTermin... 
        private void FillInTheLists(DateTime startDateTimeToWorkWith)
        {


            // Die Dauer analysieren in Tagen
            int days = Convert.ToInt32(this.duration);

            // Dauer zu den DateTimes hinzufügen
            for (int i = 0; i < days; i++)
            {
                startDateTimes.Add(startDateTimeToWorkWith.AddDays(i));
            }



        }


        public DateTime GetStartAsDateTime()
        {

            return DateTime.ParseExact(start, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

        }

        public DateTime GetEndAsDateTime()
        {

            return DateTime.ParseExact(end, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

        }


        public override string ToString()
        {
            return $"Id: {Id}, Text: {Text}, Start: {Start}, End: {End}, Repeat: {Repeat}";
        }



        public bool IsBetween(Date other)
        {

            // Überprüfen, ob das übergebene Datum null ist
            if (other == null)
            {
                return false;
            }

            // Datum und Uhrzeit in das Start- und Enddatum umwandeln
            DateTime thisStartDate = DateTime.ParseExact(start, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture).Date;
            DateTime thisEndDate = DateTime.ParseExact(end, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture).Date;

            DateTime otherStartDate = DateTime.ParseExact(other.Start, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture).Date;

            // Falls Repeat aktiviert ist, passe das Datum entsprechend an
            if ("y".Equals(this.Repeat)) // Jährliche Wiederholung
            {
                // Wiederhole das Datum auf das Jahr des geklickten Objekts
                int yearsDifference = otherStartDate.Year - thisStartDate.Year;
                DateTime repeatedStartDate = thisStartDate.AddYears(yearsDifference);
                DateTime repeatedEndDate = thisEndDate.AddYears(yearsDifference);

                return repeatedStartDate.Year == otherStartDate.Year && repeatedStartDate.Month == otherStartDate.Month;
            }
            else if ("m".Equals(this.Repeat)) // Monatliche Wiederholung
            {
                // Wiederhole das Datum auf den Monat des geklickten Objekts
                int monthsDifference = ((otherStartDate.Year - thisStartDate.Year) * 12) + otherStartDate.Month - thisStartDate.Month;
                DateTime repeatedStartDate = thisStartDate.AddMonths(monthsDifference);
                DateTime repeatedEndDate = thisEndDate.AddMonths(monthsDifference);

                return repeatedStartDate.Year == otherStartDate.Year && repeatedStartDate.Month == otherStartDate.Month;
            }
            else // Keine Wiederholung
            {
                // Nur Monat und Jahr vergleichen, Tag wird ignoriert
                return thisStartDate.Year == otherStartDate.Year && thisStartDate.Month == otherStartDate.Month;
            }
        }


    }


}
