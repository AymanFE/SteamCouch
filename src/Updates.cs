using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
namespace TVLounge {
internal sealed class UpdateRelease {public string Version,Url,Digest,Notes;public long Size;}
internal sealed class UpdateHistory {public DateTime LastCheckUtc;public string NotifiedVersion;}
internal static class UpdateService {
 public const string Repository="AymanFE/SteamCouch";
 public static Version Current {get{return typeof(Program).Assembly.GetName().Version;}}
 static HttpWebRequest Request(string url){ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;var request=(HttpWebRequest)WebRequest.Create(url);request.UserAgent="SteamCouch/"+Current;request.Accept="application/vnd.github+json";request.Timeout=20000;request.ReadWriteTimeout=20000;return request;}
 public static UpdateRelease Parse(string json,Version current){
  var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(json);
  if(Convert.ToBoolean(data["draft"])||Convert.ToBoolean(data["prerelease"]))return null;
  string tag=(string)data["tag_name"];Version version;if(!Version.TryParse(tag.TrimStart('v'),out version)||version<=new Version(current.Major,current.Minor,current.Build))return null;
  string name="SteamCouch-"+tag+"-windows-x64.zip";var assets=(System.Collections.IEnumerable)data["assets"];
  var asset=assets.Cast<Dictionary<string,object>>().FirstOrDefault(a=>(string)a["name"]==name);
  if(asset==null)throw new InvalidOperationException("This release has no Windows download yet.");
  string url=(string)asset["browser_download_url"],digest=asset.ContainsKey("digest")?asset["digest"] as string:null;
  if(!url.StartsWith("https://github.com/"+Repository+"/releases/download/"+tag+"/",StringComparison.Ordinal)||digest==null||!System.Text.RegularExpressions.Regex.IsMatch(digest,@"^sha256:[0-9a-fA-F]{64}$"))throw new InvalidOperationException("The release has no valid download checksum.");
  return new UpdateRelease{Version=version.ToString(),Url=url,Digest=digest.Substring(7).ToLowerInvariant(),Notes=data["body"] as string,Size=Convert.ToInt64(asset["size"])};
 }
 public static UpdateRelease Check(){using(var response=Request("https://api.github.com/repos/"+Repository+"/releases/latest").GetResponse())using(var reader=new StreamReader(response.GetResponseStream()))return Parse(reader.ReadToEnd(),Current);}
 public static string SafeEntry(string root,string name){
  name=name.Replace('\\','/');if(string.IsNullOrEmpty(name)||name.StartsWith("/")||name.IndexOf(':')>=0||name.Split('/').Any(p=>p==".."||p=="."))throw new InvalidDataException("Unsafe path in update.");
  string first=name.Split('/')[0];bool allowed=new[]{"SteamCouch.exe","SteamCouch.exe.config","assets","tools","README.md","LICENSE","THIRD-PARTY-NOTICES.md","CHANGELOG.md"}.Contains(first,StringComparer.OrdinalIgnoreCase);
  if(!allowed||name.Split('/').Any(p=>p.EndsWith(".")||p.EndsWith(" ")))throw new InvalidDataException("Unexpected file in update: "+name);
  string path=Path.GetFullPath(Path.Combine(root,name.Replace('/',Path.DirectorySeparatorChar)));if(!path.StartsWith(Path.GetFullPath(root)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Update path escapes staging folder.");return path;
 }
 public static void Extract(string zip,string folder){
  Directory.CreateDirectory(folder);var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);long total=0;
  using(var archive=ZipFile.OpenRead(zip))foreach(var entry in archive.Entries){string path=SafeEntry(folder,entry.FullName);if(!names.Add(path))throw new InvalidDataException("Duplicate update file.");if(((entry.ExternalAttributes>>16)&0xf000)==0xa000)throw new InvalidDataException("Links are not allowed in updates.");total+=entry.Length;if(total>150*1024*1024)throw new InvalidDataException("Update is too large.");if(entry.FullName.EndsWith("/")){Directory.CreateDirectory(path);continue;}Directory.CreateDirectory(Path.GetDirectoryName(path));entry.ExtractToFile(path);}
  foreach(string required in new[]{"SteamCouch.exe","SteamCouch.exe.config",@"assets\steamcouch.ico",@"tools\windows\apply-update.ps1",@"tools\windows\controller-overlays.ps1",@"tools\display\MultiMonitorTool.exe",@"tools\audio\SoundVolumeView.exe"})if(!File.Exists(Path.Combine(folder,required)))throw new InvalidDataException("Update is incomplete: "+required);
 }
 public static string Download(UpdateRelease release){
  if(release.Size<=0||release.Size>50*1024*1024)throw new InvalidDataException("Unexpected update size.");
  string root=Storage.PathOf("update-staging"),job=Path.Combine(root,Guid.NewGuid().ToString("N"));Directory.CreateDirectory(job);string zip=Path.Combine(job,"download.zip");
  using(var response=Request(release.Url).GetResponse())using(var input=response.GetResponseStream())using(var output=File.Create(zip)){byte[] buffer=new byte[65536];long count=0;int size;while((size=input.Read(buffer,0,buffer.Length))>0){count+=size;if(count>50*1024*1024)throw new InvalidDataException("Download is too large.");output.Write(buffer,0,size);}if(count!=release.Size)throw new InvalidDataException("Download size did not match the release.");}
  using(var hash=SHA256.Create())using(var input=File.OpenRead(zip)){string actual=BitConverter.ToString(hash.ComputeHash(input)).Replace("-","").ToLowerInvariant();if(actual!=release.Digest)throw new InvalidDataException("Update checksum did not match. The installed version is unchanged.");}
  string files=Path.Combine(job,"files");Extract(zip,files);Version version;if(!Version.TryParse(FileVersionInfo.GetVersionInfo(Path.Combine(files,"SteamCouch.exe")).FileVersion,out version)||version!=new Version(release.Version+".0"))throw new InvalidDataException("Downloaded app version did not match the release.");return files;
 }
 public static void Install(string files,bool tray){
  string parent=Path.GetFullPath(Storage.PathOf("update-staging"))+Path.DirectorySeparatorChar;if(!Path.GetFullPath(files).StartsWith(parent,StringComparison.OrdinalIgnoreCase)||Path.GetFileName(files)!="files")throw new InvalidOperationException("Invalid update folder.");
  string script=Path.Combine(Storage.Root,@"tools\windows\apply-update.ps1"),shell=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),@"System32\WindowsPowerShell\v1.0\powershell.exe");
  if(!File.Exists(script))throw new FileNotFoundException("Update helper is missing.");
  Process.Start(new ProcessStartInfo(shell,"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "+Devices.Quote(script)+" -Stage "+Devices.Quote(files)+" -Target "+Devices.Quote(Path.GetFullPath(Storage.Root))+" -WaitPid "+Process.GetCurrentProcess().Id+(tray?" -Tray":"")){UseShellExecute=false,CreateNoWindow=true});
 }
}
internal static class UpdateTests {
 public static void Run(){
  string root=Path.Combine(Path.GetTempPath(),"SteamCouch-test-"+Guid.NewGuid().ToString("N"));
  foreach(string name in new[]{"../escape.exe","C:/escape.exe","data/settings.json","tools/../../escape.exe","assets/a. ","/SteamCouch.exe"}){bool rejected=false;try{UpdateService.SafeEntry(root,name);}catch(InvalidDataException){rejected=true;}if(!rejected)throw new Exception("Update path was not rejected: "+name);}
  if(!UpdateService.SafeEntry(root,"tools/windows/apply-update.ps1").StartsWith(root))throw new Exception("Safe update file rejected.");
  string release="{\"draft\":false,\"prerelease\":false,\"tag_name\":\"v9.1.0\",\"body\":\"Changes\",\"assets\":[{\"name\":\"SteamCouch-v9.1.0-windows-x64.zip\",\"browser_download_url\":\"https://github.com/AymanFE/SteamCouch/releases/download/v9.1.0/app.zip\",\"digest\":\"sha256:"+new string('a',64)+"\",\"size\":123}]}";
  if(UpdateService.Parse(release,new Version(1,0,0))==null||UpdateService.Parse(release,new Version(9,1,0))!=null)throw new Exception("Update version comparison failed.");bool invalid=false;try{UpdateService.Parse(release.Replace("github.com/AymanFE/SteamCouch","example.com/other"),new Version(1,0,0));}catch(InvalidOperationException){invalid=true;}if(!invalid)throw new Exception("Foreign update accepted.");
  File.WriteAllText(Storage.PathOf("updates-test.txt"),"PASS: newer/equal version handling, fixed repository, checksum requirement, protected data paths and traversal rejection.");
 }
}
}
