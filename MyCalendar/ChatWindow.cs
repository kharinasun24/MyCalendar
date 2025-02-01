using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyCalendar //TODO: 1.: me und buddy ersetzen. 2. Layout dieses Windows verbessern.
{
    public partial class ChatWindow : Form
    {
        private RichTextBox chatBox;   // Chatverlauf
        private RichTextBox inputBox;  // Eingabefeld für eigene Nachrichten
        private Button sendButton;     // Button zum Senden
        private ClientWebSocket webSocket;
        public string OnionAddress { get; }

        public event Action<string> ChatClosed; // Event für Schließen

        public ChatWindow(string onionAddress, ClientWebSocket webSocket)
        {
            this.OnionAddress = onionAddress;
            this.webSocket = webSocket;
            this.Text = $"Chat mit {onionAddress}";
            this.Width = 400;
            this.Height = 500;

            // Chat-Anzeige (empfangene Nachrichten)
            chatBox = new RichTextBox
            {
                Dock = DockStyle.Top,
                Height = 300,
                ReadOnly = true
            };

            // Eingabebox (zum Schreiben eigener Nachrichten)
            inputBox = new RichTextBox
            {
                Dock = DockStyle.Top,
                Height = 80
            };

            // Sende-Button
            sendButton = new Button
            {
                Text = "Senden",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            sendButton.Click += SendMessage;

            // Alle Steuerelemente zur Form hinzufügen
            this.Controls.Add(chatBox);
            this.Controls.Add(inputBox);
            this.Controls.Add(sendButton);

            // Verbindung starten
            _ = StartCommunication();
        }

        private async Task StartCommunication()
        {
            Uri serverUri = new Uri($"ws://{OnionAddress}");
            try
            {
                await webSocket.ConnectAsync(serverUri, CancellationToken.None);
                AppendMessage("Verbunden mit " + OnionAddress);

                // Starte das Empfangen von Nachrichten
                _ = Task.Run(() => ReceiveMessages());
            }
            catch (Exception ex)
            {
                AppendMessage($"Fehler: {ex.Message}");
            }
        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[1024];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        AppendMessage("Verbindung getrennt.");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    AppendMessage($"Buddy: {message}");
                }
            }
            catch (Exception ex)
            {
                AppendMessage($"Verbindungsfehler: {ex.Message}");
            }
        }

        private async void SendMessage(object sender, EventArgs e)
        {
            if (webSocket == null || webSocket.State != WebSocketState.Open)
            {
                AppendMessage("Verbindung nicht aktiv.");
                return;
            }

            string message = inputBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

            AppendMessage($"me: {message}");
            inputBox.Clear(); // Eingabefeld leeren
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // WebSocket schließen
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                _ = webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Chat geschlossen", CancellationToken.None);
            }

            // Event auslösen, um den Chat aus activeChats zu entfernen
            ChatClosed?.Invoke(OnionAddress);
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
