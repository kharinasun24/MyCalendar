using Nager.Holiday;
using System.Data;
using System.Globalization;
using System.Resources;
using System.Security.Principal;


namespace MyCalendar
{

    public partial class Form1 : Form
    {

        ResourceManager resourceManager;

        CultureInfo ci;

        public MonthCalendar monthCalendar;

        private DateDao dateDao;
        private LanguageDao languageDao;

        //private string lastDataHash = "";

        private List<Date> exceptions;

        private HashSet<int> dismissedAppointments = new HashSet<int>();


        private DataGridView dataGridViewAppointmentsOnClickedDay;
        private DataTable appointments;

        private System.Windows.Forms.Timer appointmentTimer;

        private Button showWeatherButton;
        private Button btnShowAllAppointments;

        private ToolTip boldedOrHoliDayToolTip;

        private Label currentDateLabel;
        private Label placeHolder;
        private Label pickDay;
        private Label pickClock;

        private Panel calendarPanel;

        private TextBox location;
        private ComboBox languageComboBox;

        private static bool isOpen = false;

        private List<PublicHoliday> holidays;

        int d; int m; int y;

        public Form1()
        {


            Size = new Size(900, 600);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = true;

            InitializeCulture();

            CreateTimer();

            InitToolTip();

            CreateCalendar();

            CreateUIControls();

            InitializeShowAllButton();

        }

