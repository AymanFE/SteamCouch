using System;
using System.Linq;
using System.Collections.Generic;
using Windows.Gaming.Input;
using Windows.System.Power;
namespace TVLounge {
// Windows battery reports are independent of XInput's wired/coarse classification.
internal static class WindowsControllerBattery {
 internal static int Percentage(int? remaining,int? full){if(!remaining.HasValue||!full.HasValue||full.Value<=0||remaining.Value<0||remaining.Value>full.Value)return -1;return (int)Math.Round(100.0*remaining.Value/full.Value);}
 internal static bool Unambiguous(int sourceCount,int targetCount){return sourceCount==1&&targetCount==1;}
 internal static bool Apply(BatteryReading target,BatteryStatus status,int? remaining,int? full){int percent=Percentage(remaining,full);if(status==BatteryStatus.NotPresent||percent<0)return false;target.State=status==BatteryStatus.Charging?3:status==BatteryStatus.Idle&&percent==100?4:1;target.Percent=percent;target.Category=-1;target.Source="Windows controller battery report";return true;}
 internal static void Merge(List<BatteryReading> readings){try{var devices=RawGameController.RawGameControllers.ToArray();foreach(var group in devices.GroupBy(c=>((uint)c.HardwareVendorId<<16)|c.HardwareProductId)){var targets=readings.Where(r=>(((uint)r.Vendor<<16)|r.Product)==group.Key).ToArray();if(!Unambiguous(group.Count(),targets.Length))continue;try{var report=group.Single().TryGetBatteryReport();if(report!=null)Apply(targets[0],report.Status,report.RemainingCapacityInMilliwattHours,report.FullChargeCapacityInMilliwattHours);}catch(Exception){/* Keep the standard reading if this driver's battery query fails. */}}}catch(Exception){/* Missing Windows provider must not disable SDL/XInput monitoring. */}}
}
}
