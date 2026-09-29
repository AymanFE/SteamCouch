using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
namespace TVLounge {
 public static class SteamClient {
  public static bool IsBigPictureOpen() {
   bool found=false;
   Native.EnumWindows(delegate(IntPtr handle,IntPtr unused) {
    if(!Native.IsWindowVisible(handle))return true;
    var title=new System.Text.StringBuilder(512);Native.GetWindowText(handle,title,title.Capacity);
    string text=title.ToString();
    if(text.IndexOf("Big Picture",StringComparison.OrdinalIgnoreCase)<0&&!text.StartsWith("SP BPM",StringComparison.OrdinalIgnoreCase))return true;
    uint pid;Native.GetWindowThreadProcessId(handle,out pid);
    try {using(var process=Process.GetProcessById((int)pid)) {if(process.ProcessName=="steam"||process.ProcessName=="steamwebhelper"){found=true;return false;}}}
    catch(ArgumentException){}catch(InvalidOperationException){}
    return true;
   },IntPtr.Zero);
   return found;
  }
  public static void ExitBigPicture(int timeoutSeconds) {
   // Do not start a stopped Steam client just to send a close request.
   var processes=Process.GetProcessesByName("steam");
   if(processes.Length==0)return;
   string executable;
   try {executable=processes[0].MainModule.FileName;}
   finally {foreach(var process in processes)process.Dispose();}
   // Ask Steam itself to leave GamepadUI. No process or game is terminated.
   using(var request=Process.Start(new ProcessStartInfo(executable,"steam://close/bigpicture"){UseShellExecute=false,CreateNoWindow=true})){}
   var timer=Stopwatch.StartNew();
   while(IsBigPictureOpen()) {
    if(timer.Elapsed.TotalSeconds>=Math.Max(1,Math.Min(timeoutSeconds,15)))throw new InvalidOperationException("Steam did not exit Big Picture. Finish any in-game dialog, then retry Restore desktop.");
    Thread.Sleep(250);
   }
   Storage.Log("Steam Big Picture exited; Steam was not shut down.");
  }
 }
}
