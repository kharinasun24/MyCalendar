using MyCalendar;
using System.Configuration;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;

namespace MyCalendar
{


    public partial class ContactEditForm : Form
    {
        ResourceManager resourceManager;

        private string id;
        private bool isNew;

        private GeoFile geoFile;

        private System.Windows.Forms.TextBox nameTextBox; private Label nameLabel;
        private System.Windows.Forms.TextBox nameGivenTextBox; private Label nameGivenLabel;
        private System.Windows.Forms.TextBox phoneTextBox; private Label phoneLabel;
        private System.Windows.Forms.TextBox emailTextBox; private Label emailLabel;
        private System.Windows.Forms.TextBox notesTextBox; private Label notesLabel;
        private System.Windows.Forms.TextBox addressTextBoxTown; private Label addressLabelTown;
        private System.Windows.Forms.TextBox addressTextBoxStreet; private Label addressLabelStreet;

        private System.Windows.Forms.ComboBox comboBoxCityOption;

        private DateTimePicker birthdayDateTimePicker;
        private Label birthdayLabel;
        private DateTime originalDateTime;

        private ContactsForm contactsForm;

        private LanguageDao languageDao;
        private ContactDao contactDao;

        private List<Contact> contacts;
        private Contact contact;

        public ContactEditForm(string id, bool isNew, ContactsForm contactsForm)
        {

            this.id = id;
            this.isNew = isNew;
            this.contactsForm = contactsForm;

            Size = new Size(600, 500);

            geoFile = new GeoFile();

            languageDao = new LanguageDao();
            contactDao = new ContactDao();

            contacts = contactDao.GetContactsOrderByIsCoupleId(id);

            contact = contacts.FirstOrDefault();

            if (contact == null) { contact = new Contact("", "", "", "", "", "", "", "", ""); }

            InitializeCulture();

            CreateNameFields();

            CreatePhoneAndMailFields();

            CreateAddressChoser();

            CreateBirthdayPicker();

            CreateNotesPicker();

            System.Windows.Forms.Button saveButton, cancelButton;
            CreateSaveAndCancel(out saveButton, out cancelButton);

            // Steuerelemente zum Formular hinzufügen
            Controls.Add(nameGivenTextBox); Controls.Add(nameGivenLabel);
            Controls.Add(nameTextBox); Controls.Add(nameLabel);
            Controls.Add(phoneTextBox); Controls.Add(phoneLabel);
            Controls.Add(emailTextBox); Controls.Add(emailLabel);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);
            Controls.Add(birthdayDateTimePicker);
            Controls.Add(birthdayLabel);
            Controls.Add(notesTextBox);
            Controls.Add(notesLabel);
            Controls.Add(addressTextBoxTown);
            Controls.Add(addressLabelTown);
            Controls.Add(addressTextBoxStreet);
            Controls.Add(addressLabelStreet);
            Controls.Add(comboBoxCityOption);

        }


        private void CreateAddressChoser()
        {
            comboBoxCityOption = new System.Windows.Forms.ComboBox()
            {
                Location = new Point(10, 300),
                Size = new Size(200, 30),
                AutoSize = true
            };

            comboBoxCityOption.SelectedIndexChanged += ComboBoxCity_SelectedIndexChanged;

            foreach (string optn in geoFile.GetCities())
            {

                comboBoxCityOption.Items.Add(optn);

            }



            addressLabelStreet = new Label();
            addressLabelStreet.Text = resourceManager.GetString("address (street)");
            addressLabelStreet.Location = new Point(10, 230);
            addressLabelStreet.AutoSize = true;

            addressTextBoxStreet = new System.Windows.Forms.TextBox
            {
                Location = new Point(10, 260),
                Size = new Size(250, 30),
                Text = contact.AddressStreet,
                AutoSize = true
            };


            addressTextBoxTown = new System.Windows.Forms.TextBox
            {
                Location = new Point(280, 260),
                Size = new Size(250, 30),
                Text = contact.Address,
                AutoSize = true
            };

            addressLabelTown = new Label();
            addressLabelTown.Text = resourceManager.GetString("address (town)");
            addressLabelTown.Location = new Point(280, 230);
            addressLabelTown.AutoSize = true;

        }

        private void ComboBoxCity_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string addressText = addressTextBoxTown.Text;

            addressText = addressText + " " + comboBoxCityOption.Text;

