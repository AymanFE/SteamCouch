using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
namespace TVLounge {
public partial class MainForm {
 DarkCombo otherMonitorMode=new DarkCombo();Label otherMonitorInfo=Theme.Label("",9,false,Theme.Muted);EventHandler blackDisplayChanged;bool syncingOtherMode;BlackScreens blackScreens;
 void AddMonitorChoices(Settings s){
  foreach(string mode in OtherMonitors.Modes)otherMonitorMode.Items.Add(new Choice(mode,OtherMonitors.Label(mode)));
  Theme.Place(behavior,Theme.Label("Other monitors during TV mode",11,true,Theme.Text),24,16,720,28);
  Theme.Place(behavior,otherMonitorMode,24,54,760,38);otherMonitorMode.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;
  Theme.Place(behavior,otherMonitorInfo,24,101,760,26);otherMonitorInfo.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;otherMonitorInfo.AutoEllipsis=true;
  otherMonitorMode.SelectedIndexChanged+=delegate{if(otherMonitorMode.SelectedItem==null)return;string mode=((Choice)otherMonitorMode.SelectedItem).Id;syncingOtherMode=true;keep.Checked=mode!="Disconnect";syncingOtherMode=false;otherMonitorInfo.Text=mode=="Black"?"Keeps displays connected. Double-click a black screen to return to desktop.":mode=="PowerOff"?"Requires DDC/CI power control in each monitor. Original power state is restored.":mode=="Disconnect"?"Windows disables other displays. Your original layout is restored on return.":"Other monitors stay on. The TV becomes the main screen for games.";};
  keep.CheckedChanged+=delegate{if(!syncingOtherMode)otherMonitorMode.SelectedItem=otherMonitorMode.Items.Cast<Choice>().First(c=>c.Id==(keep.Checked?"Keep":"Disconnect"));};
  SetOtherMonitorMode(s);blackDisplayChanged=delegate{if(IsHandleCreated&&!IsDisposed)try{BeginInvoke(new Action(UpdateBlackScreens));}catch(InvalidOperationException){}};Microsoft.Win32.SystemEvents.DisplaySettingsChanged+=blackDisplayChanged;
 }
 void SetOtherMonitorMode(Settings s){string mode=OtherMonitors.Resolve(s);otherMonitorMode.SelectedItem=otherMonitorMode.Items.Cast<Choice>().First(c=>c.Id==mode);}
 void UpdateBlackScreens(){
  if(uiTest)return;if(!engine.Active){if(blackScreens!=null)blackScreens.Dispose();return;}if(busy)return;
  try{var snap=Storage.Read<Snapshot>("restore.json");if(!snap.BlackoutOthers){if(blackScreens!=null)blackScreens.Dispose();return;}string tv=engine.SessionDisplay();if(blackScreens==null)blackScreens=new BlackScreens(async()=>await Work(()=>engine.Restore(settings.TimeoutSeconds),true));blackScreens.Update(tv);}
  catch(Exception e){if(blackScreens!=null)blackScreens.Dispose();Storage.Log("Black screens unavailable: "+e.Message);status.Text="Black screens could not be placed. Return to desktop and check the TV connection.";}
 }
 void CheckMonitorChoices(){
  string path=Storage.PathOf("settings.json"),original=File.ReadAllText(path);var before=Clone(settings);
  try{foreach(string mode in OtherMonitors.Modes){otherMonitorMode.SelectedItem=otherMonitorMode.Items.Cast<Choice>().First(c=>c.Id==mode);Save();var saved=Storage.Read<Settings>("settings.json");if(saved.OtherMonitorMode!=mode||saved.KeepOthers!=(mode!="Disconnect"))throw new Exception("Other-monitor choice did not persist");}}
  finally{File.WriteAllText(path,original);settings=before;SetOtherMonitorMode(before);}
 }
}
}namespace TVLounge {
internal sealed class CouchCheckBox : System.Windows.Forms.CheckBox {
 public CouchCheckBox(){SetStyle(System.Windows.Forms.ControlStyles.UserPaint|System.Windows.Forms.ControlStyles.AllPaintingInWmPaint|System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,true);Font=new System.Drawing.Font("Segoe UI",10);CheckedChanged+=delegate{Invalidate();};EnabledChanged+=delegate{Invalidate();};}
 protected override void OnPaint(System.Windows.Forms.PaintEventArgs e){e.Graphics.Clear(Parent==null?Theme.Surface:Parent.BackColor);int size=Theme.Px(this,17),gap=Theme.Px(this,10),top=(Height-size)/2;var box=new System.Drawing.Rectangle(1,top,size,size);using(var brush=new System.Drawing.SolidBrush(Theme.Input))e.Graphics.FillRectangle(brush,box);using(var pen=new System.Drawing.Pen(Enabled?Theme.Accent:Theme.Muted,Theme.Px(this,1)))e.Graphics.DrawRectangle(pen,box);if(Checked)using(var pen=new System.Drawing.Pen(Enabled?Theme.Accent:Theme.Muted,Theme.Px(this,2))){e.Graphics.DrawLines(pen,new[]{new System.Drawing.Point(1+size/5,top+size/2),new System.Drawing.Point(1+size*2/5,top+size*3/4),new System.Drawing.Point(1+size*4/5,top+size/4)});}System.Windows.Forms.TextRenderer.DrawText(e.Graphics,Text,Font,new System.Drawing.Rectangle(size+gap,0,Width-size-gap,Height),Enabled?Theme.Text:Theme.Muted,System.Windows.Forms.TextFormatFlags.VerticalCenter|System.Windows.Forms.TextFormatFlags.EndEllipsis|System.Windows.Forms.TextFormatFlags.NoPrefix);if(Focused)System.Windows.Forms.ControlPaint.DrawFocusRectangle(e.Graphics,new System.Drawing.Rectangle(size+gap,2,Width-size-gap-2,Height-4));}
}
}
