
namespace MyCalendar
{
    public class Doubles
    {
        public Doubles()
        {
        }

        private int LevenshteinDistance(string str1, string str2)
        {
            int len1 = str1.Length;
            int len2 = str2.Length;

            int[,] dp = new int[len1 + 1, len2 + 1];

            for (int i = 0; i <= len1; i++)
            {
                for (int j = 0; j <= len2; j++)
                {
                    if (i == 0)
                    {
                        dp[i, j] = j;
                    }
                    else if (j == 0)
                    {
                        dp[i, j] = i;
                    }
                    else if (str1[i - 1] == str2[j - 1])
                    {
                        dp[i, j] = dp[i - 1, j - 1];
                    }
                    else
                    {
                        dp[i, j] = 1 + Math.Min(dp[i - 1, j - 1],
                                               Math.Min(dp[i - 1, j],
                                                       dp[i, j - 1]));
                    }
                }
            }

            return dp[len1, len2];
        }

        private List<string[]> SortStringsByLevenshteinDistance(List<string> strings, int barrier)
        {
            // 1. Sortieren nach der Summe der Levenshtein-Distanzen
            strings.Sort((str1, str2) =>
            {
                int distanceSum1 = 0;
                int distanceSum2 = 0;

                foreach (var other in strings)
                {
                    if (str1 != other) distanceSum1 += LevenshteinDistance(str1, other);
                    if (str2 != other) distanceSum2 += LevenshteinDistance(str2, other);
                }

                return distanceSum1.CompareTo(distanceSum2);
            });

            // 2. Filtern: Nur Paare behalten, bei denen die Distanz zwischen 1 und 3 liegt
            List<string[]> filteredStrings =
                           new List<string[]>();


            for (int i = 0; i < strings.Count - 1; i++)
            {
                string current = strings[i];
                string next = strings[i + 1];
                int distance = LevenshteinDistance(current, next);

                if (distance > 0 && distance < barrier)
                {
                    filteredStrings.Add([current, next]);

                }
            }

            return filteredStrings;

        }



        //TODO: Ready for testing.
        public void ShowDoubles(int lev)
        {
            //Gegeben:
            List<string> names = AllContactNames();

            List<string[]> potentialDoubles = SortStringsByLevenshteinDistance(names, 2);


            string potentialDbls = "Potential double contacts: ";
            //Hier sind die Dubletten...
            foreach (string[] dbl in potentialDoubles)
            {
                string result = string.Join(", ", dbl);
                potentialDbls += result;
                potentialDbls += " ";
            }


            MessageBox.Show(potentialDbls);
        }

        private List<string> AllContactNames()
        {
            List<string> contactNames = new List<string>();

            ContactDao contactDao = new ContactDao();   

            List<Contact> contacts = contactDao.GetAllContacts();

            foreach (Contact c in contacts) {

                contactNames.Add(c.Name);
            }

            return contactNames;
        }
    }
}