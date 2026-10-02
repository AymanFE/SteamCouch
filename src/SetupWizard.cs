using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
namespace TVLounge {
internal sealed class SetupWizard : Form {
 public Settings Result;readonly Engine engine;int step;bool testing,testPassed;
 Label heading=Theme.Label("Welcome to your couch",21,true,Theme.Text),help=Theme.Label("",10,false,Theme.Muted),review=Theme.Label("",10,false,Theme.Text),testInfo=Theme.Label("You can finish without testing, or try a TV round trip first.",9,false,Theme.Muted);
 DarkCombo tv=new DarkCombo(),audio=new DarkCombo(),launcher=new DarkCombo();ModernSwitch keep=new ModernSwitch();ModernButton back=new ModernButton{Text="Back"},next=new ModernButton{Text="Next",Primary=true},test=new ModernButton{Text="Test TV mode"};
 TextBox appPath=new TextBox();ModernButton browseSteam=new ModernButton{Text="Browse Steam"};
 Panel surface=new Panel();Label field=Theme.Label("",9,true,Theme.Accent),keepLabel=Theme.Label("Keep other monitors on",10,true,Theme.Text);
 public SetupWizard(Settings settings,List<Row> displays,List<Row> outputs,Engine engine){
  Result=settings;this.engine=engine;Text="SteamCouch — Setup";Font=new Font("Segoe UI",10);BackColor=Theme.Background;ForeColor=Theme.Text;ClientSize=new Size(760,460);MinimumSize=Size;StartPosition=FormStartPosition.CenterParent;MinimizeBox=false;MaximizeBox=false;AutoScaleMode=AutoScaleMode.None;
  Theme.Place(this,heading,32,24,690,52);Theme.Place(this,help,34,88,690,56);Theme.Place(this,surface,32,151,696,230);surface.BackColor=Theme.Surface;
  Theme.Place(surface,field,24,20,630,25);Theme.Place(surface,tv,24,62,648,40);Theme.Place(surface,audio,24,62,648,40);Theme.Place(surface,launcher,24,62,648,40);
  foreach(var display in displays.Where(d=>!string.IsNullOrEmpty(d.Get("Monitor ID"))))tv.Items.Add(new Choice(display.Get("Monitor ID"),(display.Get("Monitor Name")==""?display.Get("Name"):display.Get("Monitor Name"))+(Engine.IsOn(display)?" (connected)":" (disconnected)")));
  tv.SelectedItem=tv.Items.Cast<Choice>().FirstOrDefault(c=>c.Id==settings.MonitorId);if(tv.SelectedItem==null&&tv.Items.Count>0)tv.SelectedItem=tv.Items[0];audio.Items.Add(new Choice("","Automatic — TV audio"));foreach(var output in outputs.Where(a=>a.Get("Direction")=="Render"))audio.Items.Add(new Choice(output.Get("Item ID"),output.Get("Name")));audio.SelectedItem=audio.Items.Cast<Choice>().FirstOrDefault(c=>c.Id==settings.AudioId)??audio.Items[0];launcher.Items.Add(new Choice("Steam","Steam Big Picture"));launcher.Items.Add(new Choice("Xbox","Xbox mode"));launcher.SelectedItem=launcher.Items.Cast<Choice>().FirstOrDefault(c=>c.Id==settings.Launcher)??launcher.Items[0];
  appPath.Text=settings.SteamPath;appPath.BackColor=Theme.Input;appPath.ForeColor=Theme.Text;appPath.BorderStyle=BorderStyle.FixedSingle;Theme.Place(surface,appPath,24,112,488,32);Theme.Place(surface,browseSteam,524,110,148,34);browseSteam.Click+=delegate{using(var picker=new OpenFileDialog{Filter="Steam program|steam.exe",FileName=appPath.Text})if(picker.ShowDialog(this)==DialogResult.OK)appPath.Text=picker.FileName;};launcher.SelectedIndexChanged+=delegate{appPath.Visible=browseSteam.Visible=step==2&&((Choice)launcher.SelectedItem).Id=="Steam";};Theme.Place(surface,keepLabel,24,169,520,28);Theme.Place(surface,keep,602,169,46,28);keep.Checked=settings.KeepOthers;
  Theme.Place(surface,review,24,20,648,115);Theme.Place(surface,testInfo,24,143,648,40);Theme.Place(surface,test,24,184,180,36);
  Theme.Place(this,back,420,402,140,38);Theme.Place(this,next,576,402,150,38);back.Click+=delegate{if(step>0){step--;ShowStep();}};next.Click+=delegate{try{ReadStep();if(step==3){Result.SetupCompleted=true;DialogResult=DialogResult.OK;Close();}else{step++;ShowStep();}}catch(Exception e){help.Text=e.Message;help.ForeColor=Color.Orange;}};test.Click+=async delegate{await TestSetup();};
  FormClosing+=delegate(object sender,FormClosingEventArgs e){if(testing)e.Cancel=true;};Shown+=delegate{Theme.DarkTitle(this);};AutoScaleMode=AutoScaleMode.Dpi;AutoScaleDimensions=new SizeF(96,96);PerformAutoScale();ShowStep();
 }
 internal void PreviewStep(int value){step=value;ShowStep();}
 void ReadStep(){if(step==0){if(tv.SelectedItem==null)throw new InvalidOperationException("No display found. Check your TV power and cable, then reopen setup.");Result.MonitorId=((Choice)tv.SelectedItem).Id;}if(step==1)Result.AudioId=((Choice)audio.SelectedItem).Id;if(step>=2){Result.Launcher=((Choice)launcher.SelectedItem).Id;Result.SteamPath=appPath.Text.Trim();Result.KeepOthers=keep.Checked;Result.LaunchSteam=true;if(Result.Launcher=="Steam"&&!File.Exists(Result.SteamPath))throw new InvalidOperationException("Steam.exe was not found. Use Browse Steam to choose your Steam program.");if(Result.Launcher=="Xbox")XboxClient.Validate();}}
 void ShowStep(){
  string[] titles={"1 / 4 — Choose your TV","2 / 4 — Choose playback audio","3 / 4 — Choose your gaming mode","4 / 4 — Ready for the couch"};string[] helpers={"Choose the screen you want to play on. A disconnected TV can be enabled when TV mode starts.","Automatic finds the TV audio after it reconnects. Choose an output explicitly if you prefer.","Your TV becomes the main display during TV mode. Your desktop layout is saved for your return.","Review your setup. The optional test enters TV mode, then restores your desktop automatically."};heading.Text=titles[step];help.Text=helpers[step];help.ForeColor=Theme.Muted;field.Text=step==0?"TV DISPLAY":step==1?"PLAYBACK AUDIO":"GAMING MODE";tv.Visible=step==0;audio.Visible=step==1;launcher.Visible=step==2;appPath.Visible=browseSteam.Visible=step==2&&((Choice)launcher.SelectedItem).Id=="Steam";keep.Visible=keepLabel.Visible=step==2;field.Visible=step<3;review.Visible=test.Visible=testInfo.Visible=step==3;review.Text="Display: "+tv.Text+"\nAudio: "+audio.Text+"\nGaming mode: "+launcher.Text+"\nOther monitors: "+(keep.Checked?"Stay on":"TV only")+"\nPress your configured shortcut again to return to desktop.";testInfo.Text=testPassed?"TV mode and desktop restoration completed successfully.":"You can finish without testing, or try a TV round trip first.";back.Enabled=step>0&&!testing;next.Text=step==3?"Finish":"Next";
 }
 async Task TestSetup(){
  if(testing)return;testing=true;back.Enabled=next.Enabled=test.Enabled=false;testInfo.Text="Testing TV mode. The desktop will be restored automatically...";
  try{await Task.Run(()=>{try{engine.Activate(Result);}finally{if(engine.Active)engine.Restore(Result.TimeoutSeconds);}});testPassed=true;testInfo.Text="TV mode and desktop restoration completed successfully.";}
  catch(Exception e){testInfo.Text=e.Message;testPassed=false;}
  finally{testing=false;back.Enabled=true;next.Enabled=!engine.Active;test.Enabled=!engine.Active;if(engine.Active){help.Text="Restore point retained. Close setup and use Restore desktop after reconnecting your screens.";help.ForeColor=Color.Orange;}}
 }
}
}
