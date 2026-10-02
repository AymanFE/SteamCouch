using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TVLounge {
internal static class Theme {
 public static Color Background=Color.FromArgb(38,38,38), Surface=Color.FromArgb(48,48,48), Input=Color.FromArgb(57,57,57), Border=Color.FromArgb(64,64,64), Text=Color.FromArgb(244,244,246), Muted=Color.FromArgb(165,166,177), Accent=Color.FromArgb(105,202,255), Green=Color.FromArgb(49,211,158);
 public static int Dpi(Control c){var form=c.FindForm();return form==null?c.DeviceDpi:form.DeviceDpi;}
 public static int Px(Control c,int value){return (int)Math.Round(value*Dpi(c)/96.0);}
 public static GraphicsPath Round(RectangleF r,float radius) { var p=new GraphicsPath(); float d=radius*2; p.AddArc(r.X,r.Y,d,d,180,90); p.AddArc(r.Right-d,r.Y,d,d,270,90); p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90); p.AddArc(r.X,r.Bottom-d,d,d,90,90); p.CloseFigure(); return p; }
 public static Label Label(string text,float size,bool bold,Color color) { return new Label{Text=text,AutoSize=false,Font=new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular),ForeColor=color,BackColor=Color.Transparent,UseCompatibleTextRendering=false,UseMnemonic=false}; }
 public static void Place(Control parent,Control child,int x,int y,int w,int h) { child.SetBounds(x,y,w,h); parent.Controls.Add(child); }
 [DllImport("uxtheme.dll",CharSet=CharSet.Unicode)] public static extern int SetWindowTheme(IntPtr handle,string app,string list);
 [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr h,int attribute,ref int value,int size);
 public static void DarkTitle(Form f) { try { int on=1; if(DwmSetWindowAttribute(f.Handle,20,ref on,4)!=0)DwmSetWindowAttribute(f.Handle,19,ref on,4); int color=0x202020; DwmSetWindowAttribute(f.Handle,35,ref color,4); } catch(DllNotFoundException){} }
}
internal class Card : Panel {
 public bool Hero;
 public Card(){SetStyle(ControlStyles.ResizeRedraw,true);DoubleBuffered=true; BackColor=Theme.Surface; Padding=new Padding(20);}
 protected override void OnPaintBackground(PaintEventArgs e){ e.Graphics.Clear(Theme.Background); e.Graphics.SmoothingMode=SmoothingMode.AntiAlias; using(var shape=Theme.Round(new RectangleF(.5f,.5f,Width-1,Height-1),Theme.Px(this,14))) {
  if(Hero) { using(var fill=new LinearGradientBrush(ClientRectangle,Color.FromArgb(38,57,69),Color.FromArgb(48,48,48),20))e.Graphics.FillPath(fill,shape); }
  else using(var fill=new SolidBrush(Theme.Surface))e.Graphics.FillPath(fill,shape);
  using(var line=new Pen(Hero?Color.FromArgb(62,86,103):Theme.Border))e.Graphics.DrawPath(line,shape);
 }}
}
internal class ModernButton : Button {
 public bool Primary; bool hover;
 public ModernButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true); FlatStyle=FlatStyle.Flat; FlatAppearance.BorderSize=0; BackColor=Theme.Surface; ForeColor=Theme.Text; Font=new Font("Segoe UI",10,FontStyle.Bold); Cursor=Cursors.Hand; Height=40;}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);} protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnPaint(PaintEventArgs e){ e.Graphics.SmoothingMode=SmoothingMode.AntiAlias; e.Graphics.Clear(Parent==null?Theme.Background:Parent.BackColor);
  Color fill=Primary?(hover?Color.FromArgb(149,219,255):Theme.Accent):(hover?Color.FromArgb(69,69,73):Theme.Input);
  using(var p=Theme.Round(new RectangleF(1,1,Width-2,Height-2),Theme.Px(this,8))) { { using(var b=new SolidBrush(Enabled?fill:Theme.Input))e.Graphics.FillPath(b,p); if(!Primary)using(var pen=new Pen(Theme.Border))e.Graphics.DrawPath(pen,p); } }
  TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,Enabled?(Primary?Color.FromArgb(12,37,53):Theme.Text):Theme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
  if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,new Rectangle(5,5,Width-10,Height-10),Theme.Text,fill);
 }
}
internal class ModernSwitch : CheckBox {
 public ModernSwitch(){AutoSize=false; Size=new Size(46,28); SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true); Cursor=Cursors.Hand;}
 protected override void OnPaint(PaintEventArgs e){ e.Graphics.SmoothingMode=SmoothingMode.AntiAlias; e.Graphics.Clear(Theme.Surface); float u=Theme.Dpi(this)/96f;float h=Height-6*u,w=Width-2*u; using(var p=Theme.Round(new RectangleF(u,3*u,w,h),h/2))using(var b=new SolidBrush(Checked&&Enabled?Theme.Accent:Color.FromArgb(83,83,90)))e.Graphics.FillPath(b,p); float diameter=h-6*u; using(var b=new SolidBrush(Enabled?Color.FromArgb(246,250,254):Theme.Muted))e.Graphics.FillEllipse(b,Checked?w-diameter-2*u:4*u,6*u,diameter,diameter); if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,ClientRectangle); }
 protected override void OnCheckedChanged(EventArgs e){base.OnCheckedChanged(e);Invalidate();}
}
internal class KeyChip : CheckBox {
 public KeyChip(){AutoSize=false; Appearance=Appearance.Button; TextAlign=ContentAlignment.MiddleCenter; Size=new Size(60,36); Cursor=Cursors.Hand; SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;e.Graphics.Clear(Theme.Surface); using(var p=Theme.Round(new RectangleF(1,1,Width-2,Height-2),Theme.Px(this,7))){using(var b=new SolidBrush(Checked?Color.FromArgb(40,67,85):Theme.Input))e.Graphics.FillPath(b,p);using(var line=new Pen(Checked?Theme.Accent:Theme.Border))e.Graphics.DrawPath(line,p);} TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,Checked?Color.FromArgb(193,232,255):Theme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,new Rectangle(4,4,Width-8,Height-8));}
 protected override void OnCheckedChanged(EventArgs e){base.OnCheckedChanged(e);Invalidate();}
}
internal class DarkCombo : Control {
 public List<object> Items=new List<object>(); public int DropDownWidth=650; object selected;ContextMenuStrip dropDown;
 public event EventHandler SelectedIndexChanged;
 public object SelectedItem {get{return selected;}set{if(!object.Equals(selected,value)){selected=value;Invalidate();if(SelectedIndexChanged!=null)SelectedIndexChanged(this,EventArgs.Empty);}}}
 public override string Text{get{return selected==null?"Select an option":selected.ToString();}set{base.Text=value;}}
 public DarkCombo(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);BackColor=Theme.Surface;ForeColor=Theme.Text;Font=new Font("Segoe UI",10);TabStop=true;Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.ComboBox;}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;e.Graphics.Clear(Theme.Surface);float scale=Theme.Dpi(this)/96f;using(var p=Theme.Round(new RectangleF(1,1,Width-2,Height-2),7*scale)){using(var b=new SolidBrush(Theme.Input))e.Graphics.FillPath(b,p);using(var pen=new Pen(Focused?Theme.Accent:Theme.Border))e.Graphics.DrawPath(pen,p);}var rect=new Rectangle((int)(12*scale),0,Width-(int)(45*scale),Height);TextRenderer.DrawText(e.Graphics,Text,Font,rect,Enabled?Theme.Text:Theme.Muted,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);float x=Width-19*scale,y=Height/2f;using(var pen=new Pen(Theme.Muted,1.6f*scale))e.Graphics.DrawLines(pen,new[]{new PointF(x-4*scale,y-2*scale),new PointF(x,y+2*scale),new PointF(x+4*scale,y-2*scale)});}
 void Open(){
  if(!Enabled||Items.Count==0)return;Focus();
  if(dropDown==null)dropDown=new ContextMenuStrip{BackColor=Theme.Surface,ForeColor=Theme.Text,ShowImageMargin=false,Renderer=new ToolStripProfessionalRenderer(new DarkMenuColors())};
  if(dropDown.Visible)dropDown.Close();
  foreach(ToolStripItem item in dropDown.Items.Cast<ToolStripItem>().ToArray())item.Dispose();
  dropDown.Items.Clear();dropDown.Font=Font;
  int width=Math.Max(Width,Math.Min(Theme.Px(this,DropDownWidth),Screen.FromControl(this).WorkingArea.Width-Theme.Px(this,60)));dropDown.MinimumSize=new Size(width,0);
  foreach(var value in Items){object item=value;var option=new ToolStripMenuItem(value.ToString()){ForeColor=Theme.Text,Checked=object.Equals(value,SelectedItem)};option.Click+=delegate{SelectedItem=item;};dropDown.Items.Add(option);}
  dropDown.Show(this,new Point(0,Height+Theme.Px(this,3)));
 }
 internal void CheckMenuLifetime(){
  object original=SelectedItem;
  try{
   for(int cycle=0;cycle<12;cycle++){
    Open();Application.DoEvents();if(dropDown==null||dropDown.IsDisposed||!dropDown.Visible)throw new InvalidOperationException("Dropdown did not open.");
    if(cycle%3==0){((ToolStripMenuItem)dropDown.Items[cycle%dropDown.Items.Count]).PerformClick();dropDown.Close(ToolStripDropDownCloseReason.ItemClicked);}
    else dropDown.Close(cycle%3==1?ToolStripDropDownCloseReason.AppClicked:ToolStripDropDownCloseReason.Keyboard);
    Application.DoEvents();if(dropDown.IsDisposed)throw new InvalidOperationException("Dropdown was disposed while closing.");
   }
  }finally{SelectedItem=original;if(dropDown!=null&&!dropDown.IsDisposed)dropDown.Close();}
 }
 protected override void Dispose(bool disposing){if(disposing&&dropDown!=null){dropDown.Dispose();dropDown=null;}base.Dispose(disposing);} protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left)Open();}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Space||e.KeyCode==Keys.Enter||(e.Alt&&e.KeyCode==Keys.Down)){Open();e.Handled=true;}else if(e.KeyCode==Keys.Down||e.KeyCode==Keys.Up){int index=Items.IndexOf(SelectedItem)+(e.KeyCode==Keys.Down?1:-1);if(Items.Count>0)SelectedItem=Items[Math.Max(0,Math.Min(Items.Count-1,index))];e.Handled=true;}}
 protected override bool IsInputKey(Keys keyData){return keyData==Keys.Up||keyData==Keys.Down||base.IsInputKey(keyData);}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
}
internal class NumberStepper : Control {
 public decimal Minimum=3,Maximum=120;decimal number=30;public decimal Value{get{return number;}set{number=Math.Max(Minimum,Math.Min(Maximum,value));Invalidate();}}
 public NumberStepper(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);TabStop=true;AccessibleRole=AccessibleRole.SpinButton;Cursor=Cursors.Hand;}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;e.Graphics.Clear(Theme.Surface);using(var p=Theme.Round(new RectangleF(1,1,Width-2,Height-2),Theme.Px(this,7))){using(var b=new SolidBrush(Theme.Input))e.Graphics.FillPath(b,p);using(var pen=new Pen(Focused?Theme.Accent:Theme.Border))e.Graphics.DrawPath(pen,p);}TextRenderer.DrawText(e.Graphics,"-",Font,new Rectangle(0,0,Width/4,Height),Theme.Accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);TextRenderer.DrawText(e.Graphics,Value.ToString(),Font,ClientRectangle,Theme.Text,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);TextRenderer.DrawText(e.Graphics,"+",Font,new Rectangle(Width*3/4,0,Width/4,Height),Theme.Accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);Focus();Value+=e.X<Width/2?-1:1;}protected override void OnMouseWheel(MouseEventArgs e){base.OnMouseWheel(e);if(Focused)Value+=Math.Sign(e.Delta);}
 protected override bool IsInputKey(Keys k){return k==Keys.Up||k==Keys.Down||k==Keys.Left||k==Keys.Right||base.IsInputKey(k);}protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Up||e.KeyCode==Keys.Right)Value++;if(e.KeyCode==Keys.Down||e.KeyCode==Keys.Left)Value--;if(e.KeyCode==Keys.PageUp)Value+=10;if(e.KeyCode==Keys.PageDown)Value-=10;}
}
internal class DarkMenuColors : ProfessionalColorTable {
 public override Color ToolStripDropDownBackground{get{return Theme.Surface;}} public override Color ImageMarginGradientBegin{get{return Theme.Surface;}} public override Color ImageMarginGradientMiddle{get{return Theme.Surface;}} public override Color ImageMarginGradientEnd{get{return Theme.Surface;}} public override Color MenuItemSelected{get{return Theme.Input;}} public override Color MenuBorder{get{return Theme.Border;}} public override Color MenuItemBorder{get{return Theme.Border;}}
}
internal class NavButton : Button {
 public string Glyph;public bool Selected;bool hover;
 public NavButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;ForeColor=Theme.Text;Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.PageTab;}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;if(Selected||hover){using(var p=Theme.Round(new RectangleF(0,0,Width-1,Height-1),Theme.Px(this,7)))using(var b=new SolidBrush(Selected?Color.FromArgb(47,47,47):Color.FromArgb(40,40,40)))e.Graphics.FillPath(b,p);}if(Selected)using(var b=new SolidBrush(Theme.Accent))e.Graphics.FillRectangle(b,Theme.Px(this,1),Theme.Px(this,13),Theme.Px(this,3),Theme.Px(this,18));using(var f=new Font("Segoe MDL2 Assets",13*Theme.Dpi(this)/72f,FontStyle.Regular,GraphicsUnit.Pixel))TextRenderer.DrawText(e.Graphics,Glyph,f,new Rectangle(Theme.Px(this,18),0,Theme.Px(this,24),Height),Selected?Theme.Accent:Theme.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter);TextRenderer.DrawText(e.Graphics,Text,Font,new Rectangle(Theme.Px(this,54),0,Width-Theme.Px(this,60),Height),Theme.Text,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.NoPrefix);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,new Rectangle(6,4,Width-12,Height-8));}
}
public partial class MainForm : Form {
 Settings settings; Engine engine; IDevices devices; bool busy,quitting,uiTest,layoutReady; 
 NotifyIcon tray; Icon appIcon; Image brandImage;
 DarkCombo monitors=new DarkCombo(),audio=new DarkCombo(),key=new DarkCombo(),launcher=new DarkCombo();
 ModernSwitch keep=new ModernSwitch(),steam=new ModernSwitch(),startup=new ModernSwitch(),disableGameBar=new ModernSwitch();
 KeyChip ctrl=new KeyChip(),alt=new KeyChip(),shift=new KeyChip(),win=new KeyChip();
 TextBox steamPath=new TextBox(); NumberStepper timeout=new NumberStepper();
 Label status=Theme.Label("Finding your displays and audio devices...",9,false,Theme.Muted),mode=Theme.Label("DESKTOP MODE",9,true,Theme.Green),hint=Theme.Label("",10,false,Theme.Muted);
 ModernButton toggle=new ModernButton{Primary=true},save=new ModernButton{Text="Save settings",Primary=true},restore=new ModernButton{Text="Restore desktop"};
 Panel panel=new Panel(),content=new Panel(),footer=new Panel(),header=new Panel();
 Card hero=new Card{Hero=true},deviceCard=new Card(),behavior=new Card(),shortcut=new Card(),advanced=new Card(),controllerCard=new Card();
 Panel sidebar=new Panel();
 Label pageTitle=Theme.Label("Welcome to SteamCouch",23,true,Theme.Text),pageHelp=Theme.Label("Settle in. Your next session is one shortcut away.",10,false,Theme.Muted);
 Label tvSummary=Theme.Label("",11,true,Theme.Text),audioSummary=Theme.Label("",10,false,Theme.Muted),steamSummary=Theme.Label("",10,false,Theme.Muted);
 Card overview=new Card();
 List<NavButton> navigation=new List<NavButton>();
 int currentPage;bool layingOut;
 sealed class LayoutSpec {public Rectangle Bounds;public Size Parent;public AnchorStyles Anchor;public DockStyle Dock;public Font Font;public Font OwnedFont;public int FontDpi;}
 Dictionary<Control,LayoutSpec> logicalLayout=new Dictionary<Control,LayoutSpec>();
 void CaptureLayout(Control parent){foreach(Control child in parent.Controls){logicalLayout[child]=new LayoutSpec{Bounds=child.Bounds,Parent=parent.ClientSize,Anchor=child.Anchor,Dock=child.Dock,Font=child.Font};child.Anchor=AnchorStyles.Top|AnchorStyles.Left;CaptureLayout(child);}}
 void PlaceLogicalChildren(Control parent){
  parent.SuspendLayout();
  foreach(Control child in parent.Controls){
   LayoutSpec spec;if(!logicalLayout.TryGetValue(child,out spec))continue;
   if(spec.FontDpi!=DeviceDpi||child.Font.Unit!=GraphicsUnit.Pixel||Math.Abs(child.Font.Size-spec.Font.SizeInPoints*DeviceDpi/72f)>0.01f){var replacement=new Font(spec.Font.FontFamily,spec.Font.SizeInPoints*DeviceDpi/72f,spec.Font.Style,GraphicsUnit.Pixel);child.Font=replacement;if(spec.OwnedFont!=null)spec.OwnedFont.Dispose();spec.OwnedFont=replacement;spec.FontDpi=DeviceDpi;}
   if(spec.Dock==DockStyle.None){
    int x=Theme.Px(this,spec.Bounds.X),y=Theme.Px(this,spec.Bounds.Y),w=Theme.Px(this,spec.Bounds.Width),h=Theme.Px(this,spec.Bounds.Height);
    int dx=parent.ClientSize.Width-Theme.Px(this,spec.Parent.Width),dy=parent.ClientSize.Height-Theme.Px(this,spec.Parent.Height);
    if((spec.Anchor&AnchorStyles.Right)!=0){if((spec.Anchor&AnchorStyles.Left)!=0)w+=dx;else x+=dx;}
    if((spec.Anchor&AnchorStyles.Bottom)!=0){if((spec.Anchor&AnchorStyles.Top)!=0)h+=dy;else y+=dy;}
    child.SetBounds(x,y,Math.Max(1,w),Math.Max(1,h));
   }
   PlaceLogicalChildren(child);
  }
  parent.ResumeLayout(true);
 }
 public MainForm(Settings s,IDevices d) {
  SuspendLayout();settings=s;devices=d;engine=new Engine(d);uiTest=Environment.GetCommandLineArgs().Contains("--ui-test");
  Text="SteamCouch";Font=new Font("Segoe UI",10);AutoScaleMode=AutoScaleMode.None;BackColor=Theme.Background;ForeColor=Theme.Text;ClientSize=new Size(1100,740);MinimumSize=new Size(960,640);StartPosition=FormStartPosition.CenterScreen;
  var iconFile=Path.Combine(Storage.Root,"assets","steamcouch.ico");if(File.Exists(iconFile)){appIcon=new Icon(iconFile);Icon=appIcon;}
  var imageFile=Path.Combine(Storage.Root,"assets","steamcouch.png");if(File.Exists(imageFile)){using(var source=Image.FromFile(imageFile))brandImage=new Bitmap(source);}
  sidebar.Size=new Size(212,740);sidebar.Dock=DockStyle.Left;sidebar.BackColor=Color.FromArgb(32,32,32);
  Theme.Place(sidebar,new PictureBox{Image=brandImage,SizeMode=PictureBoxSizeMode.Zoom,BackColor=sidebar.BackColor},20,23,38,38);
  Theme.Place(sidebar,Theme.Label("SteamCouch",14,true,Theme.Text),66,28,140,32);
  string[] names={"Home","TV & audio","Settings","Profiles","Help & updates"};string[] glyphs={"\uE80F","\uE7F4","\uE713","\uE8B7","\uE897"};
  for(int i=0;i<names.Length;i++){int page=i;var nav=new NavButton{Text=names[i],Glyph=glyphs[i],BackColor=sidebar.BackColor};Theme.Place(sidebar,nav,10,99+i*52,192,44);nav.Click+=delegate{SelectPage(page);};navigation.Add(nav);}
  var sideNote=Theme.Label("DESKTOP TO COUCH",8,true,Theme.Muted);Theme.Place(sidebar,sideNote,24,645,180,20);sideNote.Anchor=AnchorStyles.Left|AnchorStyles.Bottom;
  var sideInfo=Theme.Label("One shortcut. All set.\nSteamCouch  "+UpdateService.Current.ToString(2),9,false,Theme.Muted);Theme.Place(sidebar,sideInfo,24,676,180,48);sideInfo.Anchor=AnchorStyles.Left|AnchorStyles.Bottom;
  var main=new Panel{Dock=DockStyle.Fill,BackColor=Theme.Background};Controls.Add(main);Controls.Add(sidebar);
  header.Size=new Size(888,92);header.Dock=DockStyle.Top;header.BackColor=Theme.Background;
  Theme.Place(header,pageTitle,32,20,800,48);pageTitle.Anchor=AnchorStyles.Left|AnchorStyles.Top|AnchorStyles.Right;
  Theme.Place(header,pageHelp,34,69,800,23);pageHelp.Anchor=AnchorStyles.Left|AnchorStyles.Top|AnchorStyles.Right;
  footer.Size=new Size(888,80);footer.Dock=DockStyle.Bottom;footer.BackColor=Theme.Background;
  Theme.Place(footer,status,34,10,450,38);status.Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top;
  Theme.Place(footer,restore,544,8,148,40);restore.Anchor=AnchorStyles.Right|AnchorStyles.Top;
  Theme.Place(footer,save,704,8,150,40);save.Anchor=AnchorStyles.Right|AnchorStyles.Top;
  Theme.Place(footer,Theme.Label("Closing this window keeps SteamCouch in your system tray.",8,false,Theme.Muted),34,57,650,18);
  panel.Dock=DockStyle.Fill;panel.AutoScroll=true;panel.BackColor=Theme.Background;content.BackColor=Theme.Background;panel.Controls.Add(content);
  main.Controls.Add(panel);main.Controls.Add(footer);main.Controls.Add(header);
  Theme.Place(content,hero,32,8,808,166);
  Theme.Place(hero,mode,24,20,420,23);
  Theme.Place(hero,Theme.Label("Ready for the big screen?",21,true,Theme.Text),24,57,740,42);
  Theme.Place(hero,toggle,24,113,220,42);
  hint.TextAlign=ContentAlignment.MiddleLeft;Theme.Place(hero,hint,262,123,490,24);
  Theme.Place(content,overview,32,190,808,198);
  Theme.Place(overview,Theme.Label("Your couch setup",12,true,Theme.Text),24,19,650,28);
  Theme.Place(overview,Theme.Label("DISPLAY",8,true,Theme.Accent),24,62,115,24);Theme.Place(overview,tvSummary,148,60,632,29);tvSummary.Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top;
  Theme.Place(overview,Theme.Label("AUDIO",8,true,Theme.Accent),24,107,115,24);Theme.Place(overview,audioSummary,148,103,632,29);audioSummary.Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top;
  Theme.Place(overview,Theme.Label("GAMING",8,true,Theme.Accent),24,151,115,24);Theme.Place(overview,steamSummary,148,147,632,43);steamSummary.Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top;
  Theme.Place(content,deviceCard,32,8,808,232);
  Theme.Place(deviceCard,Theme.Label("Connect your TV",12,true,Theme.Text),24,19,460,27);
  var refresh=new ModernButton{Text="Refresh devices"};Theme.Place(deviceCard,refresh,638,14,146,36);refresh.Anchor=AnchorStyles.Top|AnchorStyles.Right;
  Theme.Place(deviceCard,Theme.Label("TV DISPLAY",8,true,Theme.Muted),24,61,700,20);
  Theme.Place(deviceCard,monitors,24,86,760,38);monitors.Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top;monitors.DropDownWidth=700;
  Theme.Place(deviceCard,Theme.Label("PLAYBACK AUDIO",8,true,Theme.Muted),24,145,700,20);
  Theme.Place(deviceCard,audio,24,170,760,38);audio.Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top;audio.DropDownWidth=760;
  Theme.Place(deviceCard,Theme.Label("Automatic selects your TV audio after the display reconnects.",8,false,Theme.Muted),24,209,720,20);
  Theme.Place(content,behavior,32,250,808,134);
  AddMonitorChoices(s);
  Theme.Place(content,shortcut,32,8,808,130);
  Theme.Place(shortcut,Theme.Label("Your shortcut",12,true,Theme.Text),24,16,450,26);
  Theme.Place(shortcut,Theme.Label("Press again to exit gaming mode and return to your desktop.",9,false,Theme.Muted),24,46,710,24);
  ctrl.Text="Ctrl";alt.Text="Alt";shift.Text="Shift";win.Text="Win";
  int chipX=24;foreach(var chip in new[]{ctrl,alt,shift,win}){Theme.Place(shortcut,chip,chipX,80,60,34);chipX+=70;chip.CheckedChanged+=delegate{UpdateHint();};}
  ctrl.Checked=(s.Modifiers&2)!=0;alt.Checked=(s.Modifiers&1)!=0;shift.Checked=(s.Modifiers&4)!=0;win.Checked=(s.Modifiers&8)!=0;
  Theme.Place(shortcut,key,304,80,100,34);foreach(Keys k in Enum.GetValues(typeof(Keys)).Cast<Keys>().Distinct())if((k>=Keys.F1&&k<=Keys.F24)||(k>=Keys.A&&k<=Keys.Z)||(k>=Keys.D0&&k<=Keys.D9))key.Items.Add(k);key.SelectedItem=(Keys)s.Key;key.SelectedIndexChanged+=delegate{UpdateHint();};
  Theme.Place(content,advanced,32,156,808,240);
  Theme.Place(advanced,Theme.Label("Launch gaming mode",11,true,Theme.Text),24,18,600,26);
  launcher.Items.Add(new Choice("Steam","Steam Big Picture"));launcher.Items.Add(new Choice("Xbox","Xbox mode"));launcher.Items.Add(new Choice("Playnite","Playnite fullscreen"));launcher.SelectedItem=launcher.Items.Cast<Choice>().FirstOrDefault(c=>c.Id==s.Launcher)??launcher.Items[0];Theme.Place(advanced,launcher,24,47,500,32);launcher.Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top;launcher.AccessibleName="Gaming launcher";
  Theme.Place(advanced,steam,738,24,46,28);steam.Anchor=AnchorStyles.Right|AnchorStyles.Top;steam.Checked=s.LaunchSteam;steam.AccessibleName="Launch selected gaming mode";
  var pathBox=new Panel{BackColor=Theme.Input,Padding=new Padding(10,9,10,8)};Theme.Place(advanced,pathBox,24,84,640,38);pathBox.Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top;
  steamPath.Text=s.SteamPath;steamPath.BorderStyle=BorderStyle.None;steamPath.BackColor=Theme.Input;steamPath.ForeColor=Theme.Text;steamPath.Dock=DockStyle.Fill;pathBox.Controls.Add(steamPath);
  var xboxHelp=Theme.Label("Enable Xbox mode in Windows Settings > Gaming. It may cover other screens.",9,false,Theme.Muted);Theme.Place(advanced,xboxHelp,24,84,632,44);xboxHelp.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;
  Action launcherChanged=()=>{string selected=((Choice)launcher.SelectedItem).Id;bool xbox=selected=="Xbox";pathBox.Visible=selected=="Steam";xboxHelp.Visible=selected!="Steam";xboxHelp.Text=xbox?"Enable Xbox mode in Windows Settings > Gaming. It may cover other screens.":"Select your Playnite program in the Couch options below.";};
  var xboxSettings=new ModernButton{Text="Windows settings"};Theme.Place(advanced,xboxSettings,676,84,108,38);xboxSettings.Font=new Font("Segoe UI",8,FontStyle.Bold);xboxSettings.Anchor=AnchorStyles.Right|AnchorStyles.Top;xboxSettings.Click+=delegate{System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("ms-settings:gaming"){UseShellExecute=true});};
  var browse=new ModernButton{Text="Browse"};Theme.Place(advanced,browse,676,84,108,38);browse.Anchor=AnchorStyles.Right|AnchorStyles.Top;
  Theme.Place(advanced,Theme.Label("Start with Windows",10,true,Theme.Text),24,137,570,25);
  Theme.Place(advanced,Theme.Label("Starts quietly in the system tray.",9,false,Theme.Muted),24,163,600,22);
  Theme.Place(advanced,startup,738,143,46,28);startup.Anchor=AnchorStyles.Right|AnchorStyles.Top;startup.Checked=s.Startup;startup.AccessibleName="Start with Windows";
  Theme.Place(advanced,Theme.Label("Device connection timeout",10,true,Theme.Text),24,207,480,26);
  timeout.Minimum=3;timeout.Maximum=120;timeout.Value=Math.Max(3,Math.Min(120,s.TimeoutSeconds));Theme.Place(advanced,timeout,590,198,114,36);timeout.Anchor=AnchorStyles.Right|AnchorStyles.Top;
  var seconds=Theme.Label("seconds",9,false,Theme.Muted);Theme.Place(advanced,seconds,716,207,68,24);seconds.Anchor=AnchorStyles.Right|AnchorStyles.Top;
  Theme.Place(content,controllerCard,32,408,808,80);
  Theme.Place(controllerCard,Theme.Label("Disable Game Bar & Controller Bar",10,true,Theme.Text),24,16,680,28);
  Theme.Place(controllerCard,Theme.Label("Stops Xbox button popups and Controller Bar on connection. Win + G still works.",9,false,Theme.Muted),24,49,710,25);
  Theme.Place(controllerCard,disableGameBar,738,22,46,28);disableGameBar.Anchor=AnchorStyles.Right|AnchorStyles.Top;disableGameBar.AccessibleName="Disable Game Bar & Controller Bar";disableGameBar.Checked=s.DisableControllerGameBar;
  disableGameBar.CheckedChanged+=delegate{
   if(!layoutReady||uiTest)return;bool previous=settings.DisableControllerGameBar;
   try{ControllerGameBar.Set(!disableGameBar.Checked);}
   catch(Exception error){layoutReady=false;disableGameBar.Checked=previous;layoutReady=true;ShowError(error);return;}
   settings.DisableControllerGameBar=disableGameBar.Checked;
   try{Storage.Save("settings.json",settings);status.ForeColor=Theme.Green;status.Text=disableGameBar.Checked?"Windows controller overlays blocked. Win + G remains available.":"Windows controller overlays enabled again.";}
   catch(Exception error){ShowError(error);}
  };
  DpiChanged+=delegate{BeginInvoke((Action)(()=>{LayoutCards();Invalidate(true);}));};
  launcher.SelectedIndexChanged+=delegate{launcherChanged();browse.Visible=((Choice)launcher.SelectedItem).Id=="Steam";xboxSettings.Visible=((Choice)launcher.SelectedItem).Id=="Xbox";UpdateSummary();};launcherChanged();browse.Visible=s.Launcher=="Steam";xboxSettings.Visible=((Choice)launcher.SelectedItem).Id=="Xbox";
  panel.Resize+=delegate{LayoutCards();};Resize+=delegate{LayoutCards();};
  refresh.Click+=async delegate{await Work(()=>RefreshData(),false);};
  browse.Click+=delegate{using(var dialog=new OpenFileDialog{Filter="Steam (steam.exe)|steam.exe",FileName=steamPath.Text})if(dialog.ShowDialog(this)==DialogResult.OK)steamPath.Text=dialog.FileName;};
  save.Click+=delegate{try{Save();status.ForeColor=Theme.Green;status.Text="Settings saved. You're ready for the couch.";}catch(Exception e){ShowError(e);}};
  toggle.Click+=async delegate{await Toggle();};restore.Click+=async delegate{await Work(()=>engine.Restore(settings.TimeoutSeconds),true);};
  var menu=new ContextMenuStrip{BackColor=Theme.Surface,ForeColor=Theme.Text,Renderer=new ToolStripProfessionalRenderer(new DarkMenuColors())};
  menu.Items.Add("Toggle TV mode",null,async delegate{await Toggle();});menu.Items.Add("Settings",null,delegate{SelectPage(2);Show();WindowState=FormWindowState.Normal;Activate();});menu.Items.Add("Restore desktop",null,async delegate{await Work(()=>engine.Restore(settings.TimeoutSeconds),true);});menu.Items.Add(new ToolStripSeparator());menu.Items.Add("Quit SteamCouch",null,async delegate{if(busy)return;if(engine.Active){await Work(()=>engine.Restore(settings.TimeoutSeconds),true);if(engine.Active)return;}quitting=true;Close();});
  tray=new NotifyIcon{Icon=appIcon??SystemIcons.Application,Text="SteamCouch",Visible=!uiTest,ContextMenuStrip=menu};tray.DoubleClick+=delegate{Show();WindowState=FormWindowState.Normal;Activate();};
  engine.Status=message=>{if(!IsDisposed&&IsHandleCreated)BeginInvoke((Action)(()=>{status.ForeColor=Theme.Muted;status.Text=message;}));};
  Shown+=async delegate{if(Environment.GetCommandLineArgs().Contains("--tray"))Hide();var work=Screen.FromControl(this).WorkingArea;Size=new Size(Math.Min(Width,work.Width-32),Math.Min(Height,work.Height-32));CenterToScreen();Theme.DarkTitle(this);Theme.SetWindowTheme(panel.Handle,"DarkMode_Explorer",null);if(!uiTest)try{Register(settings);}catch(Exception e){ShowError(e);}await Work(()=>RefreshData(),false);if(engine.Active)status.Text="A saved TV session is available. Restore desktop when you're ready.";UpdateMode();if(!uiTest)await StartFeatures();if(Environment.GetCommandLineArgs().Contains("--tray"))Hide();};
  FormClosing+=delegate(object sender,FormClosingEventArgs e){if(!quitting&&e.CloseReason==CloseReason.UserClosing){e.Cancel=true;Hide();}};
  FormClosed+=delegate{Microsoft.Win32.SystemEvents.DisplaySettingsChanged-=blackDisplayChanged;if(blackScreens!=null)blackScreens.Dispose();MonitorPower.Release();DisposeFeatures();Native.UnregisterHotKey(Handle,1);tray.Dispose();if(brandImage!=null)brandImage.Dispose();if(appIcon!=null)appIcon.Dispose();foreach(var spec in logicalLayout.Values)if(spec.OwnedFont!=null)spec.OwnedFont.Dispose();};
  startup.CheckedChanged+=delegate{if(!layoutReady||uiTest)return;bool previous=settings.Startup;try{StartupRegistration.Set(startup.Checked);settings.Startup=startup.Checked;Storage.Save("settings.json",settings);status.ForeColor=Theme.Green;status.Text=startup.Checked?"SteamCouch will start in your tray when you sign in.":"Windows startup is off.";}catch(Exception e){layoutReady=false;startup.Checked=previous;layoutReady=true;settings.Startup=previous;try{StartupRegistration.Set(previous);}catch{}ShowError(e);}};
  monitors.AccessibleName="TV display";audio.AccessibleName="Playback audio";key.AccessibleName="Shortcut key";steamPath.AccessibleName="Steam program";timeout.AccessibleName="Device wait limit in seconds";
  AddFeatures();
  foreach(var section in new Control[]{header,footer,sidebar,hero,overview,deviceCard,behavior,shortcut,advanced,controllerCard})CaptureLayout(section);
  AutoScaleMode=AutoScaleMode.Dpi;AutoScaleDimensions=new SizeF(96,96);PerformAutoScale();ResumeLayout(true);layoutReady=true;AddQuickMenu();AddOptionalFeatures();AddCec();AddGoogleTv();SelectPage(Environment.GetCommandLineArgs().Contains("--settings")?2:0);UpdateMode();UpdateHint();
 }

 void SelectPage(int page){UpdateSummary();currentPage=page;string[] titles={"Welcome to SteamCouch","TV & audio","Settings","Display profiles","Help & updates"};string[] helpers={"Settle in. Your next session is one shortcut away.","Choose where your games look and sound their best.","Make SteamCouch feel right for you.","Save your favorite screens and gaming setups.","Stay up to date and get your couch setup ready."};pageTitle.Text=titles[page];pageHelp.Text=helpers[page];for(int i=0;i<navigation.Count;i++){navigation[i].Selected=i==page;navigation[i].Invalidate();}panel.AutoScrollPosition=Point.Empty;LayoutCards();}
 void LayoutCards(){
  if(!layoutReady||layingOut)return;int initialViewport=panel.ClientSize.Width;layingOut=true;
  try{
   bool centered=currentPage==0&&WindowState==FormWindowState.Maximized;
   int unitMargin=Theme.Px(this,32);
   sidebar.Width=Theme.Px(this,212);footer.Height=Theme.Px(this,80);
   int topInset=centered?Math.Max(0,(header.Parent.ClientSize.Height-footer.Height-Theme.Px(this,496))/2):0;
   header.Height=Theme.Px(this,92)+topInset;
   int w=Math.Max(Theme.Px(this,660),panel.Width-Math.Max(Theme.Px(this,17),GetSystemMetricsForDpi(2,GetDpiForWindow(Handle)))-Theme.Px(this,2));content.Width=w;
   int cardWidth=centered?Math.Min(Theme.Px(this,880),w-2*unitMargin):w-2*unitMargin;
   int cardLeft=centered?(panel.ClientSize.Width-cardWidth)/2:unitMargin;
   Card[] cards={hero,overview,deviceCard,behavior,shortcut,advanced,controllerCard};int[] ys={8,190,8,250,8,156,408};int[] heights={166,198,232,134,130,240,80};
   for(int i=0;i<cards.Length;i++){var card=cards[i];card.SetBounds(centered&&i<2?cardLeft:unitMargin,Theme.Px(this,ys[i]),centered&&i<2?cardWidth:w-2*unitMargin,Theme.Px(this,heights[i]));card.Visible=currentPage==0?(i<2):currentPage==1?(i==2||i==3):currentPage==2?(i>3):false;PlaceLogicalChildren(card);}
   foreach(var section in new Control[]{header,footer,sidebar})PlaceLogicalChildren(section);
   if(centered){
    pageTitle.SetBounds(cardLeft,topInset+Theme.Px(this,20),cardWidth,Theme.Px(this,48));
    pageHelp.SetBounds(cardLeft+Theme.Px(this,2),topInset+Theme.Px(this,69),cardWidth-Theme.Px(this,2),Theme.Px(this,23));
    save.Left=cardLeft+cardWidth-save.Width;restore.Left=save.Left-Theme.Px(this,12)-restore.Width;
    status.Left=cardLeft+Theme.Px(this,2);
    foreach(Control child in footer.Controls)if(child is Label&&child!=status){child.Left=status.Left;child.Width=cardWidth;}
   }
   status.Width=Math.Max(Theme.Px(this,100),restore.Left-status.Left-Theme.Px(this,18));status.AutoEllipsis=true;
   content.Height=(currentPage==0?overview.Bottom:currentPage==1?behavior.Bottom:controllerCard.Bottom)+Theme.Px(this,8);LayoutFeatures(w);LayoutQuickMenu(w);LayoutOptionalFeatures(w);LayoutGoogleTv(w);LayoutCec(w);panel.AutoScrollMinSize=new Size(0,content.Height);
  }finally{layingOut=false;}
  if(panel.ClientSize.Width!=initialViewport)LayoutCards();
 } void UpdateSummary(){tvSummary.Text=monitors.SelectedItem==null?"Choose a TV in TV & audio":monitors.Text;audioSummary.Text=audio.Text;steamSummary.Text=steam.Checked?(launcher.SelectedItem!=null&&((Choice)launcher.SelectedItem).Id=="Xbox"?"Xbox mode opens on your TV and exits on return.":launcher.SelectedItem!=null&&((Choice)launcher.SelectedItem).Id=="Playnite"?"Playnite fullscreen opens on your TV and returns to desktop mode.":"Big Picture opens on your TV and closes on return."):"Automatic gaming-mode launch is off.";} void UpdateHint(){var values=new List<string>();if(ctrl.Checked)values.Add("Ctrl");if(alt.Checked)values.Add("Alt");if(shift.Checked)values.Add("Shift");if(win.Checked)values.Add("Win");if(key.SelectedItem!=null)values.Add(key.SelectedItem.ToString());hint.Text=string.Join("  +  ",values);}
 void UpdateMode(){UpdateBlackScreens();UpdateFeatureMode();toggle.Text=busy?"Switching...":engine.Active?"Return to desktop":"Activate TV mode";mode.Text=busy?"SWITCHING":engine.Active?"TV MODE":"DESKTOP MODE";mode.ForeColor=engine.Active?Theme.Accent:Theme.Green;restore.Enabled=!busy&&engine.Active;tray.Text=engine.Active?"SteamCouch - TV mode":"SteamCouch - Desktop mode";}
 void Register(Settings s){if(!Native.RegisterHotKey(Handle,1,s.Modifiers|0x4000,(uint)s.Key))throw new InvalidOperationException("That shortcut is already in use. Choose another combination and save.");}
 void Save(){
  if(busy)throw new InvalidOperationException("Wait for the current operation to finish.");
  if(monitors.SelectedItem==null||audio.SelectedItem==null||key.SelectedItem==null)throw new InvalidOperationException("Select a TV, audio option, and shortcut.");
  uint mods=(uint)((ctrl.Checked?2:0)|(alt.Checked?1:0)|(shift.Checked?4:0)|(win.Checked?8:0));if(mods==0)throw new InvalidOperationException("Choose at least one shortcut modifier, such as Ctrl or Alt.");
  var next=new Settings{MonitorId=((Choice)monitors.SelectedItem).Id,AudioId=((Choice)audio.SelectedItem).Id,Launcher=((Choice)launcher.SelectedItem).Id,OtherMonitorMode=((Choice)otherMonitorMode.SelectedItem).Id,KeepOthers=keep.Checked,LaunchSteam=steam.Checked,SteamPath=steamPath.Text.Trim(),Startup=startup.Checked,DisableControllerGameBar=disableGameBar.Checked,Modifiers=mods,Key=(int)(Keys)key.SelectedItem,TimeoutSeconds=(int)timeout.Value};
  SaveFeatureSettings(next);
  if(next.Launcher=="Xbox"&&next.Modifiers==8&&next.Key==(int)Keys.F11)throw new InvalidOperationException("Win + F11 belongs to Xbox mode. Choose a different SteamCouch shortcut.");
  if(next.LaunchSteam&&next.Launcher=="Steam"&&!File.Exists(next.SteamPath))throw new InvalidOperationException("Select an existing Steam program.");
  if(!uiTest){Native.UnregisterHotKey(Handle,1);try{Register(next);StartupRegistration.Set(next.Startup);Storage.Save("settings.json",next);}catch{try{Native.UnregisterHotKey(Handle,1);Register(settings);}catch{}throw;}}
  else Storage.Save("settings.json",next);
  settings=next;UpdateSummary();UpdateFeatureMode();
  if(!uiTest)StartupRegistration.Set(next.Startup);
 }
 void RefreshData(){var ms=devices.Monitors();var aud=devices.Audio();Invoke((Action)(()=>{
  string mid=monitors.SelectedItem==null?settings.MonitorId:((Choice)monitors.SelectedItem).Id;string aid=audio.SelectedItem==null?settings.AudioId:((Choice)audio.SelectedItem).Id;
  monitors.Items.Clear();foreach(var m in ms.Where(m=>m.Get("Monitor ID")!="")){string name=m.Get("Monitor Name")==""?m.Get("Short Monitor ID"):m.Get("Monitor Name");monitors.Items.Add(new Choice(m.Get("Monitor ID"),name+(Engine.IsOn(m)?"  (connected)":"  (disconnected)")));}
  audio.Items.Clear();audio.Items.Add(new Choice("","Automatic - TV audio"));foreach(var a in aud)audio.Items.Add(new Choice(a.Get("Item ID"),a.Get("Name")+" - "+a.Get("Device Name")+" ["+a.Get("Device State")+"]"));
  monitors.SelectedItem=monitors.Items.Cast<Choice>().FirstOrDefault(c=>c.Id==mid);if(monitors.SelectedItem==null&&mid!=""){var missing=new Choice(mid,"Saved TV (currently unavailable)");monitors.Items.Add(missing);monitors.SelectedItem=missing;}if(monitors.SelectedItem==null)monitors.SelectedItem=monitors.Items.Cast<Choice>().FirstOrDefault(c=>c.Label.IndexOf("TV",StringComparison.OrdinalIgnoreCase)>=0);
  audio.SelectedItem=audio.Items.Cast<Choice>().FirstOrDefault(c=>c.Id==aid);if(audio.SelectedItem==null){var missing=new Choice(aid,"Saved TV audio (currently unavailable)");audio.Items.Add(missing);audio.SelectedItem=missing;}
  UpdateSummary();status.ForeColor=Theme.Muted;status.Text="Ready. Changes are applied when you save or activate TV mode.";
 }));}
 async Task Toggle(){if(busy)return;if(!engine.Active)try{Save();}catch(Exception e){ShowError(e);return;}await Work(()=>{if(engine.Active)engine.Restore(settings.TimeoutSeconds);else engine.Activate(settings);},true);}
 async Task Work(Action action,bool notify){if(busy)return;if(quickMenu!=null&&!quickMenu.IsDisposed){if(!quickMenu.CanClose)return;quickMenu.Close();if(quickMenu!=null&&!quickMenu.IsDisposed)return;}if(notify&&engine.Active&&blackScreens!=null)blackScreens.Dispose();busy=true;panel.Enabled=false;save.Enabled=false;UpdateMode();try{await Task.Run(action);if(notify)tray.ShowBalloonTip(2500,"SteamCouch",engine.Active?"TV mode enabled.":"Desktop restored.",ToolTipIcon.Info);}catch(Exception e){ShowError(e);}finally{busy=false;panel.Enabled=true;save.Enabled=true;UpdateMode();}}
 void ShowError(Exception e){Storage.Log(e.ToString());status.ForeColor=Color.FromArgb(255,175,153);status.Text=e.Message;Show();WindowState=FormWindowState.Normal;
  if(uiTest)throw new InvalidOperationException("UI test: "+e.Message,e);
  using(var dialog=new Form{Text="SteamCouch",BackColor=Theme.Background,ForeColor=Theme.Text,Font=Font,ClientSize=new Size(540,270),StartPosition=FormStartPosition.CenterParent,MinimizeBox=false,MaximizeBox=false,FormBorderStyle=FormBorderStyle.FixedDialog}){
   var title=Theme.Label("Let's get you connected",17,true,Theme.Text);Theme.Place(dialog,title,24,20,490,38);
   var text=new TextBox{Text=e.Message,Multiline=true,ReadOnly=true,BorderStyle=BorderStyle.None,BackColor=Theme.Background,ForeColor=Theme.Muted,ScrollBars=ScrollBars.Vertical};Theme.Place(dialog,text,26,72,488,122);
   var ok=new ModernButton{Text="Got it",Primary=true,DialogResult=DialogResult.OK};Theme.Place(dialog,ok,382,213,132,38);dialog.AutoScaleMode=AutoScaleMode.Dpi;dialog.AutoScaleDimensions=new SizeF(96,96);dialog.PerformAutoScale();dialog.AcceptButton=ok;dialog.Shown+=delegate{Theme.DarkTitle(dialog);};dialog.ShowDialog(this);
  }
 }
 void CheckBindings(){
  if(busy)throw new InvalidOperationException("UI is still refreshing.");
  string path=Storage.PathOf("settings.json");if(!File.Exists(path))Storage.Save("settings.json",settings);string original=File.ReadAllText(path);var saved=Storage.Read<Settings>("settings.json");
  try {
   launcher.SelectedItem=launcher.Items.Cast<Choice>().First(c=>c.Id=="Xbox");disableGameBar.Checked=!saved.DisableControllerGameBar;keep.Checked=!saved.KeepOthers;steam.Checked=!saved.LaunchSteam;startup.Checked=!saved.Startup;timeout.Value=saved.TimeoutSeconds==45?46:45;key.SelectedItem=Keys.F11;ctrl.Checked=true;alt.Checked=false;shift.Checked=false;win.Checked=false;
   Save();var result=Storage.Read<Settings>("settings.json");
   if(result.DisableControllerGameBar==saved.DisableControllerGameBar||result.Launcher!="Xbox"||result.KeepOthers==saved.KeepOthers||result.LaunchSteam==saved.LaunchSteam||result.Startup==saved.Startup||result.Key!=(int)Keys.F11||result.Modifiers!=2||result.TimeoutSeconds!=(int)timeout.Value)throw new InvalidOperationException("The redesigned controls did not save their values.");
  } finally {
   File.WriteAllText(path,original);settings=saved;disableGameBar.Checked=saved.DisableControllerGameBar;launcher.SelectedItem=launcher.Items.Cast<Choice>().First(c=>c.Id==saved.Launcher);SetOtherMonitorMode(saved);steam.Checked=saved.LaunchSteam;startup.Checked=saved.Startup;timeout.Value=saved.TimeoutSeconds;key.SelectedItem=(Keys)saved.Key;ctrl.Checked=(saved.Modifiers&2)!=0;alt.Checked=(saved.Modifiers&1)!=0;shift.Checked=(saved.Modifiers&4)!=0;win.Checked=(saved.Modifiers&8)!=0;UpdateHint();
  }
 }
 [DllImport("user32.dll")] static extern IntPtr GetWindowDpiAwarenessContext(IntPtr h);
 [DllImport("user32.dll")] static extern int GetAwarenessFromDpiAwarenessContext(IntPtr c);
 [DllImport("user32.dll")] static extern uint GetDpiForWindow(IntPtr h);
 [DllImport("user32.dll")] static extern int GetSystemMetricsForDpi(int index,uint dpi);
 [StructLayout(LayoutKind.Sequential)] struct DpiRectangle {public int Left,Top,Right,Bottom;}
 void PreviewAtDpi(int dpi){
  Size=new Size((int)(1100*dpi/96.0),(int)(740*dpi/96.0));
  var r=new DpiRectangle{Left=Left,Top=Top,Right=Left+(int)(1100*dpi/96.0),Bottom=Top+(int)(740*dpi/96.0)};
  IntPtr memory=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DpiRectangle)));
  try{Marshal.StructureToPtr(r,memory,false);var message=Message.Create(Handle,0x2e0,new IntPtr(dpi|(dpi<<16)),memory);WndProc(ref message);Application.DoEvents();}finally{Marshal.FreeHGlobal(memory);}
 }
 void CheckDpiPreview(){
  var report=new List<string>{"Framework: "+AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName,"OS: "+Environment.OSVersion,"DPI awareness: "+GetAwarenessFromDpiAwarenessContext(GetWindowDpiAwarenessContext(Handle)),"Visual styles: "+Application.RenderWithVisualStyles};
  foreach(var screen in Screen.AllScreens){Size=new Size(960,640);Location=new Point(screen.WorkingArea.Left+10,screen.WorkingArea.Top+10);Application.DoEvents();Size=new Size(Theme.Px(this,960),Theme.Px(this,640));SelectPage(2);Application.DoEvents();using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Storage.PathOf("monitor-"+DeviceDpi+".png"));}bool correct=GetDpiForWindow(Handle)==DeviceDpi&&sidebar.Width==Theme.Px(this,212)&&footer.ClientRectangle.Contains(save.Bounds)&&advanced.ClientRectangle.Contains(startup.Bounds)&&advanced.ClientRectangle.Contains(steam.Bounds)&&advanced.ClientRectangle.Contains(timeout.Bounds)&&steamPath.Width>100&&!panel.HorizontalScroll.Visible;report.Add(screen.DeviceName+": "+(correct?"PASS":"FAIL")+" native="+GetDpiForWindow(Handle)+" managed="+DeviceDpi+" sidebar="+sidebar.Width);if(!correct){File.WriteAllLines(Storage.PathOf("dpi-test.txt"),report);throw new InvalidOperationException("Monitor DPI transition failed.");}}
  foreach(int dpi in new[]{96,144,192,288,96}){
   PreviewAtDpi(dpi);
   for(int page=0;page<5;page++){
    SelectPage(page);PerformLayout();Application.DoEvents();
    string name="dpi-"+dpi+"-"+page+".png";
    using(var bitmap=new Bitmap(Width,Height)){bitmap.SetResolution(dpi,dpi);DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Storage.PathOf(name));}
    bool valid=DeviceDpi==dpi&&sidebar.Width==Theme.Px(this,212)&&footer.ClientRectangle.Contains(save.Bounds)&&!panel.HorizontalScroll.Visible;
    report.Add("DPI "+dpi+" page "+page+": "+(valid?"PASS":"FAIL")+"; actual="+DeviceDpi+" sidebar="+sidebar.Width+" heading="+pageTitle.Height+"/"+pageTitle.Font.Height+" window="+Size);
    if(!valid){File.WriteAllLines(Storage.PathOf("dpi-test.txt"),report);throw new InvalidOperationException("DPI layout failed: "+report.Last());}
   }
  }
  File.WriteAllLines(Storage.PathOf("dpi-test.txt"),report);
 }
 public void WritePreview(){
  if(Environment.GetCommandLineArgs().Contains("--menu-test")){SelectPage(1);monitors.CheckMenuLifetime();audio.CheckMenuLifetime();SelectPage(2);launcher.CheckMenuLifetime();key.CheckMenuLifetime();File.WriteAllText(Storage.PathOf("menu-test.txt"),"PASS: display, audio, launcher, and key dropdowns each opened and closed 12 times, covering selection, outside-click dismissal, and keyboard dismissal.");}
  if(uiTest){CheckBindings();CheckFeatureBindings();CheckOptionalBindings();CheckCecBindings();CheckGoogleTvBindings();CheckMonitorChoices();}if(busy)throw new InvalidOperationException("Device refresh has not completed.");
  var report=new List<string>{"Settings binding check: PASS","TV: "+monitors.Text,"Shortcut: "+hint.Text,"Keep monitors: "+keep.Checked,"Icon: "+(appIcon!=null)};
  foreach(var size in new[]{new Size(1100,740),new Size(960,640)}){Size=new Size(Theme.Px(this,size.Width),Theme.Px(this,size.Height));PerformLayout();for(int page=0;page<5;page++){SelectPage(page);PerformLayout();using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Storage.PathOf("preview-"+page+"-"+size.Width+".png"));}bool valid=footer.ClientRectangle.Contains(save.Bounds)&&footer.ClientRectangle.Contains(restore.Bounds)&&!panel.HorizontalScroll.Visible&&(page==1||page==2||page==3||page==4||!panel.VerticalScroll.Visible);report.Add(size.Width+" page "+page+" layout: "+(valid?"PASS":"FAIL"));if(!valid){File.WriteAllLines(Storage.PathOf("ui-test.txt"),report);throw new InvalidOperationException("Preview layout overflow; DPI="+DeviceDpi+" sidebar="+sidebar.Width+" footer="+footer.Size+" save="+save.Bounds+" panel="+panel.ClientSize+" content="+content.Bounds+" min="+panel.AutoScrollMinSize+" horizontal="+panel.HorizontalScroll.Visible);}}}
  launcher.SelectedItem=launcher.Items.Cast<Choice>().First(c=>c.Id=="Xbox");SelectPage(2);Refresh();using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Storage.PathOf("xbox-settings-preview.png"));}launcher.SelectedItem=launcher.Items.Cast<Choice>().First(c=>c.Id==settings.Launcher);
  SelectPage(2);panel.ScrollControlIntoView(awakeOptions);Refresh();using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Storage.PathOf("controller-settings-preview.png"));}
  if(uiTest){PreviewOptionalFeatures();PreviewWizard();PreviewQuickMenu().GetAwaiter().GetResult();}
  WindowState=FormWindowState.Maximized;Application.DoEvents();SelectPage(0);Application.DoEvents();LayoutCards();Refresh();
  using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Storage.PathOf("home-maximized.png"));}
  bool centeredHome=hero.Width<=Theme.Px(this,880)&&Math.Abs(hero.Left-(panel.ClientSize.Width-hero.Width)/2)<=1&&pageTitle.Left==hero.Left;
  report.Add("Maximized Home centered: "+centeredHome+"; hero="+hero.Bounds+" panel="+panel.ClientSize+" titleLeft="+pageTitle.Left+" DPI="+DeviceDpi+" state="+WindowState);
  SelectPage(1);bool otherPageUnchanged=deviceCard.Left==Theme.Px(this,32);report.Add("TV & audio layout preserved: "+otherPageUnchanged);
  WindowState=FormWindowState.Normal;Application.DoEvents();SelectPage(0);bool normalHome=hero.Left==Theme.Px(this,32);report.Add("Restored Home layout preserved: "+normalHome);
  File.WriteAllLines(Storage.PathOf("ui-test.txt"),report);if(!centeredHome||!otherPageUnchanged||!normalHome)throw new InvalidOperationException("Maximized layout check failed.");
  if(Environment.GetCommandLineArgs().Contains("--dpi-test"))CheckDpiPreview();
 } protected override void WndProc(ref Message m){if(m.Msg==0x312&&m.WParam.ToInt32()==1)BeginInvoke((Action)(async()=>await Toggle()));base.WndProc(ref m);}
}
}
