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
        private DataGridView dataGridView;
        private RichTextBox textRichTextBox;
        private RichTextBox chatRichTextBox;
        private Button closeButton;
        private Button isOnButton;
        private Dictionary<string, (ClientWebSocket, ChatForm)> activeChats = new();

        private bool isCloseButtonClicked = false;

        private string DefaultName = "";

        public ChatForm()
        {
            Size = new Size(600, 400);
            InitializeComponents();
            //InitializeWebSocketClient();
            //FormClosing += OnFormClosing;
        }

      


        ///////////////////////////////////NEW////////////////////////////////////////


        private async Task StartChat(string onionAddress)
        {
            if (activeChats.ContainsKey(onionAddress)) return; // Falls bereits verbunden, nichts tun

            var chatForm = new ChatForm(); //TODO: Das ist eben nicht unsere ChatForm, welche aufgehen soll.
            chatForm.Text = $"Chat mit {onionAddress}";
            chatForm.Show();

            var webSocket = CreateTorWebSocket(); // Tor-WebSocket nutzen
            Uri serverUri = new Uri($"ws://{onionAddress}");

            try
            {
                await webSocket.ConnectAsync(serverUri, CancellationToken.None);
                chatForm.AppendMessage("Verbunden mit " + onionAddress);
                activeChats[onionAddress] = (webSocket, chatForm);

                // Starte das Empfangen von Nachrichten
                _ = ReceiveMessages(webSocket, chatForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Verbinden mit {onionAddress}: {ex.Message}");
                chatForm.Close();
            }
        }

        private async Task ReceiveMessages(ClientWebSocket webSocket, ChatForm chatForm)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    chatForm.AppendMessage(message);
                }
            }
            catch (Exception ex)
            {
                chatForm.AppendMessage($"Fehler beim Empfang: {ex.Message}");
            }
            finally
            {
                CloseChat(chatForm.Text);
            }
        }

        private void CloseChat(string onionAddress)
        {
            if (activeChats.TryGetValue(onionAddress, out var chatData))
            {
                var (webSocket, chatForm) = chatData;

                if (webSocket?.State == WebSocketState.Open)
                {
                    webSocket.Abort();
                }

                chatForm.Close();
                activeChats.Remove(onionAddress);
            }
        }


        public ClientWebSocket CreateTorWebSocket()
        {
            var clientWebSocket = new ClientWebSocket();
            var options = clientWebSocket.Options;

            options.Proxy = new WebProxy("http://127.0.0.1:8118"); // Privoxy-Port.
            return clientWebSocket;
        }


        /////////////////////////////////////OLD//////////////////////////////////////////////////////////
        /*
          
        private async void InitializeWebSocketClient()
        {
            await ConnectAsync();
        }
 
         
        public async Task ConnectAsync()
        {
            Uri serverUri = new Uri("ws://yourhiddenserviceaddress.onion");

            while (true)
            {
                using (webSocket = CreateTorWebSocket())
                {
                    try
                    {
                        Console.WriteLine("Versuche Verbindung zu Hidden Service herzustellen...");
                        await webSocket.ConnectAsync(serverUri, CancellationToken.None);

                        Console.WriteLine("Mit dem Hidden Service verbunden.");
                        ToggleOnlineStatus(true);
                        await ReceiveMessages(webSocket);
                        break;
                    }
                    catch (WebSocketException ex)
                    {
                        Console.WriteLine($"WebSocket-Fehler beim Verbindungsversuch: {ex.Message}");
                        ToggleOnlineStatus(false);
                        Console.WriteLine("Erneuter Verbindungsversuch in 5 Sekunden...");

                        await Task.Delay(5000);
                    }
                }
            }
        }

        public ClientWebSocket CreateTorWebSocket()
        {
            var clientWebSocket = new ClientWebSocket();
            var options = clientWebSocket.Options;

            options.Proxy = new WebProxy("http://127.0.0.1:8118"); // Privoxy-Port.
            return clientWebSocket;
        }

      

        private async void ChatRichTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Benutzername
            string me = "me";

            // Prüfen, ob Enter gedrückt wurde
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // Unterdrückt den Zeilenumbruch in der RichTextBox

                // Nachricht formatieren
                string message = $"{DateTime.Now:HH:mm} {chatRichTextBox.Text.Trim()}";

                // Nachricht in der eigenen RichTextBox anzeigen
                chatRichTextBox.Clear(); // Eingabe zurücksetzen
                textRichTextBox.AppendText($"{me} {message}\n"); // Nachricht zur Anzeige hinzufügen

                // Nachricht an den Buddy senden
                if (webSocket != null && webSocket.State == WebSocketState.Open)
                {
                    await SendMessage(webSocket, message);
                }
                else
                {
                    // WebSocket ist nicht verbunden, Benutzer informieren
                    textRichTextBox.AppendText("WebSocket ist nicht verbunden. Nachricht konnte nicht gesendet werden.\n");
                }
            }
        }

        private async Task SendMessage(ClientWebSocket webSocket, string message)
        {
            try
            {
                // Nachricht in Bytes konvertieren und senden
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Senden der Nachricht: {ex.Message}");
                textRichTextBox.AppendText($"Fehler beim Senden der Nachricht: {ex.Message}\n");
            }
        }


        private async Task ReceiveMessages(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    // Empfangene Nachrichten lesen
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Verbindung wurde vom Server geschlossen
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed the connection", CancellationToken.None);
                        Console.WriteLine("Verbindung vom Server geschlossen.");
                        ToggleOnlineStatus(false);
                        break;
                    }

                    // Nachricht in UTF-8 dekodieren und anzeigen

                    string serverMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    textRichTextBox.Invoke((MethodInvoker)(() => textRichTextBox.AppendText(serverMessage + Environment.NewLine)));

                    Console.WriteLine($"Nachricht vom Server: {serverMessage}");
                }
            }
            catch (WebSocketException ex)
            {
                // Spezifische Fehler für WebSockets
                Console.WriteLine($"WebSocket-Fehler beim Empfang: {ex.Message}");
                ToggleOnlineStatus(false);

                // Option: Automatischer Wiederverbindungsversuch
                Console.WriteLine("Erneuter Verbindungsaufbau in 5 Sekunden...");
                await Task.Delay(5000);
                await ConnectAsync(); // Wiederverbindung starten
            }
            catch (Exception ex)
            {
                // Allgemeine Fehler
                Console.WriteLine($"Allgemeiner Fehler: {ex.Message}");
                ToggleOnlineStatus(false);
            }
        }

        */
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        private void InitializeComponents()
        {
            closeButton = CreateButton("Close", new Point(480, 320), closeButton_Click);
            Controls.Add(closeButton);

            //Schaffe die Buddy-Grid, um dort die Einträge hineinzuschrieben.
            CreateBuddyGrid();

            // Lade die Einträge aus der buddy-list.txt
            LoadBuddyList();

            textRichTextBox = CreateRichTextBox(new Point(10, 10), new Size(550, 120), readOnly: true);
            //Controls.Add(textRichTextBox);

            chatRichTextBox = CreateRichTextBox(new Point(10, 150), new Size(550, 100), readOnly: false);
            chatRichTextBox.KeyPress += ChatRichTextBox_KeyPress;
            //Controls.Add(chatRichTextBox);

            isOnButton = CreateButton(new Point(400, 320));
            isOnButton.Enabled = false;
            isOnButton.BackColor = Color.Red;
            Controls.Add(isOnButton);

        }

        private void CreateBuddyGrid()
        {
            // DataGridView initialisieren
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false, // Keine neuen Zeilen durch den Benutzer
                RowHeadersVisible = false, // Keine Zeilenköpfe anzeigen
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill // Spaltenbreite automatisch anpassen
            };

            dataGridView.CellContentClick += dataGridView_CellContentClick;

            // Spalten hinzufügen
            var checkColumn = new DataGridViewCheckBoxColumn
            {
                HeaderText = "Auswahl",
                Name = "CheckColumn",
                Width = 50
            };
            var nameColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Buddy Name",
                Name = "NameColumn",
                Width = 150
            };
            var addressColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Onion-Adresse",
                Name = "AddressColumn",
                Width = 300
            };

            dataGridView.Columns.Add(checkColumn);
            dataGridView.Columns.Add(nameColumn);
            dataGridView.Columns.Add(addressColumn);

            // DataGridView zur Form hinzufügen
            Controls.Add(dataGridView);
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


        private ComboBox CreateComboBox(Point location)
        {
            return new ComboBox
            {
                Location = location,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList // Nur auswählbare Einträge
            };
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

 
        private void LoadBuddyList()
        {
            try
            {
                // Pfad zur buddy-list.txt bestimmen
                //string filePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\MyCalendar\buddy-list.txt"));

                string filePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"buddy-list.txt"));

                if (File.Exists(filePath))
                {
                    // Datei auslesen und Einträge zur ComboBox hinzufügen
                    string[] buddies = File.ReadAllLines(filePath);
                    int me = 0;
                    foreach (string buddy in buddies)
                    {
                        // Zeile in Adresse und Name aufteilen
                        var parts = buddy.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            string address = parts[0];
                            string name = parts[1];

                            // Neue Zeile zur DataGridView hinzufügen
                            if (me < 1)
                            {
                                dataGridView.Rows.Add(true, name, address); // Erste Spalte Checkbox, dann Name, dann Adresse
                                DataGridViewCheckBoxCell checkBoxCell = (DataGridViewCheckBoxCell)dataGridView.Rows[0].Cells[0];
                                checkBoxCell.ReadOnly = true;

                            }
                            else {
                                dataGridView.Rows.Add(false, name, address); // Erste Spalte Checkbox, dann Name, dann Adresse
                            }
                        }
                        me++;
                    }
                }
                else
                {
                    MessageBox.Show("Die Datei buddy-list.txt wurde nicht gefunden.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Buddy-Liste: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void closeButton_Click(object sender, EventArgs e)
        {
            isCloseButtonClicked = true;
            Close();
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

        private async void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Prüfen, ob eine Checkbox-Spalte angeklickt wurde (0 = linkeste Spalte)
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                DataGridViewCheckBoxCell checkBoxCell = (DataGridViewCheckBoxCell)dataGridView.Rows[e.RowIndex].Cells[0];
                bool isChecked = (bool)checkBoxCell.Value;

                string buddyOnion = dataGridView.Rows[e.RowIndex].Cells[2].Value.ToString(); // Rechte Spalte = Onion-Adresse

                if (isChecked)
                {
                    // Verbindung starten
                    await StartChat(buddyOnion);
                }
                else
                {
                    // Verbindung trennen
                    CloseChat(buddyOnion);
                }
            }
        }

        //private void OnFormClosing(object sender, FormClosingEventArgs e)
        //{
        //    if (!isCloseButtonClicked)
        //    {
        //        e.Cancel = true;
        //    }
        //}
    }
}
