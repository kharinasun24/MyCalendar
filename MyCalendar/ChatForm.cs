using System.Net.Sockets;
using System.Net.WebSockets;
using System.Net;
using System.Text;

namespace MyCalendar
{
    public partial class ChatForm : Form
    {
        //private string meOnionAddress;

        private DataGridView dataGridView;
        private Button closeButton;
        private Button isOnButton;
        private Dictionary<string, (ClientWebSocket, ChatWindow)> activeChats = new();

        public ChatForm()
        {
            Size = new Size(600, 400);

            InitializeComponents();

            CreateHiddenService();

            InitializeWebSockets();

            Task.Run(CheckBuddyStatusAsync);  
        }

   
        private void CreateHiddenService() {

            TorHiddenService ths = new TorHiddenService();

            ths.StartTorService();

            string myOnion = ths.OnionAddress;

            // Prüfen, ob Tor gestartet ist und eine .onion-Adresse existiert
            if (string.IsNullOrEmpty(myOnion))
            {
                MessageBox.Show("Fehler: Hidden Service konnte nicht gestartet werden!");
                return;
            }

            else
            {
                // Falls es geklappt hat, setze mich (erste Zeile) auf Online
                if (dataGridView.Rows.Count > 0)
                {
                    dataGridView.Rows[0].Cells["StatusColumn"].Value = "Online";
                    dataGridView.Refresh(); // Damit die Farben direkt aktualisiert werden
                }
            }


        }

        private void InitializeWebSockets()
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells["AddressColumn"].Value is string onionAddress && !string.IsNullOrEmpty(onionAddress))
                {
                    var webSocket = CreateTorWebSocket(); // Erstelle den WebSocket
                    activeChats[onionAddress] = (webSocket, null); // Noch kein ChatWindow
                }
            }
        }


        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task StartChat(string onionAddress)
        {
            if (!activeChats.ContainsKey(onionAddress)) return;

            // Falls ChatWindow existiert, einfach anzeigen
            if (activeChats[onionAddress].Item2 != null)
            {
                activeChats[onionAddress].Item2.Show();
                return;
            }

            var webSocket = activeChats[onionAddress].Item1;
            var chatWindow = new ChatWindow(onionAddress, webSocket);

            // Event registrieren, um das Chat-Fenster beim Schließen aus der Liste zu entfernen
            chatWindow.ChatClosed += (closedOnion) =>
            {
                if (activeChats.ContainsKey(closedOnion))
                {
                    activeChats.Remove(closedOnion);
                }
            };

            chatWindow.Show();
            activeChats[onionAddress] = (webSocket, chatWindow);
        }


        /*
        private async Task ReceiveMessages(ClientWebSocket webSocket, ChatWindow chatWindow)
    {
        byte[] buffer = new byte[1024];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // UI-Thread-Aufruf, um die Nachricht im Chat-Fenster anzuzeigen
                chatWindow.Invoke((MethodInvoker)(() => chatWindow.AppendMessage($"Buddy: {message}")));
            }
        }
        catch (Exception ex)
        {
            chatWindow.Invoke((MethodInvoker)(() => chatWindow.AppendMessage($"Verbindung getrennt: {ex.Message}")));
        }
    }
    */

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public ClientWebSocket CreateTorWebSocket()
        {
            var clientWebSocket = new ClientWebSocket();
            var options = clientWebSocket.Options;

            // Tor SOCKS5-Proxy (Standardport: 9050)
            options.Proxy = new WebProxy("socks5://127.0.0.1:9050");

            return clientWebSocket;
        }


        private void InitializeComponents()
        {
            closeButton = CreateButton("Close", new Point(480, 320), closeButton_Click);
            Controls.Add(closeButton);

            //Schaffe die Buddy-Grid, um dort die Einträge hineinzuschrieben.
            CreateBuddyGrid();

            // Lade die Einträge aus der buddy-list.txt
            LoadBuddyList();

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

            dataGridView.CurrentCellDirtyStateChanged += dataGridView_CurrentCellDirtyStateChanged;
            dataGridView.CellValueChanged += dataGridView_CellValueChanged;
            dataGridView.CellFormatting += DataGridView_CellFormatting;

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
            var statusColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Status",
                Name = "StatusColumn",
                Width = 100,
                ReadOnly = true // Nutzer kann Status nicht manuell ändern
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
            dataGridView.Columns.Add(statusColumn);

            // DataGridView zur Form hinzufügen
            Controls.Add(dataGridView);
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


     

        private void CloseChat(string onionAddress)
        {
            if (activeChats.TryGetValue(onionAddress, out var chatData))
            {
                var (webSocket, chatForm) = chatData;

                if (webSocket?.State == WebSocketState.Open)
                {
                    webSocket.Abort();
                }

                chatForm.Close(); // Korrektur hier!
                activeChats.Remove(onionAddress);
            }
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


        private void dataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView.CurrentCell is DataGridViewCheckBoxCell)
            {
                dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit); // Änderung sofort übernehmen
            }
        }



        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView.Columns[e.ColumnIndex].Name == "StatusColumn" && e.Value is string status)
            {
                if (status == "Online")
                {
                    e.CellStyle.BackColor = Color.Green;
                    e.CellStyle.ForeColor = Color.White;
                }
                else
                {
                    e.CellStyle.BackColor = Color.Red;
                    e.CellStyle.ForeColor = Color.White;
                }
            }
        }


        private async void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Prüfen, ob eine Checkbox-Spalte geändert wurde (0 = linkeste Spalte)
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                DataGridViewCheckBoxCell checkBoxCell = (DataGridViewCheckBoxCell)dataGridView.Rows[e.RowIndex].Cells[0];
                bool isChecked = (bool)checkBoxCell.Value;

                string buddyOnion = dataGridView.Rows[e.RowIndex].Cells[2].Value.ToString(); // Rechte Spalte = Onion-Adresse

                if (isChecked)
                {
                    await StartChat(buddyOnion); // Chat starten
                }
                else
                {
                    CloseChat(buddyOnion); // Chat schließen
                }
            }
        }

        private async void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async Task CheckBuddyStatusAsync()
        {
            while (true) // Dauerschleife für wiederholte Überprüfung
            {
                for (int i = 1; i < dataGridView.Rows.Count; i++) // Start bei 1, um die erste Zeile zu überspringen
                {
                    DataGridViewRow row = dataGridView.Rows[i];
                    if (row.Cells["AddressColumn"].Value is string onionAddress && !string.IsNullOrEmpty(onionAddress))
                    {
                        bool isOnline = await IsBuddyOnline(onionAddress);

                        row.Cells["StatusColumn"].Value = isOnline ? "Online" : "Offline";

                        // Farbe entsprechend anpassen
                        row.Cells["StatusColumn"].Style.BackColor = isOnline ? Color.Green : Color.Red;
                        row.Cells["StatusColumn"].Style.ForeColor = Color.White;
                    }
                }

                await Task.Delay(10000); // 10 Sekunden warten, bevor erneut geprüft wird
            }
        }

        private async Task<bool> IsBuddyOnline(string onionAddress)
        {
            try
            {
                using (var webSocket = new ClientWebSocket())
                {
                    webSocket.Options.Proxy = new WebProxy("socks5://127.0.0.1:9050");

                    var uri = new Uri($"ws://{onionAddress}:80"); // WebSocket-Verbindung zur Onion-Adresse
                    await webSocket.ConnectAsync(uri, CancellationToken.None);

                    return webSocket.State == WebSocketState.Open;
                }
            }
            catch
            {
                return false; // Verbindung fehlgeschlagen, also offline
            }
        }

    }
}
