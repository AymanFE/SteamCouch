using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace TVLounge {
public sealed class MonitorPowerState {public string MonitorId,Device;public int Index;public uint Power;}
internal static class OtherMonitors {
 public static readonly string[] Modes={"Keep","Disconnect","PowerOff","Black"};
 public static string Resolve(Settings s){string mode=string.IsNullOrEmpty(s.OtherMonitorMode)?(s.KeepOthers?"Keep":"Disconnect"):s.OtherMonitorMode;if(!Modes.Contains(mode))throw new InvalidOperationException("Choose how other monitors behave in TV mode.");return mode;}
 public static string Label(string mode){return mode=="Disconnect"?"Disconnect other monitors":mode=="PowerOff"?"Turn off other monitors (DDC/CI)":mode=="Black"?"Black screens — keep connected":"Leave other monitors on";}
 public static IEnumerable<string> Covered(IEnumerable<string> displays,string tv){if(string.IsNullOrEmpty(tv))throw new InvalidOperationException("The TV display cannot be identified.");return displays.Where(name=>!string.Equals(name,tv,StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase);}
}
internal static class MonitorPower {
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct Physical {public IntPtr Handle;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string Description;}
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct Info {public int Size;public WindowPlacement.Rect Monitor,Work;public uint Flags;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string Device;}
 delegate bool Callback(IntPtr monitor,IntPtr dc,IntPtr rect,IntPtr data);
 [DllImport("user32.dll")]static extern bool EnumDisplayMonitors(IntPtr dc,IntPtr clip,Callback cb,IntPtr data);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern bool GetMonitorInfo(IntPtr h,ref Info info);
 [DllImport("dxva2.dll")]static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr h,out uint count);
 [DllImport("dxva2.dll")]static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr h,uint count,[Out]Physical[] monitors);
 [DllImport("dxva2.dll")]static extern bool DestroyPhysicalMonitor(IntPtr h);
 [DllImport("dxva2.dll")]static extern bool GetVCPFeatureAndVCPFeatureReply(IntPtr h,byte code,out uint type,out uint current,out uint maximum);
 [DllImport("dxva2.dll")]static extern bool SetVCPFeature(IntPtr h,byte code,uint value);
 static Dictionary<string,IntPtr> leases=new Dictionary<string,IntPtr>();
 static string Key(MonitorPowerState state){return state.MonitorId+":"+state.Index;}
 static Physical[] Find(string device){using(var scope=new WindowPlacement.PhysicalScope()){IntPtr found=IntPtr.Zero;EnumDisplayMonitors(IntPtr.Zero,IntPtr.Zero,delegate(IntPtr h,IntPtr dc,IntPtr rect,IntPtr data){var info=new Info{Size=Marshal.SizeOf(typeof(Info))};if(GetMonitorInfo(h,ref info)&&string.Equals(device,info.Device,StringComparison.OrdinalIgnoreCase)){found=h;return false;}return true;},IntPtr.Zero);uint count;if(found==IntPtr.Zero||!GetNumberOfPhysicalMonitorsFromHMONITOR(found,out count)||count==0||count>64)throw new InvalidOperationException("Monitor is unavailable for power control. Wake it using its power button and retry.");var result=new Physical[count];if(!GetPhysicalMonitorsFromHMONITOR(found,count,result))throw new InvalidOperationException("Windows could not open this monitor's power controls.");return result;}}
 public static List<MonitorPowerState> Capture(List<Row> displays,string tvId){Release();var result=new List<MonitorPowerState>();try{foreach(var row in displays.Where(r=>Engine.IsOn(r)&&r.Get("Monitor ID")!=tvId)){var physical=Find(row.Get("Name"));for(int i=0;i<physical.Length;i++){var state=new MonitorPowerState{MonitorId=row.Get("Monitor ID"),Device=row.Get("Name"),Index=i};leases.Add(Key(state),physical[i].Handle);}for(int i=0;i<physical.Length;i++){uint type,power,max;if(!GetVCPFeatureAndVCPFeatureReply(physical[i].Handle,0xd6,out type,out power,out max)||power!=1)throw new InvalidOperationException("This monitor does not expose usable DDC/CI power control. Enable DDC/CI in its own menu, or choose black screens/disconnect instead.");result.Add(new MonitorPowerState{MonitorId=row.Get("Monitor ID"),Device=row.Get("Name"),Index=i,Power=power});}}return result;}catch{Release();throw;}}
 public static void Set(MonitorPowerState state,uint power,List<Row> displays){if(power!=1&&power!=4)throw new InvalidOperationException("Unsupported monitor power request.");IntPtr h;if(!leases.TryGetValue(Key(state),out h)){var row=displays.FirstOrDefault(r=>r.Get("Monitor ID")==state.MonitorId);if(row==null)throw new InvalidOperationException("Wake the desktop monitor using its power button, then retry Restore desktop.");var physical=Find(row.Get("Name"));for(int i=0;i<physical.Length;i++)leases[ state.MonitorId+":"+i ]=physical[i].Handle;if(!leases.TryGetValue(Key(state),out h))throw new InvalidOperationException("The desktop monitor's power controls changed. Wake it manually and retry.");}if(!SetVCPFeature(h,0xd6,power))throw new InvalidOperationException(power==4?"Monitor rejected the power-off command. Try black screens instead.":"Monitor could not be awakened. Use its power button and retry Restore desktop.");}
 public static void Release(){foreach(var h in leases.Values.Distinct())DestroyPhysicalMonitor(h);leases.Clear();}
}
internal sealed class BlackScreen : Form {
 protected override bool ShowWithoutActivation {get{return true;}}
 protected override CreateParams CreateParams {get{var p=base.CreateParams;p.ExStyle|=0x08000000|0x80;return p;}}
 public BlackScreen(Action restore){Text="SteamCouch black screen";BackColor=Color.Black;FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;AutoScaleMode=AutoScaleMode.None;StartPosition=FormStartPosition.Manual;TopMost=true;DoubleClick+=delegate{restore();};var menu=new ContextMenuStrip();menu.Items.Add("Return to desktop",null,delegate{restore();});ContextMenuStrip=menu;FormClosed+=delegate{menu.Dispose();};}
 internal void Place(string device){bool fit=false;for(int attempt=0;attempt<3&&!fit;attempt++)fit=WindowPlacement.Fit(Handle,device);if(!fit)throw new InvalidOperationException("Could not fit black screen to "+device+"; requested="+WindowPlacement.MonitorBounds(device)+" actual="+Bounds+" maximum="+MaximumSize);Native.SetWindowPos(Handle,new IntPtr(-1),0,0,0,0,0x13);}
}
internal sealed class BlackScreens : IDisposable {
 readonly Dictionary<string,BlackScreen> windows=new Dictionary<string,BlackScreen>(StringComparer.OrdinalIgnoreCase);readonly Action restore;
 public BlackScreens(Action restore){this.restore=restore;}
 public void Update(string tv){using(var scope=new WindowPlacement.PhysicalScope()){var targets=OtherMonitors.Covered(Screen.AllScreens.Select(s=>s.DeviceName),tv).ToList();foreach(var old in windows.Keys.Where(k=>!targets.Contains(k,StringComparer.OrdinalIgnoreCase)).ToList()){windows[old].Close();windows.Remove(old);}try{foreach(var target in targets){BlackScreen form;if(!windows.TryGetValue(target,out form)){form=new BlackScreen(restore);windows.Add(target,form);form.Place(target);form.Show();}form.Place(target);}}catch{Dispose();throw;}}}
 public void Dispose(){foreach(var form in windows.Values){form.Close();form.Dispose();}windows.Clear();}
 public static void Test(){var selected=OtherMonitors.Covered(new[]{"desk1","TV","desk2","DESK1"},"tv").ToArray();if(!selected.SequenceEqual(new[]{"desk1","desk2"}))throw new Exception("Black screens must exclude TV and duplicates");foreach(bool keep in new[]{false,true})if(OtherMonitors.Resolve(new Settings{KeepOthers=keep})!=(keep?"Keep":"Disconnect"))throw new Exception("Legacy other-monitor setting changed");using(var form=new BlackScreen(delegate{})){var h=form.Handle;foreach(var screen in Screen.AllScreens){form.Place(screen.DeviceName);if(form.Bounds!=WindowPlacement.MonitorBounds(screen.DeviceName).Value)throw new Exception("Black screen does not cover physical display bounds");}if(form.ShowInTaskbar||!form.TopMost||form.BackColor!=Color.Black)throw new Exception("Black screen window configuration");}File.WriteAllText(Storage.PathOf("other-monitors-test.txt"),"PASS: legacy preference migration; TV exclusion; duplicate handling; black-window physical placement on connected monitors. Test windows were not shown; no monitor power commands were sent.");}
}
}