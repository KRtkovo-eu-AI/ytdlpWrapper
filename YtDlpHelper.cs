using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace ytdlpWrapper
{
    class YtDlpHelper
    {
        static readonly string LastUrlPath = "H:\\Video\\youtube.com\\last_url.txt";
        static readonly string YtDlpPath = "H:\\Video\\youtube.com\\yt-dlp.exe";

        public static void SaveLastUrl(string url)
        {
            File.WriteAllText(LastUrlPath, url.Trim());
        }

        public static bool IsSameUrl(string url)
        {
            if (!File.Exists(LastUrlPath))
                return false;

            string lastUrl = File.ReadAllText(LastUrlPath).Trim();
            return lastUrl == url.Trim();
        }

        public static void DownloadLatestYtDlp()
        {
            WebClient client = null;
            try
            {
                Console.WriteLine("🔄 Zjišťuji nejnovější verzi yt-dlp...");

                // Získání přesměrování z "latest" na konkrétní verzi
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://github.com/yt-dlp/yt-dlp/releases/latest");
                request.Method = "HEAD";
                request.AllowAutoRedirect = false;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string redirectUrl = response.Headers["Location"];
                response.Close(); // C# 7.3 neumožňuje `using`, tak musíme ručně zavřít

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    Console.WriteLine("❌ Nepodařilo se zjistit nejnovější verzi.");
                    return;
                }

                // Extrahování verze z URL
                Match match = Regex.Match(redirectUrl, @"/tag/([^/]+)$");
                string version = match.Success ? match.Groups[1].Value : null;

                if (string.IsNullOrWhiteSpace(version))
                {
                    Console.WriteLine("❌ Nepodařilo se zjistit verzi.");
                    return;
                }

                Console.WriteLine("✅ Nejnovější verze: " + version);

                // Složení URL pro stažení
                string downloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/" + version + "/yt-dlp.exe";
                Console.WriteLine("⬇️ Stahuji: " + downloadUrl);

                // Stáhnutí souboru
                client = new WebClient();
                client.DownloadFile(downloadUrl, YtDlpPath);

                Console.WriteLine("✅ yt-dlp.exe byl úspěšně aktualizován.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Chyba při aktualizaci yt-dlp: " + ex.Message);
            }
            finally
            {
                // Uvolnění WebClient
                if (client != null)
                    client.Dispose();
            }
        }
    }
}
