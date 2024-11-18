using Matrix.Xmpp.HttpUpload;
using MyCalendar;
using System.Globalization;
using System.Resources;

namespace MyCalendar
{
    public partial class AppointmentAddForm : Form
    {
        ResourceManager resourceManager;

        private Form1 form1;

        private DateDao dateDao;
        private LanguageDao languageDao;

        private DateTimePicker dateTimePickerStart;
        private Label labelStart;
        private TextBox textBox1;
        private Button saveBtn;
        private Button closeBtn;

        private CheckBox checkBoxMonthly;
        private CheckBox checkBoxYearly;
        private CheckBox checkBoxWeekly;
        private CheckBox checkBoxWholeDay;

        private DateTimePicker dateTimePickerEnd;
        private Label labelEnd;

        private int dy; private int m; private int y;

        private Label labelDt;

        public AppointmentAddForm(MonthCalendar monthCalendar, Form1 f1)
        {

            form1 = f1;

            int day = monthCalendar.SelectionStart.Day;
            int month = monthCalendar.SelectionStart.Month;
            int year = monthCalendar.SelectionStart.Year;

            dy = monthCalendar.SelectionStart.Day;
            m = monthCalendar.SelectionStart.Month;
            y = monthCalendar.SelectionStart.Year;


            InitializeCulture();

            // CheckBoxes erstellen und Eigenschaften setzen
            checkBoxMonthly = new System.Windows.Forms.CheckBox();
            checkBoxMonthly.Text = resourceManager.GetString("monthly");
            checkBoxMonthly.Location = new Point(460, 10);
            checkBoxMonthly.AutoSize = true;
            checkBoxMonthly.CheckedChanged += checkBox_CheckedChanged;

            checkBoxYearly = new System.Windows.Forms.CheckBox();
            checkBoxYearly.Text = resourceManager.GetString("yearly");
            checkBoxYearly.Location = new Point(460, 50);
            checkBoxYearly.AutoSize = true;
            checkBoxYearly.CheckedChanged += checkBox_CheckedChanged;

            checkBoxWeekly = new System.Windows.Forms.CheckBox();
            checkBoxWeekly.Text = resourceManager.GetString("weekly individual til end of year");
            checkBoxWeekly.Location = new Point(460, 90);
            checkBoxWeekly.AutoSize = true;
            checkBoxWeekly.CheckedChanged += checkBox_CheckedChanged;


            checkBoxWholeDay = new System.Windows.Forms.CheckBox();
            checkBoxWholeDay.Text = resourceManager.GetString("whole day");
            checkBoxWholeDay.Location = new Point(460, 130);
            checkBoxWholeDay.AutoSize = true;
            checkBoxWholeDay.CheckedChanged += checkBox_CheckedChangedWholeDay;


            // CheckBoxes zur Form hinzufügen
            Controls.Add(checkBoxMonthly);
            Controls.Add(checkBoxYearly);
            Controls.Add(checkBoxWeekly);
            Controls.Add(checkBoxWholeDay);

            dateDao = new DateDao();

            Size = new Size(800, 500);

            textBox1 = new System.Windows.Forms.TextBox();
            textBox1.Location = new Point(10, 100);
            textBox1.Size = new Size(200, 10);
            Controls.Add(textBox1);

            labelDt = new System.Windows.Forms.Label();
            labelDt.Location = new Point(570, 10);
            labelDt.AutoSize = true;
            labelDt.Text = resourceManager.GetString("Start") + ": " + day.ToString("D2") + "." + month.ToString("D2") + "." + year.ToString("D4");
            Controls.Add(labelDt);


            labelStart = new System.Windows.Forms.Label();
            labelStart.Location = new Point(10, 10);
            labelStart.Text = resourceManager.GetString("Start");
            Controls.Add(labelStart);

            DateTime givenDate = new DateTime(year, month, day);

            DateTime currentDate = DateTime.Now;

            dateTimePickerStart = new DateTimePicker();
            dateTimePickerStart.CustomFormat = "HH:mm";
            dateTimePickerStart.Format = DateTimePickerFormat.Custom;
            dateTimePickerStart.ShowUpDown = false;
            dateTimePickerStart.Location = new Point(10, 40);


            dateTimePickerEnd = new DateTimePicker();
            dateTimePickerEnd.CustomFormat = "HH:mm";
            dateTimePickerEnd.Format = DateTimePickerFormat.Custom;
            dateTimePickerEnd.ShowUpDown = false;
            dateTimePickerEnd.Location = new Point(250, 40);

            SetDateTimePickersDate(day, month, year, givenDate, currentDate);

            dateTimePickerStart.ValueChanged += (sender, e) =>
            {
                labelDt.Text = resourceManager.GetString("Start") + ": " + dateTimePickerStart.Value.Day.ToString("D2") + "." + dateTimePickerStart.Value.Month.ToString("D2") + "." + dateTimePickerStart.Value.Year.ToString("D4");
                RoundedEntries();
            };


            Controls.Add(dateTimePickerStart);


            labelEnd = new System.Windows.Forms.Label();
            labelEnd.Location = new Point(250, 10);
            labelEnd.Text = resourceManager.GetString("End");
            Controls.Add(labelEnd);




            dateTimePickerEnd.ValueChanged += (sender, e) =>
            {
                RoundedEntries2();
            };

            Controls.Add(dateTimePickerEnd);


            saveBtn = new System.Windows.Forms.Button();
            saveBtn.Text = resourceManager.GetString("Save");
            saveBtn.Location = new Point(10, 300);
            saveBtn.AutoSize = true;
            saveBtn.Click += saveBtn_Click;
            Controls.Add(saveBtn);

            closeBtn = new System.Windows.Forms.Button();
            closeBtn.Text = resourceManager.GetString("Close");
            closeBtn.Location = new Point(100, 300);
            closeBtn.AutoSize = true;
            closeBtn.Click += closeBtn_Click;
            Controls.Add(closeBtn);

        }


