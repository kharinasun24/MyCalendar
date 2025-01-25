using System.Data;
using System.Globalization;
using System.Resources;
using System.Security.Principal;
using Nager.Holiday;


namespace MyCalendar
{

    public partial class Form1 : Form
    {

        ResourceManager resourceManager;

        public MonthCalendar monthCalendar;

        private DateDao dateDao;
        private LanguageDao languageDao;

        private List<Date> exceptions;

        private DataGridView dataGridViewAppointmentsOnClickedDay;
        private DataTable appointments;

        private System.Windows.Forms.Timer appointmentTimer;

        private Button showWeatherButton;

        private Label placeHolder;
        private Label pickDay;
        private Label pickClock;
        private TextBox location;
        private ComboBox languageComboBox;

        private List<PublicHoliday> holidays;

        int d; int m; int y;

        public Form1()
        {


            Size = new Size(900, 600);

            InitializeCulture();

            CreateTimer();

            CreateCalendar();

            CreateUIControls();

        }

        private void InitializeGrid()
        {

            dataGridViewAppointmentsOnClickedDay = new DataGridView();

            dataGridViewAppointmentsOnClickedDay.CellFormatting += DataGridViewAppointmentsOnClickedDay_CellFormatting;

            dataGridViewAppointmentsOnClickedDay.Location = new System.Drawing.Point(250, 10);
            dataGridViewAppointmentsOnClickedDay.Size = new System.Drawing.Size(600, 250);

            dataGridViewAppointmentsOnClickedDay.ScrollBars = ScrollBars.Vertical;
            dataGridViewAppointmentsOnClickedDay.AllowUserToAddRows = false;
            dataGridViewAppointmentsOnClickedDay.ReadOnly = true;
            dataGridViewAppointmentsOnClickedDay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridViewAppointmentsOnClickedDay.CellClick += DataGridView_CellClick;

            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "id";
            column.Name = "id";
            dataGridViewAppointmentsOnClickedDay.Columns.Add(column);
            dataGridViewAppointmentsOnClickedDay.Columns["id"].Visible = false;

            column = new DataGridViewTextBoxColumn();
            column.Name = "xColumn";
            column.HeaderText = "";
            dataGridViewAppointmentsOnClickedDay.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.HeaderText = resourceManager.GetString("Appointment");
            column.DataPropertyName = "text";
            column.Name = "text";
            dataGridViewAppointmentsOnClickedDay.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "start";
            column.Name = "start";
            dataGridViewAppointmentsOnClickedDay.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "end";
            column.Name = "end";
            dataGridViewAppointmentsOnClickedDay.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.HeaderText = resourceManager.GetString("repetition");
            column.DataPropertyName = "repeat";
            column.Name = "repeat";
            dataGridViewAppointmentsOnClickedDay.Columns.Add(column);

            dataGridViewAppointmentsOnClickedDay.RowsAdded += new DataGridViewRowsAddedEventHandler(DataGridView_RowsAdded);

            Controls.Add(dataGridViewAppointmentsOnClickedDay);

        }


