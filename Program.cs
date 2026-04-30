using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ytdlpWrapper
{
    internal class Program
    {


        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: No URL provided.");
                return;
            }

            // Získání URL z argumentů
            string url = args[0];

            // Fix divného chování - z protokolu vypadne dvojtečka
            url = url.Replace("https//", "https://");

            // Odstranění prefixu "ytdlp://"
            if (url.StartsWith("ytdlp://"))
            {
                url = url.Replace("ytdlp://", "");
                Console.WriteLine($"Trying to download {url}");
            }

            // Pokud je URL playlist, zeptej se uživatele, že opravdu chce pokračovat
            if (url.Contains("&list="))
            {
                Console.WriteLine();
                Console.WriteLine("URL is playlist, all videos contained in it will be downloaded.");
                Console.WriteLine("Do you want to continue? [y/N]");
                string consoleInput = Console.ReadLine();
                if (!String.IsNullOrEmpty(consoleInput) && consoleInput.ToLowerInvariant().Equals("y"))
                {
                    Console.WriteLine();
                    Console.WriteLine("Proceeding...");
                }
                else { return; }
            }

            // Pokud je URL stejná jako posledně, stáhneme nový yt-dlp
            if (YtDlpHelper.IsSameUrl(url))
            {
                Console.WriteLine("URL is the same as last. Trying to download latest yt-dlp...");
                YtDlpHelper.DownloadLatestYtDlp();
            }
            else
            {
                Console.WriteLine("Good, URL is different.");
                YtDlpHelper.SaveLastUrl(url);
            }


            // Příprava příkazu pro yt-dlp.exe
            string ytDlpCommand = $"-o \"H:\\Video\\youtube.com\\%(title)s.%(ext)s\" -f \"bestvideo[height<=4000][ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\" \"{url}\"";
            Console.WriteLine("yt-dlp.exe " + ytDlpCommand);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(intercept: true);
            Console.WriteLine();

            // Spuštění nové instance Windows Terminalu a předání příkazu pro yt-dlp.exe.
            // Přepínač "-w new" vynutí novou instanci okna i pokud už WT běží.
            // Shell příkaz je potřeba zabalit do cmd /k, aby jej Windows Terminal správně převzal.
            string escapedYtDlpCommand = ytDlpCommand.Replace("\"", "\\\"");
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "wt.exe",
                WorkingDirectory = "H:\\Video\\youtube.com",
                Arguments = $"-w new new-tab --startingDirectory \"H:\\Video\\youtube.com\" cmd /k \"H:\\Video\\youtube.com\\yt-dlp.exe {escapedYtDlpCommand}\"",
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                // Spuštění procesu
                using (Process process = new Process())
                {
                    process.StartInfo = psi;
                    process.EnableRaisingEvents = true;
                    process.Exited += new EventHandler(myProcess_Exited);
                    process.Start();

                    // Čekání na dokončení procesu
                    Console.WriteLine("yt-dlp started successfully as Admin.");
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void myProcess_Exited(object sender, EventArgs e)
        {
            Thread.Sleep(1500);

            string processName = "yt-dlp"; // Název procesu bez přípony
            int? processId = GetProcessIdByName(processName);

            if (processId.HasValue)
            {
                while (IsProcessRunning(processId.Value))
                {
                    Thread.Sleep(100);
                }

                // Zavolání funkce pro otevření složky
                OpenDownloadedVideoInWindowsExplorer();
            }
        }

        static void OpenDownloadedVideoInWindowsExplorer()
        {
            string folderPath = @"H:\Video\youtube.com\";
            string fileExtension = "*.mp4";

            try
            {
                // Získání všech video souborů v dané složce
                var directoryInfo = new DirectoryInfo(folderPath);
                var files = directoryInfo.GetFiles(fileExtension);

                // Získání posledního vytvořeného souboru
                var latestFile = files.OrderByDescending(f => f.CreationTime).FirstOrDefault();

                if (latestFile != null)
                {
                    // Otevření složky ve Windows Exploreru a vybrání posledního souboru
                    Process.Start("explorer.exe", $"/select,\"{latestFile.FullName}\"");
                }
                else
                {
                    Console.WriteLine("No video files found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        static int? GetProcessIdByName(string processName)
        {
            // Načtení všech procesů
            Process[] processes = Process.GetProcesses();

            // Pokud najdeme procesy, vrátíme ID prvního
            var ytdlpProc = processes.FirstOrDefault<Process>(p => p.ProcessName.ToLowerInvariant().Contains(processName.ToLowerInvariant()));
            
            if (ytdlpProc != null) return ytdlpProc.Id;
            return null; // Žádný proces nenalezen
        }
        static bool IsProcessRunning(int processId)
        {
            try
            {
                // Získání procesu podle ID
                using (Process process = Process.GetProcessById(processId))
                {
                    // Pokud je proces nalezen a jeho MainWindowHandle je nulový, proces je stále spuštěn
                    return !process.HasExited;
                }
            }
            catch (ArgumentException)
            {
                // ArgumentException se vyvolá, pokud proces s tímto ID neexistuje
                return false;
            }
        }
    }
}
