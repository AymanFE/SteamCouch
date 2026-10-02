using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

namespace TVLounge {
public class Settings {
 public string MonitorId = "";
 public string AudioId = "";
 public string Launcher = "Steam";
 public string SteamPath = @"C:\Program Files (x86)\Steam\steam.exe";
 public bool KeepOthers = true, LaunchSteam = true, Startup = false;
 public int TimeoutSeconds = 30;
 public uint Modifiers = 3;
 public int Key = (int)Keys.F12;
}
public class Row : Dictionary<string,string> {
 public string Get(string key) { return ContainsKey(key) ? this[key] : ""; }
}
public class Snapshot {
 public List<Row> Monitors = new List<Row>();
 public string[] Audio = new string[3];
 public string DisplayFile;
 public string Launcher;
}
public static class Storage {
 public static string Root = AppDomain.CurrentDomain.BaseDirectory;
 public static string Data = Path.Combine(Root,"data");
 public static string PathOf(string name) { Directory.CreateDirectory(Data); return Path.Combine(Data,name); }
 public static void Save<T>(string name,T value) {
  string path=PathOf(name), temp=path+".tmp";
  File.WriteAllText(temp,new JavaScriptSerializer().Serialize(value));
  if(File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path);
 }
 public static T Read<T>(string name) { return new JavaScriptSerializer().Deserialize<T>(File.ReadAllText(PathOf(name))); }
 public static void Log(string value) { File.AppendAllText(PathOf("activity.log"),DateTime.Now.ToString("s")+" "+value+Environment.NewLine); }
}
public interface IDevices {
 List<Row> Monitors(); List<Row> Audio();
 void SaveDisplays(string file); void LoadDisplays(string file); void ReconnectDisplays();
 void Enable(string id); void Primary(string id); void Disable(string id); void SetAudio(string id,int role);
 void ValidateXbox();void EnterXbox(string monitor,int timeout);void ExitXbox(int timeout);
 void Steam(string path); void ExitBigPicture(int timeout); bool PlaceSteam(string monitor); void Pause(int milliseconds);
}
public class Devices : IDevices {
 string display=Path.Combine(Storage.Root,@"tools\display\MultiMonitorTool.exe");
 string audio=Path.Combine(Storage.Root,@"tools\audio\SoundVolumeView.exe");
 public static string Quote(string value) {
  if(value.IndexOf('"')>=0 || value.IndexOf('\n')>=0 || value.IndexOf('\r')>=0) throw new ArgumentException("Invalid quote or newline in setting.");
  return "\""+value.ReplaceTrailingSlashes()+"\"";
 }
 public static void Run(string exe,string arguments) {
  if(!File.Exists(exe)) throw new FileNotFoundException("Required file missing: "+exe);
  using(var p=Process.Start(new ProcessStartInfo(exe,arguments){UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden})) {
   if(!p.WaitForExit(20000)) { p.Kill(); p.WaitForExit(); throw new TimeoutException(Path.GetFileName(exe)+" timed out."); }
   // NirSoft commands do not reliably signal failed hardware changes by exit code.
   // All mutations are verified against a fresh exported inventory by Engine.
  }
 }
 List<Row> Read(string tool,string name,string options) {
  string file=Storage.PathOf(name);
  if(File.Exists(file)) File.Delete(file);
  Run(tool,options+" /scomma "+Quote(file));
  var rows=new List<Row>();
  using(var parser=new TextFieldParser(file)) {
   parser.SetDelimiters(","); parser.HasFieldsEnclosedInQuotes=true;
   string[] headers=parser.ReadFields();
   if(headers==null) throw new IOException("Device inventory was empty.");
   while(!parser.EndOfData) { string[] values=parser.ReadFields(); var row=new Row(); for(int i=0;i<headers.Length && i<values.Length;i++) row[headers[i]]=values[i]; rows.Add(row); }
  }
  return rows;
 }
 public List<Row> Monitors() { return Read(display,"monitors.csv","/HideInactiveMonitors 0 /ShowDisconnectedMonitors 1"); }
 public List<Row> Audio() { return Read(audio,"audio.csv","/ShowUnpluggedDevices 1 /ShowDisabledDevices 1 /SaveFileEncoding 3").Where(r=>r.Get("Type")=="Device" && r.Get("Direction")=="Render").ToList(); }
 public void SaveDisplays(string file) { NativeDisplay.Save(file+".ccd.json"); Run(display,"/SaveConfig "+Quote(file)); if(!File.Exists(file)||new FileInfo(file).Length==0) throw new IOException("Could not save display restore point."); }
 public void LoadDisplays(string file) { if(File.Exists(file+".ccd.json")) { try { NativeDisplay.Restore(file+".ccd.json"); } catch(Exception e) { Storage.Log("Native restore fallback: "+e.Message); } } if(!File.Exists(file)) throw new IOException("Display restore point missing."); Run(display,"/LoadConfig "+Quote(file)); }
 public void ReconnectDisplays() { Run(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"DisplaySwitch.exe"),"/extend"); }
 public void Enable(string id) { Run(display,"/enable "+Quote(id)); }
 public void Primary(string id) { Run(display,"/SetPrimary "+Quote(id)); }
 public void Disable(string id) { Run(display,"/disable "+Quote(id)); }
 public void SetAudio(string id,int role) { Run(audio,"/SetDefault "+Quote(id)+" "+role); }
 public void Pause(int milliseconds) { Thread.Sleep(milliseconds); }
 public void ValidateXbox(){XboxClient.Validate();} public void EnterXbox(string monitor,int timeout){XboxClient.Enter(monitor,timeout);} public void ExitXbox(int timeout){XboxClient.Set(false,timeout);}
 public void ExitBigPicture(int timeout) { SteamClient.ExitBigPicture(timeout); }
 public void Steam(string path) { Process.Start(new ProcessStartInfo(path,"steam://open/bigpicture"){UseShellExecute=true}); }
 public bool PlaceSteam(string monitor) { IntPtr previous=Native.SetThreadDpiAwarenessContext(new IntPtr(-2)); try { bool found=false;
  // Move only the Big Picture window, never chat, store, or game windows.
  Native.EnumWindows(delegate(IntPtr h,IntPtr unused) {
   if(!Native.IsWindowVisible(h)) return true;
   var title=new System.Text.StringBuilder(512); Native.GetWindowText(h,title,title.Capacity);
   string t=title.ToString();
   if(t.IndexOf("Big Picture",StringComparison.OrdinalIgnoreCase)<0 && !t.StartsWith("SP BPM",StringComparison.OrdinalIgnoreCase)) return true;
   uint pid; Native.GetWindowThreadProcessId(h,out pid);
   try { using(var p=Process.GetProcessById((int)pid)) {
    if(p.ProcessName!="steam" && p.ProcessName!="steamwebhelper") return true;
    var screen=Screen.AllScreens.FirstOrDefault(s=>s.DeviceName==monitor);
    if(screen!=null) { var b=screen.Bounds; found=Native.SetWindowPos(h,IntPtr.Zero,b.X,b.Y,b.Width,b.Height,0x14); }
   }} catch(ArgumentException) { }
   return true;
  },IntPtr.Zero); return found; } finally { if(previous!=IntPtr.Zero)Native.SetThreadDpiAwarenessContext(previous); }
 }
}
public static class Extensions {
 public static string ReplaceTrailingSlashes(this string s) { int n=s.Length-s.TrimEnd('\\').Length; return s+new string('\\',n); }
}
public class Engine {
 public static string[] Roles={"Default","Default Multimedia","Default Communications"};
 IDevices d;
 public Action<string> Status=delegate{};
 public Engine(IDevices devices) { d=devices; }
 public bool Active { get { return File.Exists(Storage.PathOf("restore.json")); } }
 static string Id(Row r) { return r.Get("Monitor ID"); }
 public static bool IsOn(Row r) { return r.Get("Active")=="Yes"; }
 public static string[] Defaults(List<Row> audio) { return Roles.Select(role=> { var r=audio.SingleOrDefault(a=>a.Get(role)=="Render"); return r==null?null:r.Get("Item ID"); }).ToArray(); }
 void Wait(Func<bool> condition,int seconds,string message) {
  var end=DateTime.UtcNow.AddSeconds(seconds);
  do { if(condition()) return; d.Pause(600); } while(DateTime.UtcNow<end);
  throw new InvalidOperationException(message);
 }
 public void Activate(Settings s) {
  if(Active) throw new InvalidOperationException("Restore desktop first; a restore point is already present.");
  if(s.TimeoutSeconds<3 || s.TimeoutSeconds>120) throw new InvalidOperationException("Readiness timeout must be 3 to 120 seconds.");
  var monitors=d.Monitors(); var target=monitors.SingleOrDefault(r=>Id(r)==s.MonitorId);
  if(target==null || string.IsNullOrEmpty(s.MonitorId)) throw new InvalidOperationException("Select an available TV in Settings. Check its cable and power.");
  if(!monitors.Any(IsOn)) throw new InvalidOperationException("No active desktop display detected.");
  if(s.Launcher!="Steam"&&s.Launcher!="Xbox")throw new InvalidOperationException("Choose Steam Big Picture or Xbox mode in Settings.");
  if(s.Launcher=="Xbox"&&s.Modifiers==8&&s.Key==(int)Keys.F11)throw new InvalidOperationException("Choose a SteamCouch shortcut other than Xbox mode's Win + F11.");
  if(s.LaunchSteam&&s.Launcher=="Xbox")d.ValidateXbox();
  if(s.LaunchSteam && s.Launcher=="Steam" && !File.Exists(s.SteamPath)) throw new InvalidOperationException("Select the Steam executable in Settings.");
  var beforeAudio=d.Audio(); var defaults=Defaults(beforeAudio);
  if(defaults.Any(string.IsNullOrEmpty)) throw new InvalidOperationException("Could not identify all current playback defaults; no settings were changed.");
  var snapshot=new Snapshot{Monitors=monitors,Audio=defaults,DisplayFile=Storage.PathOf("desktop.cfg"),Launcher=s.LaunchSteam?s.Launcher:"Steam"};
  d.SaveDisplays(snapshot.DisplayFile); Storage.Save("restore.json",snapshot);
  try {
   Status("Connecting TV..."); d.Enable(s.MonitorId);
   Wait(()=>d.Monitors().Any(r=>Id(r)==s.MonitorId && IsOn(r)),s.TimeoutSeconds,"Windows could not enable the TV. Check its power and cable.");
   Status("Making TV the main display..."); d.Primary(s.MonitorId);
   Wait(()=>d.Monitors().Any(r=>Id(r)==s.MonitorId && r.Get("Primary")=="Yes"),s.TimeoutSeconds,"Windows could not make the TV primary.");
   if(!s.KeepOthers) {
    foreach(var m in d.Monitors().Where(r=>Id(r)!=s.MonitorId && IsOn(r))) d.Disable(Id(m));
    Wait(()=>d.Monitors().Where(IsOn).All(r=>Id(r)==s.MonitorId),s.TimeoutSeconds,"Some other monitors remained enabled.");
   } else {
    if(monitors.Where(IsOn).Any(old=>!d.Monitors().Any(now=>Id(now)==Id(old)&&IsOn(now)))) throw new InvalidOperationException("An existing monitor was unexpectedly disabled.");
   }
   Status("Waiting for TV audio..."); string audioId=null;
   Wait(()=> {
    var now=d.Audio().Where(a=>a.Get("Device State")=="Active").ToList();
    if(!string.IsNullOrEmpty(s.AudioId)) { audioId=now.Where(a=>a.Get("Item ID")==s.AudioId).Select(a=>a.Get("Item ID")).SingleOrDefault(); }
    else {
     var matches=now.Where(a=>!string.IsNullOrEmpty(target.Get("Monitor Name")) && string.Equals(a.Get("Name"),target.Get("Monitor Name"),StringComparison.OrdinalIgnoreCase)).ToList();
     if(matches.Count==1) audioId=matches[0].Get("Item ID");
     else { var newDevices=now.Where(a=>!beforeAudio.Any(b=>b.Get("Item ID")==a.Get("Item ID")&&b.Get("Device State")=="Active")).ToList(); if(newDevices.Count==1) audioId=newDevices[0].Get("Item ID"); }
    }
    return audioId!=null;
   },s.TimeoutSeconds,"TV audio was unavailable or ambiguous. Enable the TV in Windows, then refresh Settings and select its audio output.");
   for(int role=0;role<3;role++) d.SetAudio(audioId,role);
   Wait(()=>Defaults(d.Audio()).All(id=>id==audioId),s.TimeoutSeconds,"Windows did not switch playback audio to the TV.");
   if(s.LaunchSteam&&s.Launcher=="Xbox"){Status("Opening Xbox mode on your TV...");var tv=d.Monitors().Single(r=>Id(r)==s.MonitorId);d.EnterXbox(tv.Get("Name"),s.TimeoutSeconds);}
   if(s.LaunchSteam&&s.Launcher=="Steam") {
    Status("Opening Steam Big Picture..."); d.Steam(s.SteamPath);
    Wait(()=> { var tv=d.Monitors().Single(r=>Id(r)==s.MonitorId); if(tv.Get("Primary")!="Yes") d.Primary(s.MonitorId); return d.PlaceSteam(tv.Get("Name")); },s.TimeoutSeconds,"Steam Big Picture did not become ready. Finish any Steam update or sign-in, then try again.");
   }
   Storage.Log("TV mode activated; keep other monitors="+s.KeepOthers); Status("TV mode is on. Press the shortcut again to restore desktop.");
  } catch(Exception error) {
   Storage.Log("Activation failed: "+error.Message);
   try { Restore(s.TimeoutSeconds); } catch(Exception restore) { throw new InvalidOperationException(error.Message+"\nRestore also needs attention: "+restore.Message); }
   throw new InvalidOperationException(error.Message+"\nYour previous setup was restored.",error);
  }
 }
 public void Restore(int timeout) {
  var errors=new List<string>();
  Snapshot snapshot=Active?Storage.Read<Snapshot>("restore.json"):null;bool xbox=snapshot!=null&&snapshot.Launcher=="Xbox";Status(xbox?"Exiting Xbox mode...":"Exiting Steam Big Picture...");
  try { if(xbox)d.ExitXbox(timeout);else d.ExitBigPicture(timeout); } catch(Exception e) { errors.Add(e.Message); }
  if(!Active) {
   if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));
   Status("Desktop mode is active. Steam Big Picture is closed."); return;
  }

  Status("Restoring desktop displays...");
  try {
   d.LoadDisplays(snapshot.DisplayFile);
   if(!Matches(snapshot.Monitors,d.Monitors())) { Status("Reconnecting sleeping desktop displays..."); d.ReconnectDisplays(); d.Pause(2000); d.LoadDisplays(snapshot.DisplayFile); }
   Wait(()=>Matches(snapshot.Monitors,d.Monitors()),timeout,"Display restoration did not match the saved layout.");
  } catch(Exception e) { errors.Add(e.Message); }
  Status("Restoring desktop audio...");
  try {
   for(int role=0;role<3;role++) { int index=role;
    Wait(()=>d.Audio().Any(a=>a.Get("Item ID")==snapshot.Audio[index] && a.Get("Device State")=="Active"),timeout,"Original audio device is not available.");
    d.SetAudio(snapshot.Audio[index],index);
   }
   Wait(()=>Defaults(d.Audio()).SequenceEqual(snapshot.Audio),timeout,"Playback audio could not be restored.");
  } catch(Exception e) { errors.Add(e.Message); }
  if(errors.Count>0) throw new InvalidOperationException(string.Join("\n",errors)+"\nRestore point retained. Reconnect devices and click Restore desktop to retry.");
  File.Delete(Storage.PathOf("restore.json")); Storage.Log("Desktop restored."); Status("Desktop restored. Ready for TV mode.");
 }
 public static bool Matches(List<Row> saved,List<Row> actual) {
  foreach(var old in saved) {
   var now=actual.FirstOrDefault(r=>Id(r)==Id(old));
   if(now==null) { if(IsOn(old)) return false; else continue; }
   if(IsOn(old)!=IsOn(now)) return false;
   if(IsOn(old) && new[]{"Primary","Resolution","Left-Top","Frequency","Orientation"}.Any(k=>old.Get(k)!=now.Get(k))) return false;
  }
  return !actual.Any(r=>IsOn(r)&&!saved.Any(s=>Id(s)==Id(r)&&IsOn(s)));
 }
}
public static class Native {
 [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr h,int id,uint modifiers,uint key);
 [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr h,int id);
 [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
 public delegate bool WindowProc(IntPtr h,IntPtr value);
 [DllImport("user32.dll")] public static extern bool EnumWindows(WindowProc proc,IntPtr value);
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr h,System.Text.StringBuilder text,int count);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h,out uint id);
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int width,int height,uint flags);
}
public class Choice {
 public string Id,Label;
 public Choice(string id,string label) { Id=id; Label=label; }
 public override string ToString() { return Label; }
}
public static class Program {
 [STAThread] public static int Main(string[] args) {
  Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
  bool created; using(var mutex=new Mutex(true,(args.Contains("--ui-test")||args.Contains("--self-test")?@"Local\SteamCouch.Preview":@"Local\TVLounge.SingleInstance"),out created)) {
   if(!created) { if(args.Contains("--tray"))return 0; MessageBox.Show("SteamCouch is already running. Open Settings from its system tray icon.","SteamCouch"); return 1; }
   try {
    if(args.Contains("--self-test")) { SelfTest.Run(); StartupRegistration.Test(); return 0; }
    if(args.Contains("--ui-test")) { string oldData=Storage.Data; string preview=Path.Combine(oldData,"ui-preview"); Directory.CreateDirectory(preview); foreach(string name in new[]{"settings.json","restore.json"}) { string from=Path.Combine(oldData,name),to=Path.Combine(preview,name); if(File.Exists(from))File.Copy(from,to,true); else if(File.Exists(to))File.Delete(to); } Storage.Data=preview; }
    var d=new Devices();
    if(args.Contains("--native-validate")) { NativeDisplay.Save(Storage.PathOf("native-validation.json")); return 0; }
    if(args.Contains("--diagnose")) { Storage.Save("diagnostics.json",new{Monitors=d.Monitors(),Audio=d.Audio()}); return 0; }
    var s=File.Exists(Storage.PathOf("settings.json"))?Storage.Read<Settings>("settings.json"):new Settings();
    if(args.Contains("--xbox-mode-test")){bool before=XboxClient.Active;try{XboxClient.Set(true,10);if(!XboxClient.Active)throw new Exception("Xbox did not enter");XboxClient.Set(false,10);File.WriteAllText(Storage.PathOf("xbox-mode-test.txt"),"PASS: actual Xbox-mode entry and exit verified through the Windows gaming-experience API.");}finally{if(XboxClient.Active!=before)XboxClient.Set(before,10);}return 0;}
    if(args.Contains("--startup-sync")){StartupRegistration.Set(s.Startup);return 0;}
    if(args.Contains("--steam-exit-test")) {
     var running=Process.GetProcessesByName("steam"); if(running.Length==0)throw new InvalidOperationException("Start Steam before running this test."); var ids=running.Select(p=>p.Id).ToArray();foreach(var process in running)process.Dispose();
     try {
      d.Steam(s.SteamPath);var watch=Stopwatch.StartNew();while(!SteamClient.IsBigPictureOpen()){if(watch.Elapsed.TotalSeconds>30)throw new TimeoutException("Big Picture did not open for the test.");Thread.Sleep(250);}
      d.ExitBigPicture(s.TimeoutSeconds);
      var after=Process.GetProcessesByName("steam");bool same=after.Any(p=>ids.Contains(p.Id));foreach(var process in after)process.Dispose();
      if(SteamClient.IsBigPictureOpen()||!same)throw new InvalidOperationException("Steam exit verification failed.");
      File.WriteAllText(Storage.PathOf("big-picture-exit-test.txt"),"PASS: opened and detected a real Steam Big Picture window, closed it through Steam's exit command, and confirmed the original Steam process remained running.");
     } finally {if(SteamClient.IsBigPictureOpen())d.ExitBigPicture(s.TimeoutSeconds);}
     return 0;
    }
    if(args.Contains("--restore")) { new Engine(d).Restore(s.TimeoutSeconds); return 0; }
    if(args.Contains("--hardware-test")) {
     var engine=new Engine(d); if(engine.Active) throw new InvalidOperationException("Restore existing session before testing.");
     s.LaunchSteam=false;
     try { engine.Activate(s); engine.Restore(s.TimeoutSeconds); File.WriteAllText(Storage.PathOf("hardware-test.txt"),"PASS: enabled TV, primary display, selected TV audio, and restored displays and all playback defaults."); }
     finally { if(engine.Active) engine.Restore(s.TimeoutSeconds); }
     return 0;
    }
    if(!args.Contains("--ui-test"))s.Startup=StartupRegistration.Enabled; var form=new MainForm(s,d); if(args.Contains("--ui-test")) { var timer=new System.Windows.Forms.Timer{Interval=4000}; timer.Tick+=delegate { timer.Stop();try{form.WritePreview();}catch(Exception error){File.WriteAllText(Storage.PathOf("preview-error.txt"),error.ToString());Environment.ExitCode=1;}finally{Application.Exit();} }; timer.Start(); } Application.Run(form); return Environment.ExitCode;
   } catch(Exception e) { Storage.Log(e.ToString()); File.WriteAllText(Storage.PathOf("last-error.txt"),e.ToString()); if(!args.Any(a=>a.StartsWith("--")))MessageBox.Show(e.Message,"SteamCouch"); return 1; }
  }
 }
}
public class FakeDevices : IDevices {
 public List<Row> Displays; public List<Row> Outputs; List<Row> original; public bool XboxOpen,FailXboxExit,FailXboxEnter,XboxUnavailable;public int XboxCalls; public bool FailAudio,FailRestore,NeedsReconnect,FailSteamExit,BigPictureOpen; public int SteamCalls,ExitCalls; public List<string> Events=new List<string>();
 public FakeDevices() {
  Displays=new List<Row>{new Row{{"Monitor ID","desk"},{"Name","desktop"},{"Active","Yes"},{"Primary","Yes"},{"Resolution","1920 X 1080"},{"Left-Top","0, 0"}},new Row{{"Monitor ID","tv"},{"Name","tv"},{"Monitor Name","Beyond TV"},{"Active","No"},{"Primary","No"}}};
  Outputs=new List<Row>{new Row{{"Item ID","speakers"},{"Name","Speakers"},{"Device State","Active"},{"Default","Render"},{"Default Multimedia","Render"},{"Default Communications","Render"}}};
 }
 static List<Row> Copy(List<Row> rows) { return rows.Select(r=> { var n=new Row(); foreach(var p in r)n.Add(p.Key,p.Value); return n; }).ToList(); }
 public List<Row> Monitors(){return Copy(Displays);} public List<Row> Audio(){return Copy(Outputs);}
 public void SaveDisplays(string path){original=Copy(Displays); File.WriteAllText(path,"test");}
 public void LoadDisplays(string path){Events.Add("restore-displays");if(FailRestore)throw new Exception("Simulated restore failure"); if(!NeedsReconnect)Displays=Copy(original);}
 public void ReconnectDisplays(){NeedsReconnect=false;}
 public void Enable(string id){Displays.Single(r=>r.Get("Monitor ID")==id)["Active"]="Yes"; Outputs.Add(new Row{{"Item ID","tv-audio"},{"Name","Beyond TV"},{"Device State","Active"}});}
 public void Primary(string id){foreach(var r in Displays)r["Primary"]=r.Get("Monitor ID")==id?"Yes":"No";}
 public void Disable(string id){Displays.Single(r=>r.Get("Monitor ID")==id)["Active"]="No";}
 public void SetAudio(string id,int role){if(FailAudio && id=="tv-audio")throw new Exception("Simulated audio failure"); foreach(var r in Outputs)r[Engine.Roles[role]]=r.Get("Item ID")==id?"Render":"";}
 public void ValidateXbox(){if(XboxUnavailable)throw new Exception("Xbox unavailable");}public void EnterXbox(string monitor,int timeout){if(FailXboxEnter)throw new Exception("Xbox entry failed");if(Displays.Single(r=>r.Get("Monitor ID")=="tv").Get("Primary")!="Yes"||!Engine.Defaults(Audio()).All(x=>x=="tv-audio"))throw new Exception("Xbox before devices");XboxCalls++;XboxOpen=true;}public void ExitXbox(int timeout){Events.Add("exit-xbox");if(FailXboxExit)throw new Exception("Xbox exit failed");XboxOpen=false;}
 public void Steam(string path){if(Displays.Single(r=>r.Get("Monitor ID")=="tv").Get("Primary")!="Yes"||!Engine.Defaults(Audio()).All(x=>x=="tv-audio"))throw new Exception("Steam launched before devices ready"); SteamCalls++;BigPictureOpen=true;}
 public void ExitBigPicture(int timeout){ExitCalls++;Events.Add("exit-bigpicture");if(FailSteamExit)throw new Exception("Simulated Steam exit failure");BigPictureOpen=false;} public bool PlaceSteam(string monitor){return true;} public void Pause(int milliseconds){}
}
public static class SelfTest {
 static void Assert(bool yes,string message){if(!yes)throw new Exception("FAIL: "+message);}
 public static void Run(){
  string data=Storage.Data; Storage.Data=Path.Combine(data,"selftest-"+Guid.NewGuid().ToString("N")); var report=new List<string>();
  try {
   foreach(bool keep in new[]{true,false}) {
    var fake=new FakeDevices(); var engine=new Engine(fake); var s=new Settings{MonitorId="tv",KeepOthers=keep,LaunchSteam=false,TimeoutSeconds=3};
    engine.Activate(s); Assert(engine.Active,"persistent recovery state"); Assert(Engine.IsOn(fake.Displays[0])==keep,"monitor checkbox"); Assert(fake.Displays[1].Get("Primary")=="Yes","TV primary"); Assert(Engine.Defaults(fake.Audio()).All(x=>x=="tv-audio"),"all audio roles");
    new Engine(fake).Restore(3); Assert(!engine.Active,"restart recovery clears marker"); Assert(Engine.IsOn(fake.Displays[0])&&!Engine.IsOn(fake.Displays[1]),"desktop restored"); Assert(Engine.Defaults(fake.Audio()).All(x=>x=="speakers"),"audio restored"); report.Add("PASS: "+(keep?"keep monitors":"TV only")+", audio switching, restart recovery");
   }
   var failing=new FakeDevices{FailAudio=true}; var failedEngine=new Engine(failing); bool threw=false;
   try{failedEngine.Activate(new Settings{MonitorId="tv",LaunchSteam=false,TimeoutSeconds=3});}catch{threw=true;}
   Assert(threw&&!failedEngine.Active&&!Engine.IsOn(failing.Displays[1]),"rollback on audio failure"); report.Add("PASS: activation failure rolls back");
   var retry=new FakeDevices(); var retryEngine=new Engine(retry); retryEngine.Activate(new Settings{MonitorId="tv",LaunchSteam=false,TimeoutSeconds=3}); retry.FailRestore=true;
   try{retryEngine.Restore(3);}catch{}
   Assert(retryEngine.Active,"failed restore retains recovery"); retry.FailRestore=false; retryEngine.Restore(3); Assert(!retryEngine.Active,"retry succeeds"); report.Add("PASS: failed restore retained and retried");
   var missing=new FakeDevices(); var missingEngine=new Engine(missing); threw=false;
   try{missingEngine.Activate(new Settings{MonitorId="missing",LaunchSteam=false});}catch{threw=true;}
   Assert(threw&&!missingEngine.Active,"missing TV does not mutate"); report.Add("PASS: missing TV rejected without changes");
   var reconnect=new FakeDevices{NeedsReconnect=true}; var re=new Engine(reconnect); re.Activate(new Settings{MonitorId="tv",LaunchSteam=true,SteamPath=Application.ExecutablePath,TimeoutSeconds=3}); Assert(reconnect.SteamCalls==1,"Steam started after device setup"); re.Restore(3); Assert(!re.Active&&!reconnect.NeedsReconnect,"reconnect fallback"); report.Add("PASS: Steam launch ordering and reconnect fallback");
   var steamExit=new FakeDevices(); var se=new Engine(steamExit); se.Activate(new Settings{MonitorId="tv",LaunchSteam=true,SteamPath=Application.ExecutablePath,TimeoutSeconds=3}); new Engine(steamExit).Restore(3);
   Assert(!steamExit.BigPictureOpen&&steamExit.ExitCalls==1,"Big Picture closes after app restart"); Assert(steamExit.Events.IndexOf("exit-bigpicture")<steamExit.Events.IndexOf("restore-displays"),"Steam exits before display restoration"); report.Add("PASS: Big Picture exits before desktop restore, including after app restart");
   var exitFailure=new FakeDevices{FailSteamExit=true};var ef=new Engine(exitFailure);ef.Activate(new Settings{MonitorId="tv",LaunchSteam=false,TimeoutSeconds=3});threw=false;
   try{ef.Restore(3);}catch{threw=true;}
   Assert(threw&&ef.Active&&Engine.IsOn(exitFailure.Displays[0])&&!Engine.IsOn(exitFailure.Displays[1])&&Engine.Defaults(exitFailure.Audio()).All(x=>x=="speakers"),"Steam exit failure still restores desktop and audio");exitFailure.FailSteamExit=false;ef.Restore(3);Assert(!ef.Active,"Steam exit retry clears recovery point");report.Add("PASS: Steam exit failure cannot prevent display/audio restoration; retry works");
   var alreadyDesktop=new FakeDevices{BigPictureOpen=true};new Engine(alreadyDesktop).Restore(3);Assert(!alreadyDesktop.BigPictureOpen&&!alreadyDesktop.Events.Contains("restore-displays"),"Desktop restore can close leftover Big Picture without touching displays");report.Add("PASS: leftover Big Picture closes without a saved TV session");
   var xbox=new FakeDevices();var xe=new Engine(xbox);xe.Activate(new Settings{MonitorId="tv",Launcher="Xbox",LaunchSteam=true,SteamPath="missing.exe",TimeoutSeconds=3});
   Assert(xbox.XboxCalls==1&&xbox.SteamCalls==0&&xbox.XboxOpen,"Xbox launches after TV/audio without requiring Steam");
   Assert(Storage.Read<Snapshot>("restore.json").Launcher=="Xbox","Xbox launcher persisted in restore point");
   new Engine(xbox).Restore(3);Assert(!xbox.XboxOpen&&!xe.Active&&xbox.Events.IndexOf("exit-xbox")<xbox.Events.IndexOf("restore-displays"),"Xbox exits before restoring displays after restart");
   report.Add("PASS: Xbox launcher selection, device ordering, persisted restart recovery");
   var xboxFail=new FakeDevices{FailXboxExit=true};var xf=new Engine(xboxFail);xf.Activate(new Settings{MonitorId="tv",Launcher="Xbox",LaunchSteam=true,TimeoutSeconds=3});
   try{xf.Restore(3);}catch{}Assert(xf.Active&&Engine.Defaults(xboxFail.Audio()).All(x=>x=="speakers")&&!Engine.IsOn(xboxFail.Displays[1]),"Xbox exit failure still restores devices");
   xboxFail.FailXboxExit=false;new Engine(xboxFail).Restore(3);Assert(!xf.Active&&!xboxFail.XboxOpen,"Xbox retry recovers");report.Add("PASS: Xbox exit failure restores devices and retains retry");
   var noXbox=new FakeDevices{XboxUnavailable=true};var nx=new Engine(noXbox);threw=false;try{nx.Activate(new Settings{MonitorId="tv",Launcher="Xbox",TimeoutSeconds=3});}catch{threw=true;}Assert(threw&&!nx.Active&&!Engine.IsOn(noXbox.Displays[1]),"Unsupported Xbox rejected before hardware changes");report.Add("PASS: unavailable Xbox rejected without display/audio changes");
   var entryXbox=new FakeDevices{FailXboxEnter=true};var ex=new Engine(entryXbox);threw=false;try{ex.Activate(new Settings{MonitorId="tv",Launcher="Xbox",TimeoutSeconds=3});}catch{threw=true;}Assert(threw&&!ex.Active&&!Engine.IsOn(entryXbox.Displays[1])&&Engine.Defaults(entryXbox.Audio()).All(x=>x=="speakers"),"Xbox entry failure rolls back");report.Add("PASS: Xbox entry failure rolls back displays and audio");
   Assert(Devices.Quote(@"C:\folder with spaces\")=="\"C:\\folder with spaces\\\\\"","Windows trailing slash quoting"); report.Add("PASS: argument quoting");
  } finally { Storage.Data=data; File.WriteAllLines(Storage.PathOf("self-test.txt"),report); }
 }
}
}