        private void CreateUIControls()
        {

            pickDay = new Label
            {
                Location = new Point(10, 15),
                AutoSize = true,
                Text = resourceManager.GetString("Pick day and below hour")

            };

            Controls.Add(pickDay);

            Button addButton = new System.Windows.Forms.Button
            {
                Text = "⌚",
                Location = new Point(10, 250),
                AutoSize = true,
                Size = new Size(100, 50)
            };

            addButton.Click += AddButton_Click;

            Controls.Add(addButton);

            Button addContactButton = new Button
            {
                Text = "👨‍👩‍👧",
                Location = new Point(120, 250),
                AutoSize = true,
                Size = new Size(100, 50)
            };

            addContactButton.Click += AddContactButton_Click;

            Controls.Add(addContactButton);


            Button readIcsButton = new Button
            {
                Text = "ICS", 
                Location = new Point(10, 320),
                AutoSize = true,
                Size = new Size(100, 50)
            };

            readIcsButton.Click += ReadIcsButton_Click;

            Controls.Add(readIcsButton);


            languageComboBox = new System.Windows.Forms.ComboBox();
            languageComboBox.Items.Add("Deutsch");
            languageComboBox.Items.Add("English");
            languageComboBox.Items.Add("magyar");
            languageComboBox.Items.Add("русский");


            string lang = languageDao.GetCurrentLanguage();

            int index;

            switch (lang)
            {
                case "English":
                    index = 1;
                    break;

                case "magyar":
                    index = 2;
                    break;

                case "русский":
                    index = 3;
                    break;

                default:
                    index = 0;
                    break;
            }


            languageComboBox.SelectedIndex = index;
            //languageComboBox.Location = new Point(120, 335);

            // Handle selection change
            languageComboBox.SelectedIndexChanged += (sender, e) =>
            {

                string language = languageComboBox.SelectedItem.ToString();

                languageDao.SetLanguage(language);

                Application.Restart();
                Environment.Exit(0);

            };

            //Controls.Add(languageComboBox);

            FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
            {
                Location = new Point(10, 440),
                Size = new Size(200, 50),  // Größe des Panels
                FlowDirection = FlowDirection.LeftToRight,  // Steuerelemente nebeneinander anordnen
                AutoSize = true
            };

            Button deleteAllContactsButton = new System.Windows.Forms.Button
            {
                Text = "delete all contacts",
                Location = new Point(10, 440),
                AutoSize = true
            };

            deleteAllContactsButton.Click += DeleteAllContactsButton_Click;

            //Controls.Add(deleteAllContactsButton);

            Button deleteAllDatesButton = new System.Windows.Forms.Button
            {
                Text = "delete all dates",
                Location = new Point(160, 440),
                AutoSize = true
            };

            deleteAllDatesButton.Click += DeleteAllDatesButton_Click;

            //Controls.Add(deleteAllDatesButton);

            flowLayoutPanel2.Controls.Add(deleteAllContactsButton);

            flowLayoutPanel2.Controls.Add(deleteAllDatesButton);

            Controls.Add(flowLayoutPanel2);


            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
            {
                Location = new Point(10, 390),
                Size = new Size(200, 100),  // Größe des Panels
                FlowDirection = FlowDirection.LeftToRight,  // Steuerelemente nebeneinander anordnen
                AutoSize = true
            };

            // Erstelle den Button "showWeatherButton"
            showWeatherButton = new Button
            {
                Text = "  🌦  ",
                AutoSize = true
            };
            showWeatherButton.Click += ShowWeatherButton_Click;

            // Füge den Button zum FlowLayoutPanel hinzu
            flowLayoutPanel.Controls.Add(showWeatherButton);

            // Erstelle das Textfeld "location"
            location = new TextBox
            {
                AutoSize = true
            };

            location.KeyDown += new KeyEventHandler(ShowWeatherLocation_KeyDown);

            // Füge das Textfeld zum FlowLayoutPanel hinzu
            flowLayoutPanel.Controls.Add(location);

            flowLayoutPanel.Controls.Add(languageComboBox);


            // Füge das FlowLayoutPanel zum Formular hinzu
            Controls.Add(flowLayoutPanel);


            Button xxXXButton = new System.Windows.Forms.Button
            {
                Text = " - 👥 - ",
                Location = new Point(120, 320),
                AutoSize = true
            };

            xxXXButton.Click += ctButton_Click;

            Controls.Add(xxXXButton);


        }





