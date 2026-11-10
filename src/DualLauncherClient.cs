using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
namespace TVLounge {
// Steam Dual Launcher integration: lets the user pick Clean or Modded Steam on the TV before Big Picture opens.
// Discovery uses %LOCALAPPDATA%\SteamDualLauncher\install.json, which the launcher writes on every start.
public static class DualLauncherClient {
 public const string Id="SteamDual";
 public const string Label="Steam Dual Launcher (Clean / Modded)";
 const string ProcessName="SteamDualLauncher.BigPicture";
 public static string RegistryFile {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),@"SteamDualLauncher\install.json");}}
 public static string Find(){
  try{
   if(File.Exists(RegistryFile)){
    var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(RegistryFile));
    object exe;if(data!=null&&data.TryGetValue("bigPictureExe",out exe)){var path=exe as string;if(!string.IsNullOrEmpty(path)&&File.Exists(path))return path;}
   }
  }catch(Exception){}
  foreach(var dir in new[]{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),@"Programs\SteamDualLauncher"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"SteamDualLauncher")}){
   var candidate=Path.Combine(dir,ProcessName+".exe");if(File.Exists(candidate))return candidate;
  }
  return null;
 }
 public static bool Available {get{return Find()!=null;}}
 public static void Validate(){if(Find()==null)throw new InvalidOperationException("Steam Dual Launcher was not found. Open it once (the Steam — Big Picture shortcut) so SteamCouch can detect it.");}
 static Process[] Selectors(){return Process.GetProcessesByName(ProcessName);}
 static bool SelectorRunning(){var p=Selectors();try{return p.Length>0;}finally{foreach(var x in p)x.Dispose();}}
 // Selector still open but stepped aside (minimized/hidden) after starting Steam — happens when "close after launch" is off.
 static bool SelectorDone(){var p=Selectors();try{return p.All(x=>{x.Refresh();var h=x.MainWindowHandle;return h==IntPtr.Zero||!Native.IsWindowVisible(h)||WindowState.IsMinimized(h);});}finally{foreach(var x in p)x.Dispose();}}
 public static void Close(){var p=Selectors();try{foreach(var x in p){try{x.CloseMainWindow();}catch(InvalidOperationException){}}}finally{foreach(var x in p)x.Dispose();}}
 // Opens the selector on the TV and returns once the user's chosen Steam is in Big Picture.
 public static void Enter(string monitor,int timeoutSeconds){
  string exe=Find();if(exe==null)Validate();
  using(Process.Start(new ProcessStartInfo(exe,"--display "+Devices.Quote(monitor)){UseShellExecute=false,WorkingDirectory=Path.GetDirectoryName(exe)})){}
  Storage.Log("Steam Dual Launcher opened on "+monitor);
  var watch=Stopwatch.StartNew();bool seen=false;TimeSpan? closedAt=null;
  int pick=Math.Max(600,timeoutSeconds);
  while(true){
   Thread.Sleep(300);
   bool running=SelectorRunning();bool bigPicture=SteamClient.IsBigPictureOpen();
   if(running){seen=true;closedAt=null;if(bigPicture&&SelectorDone())break;}
   else if(seen||watch.Elapsed.TotalSeconds>10){
    if(bigPicture)break;
    if(!closedAt.HasValue)closedAt=watch.Elapsed;
    if((watch.Elapsed-closedAt.Value).TotalSeconds>=Math.Max(5,timeoutSeconds))throw new InvalidOperationException("Steam Dual Launcher closed without opening Steam Big Picture.");
   }
   if(watch.Elapsed.TotalSeconds>=pick){Close();throw new InvalidOperationException("No Steam installation was chosen in time.");}
  }
  Storage.Log("Steam Dual Launcher handed off to Steam Big Picture.");
 }
}
static class WindowState {
 [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool IsIconic(IntPtr h);
 public static bool IsMinimized(IntPtr h){return IsIconic(h);}
}
}
