using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyCalendar
{
    public partial class ChatWindow : Form
    {
        private RichTextBox chatBox;

        public string OnionAddress { get; }

        public ChatWindow(string onionAddress)
        {
            this.OnionAddress = onionAddress;
            this.Text = $"Chat mit {onionAddress}";
            this.Width = 400;
            this.Height = 500;

            chatBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            this.Controls.Add(chatBox);
        }

        public void AppendMessage(string message)
        {
            if (chatBox.InvokeRequired)
            {
                chatBox.Invoke(new Action(() => chatBox.AppendText(message + Environment.NewLine)));
            }
            else
            {
                chatBox.AppendText(message + Environment.NewLine);
            }
        }
    }
}
