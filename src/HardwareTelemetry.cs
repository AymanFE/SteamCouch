using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
namespace TVLounge {
internal sealed class TelemetrySnapshot {public float? Cpu;public float[] Cores=new float[0];public ulong? RamUsed,RamTotal;public List<GpuReading> Gpus=new List<GpuReading>();public string CpuTemperature="Unavailable",CpuPower="Unavailable";}
internal sealed class GpuReading {public string Name;public uint? Usage,Temperature,Clock,MemoryClock;public double? Watts;public ulong? Used,Total;}
internal sealed class HardwareTelemetry : IDisposable {
 [StructLayout(LayoutKind.Sequential)]struct CpuTime {public long Idle,Kernel,User,Dpc,Interrupt;public uint Count;}
 [DllImport("ntdll.dll")]static extern int NtQuerySystemInformation(int type,IntPtr data,int length,out int returned);
 [StructLayout(LayoutKind.Sequential)]struct Memory {public uint Length,Load;public ulong Total,Available,PageTotal,PageAvailable,VirtualTotal,VirtualAvailable,Extended;}
 [DllImport("kernel32.dll")]static extern bool GlobalMemoryStatusEx(ref Memory memory);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern IntPtr LoadLibraryEx(string path,IntPtr file,uint flags);
 [DllImport("nvml.dll")]static extern int nvmlInit_v2();[DllImport("nvml.dll")]static extern int nvmlShutdown();[DllImport("nvml.dll")]static extern int nvmlDeviceGetCount_v2(out uint count);[DllImport("nvml.dll")]static extern int nvmlDeviceGetHandleByIndex_v2(uint index,out IntPtr device);
 [DllImport("nvml.dll")]static extern int nvmlDeviceGetName(IntPtr device,StringBuilder name,uint size);
 [StructLayout(LayoutKind.Sequential)]struct Util {public uint Gpu,Memory;}
 [StructLayout(LayoutKind.Sequential)]struct GpuMemory {public ulong Total,Free,Used;}
 [DllImport("nvml.dll")]static extern int nvmlDeviceGetUtilizationRates(IntPtr device,out Util rates);
 [DllImport("nvml.dll")]static extern int nvmlDeviceGetTemperature(IntPtr device,int sensor,out uint value);
 [DllImport("nvml.dll")]static extern int nvmlDeviceGetPowerUsage(IntPtr device,out uint value);
 [DllImport("nvml.dll")]static extern int nvmlDeviceGetClockInfo(IntPtr device,int type,out uint value);
 [DllImport("nvml.dll")]static extern int nvmlDeviceGetMemoryInfo(IntPtr device,out GpuMemory value);
 CpuTime[] previous;bool nvmlTried,nvmlReady;
 readonly object gate=new object();public TelemetrySnapshot Read(){lock(gate)return ReadCore();} TelemetrySnapshot ReadCore(){var result=new TelemetrySnapshot();int size=Marshal.SizeOf(typeof(CpuTime)),count=Math.Max(1,Environment.ProcessorCount),length=count*size;var data=Marshal.AllocHGlobal(length);try{int returned;if(NtQuerySystemInformation(8,data,length,out returned)>=0){count=returned/size;var current=new CpuTime[count];var cores=new float[count];bool valid=previous!=null&&previous.Length==count;for(int i=0;i<count;i++){current[i]=(CpuTime)Marshal.PtrToStructure(IntPtr.Add(data,i*size),typeof(CpuTime));if(valid){long total=current[i].Kernel+current[i].User-previous[i].Kernel-previous[i].User,idle=current[i].Idle-previous[i].Idle;cores[i]=total<=0?0:Math.Max(0,Math.Min(100,100f*(total-idle)/total));}}previous=current;if(valid){result.Cores=cores;result.Cpu=cores.Average();}}}finally{Marshal.FreeHGlobal(data);}var memory=new Memory{Length=(uint)Marshal.SizeOf(typeof(Memory))};if(GlobalMemoryStatusEx(ref memory)){result.RamTotal=memory.Total;result.RamUsed=memory.Total-memory.Available;}
 try{if(!nvmlTried){nvmlTried=true;string path=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"nvml.dll");nvmlReady=System.IO.File.Exists(path)&&LoadLibraryEx(path,IntPtr.Zero,0x1100)!=IntPtr.Zero&&nvmlInit_v2()==0;}if(nvmlReady){uint n;if(nvmlDeviceGetCount_v2(out n)==0)for(uint i=0;i<Math.Min(n,16);i++){IntPtr device;if(nvmlDeviceGetHandleByIndex_v2(i,out device)!=0)continue;var gpu=new GpuReading();var name=new StringBuilder(128);gpu.Name=nvmlDeviceGetName(device,name,128)==0?name.ToString():"NVIDIA GPU";Util usage;uint value;GpuMemory mem;if(nvmlDeviceGetUtilizationRates(device,out usage)==0)gpu.Usage=usage.Gpu;if(nvmlDeviceGetTemperature(device,0,out value)==0)gpu.Temperature=value;if(nvmlDeviceGetPowerUsage(device,out value)==0)gpu.Watts=value/1000.0;if(nvmlDeviceGetClockInfo(device,0,out value)==0)gpu.Clock=value;if(nvmlDeviceGetClockInfo(device,2,out value)==0)gpu.MemoryClock=value;if(nvmlDeviceGetMemoryInfo(device,out mem)==0){gpu.Used=mem.Used;gpu.Total=mem.Total;}result.Gpus.Add(gpu);}}}catch(DllNotFoundException){}catch(EntryPointNotFoundException){}return result;}
 public void Dispose(){lock(gate)DisposeCore();}void DisposeCore(){if(nvmlReady){try{nvmlShutdown();}catch{}nvmlReady=false;}}
}
}