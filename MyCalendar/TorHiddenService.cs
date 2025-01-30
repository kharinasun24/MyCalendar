using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace MyCalendar
{
    public class TorHiddenService
    {
        private const string TorControlHost = "127.0.0.1";
        private const int TorControlPort = 9051;
        private const string HiddenServiceDir = "tor_hidden_service"; // Relativer Pfad

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