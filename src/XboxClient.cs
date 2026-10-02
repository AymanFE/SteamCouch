using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
namespace TVLounge {
public static class XboxClient {
 const string Api="api-ms-win-gaming-experience-l1-1-0.dll";
 [DllImport(Api,ExactSpelling=true)][return:MarshalAs(UnmanagedType.Bool)] static extern bool IsGamingFullScreenExperienceSupported();
 [DllImport(Api,ExactSpelling=true)][return:MarshalAs(UnmanagedType.Bool)] static extern bool IsGamingFullScreenExperienceActive();
 [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
 [StructLayout(LayoutKind.Sequential)] struct Keyboard {public ushort Key,Scan;public uint Flags,Time;public UIntPtr Extra;}
 [StructLayout(LayoutKind.Explicit,Size=32)] struct InputData {[FieldOffset(0)]public Keyboard Keyboard;}
 [StructLayout(LayoutKind.Sequential)] struct Input {public uint Type;public InputData Data;}
 [DllImport("user32.dll",SetLastError=true)] static extern uint SendInput(uint count,Input[] inputs,int size);
 [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h,out Rect rect);
 [StructLayout(LayoutKind.Sequential)] struct Rect {public int Left,Top,Right,Bottom;}
 public static bool Supported {get{try{return IsGamingFullScreenExperienceSupported();}catch(DllNotFoundException){return false;}catch(EntryPointNotFoundException){return false;}}}
 public static bool Active {get{try{return IsGamingFullScreenExperienceActive();}catch(DllNotFoundException){return false;}catch(EntryPointNotFoundException){return false;}}}
 public static void Validate(){
  if(!Supported)throw new InvalidOperationException("Xbox mode is unavailable. Update Windows 11 and enable Xbox mode in Windows Settings > Gaming.");
  using(var key=Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("xbox"))if(key==null)throw new InvalidOperationException("Install the Xbox app from Microsoft Store before using Xbox mode.");
 }
 static Input Key(ushort key,bool up){return new Input{Type=1,Data=new InputData{Keyboard=new Keyboard{Key=key,Flags=up?2u:0u}}};}
 public static void Set(bool active,int timeout){
  if(Active==active)return;
  Validate();
  var watch=Stopwatch.StartNew();
  while(new[]{0x10,0x11,0x12,0x5b,0x5c}.Any(k=>(GetAsyncKeyState(k)&0x8000)!=0)){
   if(watch.Elapsed.TotalSeconds>5)throw new InvalidOperationException("Release the shortcut keys, then try again.");Thread.Sleep(50);
  }
  // Windows documents Win+F11 as its Xbox-mode entry/exit shortcut.
  var inputs=new[]{Key(0x5b,false),Key(0x7a,false),Key(0x7a,true),Key(0x5b,true)};
  if(SendInput(4,inputs,Marshal.SizeOf(typeof(Input)))!=4)throw new InvalidOperationException("Windows could not send the Xbox-mode shortcut.");
  watch.Restart();while(Active!=active){
   if(watch.Elapsed.TotalSeconds>timeout)throw new InvalidOperationException(active?"Xbox mode did not open. Enable it in Windows Settings > Gaming > Xbox mode, then try again.":"Xbox mode did not exit. Return to desktop with Win + F11, then retry Restore desktop.");
   Thread.Sleep(250);
  }
 }
 public static void Enter(string monitor,int timeout){
  Validate();Set(true,timeout);
  var watch=Stopwatch.StartNew();while(!OnScreen(monitor)){
   if(watch.Elapsed.TotalSeconds>timeout)throw new InvalidOperationException("Xbox mode opened but could not be verified on the selected TV. Return to desktop and try again.");Thread.Sleep(250);
  }
 }
 static bool OnScreen(string monitor){
  bool found=false;IntPtr previous=Native.SetThreadDpiAwarenessContext(new IntPtr(-2));
  try{
   var screen=Screen.AllScreens.FirstOrDefault(s=>s.DeviceName==monitor);if(screen==null||!screen.Primary)return false;
   Native.EnumWindows(delegate(IntPtr h,IntPtr unused){
    if(!Native.IsWindowVisible(h))return true;uint pid;Native.GetWindowThreadProcessId(h,out pid);
    try{using(var process=Process.GetProcessById((int)pid)){
     if(process.ProcessName!="XboxPcApp"&&process.ProcessName!="XboxPcAppCE")return true;
     Rect r;if(GetWindowRect(h,out r)){var b=screen.Bounds;found=r.Left<=b.Left+16&&r.Top<=b.Top+16&&r.Right>=b.Right-16&&r.Bottom>=b.Bottom-16;}
    }}catch(ArgumentException){}return !found;
   },IntPtr.Zero);return found;
  }finally{if(previous!=IntPtr.Zero)Native.SetThreadDpiAwarenessContext(previous);}
 }
}
}
