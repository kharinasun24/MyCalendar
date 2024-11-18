using MyCalendar;
using System.Globalization;
using System.Resources;

namespace MyCalendar
{


    public partial class ContactViewForm : Form
    {
        ResourceManager resourceManager;

        private string id;

        private System.Windows.Forms.TextBox nameTextBox; private Label nameLabel;
        private System.Windows.Forms.TextBox nameGivenTextBox; private Label nameGivenLabel;
        private System.Windows.Forms.TextBox phoneTextBox; private Label phoneLabel;
        private System.Windows.Forms.TextBox emailTextBox; private Label emailLabel;
        private System.Windows.Forms.TextBox notesTextBox; private Label notesLabel;
        private System.Windows.Forms.TextBox addressTextBox; private Label addressLabel;
        private System.Windows.Forms.TextBox birthdayTextBox; private Label birthdayLabel;

        //private DateTime originalDateTime;

        private ContactDao contactDao; private LanguageDao languageDao;


        private Contact contact;

        public ContactViewForm(string cid)
        {
            this.id = cid;

            Size = new Size(600, 500);

            languageDao = new LanguageDao();
            contactDao = new ContactDao();

            InitializeCulture();

            contact = InitializeContact(this.id);

            CreateNameFields();

            CreatePhoneAndMailFields();

            CreateNotesPicker();

            System.Windows.Forms.Button cancelButton;
            CreateCancel(out cancelButton);

            // Steuerelemente zum Formular hinzufügen
            Controls.Add(nameGivenTextBox); Controls.Add(nameGivenLabel);
            Controls.Add(nameTextBox); Controls.Add(nameLabel);
            Controls.Add(phoneTextBox); Controls.Add(phoneLabel);
            Controls.Add(emailTextBox); Controls.Add(emailLabel);
            Controls.Add(cancelButton);
            Controls.Add(notesTextBox);
            Controls.Add(notesLabel);
            Controls.Add(addressTextBox);
            Controls.Add(addressLabel);
            Controls.Add(birthdayTextBox);
            Controls.Add(birthdayLabel);


        }

        private Contact? InitializeContact(string contid)
        {
            Contact cct = new Contact("-1", "", "", "", "", "", "", "", "");

            Contact contact = contactDao.GetContactByContactId(contid)?.First();
            if (contact != null)
            {
                cct = contact;
            }

            return cct;
        }

        private void CreateNameFields()
        {

            nameGivenTextBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(200, 30),
                Text = contact.NameGiven,
                AutoSize = true,
                ReadOnly = true
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
                AutoSize = true,
                ReadOnly = true
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
                AutoSize = true,
                ReadOnly = true
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
                AutoSize = true,
                ReadOnly = true
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
                AutoSize = true,
                ReadOnly = true
            };

            emailLabel = new Label();
            emailLabel.Text = resourceManager.GetString("email");
            emailLabel.Location = new Point(280, 150);
            emailLabel.AutoSize = true;


            addressTextBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(10, 200),
                Size = new Size(550, 30),
                Text = contact.AddressStreet + ", " + contact.Address,
                AutoSize = true,
                ReadOnly = true
            };

            //addressLabel = new Label();
            //addressLabel.Text = resourceManager.GetString("address");
            //addressLabel.Location = new Point(430, 200);
            //addressLabel.AutoSize = true;



            birthdayTextBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(10, 250),
                Size = new Size(200, 30),
                Text = contact.Birthday,
                AutoSize = true,
                ReadOnly = true
            };

            birthdayLabel = new Label();
            birthdayLabel.Text = resourceManager.GetString("birthday");
            birthdayLabel.Location = new Point(280, 250);
            birthdayLabel.AutoSize = true;

        }



        private void CreateCancel(out System.Windows.Forms.Button cancelButton)
        {


            cancelButton = new System.Windows.Forms.Button
            {
                Text = resourceManager.GetString("Close"),
                Location = new Point(10, 400),
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


    }
}