        private async Task CreateCalendar(int year, int month, int day)
        {
            holidays = await LoadHolidaysAsync();

            // Entfernen der alten Kalender-Labels
            RemoveOldCalendarLabels();
            List<KeyValuePair<string, DateTime>> appointmentsToIDsDict = StringValidators.Instance.AppointmentsToDateTimeDict(day, month, year, appointments);

            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            DateTime givenDate;
            // Labels für die Tage des Monats erstellen mit
            string test = "";
            string testHolidayToAdd = "";
            for (int d = 1; d <= daysInMonth; d++)
            {
                givenDate = new DateTime(year, month, d);

                // Überprüfen, ob das Datum in der Liste vorhanden ist
                //bool dateExists = appointmentsToIDsDict.Values.Contains(givenDate);
                bool dateExists = appointmentsToIDsDict.Any(kvp => kvp.Value == givenDate);


                bool isNotInExceptions = StringValidators.Instance.IsNotInExceptionsMethod(appointmentsToIDsDict, givenDate, year, month, d);

                DateTime currentDateCal = DateTime.Now.Date;

                ToolTip boldedOrHoliDayToolTip = new ToolTip();

                if (dateExists && isNotInExceptions)
                {
                    //Hier den Namen des Feiertages herausfinden und setzen.
                    bool isHoliday = holidays?.Any(h => h.Date.Date == givenDate.Date) ?? false;
                    Color backgroundColor = givenDate.Date == currentDateCal ? Color.LightBlue : (isHoliday ? Color.Red : Color.White);



                    Label dayLabel = new Label
                    {
                        Text = d.ToString() + " " + StringValidators.Instance.DayName(givenDate.DayOfWeek.ToString().Substring(0, 2)),
                        Width = 40,
                        Height = 40,
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                        BorderStyle = BorderStyle.FixedSingle,
                        Location = new System.Drawing.Point(40 * ((startDayOfWeek + d - 1) % 7) + 550, 40 * ((startDayOfWeek + d - 1) / 7) + 265),
                        Tag = "calendar", // Setze ein spezifisches Tag für Kalender-Labels
                        BackColor = backgroundColor,
                        Font = new System.Drawing.Font("Arial", 8, FontStyle.Bold) // Setzt den Text auf fett
                    };



                    //if (appointmentsToIDsDict.Values.Contains(givenDate)) 
                    if (appointmentsToIDsDict.Any(kvp => kvp.Value == givenDate))
                    {

                        appointments = dateDao.GetDatesFor(day, month, year);



                        int selectedMonth = monthCalendar.SelectionStart.Month;
                        int selectedYear = monthCalendar.SelectionStart.Year;
                        StringValidators.Instance.GetMonthsAppointments(selectedMonth, selectedYear, appointments, exceptions);

                        boldedOrHoliDayToolTip.ToolTipIcon = ToolTipIcon.None;
                        boldedOrHoliDayToolTip.IsBalloon = true;
                        boldedOrHoliDayToolTip.ShowAlways = true;

                        var od = appointments.Rows;
                        foreach (DataRow row in appointments.Rows)
                        {
                            DateTime dateStart = DateTime.ParseExact(row.Field<string>("start").Split(' ')[0], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                            DateTime dateToolTip = DateTime.ParseExact(givenDate.ToString().Split(' ')[0], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                            DateTime dateEnd = DateTime.ParseExact(row.Field<string>("end").Split(' ')[0], "dd.MM.yyyy", CultureInfo.InvariantCulture);


                            DateTime adjustedDateStart = new DateTime(selectedYear, selectedMonth, dateStart.Day, dateStart.Hour, dateStart.Minute, dateStart.Second);
                            DateTime adjustedDateEnd = new DateTime(selectedYear, selectedMonth, dateEnd.Day, dateEnd.Hour, dateEnd.Minute, dateEnd.Second);

                            // Überprüfen, ob das mittlere Datum zwischen dem ersten und letzten liegt
                            if (adjustedDateStart <= dateToolTip && dateToolTip <= adjustedDateStart)
                            {
                                test += row.Field<string>("text");

                                test += ", ";

                            }


                            bool isDifferentdays = adjustedDateStart.Day != adjustedDateEnd.Day;

                            if (isDifferentdays && adjustedDateEnd >= dateToolTip && dateToolTip >= adjustedDateEnd)
                            {
                                test += row.Field<string>("text");

                                test += ", ";

                            }



                            if (dayLabel.BackColor == Color.Red)
                            {
                                var holiday = holidays.FirstOrDefault(h => h.Date == dateToolTip);
                                testHolidayToAdd = " " + holiday.LocalName;
                            }

                        }

                        boldedOrHoliDayToolTip.SetToolTip(dayLabel, $"{test}{testHolidayToAdd}");

                        test = ""; testHolidayToAdd = "";
                    }

                    Controls.Add(dayLabel);

                }


                else
                {

                    bool isHoliday = holidays?.Any(h => h.Date.Date == givenDate.Date) ?? false;
                    Color backgroundColor = givenDate.Date == currentDateCal ? Color.LightBlue : (isHoliday ? Color.Red : SystemColors.Control);

                    boldedOrHoliDayToolTip.ToolTipIcon = ToolTipIcon.None;
                    boldedOrHoliDayToolTip.IsBalloon = true;
                    boldedOrHoliDayToolTip.ShowAlways = true;

                    DateTime dateToolTip = DateTime.ParseExact(givenDate.ToString().Split(' ')[0], "dd.MM.yyyy", CultureInfo.InvariantCulture);

                    Label dayLabel = new Label
                    {
                        Text = d.ToString() + " " + StringValidators.Instance.DayName(givenDate.DayOfWeek.ToString().Substring(0, 2)),
                        Width = 40,
                        Height = 40,
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                        BorderStyle = BorderStyle.FixedSingle,
                        Location = new System.Drawing.Point(40 * ((startDayOfWeek + d - 1) % 7) + 550, 40 * ((startDayOfWeek + d - 1) / 7) + 265),
                        Tag = "calendar", // Setze ein spezifisches Tag für Kalender-Labels
                        BackColor = backgroundColor,
                        Font = new System.Drawing.Font("Arial", 8)
                    };

                    if (dayLabel.BackColor == Color.Red)
                    {
                        var holiday = holidays.FirstOrDefault(h => h.Date == dateToolTip);
                        testHolidayToAdd = holiday.LocalName;

                        boldedOrHoliDayToolTip.SetToolTip(dayLabel, $"{testHolidayToAdd}");
                        testHolidayToAdd = "";
                    }

                    Controls.Add(dayLabel);

                }
            }
        }


        private void CreateCalendar()
        {

            //Kalender
            monthCalendar = new MonthCalendar
            {
                CalendarDimensions = new Size(1, 1),
                Location = new Point(10, 40)
            };

            monthCalendar.DateChanged += MonthCalendar_DateChanged;

            d = monthCalendar.SelectionStart.Day; m = monthCalendar.SelectionStart.Month; y = monthCalendar.SelectionStart.Year;

            Controls.Add(monthCalendar);

        }


        private void CreateTimer()
        {
            appointmentTimer = new System.Windows.Forms.Timer();
            appointmentTimer.Interval = 60000; // Check every minute
            appointmentTimer.Tick += AppointmentTimer_Tick;
            appointmentTimer.Start();
        }


        private void CreatePlaceHolder(string text)
        {
            Controls.Remove(placeHolder);

            placeHolder = new Label
            {
                Location = new Point(10, 500),
                Size = new Size(350, 30),
                Text = text,
                //AutoSize = true,
                BackColor = Color.LightBlue
            };

            placeHolder.Click += Placeholder_Click;

            Controls.Add(placeHolder);
        }




        private void UpdateAppointments(int day, int month, int year)
        {
            appointments = dateDao.GetDatesFor(day, month, year);



            exceptions = UpdateExceptions(exceptions);

            StringValidators.Instance.GetMonthsAppointments(monthCalendar.SelectionStart.Month, monthCalendar.SelectionStart.Year, appointments, exceptions);



            dataGridViewAppointmentsOnClickedDay.DataSource = appointments;
        }

        private List<Date> UpdateExceptions(List<Date> exceptions)
        {

            exceptions = dateDao.GetExceptions();

            return exceptions;
        }

        public void DrawAppointmentsOnClickedDay(int day, int month, int year, DateDao dateDao)
        {

            UpdateAppointments(day, month, year);

            LoadHolidays();

            CreateCalendar(year, month, day);

        }


        private async Task<List<PublicHoliday>> LoadHolidaysAsync()
        {

            string currentCulture = Thread.CurrentThread.CurrentCulture.Name;
            string[] cultureParts = currentCulture.Split('-');

            string region = cultureParts.Length > 1 ? cultureParts[1] : "DE";

            var holidayClient = new HolidayClient();
            try
            {
                PublicHoliday[] phs = await holidayClient.GetHolidaysAsync(monthCalendar.SelectionStart.Year, region);

                holidays = phs.ToList();

            }
            catch (Exception ex)
            {
                //MessageBox.Show("Fehler beim Abrufen der Feiertage: " + ex.Message);
                //SimpleLogger.Instance.Log("Fehler beim Abrufen der Feiertage: " + ex.Message);
            }
            return holidays;
        }


        // Methode zum Entfernen der alten Kalender-Labels
        private void RemoveOldCalendarLabels()
        {
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                if (Controls[i] is Label label && label.Tag?.ToString() == "calendar")
                {
                    Controls.Remove(label);
                }
            }
        }



        private async void LoadHolidays()
        {

            string currentCulture = Thread.CurrentThread.CurrentCulture.Name;
            string[] cultureParts = currentCulture.Split('-');

            string region = cultureParts.Length > 1 ? cultureParts[1] : "DE";


            var holidayClient = new HolidayClient();
            try
            {

                PublicHoliday[] phs = await holidayClient.GetHolidaysAsync(monthCalendar.SelectionStart.Year, region);

                holidays = phs.ToList();

            }
            catch (Exception ex)
            {
                //MessageBox.Show("Fehler beim Abrufen der Feiertage: " + ex.Message);
                //SimpleLogger.Instance.Log("Fehler beim Abrufen der Feiertage: " + ex.Message);
            }
        }



        private void InitializeCulture()
        {

            dateDao = new DateDao();

            exceptions = dateDao.GetExceptions();

            languageDao = new LanguageDao();

            resourceManager = new ResourceManager("MyCalendar.Resources.ResXFile", typeof(Form1).Assembly);

            string language = languageDao.GetCurrentLanguage();

            string culture = "";
            switch (language)
            {
                case "English":
                    culture = "en-GB";
                    break;

                case "magyar":
                    culture = "hu-HU";
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


        /// ///////////// EVENT HANDLER /// //////////////////////////////////////////


        private void ctButton_Click(object sender, EventArgs e)
        {

            ChatForm ctForm = new ChatForm();
            ctForm.ShowDialog();


        }


        private void MonthCalendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            DrawAppointmentsOnClickedDay(monthCalendar.SelectionStart.Day, monthCalendar.SelectionStart.Month, monthCalendar.SelectionStart.Year, dateDao);
        }


        private void AppointmentTimer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;


            DataTable appointments = dateDao.GetDatesFor(d, m, y);



            foreach (DataRow row in appointments.Rows)
            {
                DateTime appointmentStart = DateTime.Parse(row["start"].ToString());
                int notificationLeadTime = 10; // In this case, 5 means five minutes, dude...

                if (appointmentStart > now && appointmentStart.Subtract(now).TotalMinutes <= notificationLeadTime)
                {
                    //System.Media.SystemSounds.Asterisk.Play();
                    System.Media.SystemSounds.Hand.Play();
                    //System.Media.SystemSounds.Beep.Play();

                    string appointmentText = row["text"].ToString();
                    CreatePlaceHolder(appointmentText + " " + "beginnt in 10 Minuten");

                    // Erinnerung:
                    if (this.WindowState == FormWindowState.Minimized)
                    {
                        this.WindowState = FormWindowState.Normal;
                    }

                    this.Activate();


                }
            }
        }



        private void AddContactButton_Click(object sender, EventArgs e)
        {
            ContactsForm contactsForm = new ContactsForm();
            contactsForm.ShowDialog();
        }

        private void ReadIcsButton_Click(object sender, EventArgs e)
        {
            Reader icsReader = new Reader();

            icsReader.ReadICS();

            //TODO: BUG! Beim Einlesen der Datei erscheint der neue Termin nicht. 
            //Erst wenn der Kalender neu gestartet wird. Dies ist kein Zustand. Restart ist erstmal eine
            //Behelfslösung...
            Application.Restart();
            Environment.Exit(0);

        }


        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == dataGridViewAppointmentsOnClickedDay.Columns["xColumn"].Index)
                {

                    // Zugriff auf die aktuelle Reihe
                    DataGridViewRow row = ((DataGridView)sender).Rows[e.RowIndex];


                    string idToDelete = row.Cells["id"].Value.ToString();

                    string whichRepetition = row.Cells["repeat"].Value.ToString();



                    if ("y".Equals(whichRepetition) || "m".Equals(whichRepetition))
                    {

                        // Abfrage: Serie oder einzelnes Ereignis löschen
                        string askDelete = resourceManager.GetString("Do you want to delete the entire series (yes) or just this individual event (no)?");
                        string confirm = resourceManager.GetString("Confirm deletion");

                        DialogResult result = MessageBox.Show(
                            askDelete,
                            confirm,
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button3);

                        // Entscheidung anhand der Auswahl des Benutzers
                        if (result == DialogResult.Yes)
                        {
                            // Ganze Serie löschen
                            dateDao.DeleteEntryById(idToDelete);

                            DrawAppointmentsOnClickedDay(d, m, y, dateDao);

                        }
                        else if (result == DialogResult.No)
                        {
                            string exception_start = row.Cells["start"].Value.ToString();
                            DateTime selectedDate = monthCalendar.SelectionStart;

                            string[] dateParts = exception_start.Split('.');
                            int day = Convert.ToInt32(dateParts[0]);
                            int month = Convert.ToInt32(dateParts[1]);
                            int year = Convert.ToInt32(dateParts[2].Substring(0, 4));

                            DateTime newDateTime = new DateTime(selectedDate.Year, selectedDate.Month, day);
                            string formattedDate = newDateTime.ToString("dd.MM.yyyy");

                            exception_start = formattedDate;

                            string exception_end = row.Cells["end"].Value.ToString();
                            selectedDate = monthCalendar.SelectionStart;

                            dateParts = exception_end.Split('.');
                            day = Convert.ToInt32(dateParts[0]);
                            month = Convert.ToInt32(dateParts[1]);
                            year = Convert.ToInt32(dateParts[2].Substring(0, 4));

                            newDateTime = new DateTime(selectedDate.Year, selectedDate.Month, day);
                            formattedDate = newDateTime.ToString("dd.MM.yyyy");

                            exception_end = formattedDate;

                            dateDao.WriteExceptionIntoExceptionTBL(idToDelete, exception_start, exception_end);

                            DrawAppointmentsOnClickedDay(d, m, y, dateDao);


                            //Application.Restart();
                            //Environment.Exit(0);

                        }


                    }
                    else
                    {

                        DialogResult result = MessageBox.Show(resourceManager.GetString("Do you really want to delete date?"), resourceManager.GetString("Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {

                            dateDao.DeleteEntryById(idToDelete);
                            int d = monthCalendar.SelectionStart.Day;
                            int m = monthCalendar.SelectionStart.Month;
                            int y = monthCalendar.SelectionStart.Year;

                            DrawAppointmentsOnClickedDay(d, m, y, dateDao);

                        }
                    }
                }
                else
                {

                    // Zugriff auf die aktuelle Reihe
                    DataGridViewRow row = ((DataGridView)sender).Rows[e.RowIndex];
                    string id = row.Cells["id"].Value.ToString();

                    DetailsForm detailsForm = new DetailsForm(id, this);
                    detailsForm.ShowDialog();

                }
            }
        }


