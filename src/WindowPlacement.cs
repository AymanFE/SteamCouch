using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace TVLounge {
internal static class WindowPlacement {
 [StructLayout(LayoutKind.Sequential)] internal struct Rect { public int Left,Top,Right,Bottom; public Rectangle Bounds {get{return Rectangle.FromLTRB(Left,Top,Right,Bottom);}} }
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct MonitorInfo {public int Size;public Rect Monitor,Work;public uint Flags;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string Device;}
 delegate bool MonitorCallback(IntPtr monitor,IntPtr dc,IntPtr rect,IntPtr data);
 [DllImport("user32.dll")] static extern bool EnumDisplayMonitors(IntPtr dc,IntPtr clip,MonitorCallback callback,IntPtr data);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern bool GetMonitorInfo(IntPtr monitor,ref MonitorInfo info);
 [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr window,out Rect rect);
 internal sealed class PhysicalScope : IDisposable {
  readonly IntPtr previous=Native.SetThreadDpiAwarenessContext(new IntPtr(-4));
  public PhysicalScope(){if(previous==IntPtr.Zero)throw new InvalidOperationException("Windows could not enable physical display coordinates.");}
  public void Dispose(){Native.SetThreadDpiAwarenessContext(previous);}
 }
 public static Rectangle? MonitorBounds(string device){using(var scope=new PhysicalScope()){Rectangle? result=null;EnumDisplayMonitors(IntPtr.Zero,IntPtr.Zero,delegate(IntPtr monitor,IntPtr dc,IntPtr rect,IntPtr data){var info=new MonitorInfo{Size=Marshal.SizeOf(typeof(MonitorInfo))};if(GetMonitorInfo(monitor,ref info)&&string.Equals(device,info.Device,StringComparison.OrdinalIgnoreCase)){result=info.Monitor.Bounds;return false;}return true;},IntPtr.Zero);return result;}}
 public static bool Fit(IntPtr window,string device){using(var scope=new PhysicalScope()){var monitor=MonitorBounds(device);if(!monitor.HasValue)return false;Rect current;var bounds=monitor.Value;if(GetWindowRect(window,out current)&&current.Bounds==bounds)return true;
  // Both native monitor and window APIs run in the same physical-pixel context.
  if(!Native.SetWindowPos(window,IntPtr.Zero,bounds.X,bounds.Y,bounds.Width,bounds.Height,0x14))return false;
  return GetWindowRect(window,out current)&&current.Bounds==bounds;
 }}
 public static void Test(){using(var scope=new PhysicalScope())using(var form=new Form{FormBorderStyle=FormBorderStyle.None,ShowInTaskbar=false,AutoScaleMode=AutoScaleMode.None}){var handle=form.Handle;int count=0;foreach(var screen in Screen.AllScreens){var bounds=MonitorBounds(screen.DeviceName);if(!bounds.HasValue||!Fit(handle,screen.DeviceName))throw new InvalidOperationException("Physical window placement failed: "+screen.DeviceName);Rect actual;if(!GetWindowRect(handle,out actual)||actual.Bounds!=bounds.Value)throw new InvalidOperationException("Window exceeded monitor bounds.");count++;}System.IO.File.WriteAllText(Storage.PathOf("placement-test.txt"),"PASS: physical-pixel placement on "+count+" connected monitors; caller DPI context restored.");}}
}
}
