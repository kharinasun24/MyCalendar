using MyCalendar;
using System.Globalization;
using System.Resources;

namespace MyCalendar
{
    public partial class DetailsForm : Form
    {
        ResourceManager resourceManager;

        private Form1 form1;

        private DateDao dateDao;
        private ContactDao contactDao;
        private LanguageDao languageDao;

        private TextBox filterTextBox;

        private DataGridView dataGridViewAppointmentAdd;
        private DataGridView dataGridContactAdd;

        private DateTimePicker dateTimePickerStart;
        private Label labelStart;
        private TextBox textBox1;
        private Button saveBtn;
        private Button closeBtn;

        private CheckBox checkBoxMonthly;
        private CheckBox checkBoxYearly;

        BindingSource bindingSource;

        private DateTimePicker dateTimePickerEnd;
        private Label labelEnd;

        private string id;

        public DetailsForm(string id, Form1 f1)
        {

            form1 = f1;

            this.id = id;

            dateDao = new DateDao();
            contactDao = new ContactDao();

            List<Date> foundDates = dateDao.GetEntryById(id);

            InitializeCulture();

            filterTextBox = new TextBox
            {
                Location = new Point(110, 10),
                AutoSize = true,
            };

            filterTextBox.TextChanged += FilterTextBox_TextChanged;

            Controls.Add(filterTextBox);

            // CheckBoxes erstellen und Eigenschaften setzen
            checkBoxMonthly = new System.Windows.Forms.CheckBox();
            checkBoxMonthly.Text = resourceManager.GetString("monthly");
            checkBoxMonthly.Location = new Point(460, 10);
            checkBoxMonthly.AutoSize = true;
            checkBoxMonthly.CheckedChanged += checkBox_CheckedChanged;

            checkBoxYearly = new System.Windows.Forms.CheckBox();
            checkBoxYearly.Text = resourceManager.GetString("yearly");
            checkBoxYearly.Location = new Point(460, 40);
            checkBoxYearly.AutoSize = true;
            checkBoxYearly.CheckedChanged += checkBox_CheckedChanged;


            Controls.Add(checkBoxMonthly);
            Controls.Add(checkBoxYearly);


            dateDao = new DateDao();

            Size = new Size(800, 500);

            labelStart = new System.Windows.Forms.Label();
            labelStart.Location = new Point(10, 10);
            labelStart.Text = resourceManager.GetString("Start");
            Controls.Add(labelStart);

            if (foundDates.Count == 1)
            {

                Date elementDate = foundDates.First();

                if ("y".Equals(elementDate.Repeat) || "m".Equals(elementDate.Repeat))
                {

                    checkBoxMonthly.Visible = false;
                    checkBoxYearly.Visible = false;
                }

                //Die Form besteht aus einer TextBox und DatePickern.
                textBox1 = new System.Windows.Forms.TextBox();
                textBox1.Text = elementDate.Text;
                textBox1.Location = new Point(10, 100);
                textBox1.Size = new Size(200, 10);
                Controls.Add(textBox1);

                dateTimePickerStart = new DateTimePicker();
                dateTimePickerStart.Format = DateTimePickerFormat.Custom;
                dateTimePickerStart.CustomFormat = "dd.MM.yyyy HH:mm";
                dateTimePickerStart.Location = new Point(10, 40);
                dateTimePickerStart.Value = elementDate.GetStartAsDateTime();

                Controls.Add(dateTimePickerStart);

                dateTimePickerEnd = new DateTimePicker();
                dateTimePickerEnd.Format = DateTimePickerFormat.Custom;
                dateTimePickerEnd.CustomFormat = "dd.MM.yyyy HH:mm";
                dateTimePickerEnd.Location = new Point(250, 40);
                dateTimePickerEnd.Value = elementDate.GetEndAsDateTime();

                Controls.Add(dateTimePickerEnd);


                dateTimePickerStart.ValueChanged += (sender, e) =>
                {
                    RoundedEntries();
                };


                Controls.Add(dateTimePickerStart);


                labelEnd = new System.Windows.Forms.Label();
                labelEnd.Location = new Point(250, 10);
                labelEnd.Text = resourceManager.GetString("End");
                Controls.Add(labelEnd);


                dateTimePickerEnd.ValueChanged += (sender, e) =>
                {
                    RoundedEntries();
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

                DrawContacts();

            }


        }

        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            string filterText = filterTextBox.Text;

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                // Filter für die BindingSource setzen
                bindingSource.Filter = $"name LIKE '%{filterText}%'";
            }
            else
            {
                // Filter entfernen, wenn das Textfeld leer ist
                bindingSource.RemoveFilter();
            }
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