        private void ShowWeatherLocation_KeyDown(object sender, KeyEventArgs e)
        {
            // Prüfen, ob die Enter-Taste gedrückt wurde
            if (e.KeyCode == Keys.Enter)
            {
                HandleWeather();

                e.SuppressKeyPress = true; // Verhindert, dass das Drücken der Enter-Taste einen Ton verursacht
            }
        }
        private void ShowWeatherButton_Click(object? sender, EventArgs e)
        {
            HandleWeather();
        }

        private void HandleWeather()
        {
            string loc = location.Text;

            if (!string.IsNullOrEmpty(loc))
            {

                WeatherForm weatherForm = new WeatherForm(loc);
                weatherForm.ShowDialog();

            }
            else
            {

                MessageBox.Show(resourceManager.GetString("Enter a town to look up its weather data!"));

            }
        }

        private void Placeholder_Click(object? sender, EventArgs e)
        {

            placeHolder.Text = "";
        }

        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {
                // Setzen des Wertes "x" für jede neue Zeile in der "xColumn" Spalte
                dataGridViewAppointmentsOnClickedDay.Rows[i].Cells["xColumn"].Value = "x";
            }
        }


        private void AddButton_Click(object sender, EventArgs e)
        {

            AppointmentAddForm appointmentAddForm = new AppointmentAddForm(
                monthCalendar, this);

            appointmentAddForm.ShowDialog();
        }

        private void DeleteAllDatesButton_Click(object? sender, EventArgs e)
        {
            bool isAdmin = false;

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (isAdmin)
            {

                ContactDao contactDao = new ContactDao();

                contactDao.DeleteAllDates();

            }
            else
            {

                MessageBox.Show("NO ADMIN MODE!");

            }
        }

        private void DeleteAllContactsButton_Click(object? sender, EventArgs e)
        {
            bool isAdmin = false;

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (isAdmin)
            {

                ContactDao contactDao = new ContactDao();

                contactDao.DeleteAllContacts();

            }
            else
            {

                MessageBox.Show("NO ADMIN MODE!");

            }
        }



        protected override async void OnLoad(EventArgs e)
        {
            InitializeGrid();

            UpdateAppointments(monthCalendar.SelectionStart.Day, monthCalendar.SelectionStart.Month, monthCalendar.SelectionStart.Year);

            holidays = await LoadHolidaysAsync();

            CreateCalendar(monthCalendar.SelectionStart.Year, monthCalendar.SelectionStart.Month, monthCalendar.SelectionStart.Day);

        }



        private void DataGridViewAppointmentsOnClickedDay_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Überprüfen, ob es sich um die Spalten "start" oder "end" handelt
            if (dataGridViewAppointmentsOnClickedDay.Columns[e.ColumnIndex].Name == "start" ||
                dataGridViewAppointmentsOnClickedDay.Columns[e.ColumnIndex].Name == "end")
            {
                // Hole das Startdatum aus der Zelle
                DateTime startDate = DateTime.ParseExact(dataGridViewAppointmentsOnClickedDay.Rows[e.RowIndex].Cells["start"].Value.ToString(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                // Hole das Enddatum aus der Zelle
                DateTime endDate = DateTime.ParseExact(dataGridViewAppointmentsOnClickedDay.Rows[e.RowIndex].Cells["end"].Value.ToString(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                // Vergleiche die Daten mit dem aktuellen Datum
                if (startDate.Date <= DateTime.Now.Date && endDate.Date >= DateTime.Now.Date)
                {
                    // Setze die Hintergrundfarbe der gesamten Zeile auf Hellblau
                    dataGridViewAppointmentsOnClickedDay.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                }
            }
        }



    }
}