namespace TVLounge {
internal static class StartupRegistration {
 static string Executable {get{string canonical=System.IO.Path.Combine(Storage.Root,"SteamCouch.exe");return System.IO.File.Exists(canonical)?canonical:System.Windows.Forms.Application.ExecutablePath;}}
 const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
 const string ApprovedKey=@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
 public static bool Enabled {
  get {using(var run=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunKey)){
   string command=run==null?null:run.GetValue("SteamCouch") as string;
   string name="SteamCouch";
   if(command==null){command=run==null?null:run.GetValue("TVLounge") as string;name="TVLounge";}
   if(string.IsNullOrWhiteSpace(command))return false;
   using(var approved=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(ApprovedKey)){var bytes=approved==null?null:approved.GetValue(name) as byte[];if(bytes!=null&&bytes.Length>0&&(bytes[0]==3||bytes[0]==7))return false;}
   return command==Devices.Quote(Executable)+" --tray";
  }}
 }
 public static void Set(bool enabled){Write(enabled,Executable,RunKey,ApprovedKey);if(Enabled!=enabled)throw new System.IO.IOException("Windows could not update SteamCouch startup. Please try again.");}
 internal static void Write(bool enabled,string executable,string runPath,string approvedPath){
  using(var run=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(runPath)){
   if(enabled)run.SetValue("SteamCouch",Devices.Quote(executable)+" --tray",Microsoft.Win32.RegistryValueKind.String);else run.DeleteValue("SteamCouch",false);
   run.DeleteValue("TVLounge",false);
  }
  // Explicitly enabling in the app resets only this app's disabled startup marker.
  using(var approved=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(approvedPath,true)){if(approved!=null){approved.DeleteValue("TVLounge",false);if(enabled)approved.DeleteValue("SteamCouch",false);}}
 }
 public static void Test(){
  string root=@"Software\SteamCouch\StartupTest-"+System.Guid.NewGuid().ToString("N");
  try{
   using(var run=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(root+@"\Run"))run.SetValue("TVLounge","old.exe");
   Write(true,@"C:\Folder With Spaces\SteamCouch.exe",root+@"\Run",root+@"\Approved");
   using(var run=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(root+@"\Run"))if((string)run.GetValue("SteamCouch")!="\"C:\\Folder With Spaces\\SteamCouch.exe\" --tray"||run.GetValue("TVLounge")!=null)throw new System.Exception("Startup command or migration failed.");
   using(var approved=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(root+@"\Approved"))approved.SetValue("SteamCouch",new byte[]{3,0,0,0},Microsoft.Win32.RegistryValueKind.Binary);
   Write(true,@"C:\Folder With Spaces\SteamCouch.exe",root+@"\Run",root+@"\Approved");
   using(var approved=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(root+@"\Approved"))if(approved.GetValue("SteamCouch")!=null)throw new System.Exception("Startup enable failed.");
   Write(false,@"C:\Folder With Spaces\SteamCouch.exe",root+@"\Run",root+@"\Approved");
   using(var run=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(root+@"\Run"))if(run.GetValue("SteamCouch")!=null)throw new System.Exception("Startup disable failed.");
   System.IO.File.WriteAllText(Storage.PathOf("startup-test.txt"),"PASS: quoted command, legacy migration, enable, disable, and disabled-marker reset in an isolated registry key.");
  }finally{Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(root,false);}
 }
}
}
