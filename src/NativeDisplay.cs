// Native Windows CCD snapshots retain physical target IDs as well as display modes.
// DISPLAYCONFIG_PATH_INFO is 72 bytes; DISPLAYCONFIG_MODE_INFO is 64 bytes.
// Source/target mode indices remain paired with the exact mode array captured here.
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
namespace TVLounge {
 public class DisplayState { public byte[] Paths; public byte[] Modes; public uint PathCount; public uint ModeCount; }
 public static class NativeDisplay {
  [DllImport("user32.dll")] static extern int GetDisplayConfigBufferSizes(uint flags,out uint paths,out uint modes);
  [DllImport("user32.dll")] static extern int QueryDisplayConfig(uint flags,ref uint paths,IntPtr pathData,ref uint modes,IntPtr modeData,IntPtr topology);
  [DllImport("user32.dll")] static extern int SetDisplayConfig(uint paths,IntPtr pathData,uint modes,IntPtr modeData,uint flags);
  public static DisplayState Capture() {
   for(int attempt=0;attempt<5;attempt++) {
    uint pc,mc; int code=GetDisplayConfigBufferSizes(2,out pc,out mc); Check(code,"Read display buffer sizes");
    IntPtr paths=Marshal.AllocHGlobal(checked((int)pc*72)), modes=Marshal.AllocHGlobal(checked((int)mc*64));
    try {
     code=QueryDisplayConfig(2,ref pc,paths,ref mc,modes,IntPtr.Zero); if(code==122)continue; Check(code,"Capture display paths");
     var state=new DisplayState{PathCount=pc,ModeCount=mc,Paths=new byte[checked((int)pc*72)],Modes=new byte[checked((int)mc*64)]};
     Marshal.Copy(paths,state.Paths,0,state.Paths.Length); Marshal.Copy(modes,state.Modes,0,state.Modes.Length); return state;
    } finally { Marshal.FreeHGlobal(paths); Marshal.FreeHGlobal(modes); }
   }
   throw new IOException("Display configuration kept changing during capture.");
  }
  static void Check(int code,string action) { if(code!=0)throw new InvalidOperationException(action+" failed (Windows error "+code+")."); }
  public static void Apply(DisplayState state,bool validateOnly) {
   if(state.PathCount==0 || state.PathCount>64 || state.ModeCount>256 || state.Paths.Length!=state.PathCount*72 || state.Modes.Length!=state.ModeCount*64)throw new IOException("Invalid native display snapshot.");
   IntPtr paths=Marshal.AllocHGlobal(state.Paths.Length),modes=Marshal.AllocHGlobal(state.Modes.Length);
   try {
    Marshal.Copy(state.Paths,0,paths,state.Paths.Length); Marshal.Copy(state.Modes,0,modes,state.Modes.Length);
    // SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_ALLOW_CHANGES | VALIDATE or APPLY/SAVE.
    uint flags=0x20|0x400|(validateOnly?0x40u:0x80u|0x200u);
    Check(SetDisplayConfig(state.PathCount,paths,state.ModeCount,modes,flags),validateOnly?"Validate saved display paths":"Restore saved display paths");
   } finally { Marshal.FreeHGlobal(paths); Marshal.FreeHGlobal(modes); }
  }
  public static void Save(string path) { var state=Capture(); Apply(state,true); File.WriteAllText(path,new JavaScriptSerializer().Serialize(state)); }
  public static void Restore(string path) { Apply(new JavaScriptSerializer().Deserialize<DisplayState>(File.ReadAllText(path)),false); }
 }
}