            addressTextBoxTown.Text = addressText;

        }

        private void CreateNameFields()
        {

            nameGivenTextBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(200, 30),
                Text = contact.NameGiven,
                AutoSize = true
            };

            nameGivenLabel = new Label();
            nameGivenLabel.Text = resourceManager.GetString("givenname");
            nameGivenLabel.Location = new Point(280, 10);
            nameGivenLabel.AutoSize = true;


            nameTextBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(10, 50),
                Size = new Size(200, 30),
                Text = contact.Name,
                AutoSize = true
            };

            nameLabel = new Label();
            nameLabel.Text = resourceManager.GetString("surname");
            nameLabel.Location = new Point(280, 50);
            nameLabel.AutoSize = true;
        }

        private void CreateNotesPicker()
        {
            notesTextBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(10, 360),
                Size = new Size(200, 30),
                Text = contact.Notes,
                AutoSize = true
            };

            notesLabel = new Label();
            notesLabel.Text = resourceManager.GetString("notes");
            notesLabel.Location = new Point(280, 360);
            notesLabel.AutoSize = true;

        }





        private void CreatePhoneAndMailFields()
        {
            phoneTextBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(10, 100),
                Size = new Size(200, 30),
                Text = contact.Phone,
                AutoSize = true
            };

            phoneLabel = new Label();
            phoneLabel.Text = resourceManager.GetString("phone");
            phoneLabel.Location = new Point(280, 100);
            phoneLabel.AutoSize = true;

            emailTextBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(10, 150),
                Size = new Size(200, 30),
                Text = contact.Email,
                AutoSize = true
            };

            emailLabel = new Label();
            emailLabel.Text = resourceManager.GetString("email");
            emailLabel.Location = new Point(280, 150);
            emailLabel.AutoSize = true;

        }

        private void CreateBirthdayPicker()
        {
            birthdayDateTimePicker = new DateTimePicker();
            birthdayDateTimePicker.Format = DateTimePickerFormat.Short;
            birthdayDateTimePicker.Location = new System.Drawing.Point(10, 200);

            string brthdy = contact.Birthday;

            bool IsNoBirthday = false;


            if (String.IsNullOrEmpty(brthdy))
            {
                IsNoBirthday = true;
                brthdy = DateTime.Now.ToString().Split(' ')[0];
            }

            brthdy = StringValidators.Instance.CheckDateI18N(brthdy);

            birthdayDateTimePicker.Value = DateTime.ParseExact(brthdy, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            originalDateTime = birthdayDateTimePicker.Value;

            if (IsNoBirthday)
            {
                birthdayDateTimePicker.Visible = false;
            }

            birthdayLabel = new Label();
            birthdayLabel.Text = resourceManager.GetString("birthday");
            birthdayLabel.Location = new Point(280, 200);
            birthdayLabel.AutoSize = true;
            birthdayLabel.Click += (sender, e) =>
            {
                birthdayDateTimePicker.Visible = true;
            };

        }


        private void CreateSaveAndCancel(out System.Windows.Forms.Button saveButton, out System.Windows.Forms.Button cancelButton)
        {
            saveButton = new System.Windows.Forms.Button
            {
                Text = resourceManager.GetString("Save"),
                Location = new Point(10, 400),
                Size = new Size(100, 30)
            };
            saveButton.Click += SaveButton_Click;


            cancelButton = new System.Windows.Forms.Button
            {
                Text = resourceManager.GetString("Cancel"),
                Location = new Point(120, 400),
                Size = new Size(100, 30)
            };
            cancelButton.Click += CancelButton_Click;
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






        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {

            string name = nameTextBox.Text;
            string nameGiven = nameGivenTextBox.Text;
            string addressText = addressTextBoxTown.Text;
            string addressStreetText = addressTextBoxStreet.Text;
            string phone = phoneTextBox.Text;
            string email = emailTextBox.Text;
            string birthday = birthdayDateTimePicker.Text;
            string notes = notesTextBox.Text;

            string newOrOldBirthdayValue = birthdayDateTimePicker.Value.ToString().Split(' ')[0]; ;

            bool IsValidPhoneNumber = StringValidators.Instance.IsValidPhoneNumber(Regex.Replace(phone, @"[\s()]+", ""));

            if (!IsValidPhoneNumber)
            {

                MessageBox.Show(resourceManager.GetString("Not a valid phone number!")); return;
            }

            bool IsValidEmail = StringValidators.Instance.IsValidEmail(Regex.Replace(email, @"\s+", ""));

            if (!IsValidEmail)
            {

                MessageBox.Show(resourceManager.GetString("Not a valid email!")); return;
            }

            if (String.IsNullOrEmpty(name))
            {
                //string warningContact = resourceManager.GetString("Contact must have a name!");

                MessageBox.Show(resourceManager.GetString("Contacts need a name!")); return;

            }

            if (DateTime.Now.ToString().Split(' ')[0].Equals(birthday) || DateTime.Now < birthdayDateTimePicker.Value)
            {
                MessageBox.Show(resourceManager.GetString("Info: If birthday is today or later, it will not be saved."));

                birthday = "";
            }

            if (isNew)
            {

                contactDao.CreateNewContact(name, nameGiven, phone, email, birthday, notes, addressText, addressStreetText);

                Close();

            }
            else
            {
                contactDao.UpdateContactWithId(id, name, nameGiven, phone, email, birthday, notes, addressText, addressStreetText);

                Close();

            }



            contactsForm.DrawContacts();


        }
    }
}