        private void InitToolTip()
        {
            boldedOrHoliDayToolTip = new ToolTip
            {
                OwnerDraw = true,
                InitialDelay = 100,
                ReshowDelay = 100,
                AutoPopDelay = 5000,
                ShowAlways = true
            };

            boldedOrHoliDayToolTip.Draw += (s, e) =>
            {
                e.Graphics.FillRectangle(Brushes.LightGreen, e.Bounds); // Hintergrund hellgrün
                e.Graphics.DrawRectangle(Pens.Green, e.Bounds);         // Rahmen
                TextRenderer.DrawText(
                    e.Graphics,
                    e.ToolTipText,
                    new Font("Segoe UI", 10, FontStyle.Bold),
                    e.Bounds,
                    Color.Black,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left
                );
            };

            boldedOrHoliDayToolTip.Popup += (s, e) =>
            {
                // Den Text des Tooltips für das aktuelle Control holen
                string text = boldedOrHoliDayToolTip.GetToolTip(e.AssociatedControl);

                using (Font f = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                    Size textSize = TextRenderer.MeasureText(text, f);

                    // Ein bisschen Padding hinzufügen
                    e.ToolTipSize = new Size(textSize.Width + 10, textSize.Height + 6);
                }
            };


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

        private void InitializeShowAllButton()
        {
            string showAllDates = resourceManager.GetString("Show all dates");

            btnShowAllAppointments = new Button
            {
                Text = showAllDates,
                Width = 250,
                Height = 30,
                Location = new Point(260, 320) // Position anpassen
            };
            btnShowAllAppointments.Click += BtnShowAllAppointments_Click;
            Controls.Add(btnShowAllAppointments);
        }

        private void BtnShowAllAppointments_Click(object sender, EventArgs e)
        {
            if (appointments != null)
            {
                dataGridViewAppointmentsOnClickedDay.DataSource = appointments;
            }
        }


        private void CreateUIControls()
        {
            currentDateLabel = new Label
            {
                Location = new Point(10, 10), // Position anpassen
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            Controls.Add(currentDateLabel);

            SetCurrentDateLabel();

            pickDay = new Label
            {
                Location = new Point(10, 15),
                AutoSize = true,
                Text = resourceManager.GetString("Pick day and below hour")

            };

            //Controls.Add(pickDay);

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
                Text = "import ICS", 
                Location = new Point(10, 320),
                AutoSize = true,
                Size = new Size(100, 50)
            };

            readIcsButton.Click += ReadIcsButton_Click;

            Controls.Add(readIcsButton);

            /////////////////////////////////////////////////////////////////////////////////////////

            Button writeIcsButton = new Button
            {
                Text = "export ICS",
                Location = new Point(120, 320),
                AutoSize = true,
                Size = new Size(100, 50)
            };

            writeIcsButton.Click += WriteIcsButton_Click;

            Controls.Add(writeIcsButton);

            /////////////////////////////////////////////////////////////////////////////////////////

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
        
            // Handle selection change
            languageComboBox.SelectedIndexChanged += (sender, e) =>
            {

                string language = languageComboBox.SelectedItem.ToString();

                languageDao.SetLanguage(language);

                Application.Restart();
                Environment.Exit(0);

            };

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

            Button deleteAllDatesButton = new System.Windows.Forms.Button
            {
                Text = "delete all dates",
                Location = new Point(160, 440),
                AutoSize = true
            };

            deleteAllDatesButton.Click += DeleteAllDatesButton_Click;

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

            // Erstelle das Textfeld "location"
            location = new TextBox
            {
                AutoSize = true
            };

            flowLayoutPanel.Controls.Add(languageComboBox);


            // Füge das FlowLayoutPanel zum Formular hinzu
            Controls.Add(flowLayoutPanel);


            Button chtButton = new System.Windows.Forms.Button
            {
                Text = " - 👥 - ",
                Location = new Point(240, 390),
                AutoSize = true
            };

            chtButton.Click += ctButton_Click;

            Controls.Add(chtButton);


        }

        private void SetCurrentDateLabel()
        {
            DateTime now = DateTime.Now;
            string dayOfWeekEnglish = now.ToString("dddd", CultureInfo.InvariantCulture); // Wochentag auf Englisch
        
            
            //currentDateLabel.Text = resourceManager.GetString("Today") + ": " + now.ToString("dd.MM.yyyy") + " (" + StringValidators.Instance.DayName(now.DayOfWeek.ToString().Substring(0, 2)) + ".)";


            currentDateLabel.Text = resourceManager.GetString("Today") + ": " + now.ToString("d", ci) + " (" + StringValidators.Instance.DayName(now.DayOfWeek.ToString().Substring(0, 2)) + ")";



        }

        //TODO: How to delete weekly appointments (till year's end) in one scoop by giving all creation dates a unique time stamp or hash? Why is the chat not working? Extend yearly and monthly appointments to 28 or 30 days?
        //As autumn rain starts pouring down, this work is next year's town.
        private string ComputeAppointmentsHash(DataTable appointments)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var sb = new System.Text.StringBuilder();

                foreach (DataRow row in appointments.Rows)
                {
                    foreach (DataColumn col in appointments.Columns)
                    {
                        var value = row[col] != null ? row[col].ToString() : string.Empty;
                        sb.Append(value);
                    }
                }

                var hashBytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
                return Convert.ToBase64String(hashBytes);
            }
        }

        
        private async Task CreateCalendar(int year, int month, int day)
        {
            // Panel nur einmal erstellen
            if (calendarPanel == null)
            {
                calendarPanel = new Panel
                {
                    Name = "calendarPanel",
                    Location = new Point(550, 265),
                    Size = new Size(7 * 40, 6 * 40), // 7 Tage, bis zu 6 Wochen
                    BorderStyle = BorderStyle.None,
                    AutoScroll = true
                };
                Controls.Add(calendarPanel);
            }

            // Daten laden
            holidays = await LoadHolidaysAsync();
            appointments = dateDao.GetDatesFor(day, month, year);

            //string currentHash = ComputeAppointmentsHash(appointments);
            //if (currentHash == lastDataHash)
            //    return; // Keine Änderungen, nichts tun

            this.SuspendLayout();
            calendarPanel.Controls.Clear(); // Alte Labels entfernen
            calendarPanel.SuspendLayout();


            // Hilfsstruktur für Terminprüfung
            var appointmentsDict = StringValidators.Instance.AppointmentsToDateTimeDict(day, month, year, appointments);
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int startDayOfWeek = ((int)firstDayOfMonth.DayOfWeek + 6) % 7; // Montag als 1.

            DateTime today = DateTime.Now.Date;

            var appointmentDates = new HashSet<DateTime>(
              appointmentsDict.Select(v => v.Value)
            );

            var tooltipData = appointments.AsEnumerable()
                .Select(row => new {
                Start = DateTime.ParseExact(row.Field<string>("start").Split(' ')[0], "dd.MM.yyyy", CultureInfo.InvariantCulture),
                End = DateTime.ParseExact(row.Field<string>("end").Split(' ')[0], "dd.MM.yyyy", CultureInfo.InvariantCulture),
                Text = row.Field<string>("text")
            })
            .ToList();

            Font regularFont = new Font("Arial", 8, FontStyle.Regular);
            Font boldFont = new Font("Arial", 8, FontStyle.Bold);

            var tooltipCache = new Dictionary<DateTime, string>();

            for (int d = 1; d <= daysInMonth; d++)
            {
                DateTime currentDate = new DateTime(year, month, d);

                bool hasAppointment = appointmentDates.Contains(currentDate.Date);
                bool isNotInExceptions = StringValidators.Instance.IsNotInExceptionsMethod(appointmentsDict, currentDate, year, month, d);
                bool isHoliday = holidays?.Any(h => h.Date.Date == currentDate.Date) ?? false;

                // Hintergrundfarbe
                Color backColor = currentDate == today
                    ? Color.LightBlue
                    : (isHoliday ? Color.Red : (hasAppointment && isNotInExceptions ? Color.White : SystemColors.Control));

                // Label erstellen
                Label dayLabel = new Label
                {
                    Text = $"{d} {StringValidators.Instance.DayName(currentDate.DayOfWeek.ToString().Substring(0, 2))}",
                    Width = 40,
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(40 * ((startDayOfWeek + d - 1) % 7), 40 * ((startDayOfWeek + d - 1) / 7)),
                    Name = "calendarDay",
                    Tag = currentDate.Date,
                    BackColor = backColor,
                    Font = hasAppointment && isNotInExceptions ? boldFont : regularFont
                };

            
                // Tooltiptext ggf. aus dem Cache holen
                if (!tooltipCache.TryGetValue(currentDate, out string tooltipText))
                {
                    tooltipText = "";

                    if (hasAppointment && isNotInExceptions)
                    {
                        int selectedMonth = monthCalendar.SelectionStart.Month;
                        int selectedYear = monthCalendar.SelectionStart.Year;
                        StringValidators.Instance.GetMonthsAppointments(selectedMonth, selectedYear, appointments, exceptions);

                        // Hier wird die bereits definierte tooltipData verwendet
                        foreach (var t in tooltipData)
                        {
                            if (t.Start <= currentDate && currentDate <= t.End)
                                tooltipText += $"-> {t.Text}\n";
                        }
                    }

                    // Feiertag anhängen, falls vorhanden
                    if (isHoliday)
                    {
                        var holiday = holidays.FirstOrDefault(h => h.Date == currentDate);
                        if (holiday != null)
                            tooltipText += $" {holiday.LocalName}";
                    }

                    // Im Cache speichern
                    tooltipCache[currentDate] = tooltipText;
                }
                // Tooltip setzen, falls vorhanden
                if (!string.IsNullOrEmpty(tooltipText))
                    boldedOrHoliDayToolTip.SetToolTip(dayLabel, tooltipText);

                // Rest bleibt wie gehabt
                dayLabel.Click += DayLabel_Click;
                calendarPanel.Controls.Add(dayLabel);


            }

            calendarPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            //lastDataHash = currentHash;
        }
        

 
        private void DayLabel_Click(object sender, EventArgs e)
        {
            var lbl = (Label)sender;

            if (lbl.Tag is DateTime date)
            {
                // Tag auch im MonthCalendar auswählen
                monthCalendar.SetDate(date);

                FilterGridByDate(date);
            }
        }


