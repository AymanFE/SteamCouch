using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
namespace TVLounge {
internal sealed class BatteryReading {
 public string Id,Name,Path,Source="SDL / XInput";public int Kind;public ushort Vendor,Product;public bool Connected;public int State,Percent=-1,Category=-1;public IntPtr Joystick;
 public string Value {get{if(State==2)return "Battery unavailable (driver reports wired)";if(State<=0)return "Battery unavailable";string value=Category>=0?new[]{"Empty","Low","Medium","Full"}[Category]:Percent>=0?Percent+"%":"Battery unavailable";if(State==3)return "Charging"+(Percent>=0?" · "+Percent+"%":"");if(State==4)return "Charged";return value;}}
 public bool Low {get{return State==1&&(Category>=0?Category<=1:Percent>=0&&Percent<=20);}}
 public bool Recharged {get{return State==3||State==4||State==2||(State==1&&(Category>=0?Category>=2:Percent>=30));}}
}
internal sealed class BatteryAlertPolicy {
 readonly Dictionary<string,DateTime> notified=new Dictionary<string,DateTime>();readonly HashSet<string> low=new HashSet<string>();
 public bool Check(BatteryReading r,DateTime now){if(r.Recharged)low.Remove(r.Id);if(!r.Low||low.Contains(r.Id))return false;low.Add(r.Id);DateTime last;if(notified.TryGetValue(r.Id,out last)&&(now-last).TotalMinutes<10)return false;notified[r.Id]=now;return true;}
}
internal sealed class ControllerBatteries : IDisposable {
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr LoadLibraryEx(string name,IntPtr file,uint flags);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)] [return:MarshalAs(UnmanagedType.I1)]static extern bool SDL_InitSubSystem(uint flags);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern void SDL_QuitSubSystem(uint flags);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)] [return:MarshalAs(UnmanagedType.I1)]static extern bool SDL_SetHint(string name,string value);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern IntPtr SDL_GetJoysticks(out int count);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern void SDL_free(IntPtr p);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern void SDL_UpdateJoysticks();
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern IntPtr SDL_OpenJoystick(uint id);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern void SDL_CloseJoystick(IntPtr p);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern IntPtr SDL_GetJoystickPath(IntPtr p);[DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_GetJoystickType(IntPtr p);[DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)] [return:MarshalAs(UnmanagedType.I1)]static extern bool SDL_JoystickConnected(IntPtr p);[DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern IntPtr SDL_GetJoystickName(IntPtr p);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern IntPtr SDL_GetJoystickSerial(IntPtr p);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern ushort SDL_GetJoystickVendor(IntPtr p);[DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern ushort SDL_GetJoystickProduct(IntPtr p);[DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_GetJoystickPlayerIndex(IntPtr p);
 [StructLayout(LayoutKind.Sequential)]struct Guid16 {public ulong Low,High;public bool XInput {get{return ((High>>48)&255)==120;}}public override string ToString(){return Low.ToString("x16")+High.ToString("x16");}}
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern Guid16 SDL_GetJoystickGUID(IntPtr p);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_GetJoystickPowerInfo(IntPtr p,out int percent);
 [DllImport("SDL3.dll",CallingConvention=CallingConvention.Cdecl)] [return:MarshalAs(UnmanagedType.I1)]static extern bool SDL_RumbleJoystick(IntPtr p,ushort low,ushort high,uint milliseconds);
 [StructLayout(LayoutKind.Sequential)]struct XBattery {public byte Type,Level;}
 [DllImport("xinput1_4.dll")]static extern uint XInputGetBatteryInformation(uint index,byte type,out XBattery battery);
 readonly Dictionary<uint,IntPtr> opened=new Dictionary<uint,IntPtr>();bool initialized,failed;public string Error="";
 static string Utf8(IntPtr p){if(p==IntPtr.Zero)return "";int n=0;while(n<4096&&Marshal.ReadByte(p,n)!=0)n++;var bytes=new byte[n];Marshal.Copy(p,bytes,0,n);return Encoding.UTF8.GetString(bytes);}
 public List<BatteryReading> Read(){var result=new List<BatteryReading>();if(failed)return result;try{if(!initialized){if(LoadLibraryEx(Path.Combine(Storage.Root,@"tools\controllers\SDL3.dll"),IntPtr.Zero,0x1100)==IntPtr.Zero)throw new InvalidOperationException("Controller runtime is missing.");SDL_SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS","1");SDL_SetHint("SDL_JOYSTICK_HIDAPI_PS4_RUMBLE","0");SDL_SetHint("SDL_JOYSTICK_HIDAPI_PS5_RUMBLE","0");if(!SDL_InitSubSystem(0x200))throw new InvalidOperationException("Controller battery service could not start.");initialized=true;}
 SDL_UpdateJoysticks();int count;IntPtr ids=SDL_GetJoysticks(out count);var live=new HashSet<uint>();try{for(int i=0;i<count;i++){uint id=(uint)Marshal.ReadInt32(ids,i*4);live.Add(id);IntPtr joy;if(!opened.TryGetValue(id,out joy)){joy=SDL_OpenJoystick(id);if(joy==IntPtr.Zero)continue;opened[id]=joy;}int percent;int state=SDL_GetJoystickPowerInfo(joy,out percent);var guid=SDL_GetJoystickGUID(joy);string name=Utf8(SDL_GetJoystickName(joy)),serial=Utf8(SDL_GetJoystickSerial(joy));int player=SDL_GetJoystickPlayerIndex(joy);var reading=new BatteryReading{Id=guid+"/"+(serial.Length>0?serial:!string.IsNullOrEmpty(Utf8(SDL_GetJoystickPath(joy)))?Utf8(SDL_GetJoystickPath(joy)):player>=0?player.ToString():id.ToString()),Vendor=SDL_GetJoystickVendor(joy),Product=SDL_GetJoystickProduct(joy),Path=Utf8(SDL_GetJoystickPath(joy)),Kind=SDL_GetJoystickType(joy),Connected=SDL_JoystickConnected(joy),Name=string.IsNullOrWhiteSpace(name)?"Controller":name,State=state,Percent=percent>=0&&percent<=100?percent:-1,Joystick=joy};if(guid.XInput){XBattery b;reading.Percent=-1;if(player>=0&&player<=3&&XInputGetBatteryInformation((uint)player,0,out b)==0){reading.State=b.Type==1?2:(b.Type==2||b.Type==3)?1:0;reading.Category=reading.State==1&&b.Level<=3?b.Level:-1;}else reading.State=0;}if(reading.Vendor==0x3537&&guid.XInput)reading.Name="GameSir controller (XInput "+(player+1)+")";if(reading.Connected&&IsController(reading.Path,reading.Kind))result.Add(reading);}}finally{if(ids!=IntPtr.Zero)SDL_free(ids);}foreach(var id in opened.Keys.Where(k=>!live.Contains(k)).ToArray()){SDL_CloseJoystick(opened[id]);opened.Remove(id);}WindowsControllerBattery.Merge(result);return result;
 }catch(Exception e){Error=e.Message;failed=true;Dispose();return result;}}
  [StructLayout(LayoutKind.Sequential)]struct HidCaps {public ushort Usage,UsagePage,Input,Output,Feature;[MarshalAs(UnmanagedType.ByValArray,SizeConst=17)]public ushort[] Reserved;public ushort Links,InputButtons,InputValues,InputData,OutputButtons,OutputValues,OutputData,FeatureButtons,FeatureValues,FeatureData;}
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string path,uint access,uint share,IntPtr security,uint creation,uint flags,IntPtr template);
 [DllImport("hid.dll")]static extern bool HidD_GetPreparsedData(Microsoft.Win32.SafeHandles.SafeFileHandle device,out IntPtr data);[DllImport("hid.dll")]static extern bool HidD_FreePreparsedData(IntPtr data);[DllImport("hid.dll")]static extern int HidP_GetCaps(IntPtr data,out HidCaps caps);
 internal static bool IsGameUsage(ushort page,ushort usage){return page==1&&(usage==4||usage==5||usage==8);}
 static bool IsController(string path,int kind){if(kind!=0)return true;if(!string.IsNullOrEmpty(path)&&path.StartsWith(@"\\?\HID#",StringComparison.OrdinalIgnoreCase)){using(var file=CreateFile(path,0,3,IntPtr.Zero,3,0,IntPtr.Zero)){if(file.IsInvalid)return kind!=0;IntPtr data;if(!HidD_GetPreparsedData(file,out data))return kind!=0;try{HidCaps caps;return HidP_GetCaps(data,out caps)>=0&&IsGameUsage(caps.UsagePage,caps.Usage);}finally{HidD_FreePreparsedData(data);}}}return kind!=0;}
 public void Vibrate(BatteryReading r){if(initialized&&r.Joystick!=IntPtr.Zero)SDL_RumbleJoystick(r.Joystick,5000,5000,200);}
 public void Dispose(){if(initialized){foreach(var joy in opened.Values)SDL_CloseJoystick(joy);opened.Clear();SDL_QuitSubSystem(0x200);initialized=false;}}
}
}