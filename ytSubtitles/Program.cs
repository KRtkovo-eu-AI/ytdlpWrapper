using System;
using System.Diagnostics;

namespace YoutubeSubtitlesWrapper
{
    internal class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
                {
                    Console.WriteLine("Error: No URL provided.");
                    return 1;
                }

                string rawArg = args[0].Trim();
                string youtubeUrl = ExtractYoutubeUrl(rawArg);

                if (string.IsNullOrWhiteSpace(youtubeUrl))
                {
                    Console.WriteLine("Error: Could not extract YouTube URL.");
                    return 1;
                }

                if (!LooksLikeYoutubeUrl(youtubeUrl))
                {
                    Console.WriteLine("Warning: URL does not look like a YouTube URL.");
                    Console.WriteLine("Continuing anyway...");
                }

                string targetUrl =
                    "https://www.downloadyoutubesubtitles.com/?u=" +
                    Uri.EscapeDataString(youtubeUrl);

                Console.WriteLine("Opening:");
                Console.WriteLine(targetUrl);

                var psi = new ProcessStartInfo
                {
                    FileName = targetUrl,
                    UseShellExecute = true
                };

                Process.Start(psi);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return 2;
            }
        }

        private static string ExtractYoutubeUrl(string input)
        {
            string url = input.Trim().Trim('"');

            // vlastní custom protocol
            const string customScheme = "ytsubs://";
            if (url.StartsWith(customScheme, StringComparison.OrdinalIgnoreCase))
            {
                url = url.Substring(customScheme.Length);
            }

            // oprava typického rozbití dvojtečky po předání z protokolu
            url = url.Replace("https//", "https://");
            url = url.Replace("http//", "http://");

            // některé browsery / registry mohou dodat navíc slash
            while (url.StartsWith("/"))
            {
                url = url.Substring(1);
            }

            return url;
        }

        private static bool LooksLikeYoutubeUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            string host = uri.Host.ToLowerInvariant();

            return host == "youtube.com"
                || host == "www.youtube.com"
                || host == "m.youtube.com"
                || host == "youtu.be"
                || host.EndsWith(".youtube.com");
        }
    }
}