        private void FilterGridByDate(DateTime date)
        {
            if (appointments == null)
                return;

            var filtered = appointments.Clone();

            foreach (DataRow row in appointments.Rows)
            {
                string repeat = row.Field<string>("repeat"); // "y" = jährlich, "m" = monatlich, "" = einmalig

                if (!DateTime.TryParseExact(row.Field<string>("start"), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
                    continue;
                if (!DateTime.TryParseExact(row.Field<string>("end"), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
                    continue;

                bool include = false;

                switch (repeat)
                {
                    case "y": // jährliche Wiederholung
                              // Aktuelles Jahr für Start-End prüfen
                        DateTime startY = new DateTime(date.Year, start.Month, start.Day);
                        DateTime endY = new DateTime(date.Year, end.Month, end.Day);
                        include = (date.Date >= startY.Date && date.Date <= endY.Date);
                        break;

                    case "m": // monatliche Wiederholung
                        if (date.Day == start.Day)
                        {
                            DateTime startM = new DateTime(date.Year, date.Month, start.Day);
                            DateTime endM = new DateTime(date.Year, date.Month, end.Day);
                            include = (date.Date >= startM.Date && date.Date <= endM.Date);
                        }
                        break;

                    default: // einmalige Termine
                        include = start.Date <= date.Date && end.Date >= date.Date;
                        break;
                }

                if (include)
                    filtered.ImportRow(row);
            }

            dataGridViewAppointmentsOnClickedDay.DataSource = filtered;
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


        private int? currentAppointmentId;

        private void CreatePlaceHolder(string text, int appointmentId)
        {
            Controls.Remove(placeHolder);

            currentAppointmentId = appointmentId;

            placeHolder = new Label
            {
                Location = new Point(10, 500),
                Size = new Size(350, 30),
                Text = text,
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
                if (Controls[i] is Label label && label.Name == "calendar")
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

            ci = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }


        /// ///////////// EVENT HANDLER /// //////////////////////////////////////////


        private void ctButton_Click(object sender, EventArgs e)
        {

            if (isOpen)
            {
                MessageBox.Show(resourceManager.GetString("The Chat Form has already been opened."));
                return;
            }

            ChatForm ctForm = new ChatForm();
            isOpen = true;
            ctForm.FormClosed += (s, args) => isOpen = false;
            ctForm.Show();

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
                int appointmentId = Convert.ToInt32(row["id"]);
                if (dismissedAppointments.Contains(appointmentId))
                    continue; // überspringen

                DateTime appointmentStart = DateTime.Parse(row["start"].ToString());
                int notificationLeadTime = 10;

                if (appointmentStart > now && appointmentStart.Subtract(now).TotalMinutes <= notificationLeadTime)
                {
                    System.Media.SystemSounds.Hand.Play();

                    string appointmentText = row["text"].ToString();
                    CreatePlaceHolder(appointmentText + " beginnt in 10 Minuten", appointmentId);

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

        private void WriteIcsButton_Click(object sender, EventArgs e)
        {

            Reader icsReader = new Reader();

            // Übergib deine appointments-Liste an den Reader, falls notwendig
            icsReader.WriteICS();

        }

        private void ReadIcsButton_Click(object sender, EventArgs e)
        {
            Reader icsReader = new Reader();

            icsReader.ReadICS();

            //Application.Restart();
            //Environment.Exit(0);

            //Besser: Grid neu binden.
            //dataGridViewAppointmentsOnClickedDay.DataSource = null;
            dataGridViewAppointmentsOnClickedDay.DataSource = appointments;

            // Kalender neu zeichnen
            RemoveOldCalendarLabels();
            CreateCalendar(monthCalendar.SelectionStart.Year, monthCalendar.SelectionStart.Month, monthCalendar.SelectionStart.Day);

            // "Klick" auf MonthCalendar simulieren, damit das Grid aktualisiert wird
            MonthCalendar_DateChanged(
                monthCalendar,
                new DateRangeEventArgs(monthCalendar.SelectionStart, monthCalendar.SelectionEnd)
            );

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
                            DateTime startDT = DateTime.ParseExact(exception_start, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                            string exception_end = row.Cells["end"].Value.ToString();
                            DateTime endDT = DateTime.ParseExact(exception_end, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                            dateDao.WriteExceptionIntoExceptionTBL(idToDelete, startDT.ToString("dd.MM.yyyy"), endDT.ToString("dd.MM.yyyy"));

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
                    
                    monthCalendar.SetDate(DateTime.Now);

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


        private void Placeholder_Click(object? sender, EventArgs e)
        {
            if (currentAppointmentId.HasValue)
            {
                dismissedAppointments.Add(currentAppointmentId.Value);
                currentAppointmentId = null;
            }

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
