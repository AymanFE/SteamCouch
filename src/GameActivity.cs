using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace TVLounge {
internal static class GameActivity {
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct ProcessEntry {public uint Size,Usage,Pid;public UIntPtr Heap;public uint Module,Threads,Parent;public int Priority;public uint Flags;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]public string Name;}
 [DllImport("kernel32.dll",SetLastError=true)]static extern IntPtr CreateToolhelp32Snapshot(uint flags,uint pid);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern bool Process32First(IntPtr snapshot,ref ProcessEntry entry);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern bool Process32Next(IntPtr snapshot,ref ProcessEntry entry);
 [DllImport("kernel32.dll")]static extern bool CloseHandle(IntPtr handle);
 public static HashSet<uint> LauncherIds(){var ids=new HashSet<uint>();foreach(var name in new[]{"steam","Playnite.FullscreenApp"}){var processes=Process.GetProcessesByName(name);try{foreach(var p in processes)ids.Add((uint)p.Id);}finally{foreach(var p in processes)p.Dispose();}}return ids;}
 public static bool Any(HashSet<uint> roots){var snapshot=CreateToolhelp32Snapshot(2,0);if(snapshot==new IntPtr(-1))return true;var entries=new List<ProcessEntry>();try{var p=new ProcessEntry{Size=(uint)Marshal.SizeOf(typeof(ProcessEntry))};if(!Process32First(snapshot,ref p))return true;do{entries.Add(p);}while(Process32Next(snapshot,ref p));}finally{CloseHandle(snapshot);}var parents=entries.ToDictionary(p=>p.Pid,p=>p.Parent);var harmless=new HashSet<string>(new[]{"steam.exe","steamwebhelper.exe","steamservice.exe","steamerrorreporter.exe","gameoverlayui.exe","crashpad_handler.exe","Playnite.FullscreenApp.exe","Playnite.DesktopApp.exe"},StringComparer.OrdinalIgnoreCase);foreach(var p in entries){if(harmless.Contains(p.Name)||roots.Contains(p.Pid))continue;uint parent=p.Parent;var visited=new HashSet<uint>();for(int i=0;i<32&&visited.Add(parent);i++){if(roots.Contains(parent))return true;if(!parents.TryGetValue(parent,out parent))break;}}return false;}
}
}
