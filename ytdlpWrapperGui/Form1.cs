using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ytdlpWrapperGui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void StartProcess(string url = "")
        {
            Process process = new Process();
            process.StartInfo.FileName = "yt-dlp.exe";
            if (String.IsNullOrEmpty(url)) process.StartInfo.Arguments = "--help"; // Příklady argumentů
            else process.StartInfo.Arguments = $"-f bestvideo[height<=4000][ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best {url}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.OutputDataReceived += (sender, args) => AppendOutput(args.Data);
            process.ErrorDataReceived += (sender, args) => AppendOutput(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        private void AppendOutput(string text)
        {
            if (text != null)
            {
                // Kontrola, zda je nutné použít Invoke
                if (richTextBox1.InvokeRequired)
                {
                    richTextBox1.Invoke(new Action<string>(AppendOutput), text);
                }
                else
                {
                    // Zde můžeš přidat logiku pro změnu barvy textu
                    richTextBox1.AppendText(text + Environment.NewLine);
                }
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            StartProcess();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartProcess(textBox1.Text);
        }
    }
}
