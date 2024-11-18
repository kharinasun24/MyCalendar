using System.Net.Sockets;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace MyCalendar
{
    public partial class ChatForm : Form
    {
        private ClientWebSocket webSocket;
        private RichTextBox textRichTextBox;
        private RichTextBox chatRichTextBox;
        private TextBox nameTextBox;
        private Button closeButton;
        private Button isOnButton;
        private bool isCloseButtonClicked = false;

        private const string DefaultName = "me";

        public ChatForm()
        {
            Size = new Size(600, 400);
            InitializeComponents();
            InitializeWebSocketClient();
            FormClosing += OnFormClosing;
        }

        private void InitializeComponents()
        {
            closeButton = CreateButton("Close", new Point(480, 320), closeButton_Click);
            Controls.Add(closeButton);

            nameTextBox = CreateTextBox(DefaultName, new Point(20, 320));
            Controls.Add(nameTextBox);

            textRichTextBox = CreateRichTextBox(new Point(10, 10), new Size(550, 120), readOnly: true);
            Controls.Add(textRichTextBox);

            chatRichTextBox = CreateRichTextBox(new Point(10, 150), new Size(550, 100), readOnly: false);
            chatRichTextBox.KeyPress += ChatRichTextBox_KeyPress;
            Controls.Add(chatRichTextBox);

            isOnButton = CreateButton(new Point(400, 320));
            isOnButton.Enabled = false;
            isOnButton.BackColor = Color.Red;
            Controls.Add(isOnButton);

        }


        private Button CreateButton(Point location)
        {

            var button = new Button
            {
                Location = location,
                AutoSize = true
            };

            return button;

        }

        private Button CreateButton(string text, Point location, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                AutoSize = true
            };
            button.Click += clickHandler;
            return button;
        }

        private TextBox CreateTextBox(string text, Point location)
        {
            return new TextBox
            {
                Text = text,
                Location = location,
                AutoSize = true
            };
        }

        private RichTextBox CreateRichTextBox(Point location, Size size, bool readOnly)
        {
            return new RichTextBox
            {
                Location = location,
                Size = size,
                ReadOnly = readOnly
            };
        }

        private async void ChatRichTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string message = $"{DateTime.Now:HH:mm} {nameTextBox.Text} {chatRichTextBox.Text.Trim()}";
                chatRichTextBox.Clear();

                if (!string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    await SendMessage(webSocket, message);
                }
            }
        }

        private async void closeButton_Click(object sender, EventArgs e)
        {
            isCloseButtonClicked = true;
            Close();
        }

        private async void InitializeWebSocketClient()
        {
            await ConnectAsync();
        }

        private void ToggleOnlineStatus(bool status)
        {
            if (status)
            {
                isOnButton.BackColor = Color.Green;
            }
            else
            {

                isOnButton.BackColor = Color.Red;
            }
        }


        public string GetIPv4()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            return ipEntry.AddressList
                .FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork)
                ?.ToString() ?? string.Empty;
        }

        public async Task ConnectAsync()
        {
            Uri serverUri = new Uri($"ws://{GetIPv4()}:8080");

            while (true) // Schleife für wiederholte Verbindungsversuche
            {
                using (webSocket = new ClientWebSocket())
                {
                    try
                    {
                        Console.WriteLine("Versuche Verbindung zum Server herzustellen...");
                        await webSocket.ConnectAsync(serverUri, CancellationToken.None);

                        Console.WriteLine("Mit dem Server verbunden.");
                        ToggleOnlineStatus(true);
                        await ReceiveMessages(webSocket);
                        break; // Beenden der Schleife, wenn die Verbindung erfolgreich ist
                    }
                    catch (WebSocketException ex)
                    {
                        Console.WriteLine($"WebSocket-Fehler beim Verbindungsversuch: {ex.Message}");
                        ToggleOnlineStatus(false);
                        Console.WriteLine("Erneuter Verbindungsversuch in 5 Sekunden...");

                        await Task.Delay(5000); // Wartezeit vor erneutem Verbindungsversuch
                    }
                }
            }
        }


        /*
        public async Task ConnectAsync()
        {
            using (webSocket = new ClientWebSocket())
            {
                Uri serverUri = new Uri($"ws://{GetIPv4()}:8080");

                try
                {
                    await webSocket.ConnectAsync(serverUri, CancellationToken.None);
                    Console.WriteLine("Connected to the server."); 
                    ToggleOnlineStatus(true);
                    await ReceiveMessages(webSocket);
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine($"WebSocket error during send: {ex.Message}"); 
                    ToggleOnlineStatus(false);
                }
            }
        }
        */

        private async Task SendMessage(ClientWebSocket webSocket, string message)
        {
            message += "\n";

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            if (webSocket?.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                Console.WriteLine("WebSocket is not connected. Message not sent.");

                ToggleOnlineStatus(false);

            }
        }

        private async Task ReceiveMessages(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed the connection", CancellationToken.None);
                        Console.WriteLine("Connection closed by server.");
                        ToggleOnlineStatus(false); // Sicherstellen, dass dies aufgerufen wird.
                        break;
                    }

                    string serverMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    textRichTextBox.AppendText(serverMessage);
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error during receive: {ex.Message}");
                ToggleOnlineStatus(false); // Aufruf hier für den Fall eines Fehlers.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                ToggleOnlineStatus(false); // Aufruf hier für unerwartete Fehler.
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isCloseButtonClicked)
            {
                e.Cancel = true;
            }
        }
    }
}
