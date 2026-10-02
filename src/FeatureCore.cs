using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Web.Script.Serialization;
namespace TVLounge {
internal sealed class AwakeGuard : IDisposable {
 [DllImport("kernel32.dll")] static extern uint SetThreadExecutionState(uint flags);
 readonly Func<uint,uint> request;public bool Active {get;private set;}
 public AwakeGuard():this(SetThreadExecutionState){} internal AwakeGuard(Func<uint,uint> request){this.request=request;}
 public void Update(bool tvMode,bool enabled){bool needed=tvMode&&enabled;if(needed==Active)return;if(request(needed?0x80000003u:0x80000000u)==0)throw new InvalidOperationException("Windows could not update sleep prevention.");Active=needed;}
 public void Dispose(){if(Active){request(0x80000000);Active=false;}}
}
internal sealed class HoldShortcut {
 bool armed,fired;long started=-1;
 public void Reset(){armed=false;fired=false;started=-1;}
 public bool Sample(ushort buttons,ushort mask,long now,int duration,bool blocked){
  bool held=(buttons&mask)==mask;
  if(blocked){Reset();return false;}
  if(!held){armed=true;fired=false;started=-1;return false;}
  if(!armed||fired)return false;
  if(started<0)started=now;
  if(now-started<duration)return false;
  fired=true;return true;
 }
}
internal sealed class ControllerInput {
 [StructLayout(LayoutKind.Sequential)] struct Gamepad {public ushort Buttons;public byte LeftTrigger,RightTrigger;public short LX,LY,RX,RY;}
 [StructLayout(LayoutKind.Sequential)] struct State {public uint Packet;public Gamepad Pad;}
 [DllImport("xinput1_4.dll")] static extern uint XInputGetState(uint index,out State state); [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern IntPtr LoadLibrary(string name);
 [DllImport("kernel32.dll")]static extern IntPtr GetProcAddress(IntPtr library,IntPtr ordinal);
 [UnmanagedFunctionPointer(CallingConvention.StdCall)]delegate uint ExtendedState(uint index,out State state);
 static readonly ExtendedState extended=FindExtended();
 static ExtendedState FindExtended(){try{var library=LoadLibrary("xinput1_4.dll");var function=GetProcAddress(library,new IntPtr(100));return function==IntPtr.Zero?null:(ExtendedState)Marshal.GetDelegateForFunctionPointer(function,typeof(ExtendedState));}catch{return null;}}
 public static bool GuideAvailable {get{return extended!=null;}}
 static bool Read(uint index,out State state){if(extended!=null&&extended(index,out state)==0)return true;return XInputGetState(index,out state)==0;}
 public static ushort NavigationButtons(uint index){State state;if(!Read(index,out state))return 0;ushort buttons=state.Pad.Buttons;if(state.Pad.LY>16000)buttons|=1;if(state.Pad.LY< -16000)buttons|=2;if(state.Pad.LX< -16000)buttons|=4;if(state.Pad.LX>16000)buttons|=8;return buttons;}
 readonly HoldShortcut[] holds=Enumerable.Range(0,4).Select(i=>new HoldShortcut()).ToArray();
 long lastActivity;readonly Stopwatch clock=Stopwatch.StartNew();public bool Idle {get{return clock.ElapsedMilliseconds-lastActivity>=60000;}} public int Connected {get;private set;}
 public void Reset(){foreach(var hold in holds)hold.Reset();}
 public bool Poll(bool enabled,int mask,int seconds,bool blocked){return PollMilliseconds(enabled,mask,seconds*1000,blocked);} public bool PollMilliseconds(bool enabled,int mask,int duration,bool blocked){bool fire=false;Connected=0;for(uint i=0;i<4;i++){State state;bool present=Read(i,out state);if(present){Connected++;if(state.Pad.Buttons!=0||state.Pad.LeftTrigger>30||state.Pad.RightTrigger>30||Math.Abs((int)state.Pad.LX)>8000||Math.Abs((int)state.Pad.LY)>8000||Math.Abs((int)state.Pad.RX)>8000||Math.Abs((int)state.Pad.RY)>8000)lastActivity=clock.ElapsedMilliseconds;}if(holds[i].Sample(present?state.Pad.Buttons:(ushort)0,(ushort)mask,clock.ElapsedMilliseconds,duration,!present||!enabled||blocked))fire=true;}if(fire)Reset();return fire;}
}
public sealed class DisplayProfile {
 public string Name,MonitorId,AudioId,Launcher,SteamPath;public string TvHdr="Keep";public bool TvVrr;
 public bool KeepOthers,LaunchSteam;
 public static DisplayProfile Capture(string name,Settings settings){return new DisplayProfile{Name=name,MonitorId=settings.MonitorId,AudioId=settings.AudioId,Launcher=settings.Launcher,SteamPath=settings.SteamPath,KeepOthers=settings.KeepOthers,LaunchSteam=settings.LaunchSteam,TvHdr=settings.TvHdr,TvVrr=settings.TvVrr};}
 public void Apply(Settings settings){settings.MonitorId=MonitorId;settings.AudioId=AudioId;settings.Launcher=Launcher;settings.SteamPath=SteamPath;settings.KeepOthers=KeepOthers;settings.LaunchSteam=LaunchSteam;settings.TvHdr=TvHdr??"Keep";settings.TvVrr=TvVrr;}
 public override string ToString(){return Name;}
}
internal static class ProfileStore {
 public static List<DisplayProfile> Load(){return File.Exists(Storage.PathOf("profiles.json"))?Storage.Read<List<DisplayProfile>>("profiles.json"):new List<DisplayProfile>();}
 public static void Put(List<DisplayProfile> profiles,DisplayProfile profile){profile.Name=(profile.Name??"").Trim();if(profile.Name.Length==0||profile.Name.Length>64)throw new InvalidOperationException("Use a profile name between 1 and 64 characters.");int index=profiles.FindIndex(p=>string.Equals(p.Name,profile.Name,StringComparison.OrdinalIgnoreCase));if(index>=0)profiles[index]=profile;else profiles.Add(profile);}
}
internal static class FeatureTests {
 public static void Run(){
  Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception(label);};
  var hold=new HoldShortcut();check(!hold.Sample(0x330,0x330,0,2000,false),"A held chord at startup must not fire");hold.Sample(0,0x330,10,2000,false);check(!hold.Sample(0x330,0x330,20,2000,false),"Hold starts timer");check(!hold.Sample(0x330,0x330,2019,2000,false),"Hold duration required");check(hold.Sample(0x330,0x330,2020,2000,false),"Chord fires after full hold");check(!hold.Sample(0x330,0x330,5000,2000,false),"One toggle per hold");hold.Sample(0,0x330,5100,2000,false);hold.Sample(0x330,0x330,5200,2000,false);hold.Sample(0x330,0x330,7300,2000,true);check(!hold.Sample(0x330,0x330,10000,2000,false),"Release required after busy operation");hold.Sample(0,0x330,10100,2000,false);hold.Sample(0x330,0x330,10200,2000,false);check(hold.Sample(0x330,0x330,12200,2000,false),"New hold can toggle back");
  var flags=new List<uint>();using(var awake=new AwakeGuard(f=>{flags.Add(f);return 1;})){awake.Update(false,true);check(flags.Count==0,"Desktop cannot request awake");awake.Update(true,true);awake.Update(true,true);check(flags.SequenceEqual(new[]{0x80000003u}),"TV requests system and display once");awake.Update(false,true);check(flags.Last()==0x80000000u,"Desktop clears request");awake.Update(true,true);awake.Update(true,false);check(!awake.Active,"Turning preference off clears request");awake.Update(true,true);}check(flags.Last()==0x80000000u,"Exit clears power request");
  var s=new Settings{MonitorId="tv",AudioId="hdmi",KeepOthers=false,Launcher="Xbox",ControllerShortcut=true,KeepAwake=true,TvHdr="On",TvVrr=true};var list=new List<DisplayProfile>();ProfileStore.Put(list,DisplayProfile.Capture(" Lounge ",s));ProfileStore.Put(list,DisplayProfile.Capture("lounge",s));check(list.Count==1,"Profile names deduplicate");var target=new Settings{ControllerShortcut=false,KeepAwake=false};list[0].Apply(target);check(target.MonitorId=="tv"&&target.Launcher=="Xbox"&&!target.KeepOthers&&!target.ControllerShortcut&&!target.KeepAwake&&target.TvHdr=="On"&&target.TvVrr,"Profiles preserve global preferences");
  using(var nativeAwake=new AwakeGuard()){nativeAwake.Update(true,true);nativeAwake.Update(false,true);}var nativeControllers=new ControllerInput();nativeControllers.Poll(false,0x330,2,true);
  File.WriteAllText(Storage.PathOf("features-test.txt"),"PASS: controller hold, release, rearm, busy suppression; TV-only awake request and cleanup; profile replacement and global preference preservation.");
 }
}
}