        private void DrawContacts()
        {

            Controls.Remove(dataGridContactAdd);

            dataGridContactAdd = new DataGridView();

            dataGridContactAdd.Location = new System.Drawing.Point(250, 90);
            dataGridContactAdd.Size = new System.Drawing.Size(460, 250);
            dataGridContactAdd.ScrollBars = ScrollBars.Vertical;
            dataGridContactAdd.AllowUserToAddRows = false;
            dataGridContactAdd.ReadOnly = true;
            dataGridContactAdd.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            bindingSource = new BindingSource();


            bindingSource.DataSource = contactDao.GetContactsOrderByIsCouple(this.id);


            dataGridContactAdd.DataSource = bindingSource;

            //bindingSource.Sort = "iscouple DESC"; 


            dataGridContactAdd.DataBindingComplete += (sender, e) =>
            {
                dataGridContactAdd.CurrentCell = null;
            };


            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "id";
            column.Name = "id";
            dataGridContactAdd.Columns.Add(column);
            dataGridContactAdd.Columns["id"].Visible = false;

            column = new DataGridViewTextBoxColumn();
            column.HeaderText = resourceManager.GetString("Contact");
            column.DataPropertyName = "name";
            column.Name = "name";
            dataGridContactAdd.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.HeaderText = "👀";
            column.DataPropertyName = "eyeProperty";
            column.Name = "eyeColumn";
            dataGridContactAdd.Columns.Add(column);

            dataGridContactAdd.RowsAdded += new DataGridViewRowsAddedEventHandler(DataGridView_RowsAdded);

            dataGridContactAdd.CellClick += DataGridView_CellClick;

            Controls.Add(dataGridContactAdd);
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            // Checken ob eine cell (nicht Header od. andere Elemente) geclickt
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                //zur ersten Spalte navigieren...
                int colIndex = e.ColumnIndex;
                colIndex--;

                if (colIndex == -1)
                {

                    //...und von dieser aus in den Spalten an die gewünschte Stelle navigieren.
                    int contactIndex = colIndex;
                    contactIndex += 2;

                    string clickedCellValue = dataGridContactAdd[contactIndex, e.RowIndex].Value?.ToString();

                    ContactViewForm contactViewForm = new ContactViewForm(clickedCellValue);
                    contactViewForm.ShowDialog();

                }
                else
                {

                    string clickedCellValue = dataGridContactAdd[colIndex, e.RowIndex].Value?.ToString();

                    contactDao.ToggleCouple(this.id, clickedCellValue);

                    DrawContacts();

                }
            }


        }

        //Kann weg?
        //private void btnFilter_Click(object sender, EventArgs e)
        //{

        //}




        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {

                string cellValue = dataGridContactAdd.Rows[i].Cells["id"].Value.ToString();

                bool isLink = contactDao.GetLinkedContact(this.id.ToString(), cellValue);

                if (dataGridContactAdd.Rows[i].Cells["id"].Value != null && isLink)
                {
                    dataGridContactAdd.Rows[i].DefaultCellStyle.BackColor = Color.LightSeaGreen;

                }
                else
                {
                    dataGridContactAdd.Rows[i].DefaultCellStyle.BackColor = Color.White;
                }


            }
        }


        private void checkBox_CheckedChanged(object sender, EventArgs
e)
        {
            if (checkBoxMonthly.Checked)
            {
                checkBoxYearly.Checked = false;
            }

            if (checkBoxYearly.Checked)
            {
                checkBoxMonthly.Checked = false;
            }
        }



        private void closeBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show(resourceManager.GetString("Are the checkboxes set correctly - monthly/yearly repeat?"),
                                         resourceManager.GetString("Confirm and save"),
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            DateTime startDT = new DateTime(dateTimePickerStart.Value.Year,
                                   dateTimePickerStart.Value.Month,
                                   dateTimePickerStart.Value.Day,
                                   dateTimePickerStart.Value.Hour,
                                   dateTimePickerStart.Value.Minute,
                                   dateTimePickerStart.Value.Second);


            DateTime endDT = new DateTime(dateTimePickerEnd.Value.Year,
                       dateTimePickerEnd.Value.Month,
                       dateTimePickerEnd.Value.Day,
                       dateTimePickerEnd.Value.Hour,
                       dateTimePickerEnd.Value.Minute,
                       dateTimePickerEnd.Value.Second);

            if (startDT > endDT)
            {
                return;
            }


            TimeSpan d = dateTimePickerEnd.Value - dateTimePickerStart.Value;
            int duration = d.Days;

            duration++;

            if (duration >= 1)
            {

                string start = dateTimePickerStart.Value.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                string end = dateTimePickerEnd.Value.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                string repeat = "n";

                if (checkBoxMonthly.Checked)
                {
                    repeat = "m";
                }
                else if (checkBoxYearly.Checked)
                {

                    repeat = "y";
                }

                dateDao.updateDate(id, textBox1.Text, start, end, duration.ToString(), repeat);

                form1.DrawAppointmentsOnClickedDay(form1.monthCalendar.SelectionStart.Day, form1.monthCalendar.SelectionStart.Month, form1.monthCalendar.SelectionStart.Year, dateDao);

                Close();

                //Application.Restart();
                //Environment.Exit(0);

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

            /*
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

                DateTime newDateTime = new DateTime(dateTimePickerEnd.Value.Year,
                         dateTimePickerEnd.Value.Month,
                         dateTimePickerEnd.Value.Day,
                         dt.Hour,
                         dateTimePickerEnd.Value.Minute,
                         dateTimePickerEnd.Value.Second);

                dateTimePickerEnd.Value = newDateTime;

            }
            */
        }
    }
}
