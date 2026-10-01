using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

static class ServerLauncher
{
    static string ProjectRoot()
    {
        string baseDir=AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
        DirectoryInfo parentInfo=Directory.GetParent(baseDir);string parent=parentInfo==null?null:parentInfo.FullName;
        string[] candidates={baseDir,Path.Combine(baseDir,"CookieRaid"),parent,Path.Combine(parent??string.Empty,"CookieRaid")};
        foreach(string candidate in candidates)if(!string.IsNullOrWhiteSpace(candidate)&&File.Exists(Path.Combine(candidate,"Tools","Run-Dedicated.ps1")))return candidate;
        return Path.Combine(baseDir,"CookieRaid");
    }

    [STAThread] static int Main(string[] args)
    {
        bool first;using(var mutex=new Mutex(true,"Local\\EchoesOfTheRiftServer",out first))
        {
            if(!first){MessageBox.Show("The server control window is already open.","Echoes of the Rift Server");return 0;}
            string root=ProjectRoot();
            string control=Path.Combine(root,"Logs","ServerControl",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(control);
            bool test=Array.IndexOf(args,"--test")>=0,stopping=false,allowClose=false,ready=false;
            var form=new Form{Text="Echoes of the Rift Server",ClientSize=new Size(480,180),StartPosition=FormStartPosition.CenterScreen};
            if(test)form.Shown+=(s,e)=>form.Hide();
            var label=new Label{Text="Starting the local server...",Location=new Point(24,24),Size=new Size(430,80)};
            var stop=new Button{Text="Stop server",Location=new Point(24,120),Size=new Size(180,36)};form.Controls.Add(label);form.Controls.Add(stop);
            var info=new ProcessStartInfo("powershell.exe","-NoProfile -ExecutionPolicy Bypass -File \""+Path.Combine(root,"Tools","Run-Dedicated.ps1")+"\" -ControlDirectory \""+control+"\" -OwnerProcessId "+Process.GetCurrentProcess().Id){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=root};
            using(var process=Process.Start(info))
            using(var timer=new System.Windows.Forms.Timer{Interval=500})
            {
                Action requestStop=()=>{if(stopping)return;stopping=true;File.WriteAllText(Path.Combine(control,"stop"),"");label.Text="Stopping the server and saving its database...";stop.Enabled=false;};
                stop.Click+=(s,e)=>requestStop();form.FormClosing+=(s,e)=>{if(!allowClose){e.Cancel=true;requestStop();}};
                timer.Tick+=(s,e)=>{
                    if(File.Exists(Path.Combine(control,"ready"))&&!ready){ready=true;label.Text="Server is running on this PC.\nOpen Play Echoes of the Rift to play.\nKeep this window open while players are connected.";if(test)requestStop();}
                    if(process.HasExited){allowClose=true;timer.Stop();if(!test&&process.ExitCode!=0)MessageBox.Show("Server startup failed. Check CookieRaid/Logs/Dedicated.","Echoes of the Rift Server");form.Close();}
                };
                timer.Start();Application.Run(form);return ready&&process.ExitCode==0?0:1;
            }
        }
    }
}
