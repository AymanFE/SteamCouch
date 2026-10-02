using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;
namespace TVLounge {
internal static class PlayniteClient {
 public static void Validate(string path){if(!File.Exists(path)||!new[]{"Playnite.FullscreenApp.exe","Playnite.DesktopApp.exe"}.Contains(Path.GetFileName(path),StringComparer.OrdinalIgnoreCase))throw new InvalidOperationException("Select Playnite.FullscreenApp.exe or Playnite.DesktopApp.exe in Couch options.");}
 public static bool Running {get{var processes=Process.GetProcessesByName("Playnite.FullscreenApp");try{return processes.Length>0;}finally{foreach(var p in processes)p.Dispose();}}}
 public static void Enter(string path,string monitor,int seconds){Validate(path);Process.Start(new ProcessStartInfo(path,"--startfullscreen --hidesplashscreen"){UseShellExecute=true});var end=DateTime.UtcNow.AddSeconds(seconds);do{if(Place(monitor))return;Thread.Sleep(250);}while(DateTime.UtcNow<end);throw new InvalidOperationException("Playnite fullscreen did not become ready. Finish its setup or update and try again.");}
 static bool Place(string monitor){var processes=Process.GetProcessesByName("Playnite.FullscreenApp");try{foreach(var p in processes){try{p.Refresh();if(p.MainWindowHandle!=IntPtr.Zero&&Native.IsWindowVisible(p.MainWindowHandle)&&WindowPlacement.Fit(p.MainWindowHandle,monitor))return true;}catch(InvalidOperationException){} }return false;}finally{foreach(var p in processes)p.Dispose();}}
 public static void Exit(string path,int seconds){if(!Running)return;Validate(path);Process.Start(new ProcessStartInfo(path,"--startdesktop"){UseShellExecute=true});var end=DateTime.UtcNow.AddSeconds(Math.Min(15,seconds));while(Running){if(DateTime.UtcNow>=end)throw new InvalidOperationException("Playnite did not return to desktop mode. Finish any dialog and retry Restore desktop.");Thread.Sleep(250);}}
}
}
