using MyCalendar;
using System.Globalization;
using System.Resources;
using System.Xml;

namespace MyCalendar
{
    public partial class ContactsForm : Form
    {

        ResourceManager resourceManager;

        private Button exportButton; private Button importButton;
        private Button closeButton;

        private ContactDao contactDao;
        private LanguageDao languageDao;

        private TextBox filterTextBox;
        private Button doublesButton;

        private DataGridView dataGridContactAdd;

        BindingSource bindingSource;

        public ContactsForm()
        {
            Size = new Size(550, 400);

            contactDao = new ContactDao();
            languageDao = new LanguageDao();

            InitializeCulture();

            filterTextBox = new TextBox
            {
                Location = new Point(10, 10),
                AutoSize = true,
            };

            filterTextBox.TextChanged += FilterTextBox_TextChanged;

            Controls.Add(filterTextBox);

            doublesButton = new Button
            {
                Text = "Duplex",
                Location = new Point(120, 10),
                AutoSize = true,
            };

            doublesButton.Click += FilterTextButton_Click;

            Controls.Add(doublesButton);

            Button addContactButton = new Button
            {
                Text = "👨‍👩‍👧",
                Location = new Point(10, 300),
                AutoSize = true,
                Size = new Size(100, 50)
            };

            addContactButton.Click += AddContactButton_Click;

            Controls.Add(addContactButton);

            exportButton = new Button
            {
                Text = resourceManager.GetString("export"),
                Location = new Point(110, 300),
                AutoSize = true,
                BackColor = Color.LightGray,
                Size = new Size(100, 50)
            };

            exportButton.Click += ExportButton_Click;


            Controls.Add(exportButton);

            closeButton = new Button
            {
                Text = resourceManager.GetString("Close"),
                Location = new Point(210, 300),
                AutoSize = true,
                BackColor = Color.LightGray,
                Size = new Size(100, 50)
            };


            closeButton.Click += SaveButton_Click;

            closeButton.MouseEnter += cancelButton_MouseEnter;

            closeButton.MouseLeave += cancelButton_MouseLeave;

            Controls.Add(closeButton);

            importButton = new Button
            {
                Text = resourceManager.GetString("import"),
                Location = new Point(310, 300),
                AutoSize = true,
                BackColor = Color.LightGray,
                Size = new Size(100, 50)
            };

            importButton.Click += ImportButton_Click;


            Controls.Add(importButton);

            DrawContacts();

        }

        private void FilterTextButton_Click(object? sender, EventArgs e)
        {
            Doubles doubles = new Doubles();

            doubles.ShowDoubles(3); //MAX LEVENSHTEIN DISTANCE ARGUMENT IS NOT SUPPOSED TO BE LT 2!

        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            Reader reader = new Reader();

            reader.ReadXML();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            List<Contact> contacts = contactDao.GetAllContacts();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            saveFileDialog.Title
         = "Save Contacts as XML";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                string filePath = saveFileDialog.FileName;

                XmlWriter xmlWriter = XmlWriter.Create(filePath);

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("contacts");

                foreach (Contact c in contacts)
                {
                    xmlWriter.WriteStartElement("contact");
                    xmlWriter.WriteStartElement("nameGiven");
                    xmlWriter.WriteString(c.NameGiven);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("name");
                    xmlWriter.WriteString(c.Name);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("addressstreet");
                    xmlWriter.WriteString(c.AddressStreet);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("addressstreet");
                    xmlWriter.WriteString(c.AddressStreet);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("address");
                    xmlWriter.WriteString(c.Address);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("addressstreet");
                    xmlWriter.WriteString(c.AddressStreet);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("phone");
                    xmlWriter.WriteString(c.Phone);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("email");
                    xmlWriter.WriteString(c.Email);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("birthday");
                    xmlWriter.WriteString(c.Birthday);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("notes");
                    xmlWriter.WriteString(c.Notes);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndDocument();
                xmlWriter.Close();

                MessageBox.Show("Contacts exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        public void DrawContacts()
        {
            Controls.Remove(dataGridContactAdd);

            dataGridContactAdd = new DataGridView();

            dataGridContactAdd.Location = new System.Drawing.Point(10, 40);
            dataGridContactAdd.Size = new System.Drawing.Size(460, 250);
            dataGridContactAdd.ScrollBars = ScrollBars.Vertical;
            dataGridContactAdd.AllowUserToAddRows = false;
            dataGridContactAdd.ReadOnly = true;
            dataGridContactAdd.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridContactAdd.RowsAdded += new DataGridViewRowsAddedEventHandler(DataGridView_RowsAdded);

            dataGridContactAdd.CellClick += DataGridView_CellClick;


            bindingSource = new BindingSource();


            bindingSource.DataSource = contactDao.GetContactsOrderByName();


            dataGridContactAdd.DataSource = bindingSource;


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
            column.Name = "xColumn";
            column.HeaderText = "";
            dataGridContactAdd.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.HeaderText = resourceManager.GetString("Contact");
            column.DataPropertyName = "name";
            column.Name = "name";
            dataGridContactAdd.Columns.Add(column);



            Controls.Add(dataGridContactAdd);
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == dataGridContactAdd.Columns["xColumn"].Index)
                {

                    // Zugriff auf die aktuelle Reihe
                    DataGridViewRow row = ((DataGridView)sender).Rows[e.RowIndex];

                    string idToDelete = row.Cells["id"].Value.ToString();

                    DialogResult result = MessageBox.Show(resourceManager.GetString("Do you really want to delete contact?"), resourceManager.GetString("Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    // Wenn der Benutzer "Ja" klickt, führe den Löschvorgang aus
                    if (result == DialogResult.Yes)
                    {
                        contactDao.DeleteEntryById(idToDelete);

                        DrawContacts();
                    }


                }

                else
                {

                    // Zugriff auf die aktuelle Reihe
                    DataGridViewRow row = ((DataGridView)sender).Rows[e.RowIndex];
                    string id = row.Cells["id"].Value.ToString();

                    ContactEditForm contactEditForm = new ContactEditForm(id, false, this);
                    contactEditForm.ShowDialog();

                }
            }
        }


        private void DataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {
                // Setzen des Wertes "x" für jede neue Zeile in der "xColumn" Spalte
                dataGridContactAdd.Rows[i].Cells["xColumn"].Value = "x";
            }
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AddContactButton_Click(object sender, EventArgs e)
        {

            ContactEditForm contactEditForm = new ContactEditForm("-1", true, this);
            contactEditForm.ShowDialog();

        }


        private void cancelButton_MouseEnter(object sender, EventArgs e)
        {
            //Bein ersten Mal mit der Maus drüber wird das Ding blau eingefärbt.
            closeButton.BackColor = Color.LightBlue;
            closeButton.Cursor = Cursors.Hand;
        }


        private void cancelButton_MouseLeave(object sender, EventArgs e)
        {
            //Bein ersten Mal mit der Maus drüber wird das Ding blau eingefärbt.
            closeButton.BackColor = Color.LightGray;
            closeButton.Cursor = Cursors.Hand;
        }


    }
}