        private void checkBox_CheckedChangedWholeDay(object? sender, EventArgs e)
        {

            DateTime newDateTime = new DateTime(dateTimePickerStart.Value.Year,
            dateTimePickerStart.Value.Month,
            dateTimePickerStart.Value.Day,
            0,
            0,
            0);

            dateTimePickerStart.Value = newDateTime;

            newDateTime = new DateTime(dateTimePickerEnd.Value.Year,
            dateTimePickerEnd.Value.Month,
            dateTimePickerEnd.Value.Day,
            23,
            59,
            59);

            dateTimePickerEnd.Value = newDateTime;

        }



        private void InitializeCulture()
        {


            languageDao = new LanguageDao();

            resourceManager = new ResourceManager("MyCalendar.Resources.ResXFile", typeof(Form1).Assembly);

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

                default:
                    culture = "de-DE";
                    break;
            }

            CultureInfo ci = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {

            // Alle Checkboxes in einer Liste (oder einem Array) speichern
            List<CheckBox> checkBoxes = new List<CheckBox> { checkBoxMonthly, checkBoxYearly, checkBoxWeekly };

            // Die angeklickte Checkbox ermitteln
            CheckBox clickedCheckBox = (CheckBox)sender;

            // Alle anderen Checkboxes deaktivieren
            foreach (CheckBox checkBox in checkBoxes)
            {
                if (checkBox != clickedCheckBox)
                {
                    checkBox.Checked = false;
                }
            }
        }


        private void closeBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {



            TimeSpan d = dateTimePickerEnd.Value - dateTimePickerStart.Value;
            int duration = d.Days;

            duration++;

            if (duration >= 1)
            {

                string start = dateTimePickerStart.Value.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                string end = dateTimePickerEnd.Value.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                DateTime startDate = DateTime.ParseExact(start, "dd.MM.yyyy HH:mm", null);
                DateTime endDate = DateTime.ParseExact(end, "dd.MM.yyyy HH:mm", null);

                if (startDate.Month != endDate.Month || startDate.Year != endDate.Year)
                {

                    MessageBox.Show(resourceManager.GetString("Multi-day appointments must not exceed the monthly limit."));

                    return;
                }

                string repeat = "n";

                if (checkBoxMonthly.Checked)
                {
                    repeat = "m";
                }
                else if (checkBoxYearly.Checked)
                {

                    repeat = "y";
                }
                else if (checkBoxWeekly.Checked)
                {
                    repeat = "w";
                }

                if (!"w".Equals(repeat))
                {

                    dateDao.saveDate(textBox1.Text, start, end, duration.ToString(), repeat);
                }
                else
                {
                    repeat = "n";


                    List<(DateTime Start, DateTime End)> appointments = GetWeeklyAppointments(startDate, endDate);

                    foreach (var appointment in appointments)
                    {
                        dateDao.saveDate(textBox1.Text, appointment.Start.ToString("dd.MM.yyyy HH:mm"), appointment.End.ToString("dd.MM.yyyy HH:mm"), duration.ToString(), repeat);
                    }

                }



                form1.DrawAppointmentsOnClickedDay(dy, m, y, dateDao);

                Close();
            }


        }

        private List<(DateTime Start, DateTime End)> GetWeeklyAppointments(DateTime startDate, DateTime endDate)
        {
            {
                List<(DateTime Start, DateTime End)> appointments = new List<(DateTime, DateTime)>();
                DateTime endOfYear = new DateTime(startDate.Year, 12, 31);

                // Beginne mit dem Start- und Enddatum
                DateTime nextStart = startDate;
                DateTime nextEnd = endDate;

                // Füge alle wöchentlichen Termine bis zum Jahresende hinzu
                while (nextStart <= endOfYear && nextEnd <= endOfYear)
                {
                    appointments.Add((nextStart, nextEnd));
                    nextStart = nextStart.AddDays(7); // Woche hinzufügen
                    nextEnd = nextEnd.AddDays(7); // Woche hinzufügen
                }

                return appointments;
            }
        }

