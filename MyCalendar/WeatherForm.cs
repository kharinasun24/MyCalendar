using Newtonsoft.Json;
using System.Resources;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.DirectoryServices.ActiveDirectory;
using MyCalendar.logger;
using MyCalendar.logger;
using MyCalendar;

namespace MyCalendar
{
    public partial class WeatherForm : Form
    {
        ResourceManager resourceManager;

        LanguageDao languageDao;

        string location;

        Label temperatureLabel;
        Label maxLabel;
        Label minLabel;
        Label humidityLabel;
        Label windLabel;
        Label cloudLabel;

        Label temperatureLabelLabelForeCast;
        Label maxLabelForeCast;
        Label minLabelForeCast;
        Label humidityLabelLabelForeCast;
        Label windLabelLabelForeCast;
        Label cloudLabelLabelForeCast;

        Label foreCast;
        Label foreCastTDY;

        public WeatherForm(string loc)
        {
            Size = new Size(700, 400);

            InitializeCulture();

            location = loc;

            //Text = resourceManager.GetString("Weather data for") + " " + location;

            CurrentWeather();

        }

        private void CurrentWeather()
        {
            GetWeatherData();

            CreateLabels();

            CreateLabelsForecast();
        }

        private void InitializeCulture()
        {

            languageDao = new LanguageDao();

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


        private void CreateLabels()
        {
            temperatureLabel = new Label();
            temperatureLabel.Location = new Point(10, 10);
            temperatureLabel.AutoSize = true;

            maxLabel = new Label();
            maxLabel.Location = new Point(10, 50);
            maxLabel.AutoSize = true;

            minLabel = new Label();
            minLabel.Location = new Point(10, 90);
            minLabel.AutoSize = true;

            humidityLabel = new Label();
            humidityLabel.Location = new Point(10, 130);
            humidityLabel.AutoSize = true;

            windLabel = new Label();
            windLabel.Location = new Point(10, 170);
            windLabel.AutoSize = true;

            cloudLabel = new Label();
            cloudLabel.Location = new Point(10, 210);
            cloudLabel.AutoSize = true;

            Controls.Add(temperatureLabel);
            Controls.Add(maxLabel);
            Controls.Add(minLabel);
            Controls.Add(humidityLabel);
            Controls.Add(windLabel);
            Controls.Add(cloudLabel);

        }

        private async void GetWeatherData()
        {
            string apiKey = "968363cb2d0f2ddaad161e87ad250306";
            string city = location;

            using (var client = new HttpClient())
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
                HttpResponseMessage response = await client.GetAsync(url);

                string temperature = resourceManager.GetString("temperature");
                string max = resourceManager.GetString("maximal");
                string min = resourceManager.GetString("minimal");
                string humidity = resourceManager.GetString("humidity");
                string windspeed = resourceManager.GetString("windspeed");
                string winddirection = resourceManager.GetString("winddirection");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    dynamic weatherData = JsonConvert.DeserializeObject(json);
                    Rain(weatherData);

                    temperatureLabel.Text = $"{temperature}: {weatherData.main.temp} °C".Replace(',', '.');
                    maxLabel.Text = $"{max}: {weatherData.main.temp_max} °C".Replace(',', '.');
                    minLabel.Text = $"{min}: {weatherData.main.temp_min} °C".Replace(',', '.');
                    humidityLabel.Text = $"{humidity}: {weatherData.main.humidity}%";

                    // Windgeschwindigkeit und -richtung
                    string windSpeed = $"{windspeed}: {weatherData.wind.speed} kn";

                    //windDirection = $"{winddirection}: {weatherData.wind.deg}°";

                    string s = $"{weatherData.wind.deg}";

                    string windDirection = GetWindDirection(s);

                    windLabel.Text = windSpeed + " " + windDirection;

                    // Wolkenbedeckung
                    cloudLabel.Text = $"Wolken: {weatherData.clouds.all}%";

                    //SimpleLogger.Instance.Log($"{weatherData}");

                }

                else
                {
                    MessageBox.Show("Fehler beim Abrufen der Wetterdaten.");
                }



                url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric";
                response = await client.GetAsync(url);


                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    // Deserialisiere die Antwort in ein dynamisches Objekt
                    dynamic weatherData = JsonConvert.DeserializeObject(json);

                    // Extrahiere die Vorhersagen aus der "list"
                    JArray forecasts = weatherData.list;

                    // Finde die Vorhersage für den kommenden Tag (nächste 24 Stunden)
                    DateTime tomorrow = DateTime.UtcNow.AddDays(1).Date;

                    // Verwende JObject und caste den Wert von dt_txt
                    dynamic tomorrowForecast = forecasts.FirstOrDefault(f =>
                    {
                        DateTime forecastDate = DateTime.Parse((string)f["dt_txt"]);
                        return forecastDate.Date == tomorrow;
                    });

                    if (tomorrowForecast != null)
                    {
                        // Extrahiere Temperatur, gefühlte Temperatur und Luftfeuchtigkeit
                        string forecastTemp = (string)tomorrowForecast["main"]["temp"];
                        string forecastMax = (string)tomorrowForecast["main"]["temp_max"];
                        string forecastMin = (string)tomorrowForecast["main"]["temp_min"];
                        string forecastHumidity = (string)tomorrowForecast["main"]["humidity"];

                        // Update UI-Labels mit den Vorhersagedaten
                        temperatureLabelLabelForeCast.Text = $"{resourceManager.GetString("temperature")}: {forecastTemp} °C";
                        maxLabelForeCast.Text = $"{resourceManager.GetString("maximal")}: {forecastMax} °C";
                        minLabelForeCast.Text = $"{resourceManager.GetString("minimal")}: {forecastMin} °C";
                        humidityLabelLabelForeCast.Text = $"{resourceManager.GetString("humidity")}: {forecastHumidity}%";

                        // Windgeschwindigkeit und -richtung
                        string forecastWind = (string)tomorrowForecast["wind"]["speed"];
                        string windSpeed = $"{windspeed}: {forecastWind} kn";

                        //windDirection = $"{winddirection}: {weatherData.wind.deg}°";

                        string s = (string)tomorrowForecast["wind"]["deg"];

                        string windDirection = GetWindDirection(s);

                        windLabelLabelForeCast.Text = windSpeed + " " + windDirection;

                        // Wolkenbedeckung
                        string foreCastDeg = (string)tomorrowForecast["clouds"]["all"];
                        cloudLabelLabelForeCast.Text = $"Wolken: {foreCastDeg}%";

                        string rain = "";
                        if (tomorrowForecast["rain"] != null)
                        {
                            // Extrahiere den Regenwert (z.B. für die nächste Stunde oder 3 Stunden)
                            string rainAmount = tomorrowForecast["rain"]["3h"] != null ?
                                                tomorrowForecast["rain"]["3h"].ToString() :
                                                tomorrowForecast["rain"]["1h"].ToString();


                            rain = resourceManager.GetString("Tomorrow's rain") + $" {rainAmount} mm.";


                        }

                        if (string.IsNullOrEmpty(rain))
                        {
                            rain = "  🌦  :0";
                        }

                        humidityLabelLabelForeCast.Text = humidityLabelLabelForeCast.Text + " " + rain;

                    }
                    else
                    {
                        // Falls keine Vorhersage für morgen gefunden wird
                        SimpleLogger.Instance.Log("Keine Vorhersagedaten für morgen gefunden.");
                    }

                }

