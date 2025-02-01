using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyCalendar
{
    public partial class ChatWindow : Form
    {
        private RichTextBox chatBox;
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

            chatBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            this.Controls.Add(chatBox);

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
                    AppendMessage(message);
                }
            }
            catch (Exception ex)
            {
                AppendMessage($"Verbindungsfehler: {ex.Message}");
            }
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
