using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace MyCalendar
{
    public class TorHiddenService
    {
        private const string TorControlHost = "127.0.0.1";
        private const int TorControlPort = 9051;
        private const string HiddenServiceDir = "tor_hidden_service"; // Relativer Pfad

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0; // Fenster ausblenden
        private const int SW_MINIMIZE = 1; // Fenster minimieren
        private const int SW_SHOWNOACTIVATE = 4; // Fenster anzeigen, aber nicht aktivieren


        public void StartTorService()
        {
            try
            {
                // 1. Tor-Prozess starten
                string torExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tor", "tor.exe");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = torExePath, //  TODO: Pfad zu Ihrer tor.exe-Datei ? 
                                          // Weitere Optionen für tor.exe, z.B. Konfigurationsdatei
                                          // Arguments = "-f torrc.conf",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true 
                };

                using (Process torProcess = new Process { StartInfo = startInfo })
                {
                    torProcess.Start();

                    /*
                    // Warten, bis das Fensterhandle verfügbar ist
                    torProcess.WaitForInputIdle();

                    if (torProcess.MainWindowHandle != IntPtr.Zero)
                    {
                        // Fenster in den Hintergrund verschieben (ausblenden oder minimieren)
                        ShowWindow(torProcess.MainWindowHandle, SW_HIDE);
                        // Oder:
                        // ShowWindow(torProcess.MainWindowHandle, SW_MINIMIZE);
                    }
                    */

                    // Ausgaben von Tor überwachen (optional)
                    torProcess.OutputDataReceived += (sender, e) => Console.WriteLine("Tor Output: " + e.Data);
                    torProcess.ErrorDataReceived += (sender, e) => Console.WriteLine("Tor Error: " + e.Data);

                    // 2. Warten, bis der Tor-Prozess beendet ist (optional)
                    // torProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Starten von Tor: " + ex.Message);
            }

            SetupHiddenService();
        }


        public void SetupHiddenService()
        {
            try
            {
                // 1. Verzeichnis erstellen (falls es noch nicht existiert)
                Directory.CreateDirectory(HiddenServiceDir); 

                using (TcpClient client = new TcpClient(TorControlHost, TorControlPort))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream))
                using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
                {
                    // Tor-Authentifizierung (ohne Passwort)
                    writer.WriteLine("AUTHENTICATE \"\"");
                    string response = reader.ReadLine();
                    if (!response.Contains("250"))
                    {
                        Console.WriteLine("Tor-Authentifizierung fehlgeschlagen!");
                        return;
                    }

                    // 2. Hidden Service erstellen (mit korrektem Pfad)
                    writer.WriteLine($"ADD_ONION {HiddenServiceDir} Port=80,5000"); // Korrigiert!
                    response = reader.ReadLine();

                    if (response.StartsWith("250 OK"))
                    {
                        Console.WriteLine("Hidden Service erfolgreich gestartet!");

                        // 3. Onion-Adresse auslesen
                        string onionAddress = File.ReadAllText(Path.Combine(HiddenServiceDir, "hostname")).Trim();
                        Console.WriteLine($"Deine .onion-Adresse: {onionAddress}");
                    }
                    else
                    {
                        Console.WriteLine("Fehler beim Starten des Hidden Services: " + response);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei der Hidden Service Konfiguration: " + ex.Message);
            }
        }
    }
}