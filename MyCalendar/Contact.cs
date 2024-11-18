namespace MyCalendar
{
    public class Contact
    {

        private string id;
        private string name;
        private string nameGiven;
        private string phone;
        private string email;
        private string birthday;
        private string notes;
        private string address;
        private string addressstreet;


        public string Id
        {
            get { return id; }
            set { id = value; }
        }


        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string NameGiven
        {
            get { return nameGiven; }
            set { nameGiven = value; }
        }

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        public string AddressStreet
        {
            get { return addressstreet; }
            set { addressstreet = value; }
        }

        public string Phone
        {
            get { return phone; }
            set { phone = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        public string Birthday
        {
            get { return birthday; }
            set { birthday = value; }
        }

        public string Notes
        {
            get { return notes; }
            set { notes = value; }
        }


        public override string ToString()
        {
            return $"Contact [ID: {id}, Name: {name}, Given Name: {nameGiven}, Address: {address}, Address (street): {addressstreet}, Phone: {phone}, Email: {email}, Birthday: {birthday}, Notes: {notes}]";
        }

        public Contact(string id, string name, string nameGiven, string phone, string email, string birthday, string notes, string address, string addressstreet)
        {


            this.id = id;
            this.name = name;
            this.nameGiven = nameGiven;
            this.phone = phone;
            this.email = email;
            this.birthday = birthday;
            this.notes = notes;
            this.address = address;
            this.addressstreet = addressstreet;
        }
    }
}
