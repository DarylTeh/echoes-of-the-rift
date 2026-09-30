using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

static class Launcher
{
    [STAThread]
    static int Main(string[] args)
    {
        bool first;
        using (var mutex = new Mutex(true, "Local\\EchoesOfTheRiftLauncher", out first))
        {
            if (!first) { MessageBox.Show("Echoes of the Rift is already running.", "Echoes of the Rift"); return 0; }
            string root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CookieRaid");
            if (!Directory.Exists(root)) root = @"C:\Users\daryl\OneDrive\文档\ChatGPT\Daryl\CookieRaid";
            try
            {
                var info = new ProcessStartInfo("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -File \"" + Path.Combine(root, "Tools", "Play.ps1") + "\"" + (Array.IndexOf(args, "--test") >= 0 ? " -Test" : ""));
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                info.WorkingDirectory = root;
                using (var process = Process.Start(info))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        string error = Path.Combine(root, "Logs", "Launcher", "error.txt");
                        if (Array.IndexOf(args, "--test") < 0) MessageBox.Show(File.Exists(error) ? File.ReadAllText(error) : "Could not start the game. See CookieRaid/Logs/Launcher.", "Echoes of the Rift");
                    }
                    return process.ExitCode;
                }
            }
            catch (Exception e) { MessageBox.Show(e.Message, "Echoes of the Rift"); return 1; }
        }
    }
}
