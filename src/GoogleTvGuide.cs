using System;
using System.Drawing;
using System.Windows.Forms;
namespace TVLounge {
internal sealed class GoogleTvGuide : Form {
 readonly FlowLayoutPanel steps=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(24)};
 public GoogleTvGuide(){
  Text="SteamCouch — TV setup guide";BackColor=Theme.Background;ForeColor=Theme.Text;Font=new Font("Segoe UI",10);ClientSize=new Size(740,640);MinimumSize=new Size(570,420);StartPosition=FormStartPosition.CenterParent;MinimizeBox=false;MaximizeBox=false;AutoScaleMode=AutoScaleMode.Dpi;AutoScaleDimensions=new SizeF(96,96);
  Controls.Add(steps);steps.BackColor=Theme.Background;
  Add("Connect your Google TV",18,true,Theme.Text);
  Add("One-time setup on your own TV. Your PC and TV must be on the same home network. No HDMI-CEC adapter or separate software download is needed.",10,false,Theme.Muted);
  Step("1  Enable Developer options","On the TV, open Settings → System → About. Select Android TV OS build and press OK seven times, until it says you are a developer. Select BUILD, not Android TV OS version: the version entry can open a clock.");
  Step("2  Enable debugging","Return to System → Developer options. Turn on USB debugging or Network debugging, whichever is available. Some TVs offer Wireless debugging instead; enable it. Menu names vary by model. Debugging lets an approved PC control your TV; only approve computers you trust.");
  Step("3  Enter your TV address","Find the TV's IP under Settings → Network & Internet → your connected network. In SteamCouch's TV & audio page, enter that IP. Older network debugging normally uses port 5555 automatically. For Wireless debugging, enter the connection IP:port shown on the TV, such as 192.168.1.50:37123. It may change after restarting the TV.");
  Step("4  Approve or pair this PC","Click Connect / authorize. On the TV's Allow USB debugging? prompt, choose Always allow from this computer and OK. For Wireless debugging, first choose Pair using pairing code on the TV, then Pair with code in SteamCouch. Enter its pairing IP:port and six-digit code. Pairing and connection ports are different! After pairing, use Connect / authorize with the connection port.");
  Step("5  Select HDMI and test","Choose the HDMI port your PC uses, enable Google TV network control and Save settings. Click Test TV power & input and check the TV switches to that port. If it ignores the HDMI command, it may need a model-specific Android input URI in the optional field. Do not copy another TV's URI blindly. Returning to desktop leaves the TV on.");
  Step("6  Allow waking from standby","On TCL, look under System → Power & energy for Quick start, Screenless service or Standby mode → Network standby. Network standby may instead be under Network & Internet. Enable the available options that keep the TV reachable while asleep. Availability varies by model. These settings can increase standby power use. A fully powered-off or unplugged TV cannot be guaranteed to wake over the network.");
  Step("Optional  Wake-on-LAN","If your TV supports network wake, enter the MAC address for its connected Wi-Fi/Ethernet interface in SteamCouch. The app sends a wake packet before connecting. This still requires compatible TV/network standby settings.");
  Step("If it does not connect","Connection refused: check the IP/port and debugging setting. Unauthorized: approve the prompt or pair again. Offline: wake the TV with its remote and reconnect. Works awake but fails asleep: check standby settings. A failed TV connection stops TV mode before changing Windows display/audio settings.");
  Add("You can turn network control off at any time. Revoke PC access in the TV's Developer options if needed. Keep debugging on your trusted home network; do not expose it to the internet.",10,false,Theme.Muted);
  var close=new ModernButton{Text="Got it",Primary=true,Size=new Size(180,38),Margin=new Padding(0,16,0,24)};close.Click+=delegate{Close();};steps.Controls.Add(close);AcceptButton=close;CancelButton=close;
  steps.SizeChanged+=delegate{FitText();};Shown+=delegate{Theme.DarkTitle(this);FitText();BeginInvoke(new Action(()=>{ActiveControl=null;steps.AutoScrollPosition=Point.Empty;}));};
 }
 void Add(string text,int size,bool bold,Color color){var label=Theme.Label(text,size,bold,color);label.AutoSize=true;label.Margin=new Padding(0,0,0,16);label.MaximumSize=new Size(660,0);steps.Controls.Add(label);}
 void Step(string title,string text){Add(title,12,true,Theme.Accent);Add(text,10,false,Theme.Text);}
 void FitText(){steps.SuspendLayout();foreach(Control control in steps.Controls)if(control is Label)control.MaximumSize=new Size(Math.Max(100,steps.ClientSize.Width-steps.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth-8),0);steps.ResumeLayout(true);}
}
}