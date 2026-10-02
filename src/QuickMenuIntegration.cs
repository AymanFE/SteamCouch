using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
namespace TVLounge {
public partial class MainForm {
 QuickMenuWindow quickMenu;ControllerInput menuControllers=new ControllerInput();ModernSwitch quickMenuEnabled=new ModernSwitch(),vrrSession=new ModernSwitch();DarkCombo hdrPreference=new DarkCombo(),menuChord=new DarkCombo();Card quickMenuCard=new Card(),videoOptionsCard=new Card();
 void AddQuickMenu(){
  Theme.Place(content,quickMenuCard,32,798,808,138);Theme.Place(quickMenuCard,Theme.Label("TV quick menu",12,true,Theme.Text),24,16,670,28);Theme.Place(quickMenuCard,quickMenuEnabled,738,18,46,28);quickMenuEnabled.Anchor=AnchorStyles.Top|AnchorStyles.Right;quickMenuEnabled.Checked=settings.QuickMenu;quickMenuEnabled.AccessibleName="Enable TV quick menu with View plus Y";
  Theme.Place(quickMenuCard,Theme.Label("Hold View / Select + Y briefly in TV mode. Volume, HDR, resolution and refresh rate.",9,false,Theme.Muted),24,50,750,31);foreach(var item in new[]{new Choice("32800","View / Select + Y"),new Choice("16416","View / Select + X"),new Choice("8224","View / Select + B"),new Choice("4128","View / Select + A")})menuChord.Items.Add(item);menuChord.SelectedItem=menuChord.Items.Cast<Choice>().FirstOrDefault(c=>c.Id==settings.MenuMask.ToString())??menuChord.Items[0];Theme.Place(quickMenuCard,menuChord,24,90,440,36);menuChord.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;var preview=new ModernButton{Text="Preview menu"};Theme.Place(quickMenuCard,preview,490,90,294,36);preview.Anchor=AnchorStyles.Top|AnchorStyles.Right;preview.Click+=delegate{OpenQuickMenu(true);};
  Theme.Place(content,videoOptionsCard,32,400,808,182);Theme.Place(videoOptionsCard,Theme.Label("TV video preferences",12,true,Theme.Text),24,16,700,28);Theme.Place(videoOptionsCard,Theme.Label("HDR WHEN ENTERING TV MODE",8,true,Theme.Muted),24,52,470,20);foreach(var item in new[]{new Choice("Keep","Keep current HDR setting"),new Choice("On","Turn HDR on"),new Choice("Off","Turn HDR off")})hdrPreference.Items.Add(item);hdrPreference.SelectedItem=hdrPreference.Items.Cast<Choice>().FirstOrDefault(c=>c.Id==settings.TvHdr)??hdrPreference.Items[0];Theme.Place(videoOptionsCard,hdrPreference,24,80,760,36);hdrPreference.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;
  Theme.Place(videoOptionsCard,Theme.Label("Enable Windows VRR assistance in TV mode",10,true,Theme.Text),24,128,690,25);Theme.Place(videoOptionsCard,vrrSession,738,126,46,28);vrrSession.Anchor=AnchorStyles.Top|AnchorStyles.Right;vrrSession.Checked=settings.TvVrr;Theme.Place(videoOptionsCard,Theme.Label("Previous preferences restore on return. VRR also needs TV/GPU support; restart games.",9,false,Theme.Muted),24,154,750,24);
  CaptureLayout(quickMenuCard);CaptureLayout(videoOptionsCard);
  controllerChord.Items.Add(new Choice("1056","View / Select + Xbox button (when supported)"));controllerChord.SelectedItem=controllerChord.Items.Cast<Choice>().FirstOrDefault(c=>c.Id==settings.ControllerMask.ToString())??controllerChord.Items[0];
  tray.ContextMenuStrip.Items.Insert(1,new ToolStripMenuItem("TV quick menu",null,delegate{OpenQuickMenu(false);}));
 }
 void LayoutQuickMenu(int width){quickMenuCard.SetBounds(Theme.Px(this,32),Theme.Px(this,798),width-Theme.Px(this,64),Theme.Px(this,138));quickMenuCard.Visible=currentPage==2;PlaceLogicalChildren(quickMenuCard);videoOptionsCard.SetBounds(Theme.Px(this,32),Theme.Px(this,400),width-Theme.Px(this,64),Theme.Px(this,182));videoOptionsCard.Visible=currentPage==1;PlaceLogicalChildren(videoOptionsCard);if(currentPage==2)content.Height=quickMenuCard.Bottom+Theme.Px(this,8);if(currentPage==1)content.Height=videoOptionsCard.Bottom+Theme.Px(this,8);}
 void SaveQuickMenu(Settings next){next.QuickMenu=quickMenuEnabled.Checked;next.MenuMask=int.Parse(((Choice)menuChord.SelectedItem).Id);next.TvHdr=((Choice)hdrPreference.SelectedItem).Id;next.TvVrr=vrrSession.Checked;if(next.ControllerShortcut&&next.ControllerMask==0x420&&!ControllerInput.GuideAvailable)throw new InvalidOperationException("This Windows controller driver does not expose the Xbox button. Choose View + Menu or another shortcut instead.");menuControllers.Reset();}
 void OpenQuickMenu(bool preview){
  if(busy||installingUpdate)return;if(quickMenu!=null&&!quickMenu.IsDisposed){quickMenu.Activate();return;}if(!preview&&!engine.Active){status.Text="Activate TV mode to use the quick menu. You can preview it in Settings.";return;}
  try{string monitor=preview?Screen.FromControl(this).DeviceName:engine.SessionDisplay();quickMenu=new QuickMenuWindow(devices,monitor,async()=>await Work(()=>engine.Restore(settings.TimeoutSeconds),true),preview);quickMenu.Icon=Icon;quickMenu.FormClosed+=delegate{quickMenu=null;controllers.Reset();menuControllers.Reset();};controllers.Reset();menuControllers.Reset();quickMenu.Show(this);quickMenu.Activate();}catch(Exception e){ShowError(e);}
 }
 void PollQuickMenu(){if(menuControllers.PollMilliseconds(settings.QuickMenu&&engine.Active,settings.MenuMask,400,busy||installingUpdate||OwnedForms.Length>0))OpenQuickMenu(false);}
 void SetVideoPreferences(Settings next){hdrPreference.SelectedItem=hdrPreference.Items.Cast<Choice>().FirstOrDefault(c=>c.Id==next.TvHdr)??hdrPreference.Items[0];vrrSession.Checked=next.TvVrr;}
 async Task PreviewQuickMenu(){using(var menu=new QuickMenuWindow(devices,Screen.FromControl(this).DeviceName,()=>Task.FromResult(0),true)){menu.Show(this);await menu.WritePreviews();menu.Close();}}
}
}