        private void SetDateTimePickersDate(int day, int month, int year, DateTime givenDate, DateTime currentDate)
        {


            if (givenDate.Date == currentDate.Date)
            {
                dateTimePickerStart.Value = Rounded(currentDate);

                if (dateTimePickerStart.Value.Hour < 23)
                {
                    DateTime dt = dateTimePickerStart.Value.AddHours(1);
                    dateTimePickerEnd.Value = dt;
                }
            }
            else
            {
                dateTimePickerStart.Value = Rounded(new DateTime(year, month, day, 8, 0, 0));
                if (dateTimePickerStart.Value.Hour < 23)
                {
                    DateTime dt = dateTimePickerStart.Value.AddHours(1);
                    dateTimePickerEnd.Value = dt;
                }
            }

        }

        private DateTime Rounded(DateTime dateTime)
        {
            int minutes = dateTime.Minute;
            int remainder = minutes % 5;
            int minutesToAdd = remainder == 0 ? 0 : 5 - remainder;

            return dateTime.AddMinutes(minutesToAdd).AddSeconds(-dateTime.Second).AddMilliseconds(-dateTime.Millisecond);
        }

        private void RoundedEntries2()
        {
            DateTime value = dateTimePickerStart.Value;

            int minutes = value.Minute;

            string minutesString = minutes.ToString();
            int letzteZiffer = int.Parse(minutesString.Substring(minutesString.Length - 1));

            if (letzteZiffer > 5)
            {
                minutes += 5;
            }

            if (letzteZiffer != 5)
            {

                minutes = (minutes / 5) * 5;
            }

            if (minutes >= 60)
            {
                minutes = 0;
            }

            DateTime dt = new DateTime(value.Year, value.Month, value.Day, value.Hour, minutes, 0);

            dateTimePickerStart.Value = dt;

            if (dt.Hour < 23)
            {
                if (dt.Hour > dateTimePickerEnd.Value.Hour)
                {

                    DateTime newDateTime = new DateTime(dateTimePickerEnd.Value.Year,
                                         dateTimePickerEnd.Value.Month,
                                         dateTimePickerEnd.Value.Day,
                                         dt.Hour,
                                         dateTimePickerEnd.Value.Minute,
                                         dateTimePickerEnd.Value.Second);

                    dateTimePickerEnd.Value = newDateTime;

                }

            }
            else
            {

                if (dt.Minute > dateTimePickerEnd.Value.Minute)
                {

                    DateTime newDateTime = new DateTime(dateTimePickerEnd.Value.Year,
                             dateTimePickerEnd.Value.Month,
                             dateTimePickerEnd.Value.Day,
                             dateTimePickerEnd.Value.Hour,
                             dt.Minute,
                             dateTimePickerEnd.Value.Second);

                    dateTimePickerEnd.Value = newDateTime;

                }

            }

        }

        private void RoundedEntries()
        {
            DateTime value = dateTimePickerStart.Value;

            int minutes = value.Minute;

            string minutesString = minutes.ToString();
            int letzteZiffer = int.Parse(minutesString.Substring(minutesString.Length - 1));

            if (letzteZiffer > 5)
            {
                minutes += 5;
            }

            if (letzteZiffer != 5)
            {

                minutes = (minutes / 5) * 5;
            }

            if (minutes >= 60)
            {
                minutes = 0;
            }

            DateTime dt = new DateTime(value.Year, value.Month, value.Day, value.Hour, minutes, 0);

            dateTimePickerStart.Value = dt;

            if (dt.Hour < 23)
            {
                dt = dt.AddHours(1);

                DateTime newDateTime = new DateTime(dateTimePickerEnd.Value.Year,
                                     dateTimePickerEnd.Value.Month,
                                     dateTimePickerEnd.Value.Day,
                                     dt.Hour,
                                     dateTimePickerEnd.Value.Minute,
                                     dateTimePickerEnd.Value.Second);

                dateTimePickerEnd.Value = newDateTime;

            }
            else
            {

                if (dt.Minute > dateTimePickerEnd.Value.Minute)
                {

                    DateTime newDateTime = new DateTime(dateTimePickerEnd.Value.Year,
                             dateTimePickerEnd.Value.Month,
                             dateTimePickerEnd.Value.Day,
                             dt.Hour,
                             dt.Minute,
                             dateTimePickerEnd.Value.Second);

                    dateTimePickerEnd.Value = newDateTime;

                }
                else
                {

                    DateTime newDateTime = new DateTime(dateTimePickerEnd.Value.Year,
                             dateTimePickerEnd.Value.Month,
                             dateTimePickerEnd.Value.Day,
                             dt.Hour,
                             dateTimePickerEnd.Value.Minute,
                             dateTimePickerEnd.Value.Second);

                    dateTimePickerEnd.Value = newDateTime;

                }


            }

        }
    }
}