                else
                {
                    MessageBox.Show("Fehler beim Abrufen der Wetterdaten.");
                }



            }
        }

        private void Rain(dynamic weatherData)
        {
            string rainNextHour = resourceManager.GetString("Rain quantity next hour");
            if (weatherData.rain != null)
            {
                var rainData = JsonConvert.DeserializeObject<Dictionary<string, double>>($"{weatherData.rain}");


                // Zugriff auf den Wert von "1h"
                if (rainData.ContainsKey("1h"))
                {
                    double rainValue = rainData["1h"];
                    rainNextHour += $" {rainValue} mm";
                }

                /*
                foreach (var entry in rainData)
                {
                string key = entry.Key;  // Dies ist der Schlüssel (z.B. "1h")
                double value = entry.Value;  // Dies ist der zugehörige Wert (z.B. 0.46)

                // Zeige Schlüssel und Wert an
                MessageBox.Show($"Schlüssel: {key}, Wert: {value} mm");
                }
                 */
            }

            else
            {
                rainNextHour += " 0";
            }

            Text = resourceManager.GetString("Weather data for") + " " + location + " | " + rainNextHour;
        }

        private void CreateLabelsForecast()
        {
            temperatureLabelLabelForeCast = new Label();
            temperatureLabelLabelForeCast.Location = new Point(260, 10);
            temperatureLabelLabelForeCast.AutoSize = true;

            maxLabelForeCast = new Label();
            maxLabelForeCast.Location = new Point(260, 50);
            maxLabelForeCast.AutoSize = true;

            minLabelForeCast = new Label();
            minLabelForeCast.Location = new Point(260, 90);
            minLabelForeCast.AutoSize = true;


            humidityLabelLabelForeCast = new Label();
            humidityLabelLabelForeCast.Location = new Point(260, 130);
            humidityLabelLabelForeCast.AutoSize = true;

            windLabelLabelForeCast = new Label();
            windLabelLabelForeCast.Location = new Point(260, 170);
            windLabelLabelForeCast.AutoSize = true;

            cloudLabelLabelForeCast = new Label();
            cloudLabelLabelForeCast.Location = new Point(260, 210);
            cloudLabelLabelForeCast.AutoSize = true;

            foreCast = new Label();
            foreCast.Text = resourceManager.GetString("tomorrow");
            foreCast.Font = new Font("Arial", 14, FontStyle.Bold);
            foreCast.Location = new Point(260, 280);
            foreCast.AutoSize = true;

            foreCastTDY = new Label();
            foreCastTDY.Text = resourceManager.GetString("today");
            foreCastTDY.Font = new Font("Arial", 14, FontStyle.Bold);
            foreCastTDY.Location = new Point(10, 280);
            foreCastTDY.AutoSize = true;

            Controls.Add(temperatureLabelLabelForeCast);
            Controls.Add(minLabelForeCast);
            Controls.Add(maxLabelForeCast);
            Controls.Add(humidityLabelLabelForeCast);
            Controls.Add(windLabelLabelForeCast);
            Controls.Add(cloudLabelLabelForeCast);
            Controls.Add(foreCast);
            Controls.Add(foreCastTDY);

        }


        private string GetWindDirection(string windDirectionString)
        {
            // Zunächst versuche ich, den String in einen double-Wert umzuwandeln.
            if (!double.TryParse(windDirectionString, out double windDirectionDegrees))
            {
                return "Ungültige Eingabe"; // Oder eine andere Fehlermeldung
            }


            string[] directions = GetWindroseNames();
            windDirectionDegrees = windDirectionDegrees % 360;
            if (windDirectionDegrees < 0)
            {
                windDirectionDegrees += 360;
            }
            int index = (int)Math.Floor(windDirectionDegrees / 22.5);
            return directions[index % directions.Length];
        }

        private string[] GetWindroseNames()
        {
            string[] res = new string[] { "N", "NNO", "NO", "ONO", "O", "OSO", "SO", "SSO", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };

            string language = languageDao.GetCurrentLanguage();

            if ("English".Equals(language))
            {

                res = new string[] { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
            }

            return res;
        }
    }
}