using System.Collections.Generic;

namespace MyCalendar
{
    public class CitySearch
    {
        // Statische Instanz für das Singleton
        private static CitySearch instance = null;

        // Lock-Objekt für Thread-Sicherheit
        private static readonly object padlock = new object();

        // Liste zur Speicherung der Städtenamen
        private List<string> cities;

        public List<string> Cities
        {
            get { return cities; }
            set { cities = value; }
        }

        // Privater Konstruktor verhindert die direkte Instanziierung
        private CitySearch(List<string> initialCities)
        {
            // Speichern der übergebenen Städte in der Liste
            cities = initialCities;
        }

        // Statische Methode, um die Singleton-Instanz zu erhalten
        public static CitySearch GetInstance(List<string> initialCities)
        {
            // Thread-sicherer Zugriff auf die Instanz
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new CitySearch(initialCities);
                }
                return instance;
            }
        }
    }

    public class GeoFile
    {
        public List<string> GetCities()
        {
            // Erstellen einer Liste mit Städtenamen
            List<string> cities = new List<string>();
            cities.Add("Abidjan");
            cities.Add("Abuja");
            cities.Add("Accra");
            cities.Add("Addis Abeba");
            cities.Add("Ahmedabad");
            cities.Add("Al-Chartum Bahri");
            cities.Add("Aleppo");
            cities.Add("Alexandria");
            cities.Add("Algier");
            cities.Add("Almaty");
            cities.Add("Amman");
            cities.Add("Ankara");
            cities.Add("Bagdad");
            cities.Add("Baku");
            cities.Add("Bamako");
            cities.Add("Bandung");
            cities.Add("Bangkok");
            cities.Add("Baoding");
            cities.Add("Baotou");
            cities.Add("Bekasi");
            cities.Add("Belo Horizonte");
            cities.Add("Bengaluru");
            cities.Add("Benin City");
            cities.Add("Berlin");
            cities.Add("10115 Berlin");
            cities.Add("10117 Berlin");
            cities.Add("10119 Berlin");
            cities.Add("10178 Berlin");
            cities.Add("Bogota");
            cities.Add("Brasilia");
            cities.Add("Brazzaville");
            cities.Add("Brisbane");
            cities.Add("Buenos Aires");
            cities.Add("Bukarest");
            cities.Add("Bursa");
            cities.Add("Busan");
            cities.Add("Cali");
            cities.Add("Cape Town (Kapstadt)");
            cities.Add("Caracas");
            cities.Add("Casablanca");
            cities.Add("Changchun");
            cities.Add("Changsha");
            cities.Add("Changzhou");
            cities.Add("Chengdu");
            cities.Add("Chennai");
            cities.Add("Chicago");
            cities.Add("Chittagong");
            cities.Add("Chongqing");
            cities.Add("Curitiba");
            cities.Add("Daegu");
            cities.Add("Dakar");
            cities.Add("Dalian");
            cities.Add("Damaskus");
            cities.Add("Daressalam");
            cities.Add("Datong");
            cities.Add("Delhi");
            cities.Add("Depok");
            cities.Add("Dhaka");
            cities.Add("Dongguan");
            cities.Add("Douala");
            cities.Add("Dschidda");
            cities.Add("Dubai");
            cities.Add("Ekurhuleni");
            cities.Add("Faisalabad");
            cities.Add("Fortaleza");
            cities.Add("Foshan");
            cities.Add("Fuzhou");
            cities.Add("Gazipur");
            cities.Add("Gizeh");
            cities.Add("Guangzhou");
            cities.Add("Guayaquil");
            cities.Add("Guiyang");
            cities.Add("Gujranwala");
            cities.Add("Haikou");
            cities.Add("Hamburg");
            cities.Add("Handan");
            cities.Add("Hangzhou");
            cities.Add("Hanoi");
            cities.Add("Harbin");
            cities.Add("Havanna");
            cities.Add("Hefei");
            cities.Add("Heidelberg");
            cities.Add("69115 Heidelberg");
            cities.Add("Ho-Chi-Minh-Stadt");
            cities.Add("Hohhot");
            cities.Add("Hongkong");
            cities.Add("Houston");
            cities.Add("Huizhou");
            cities.Add("Hyderabad");
            cities.Add("Ibadan");
            cities.Add("Incheon");
            cities.Add("Indore");
            cities.Add("Isfahan");
            cities.Add("Istanbul");
            cities.Add("Izmir");
            cities.Add("Jaipur");
            cities.Add("Jakarta");
            cities.Add("Jinan");
            cities.Add("Johannesburg");
            cities.Add("Kabul");
            cities.Add("Kairo");
            cities.Add("Kano");
            cities.Add("Kanpur");
            cities.Add("Kaohsiung");
            cities.Add("Karatschi");
            cities.Add("Karlsruhe");
            cities.Add("76131 Karlsruhe");
            cities.Add("76133 Karlsruhe");
            cities.Add("76135 Karlsruhe");
            cities.Add("76137 Karlsruhe");
            cities.Add("76229 Karlsruhe");
            cities.Add("Khartum");
            cities.Add("Kiew");
            cities.Add("Kinshasa");
            cities.Add("Kolkata (Kalkutta)");
            cities.Add("Kuala Lumpur");
            cities.Add("Kumasi");
            cities.Add("Kunming");
            cities.Add("Lagos");
            cities.Add("Lahore");
            cities.Add("Lanzhou");
            cities.Add("Lima");
            cities.Add("Linyi");
            cities.Add("Liuzhou");
            cities.Add("Lome");
            cities.Add("London");
            cities.Add("Los Angeles");
            cities.Add("Luanda");
            cities.Add("Lubumbashi");
            cities.Add("Lucknow");
            cities.Add("Luoyang");
            cities.Add("Lusaka");
            cities.Add("Madrid");
            cities.Add("Manaus");
            cities.Add("Manila");
            cities.Add("Maschhad");
            cities.Add("Mbuji-Mayi");
            cities.Add("Medan");
            cities.Add("Medellin");
            cities.Add("Mekka");
            cities.Add("Melbourne");
            cities.Add("Mexiko-Stadt");
            cities.Add("Minsk");
            cities.Add("Mogadischu");
            cities.Add("Mogadiscio");
            cities.Add("Moskau");
            cities.Add("Multan");
            cities.Add("Mumbai");
            cities.Add("Nagoya");
            cities.Add("Nagpur");
            cities.Add("Nairobi");
            cities.Add("Nanchang");
            cities.Add("Nanjing");
            cities.Add("Nanning");
            cities.Add("Nantong");
            cities.Add("Neu-Taipeh");
            cities.Add("New York City");
            cities.Add("Ningbo");
            cities.Add("Omdurman");
            cities.Add("Osaka");
            cities.Add("Ouagadougou");
            cities.Add("Paris");
            cities.Add("Peking");
            cities.Add("Perth");
            cities.Add("Peschawar");
            cities.Add("Philippsburg");
            cities.Add("76661 Philippsburg");
            cities.Add("Phnom Penh");
            cities.Add("Pjongjang");
            cities.Add("Port Harcourt");
            cities.Add("Pune");
            cities.Add("Qingdao");
            cities.Add("Quezon City");
            cities.Add("Quito");
            cities.Add("Rastatt");
            cities.Add("76437 Rastatt");
            cities.Add("Rangun");
            cities.Add("Rawalpindi");
            cities.Add("Riad");
            cities.Add("Rio de Janeiro");
            cities.Add("Rom");
            cities.Add("Salvador");
            cities.Add("Sanaa");
            cities.Add("Sankt Petersburg");
            cities.Add("Santa Cruz de la Sierra");
            cities.Add("Santiago de Chile");
            cities.Add("Santo Domingo");
            cities.Add("Sao Paulo");
            cities.Add("Sapporo");
            cities.Add("Seoul");
            cities.Add("Shanghai");
            cities.Add("Shantou");
            cities.Add("Shaoxing");
            cities.Add("Shenyang");
            cities.Add("Shenzhen");
            cities.Add("Shijiazhuang");
            cities.Add("Singapur");
            cities.Add("Surabaya");
            cities.Add("Surat");
            cities.Add("Suzhou (Jiangsu)");
            cities.Add("Sydney");
            cities.Add("Taichung");
            cities.Add("Tainan");
            cities.Add("Taipeh");
            cities.Add("Taiyuan");
            cities.Add("Tangerang");
            cities.Add("Tangshan");
            cities.Add("Taoyuan");
            cities.Add("Taschkent");
            cities.Add("Teheran");
            cities.Add("Tianjin");
            cities.Add("Tijuana");
            cities.Add("Tokio");
            cities.Add("Toronto");
            cities.Add("Tshwane (Pretoria)");
            cities.Add("Urumqi");
            cities.Add("Warsaw");
            cities.Add("Warschau");
            cities.Add("Weifang");
            cities.Add("Wenzhou");
            cities.Add("Wien");
            cities.Add("Wuhan");
            cities.Add("Wuxi");
            cities.Add("Xiamen");
            cities.Add("Xian");
            cities.Add("Xuzhou");
            cities.Add("Yaounde");
            cities.Add("Yokohama");
            cities.Add("Zhengzhou");
            cities.Add("Zhongshan");
            cities.Add("Zhuhai");
            cities.Add("Zibo");
            cities.Add("eThekwini (Durban)");

            // Erhalte die Singleton-Instanz mit der Liste
            CitySearch citySearch = CitySearch.GetInstance(cities);

            return citySearch.Cities;
        }
    }
}