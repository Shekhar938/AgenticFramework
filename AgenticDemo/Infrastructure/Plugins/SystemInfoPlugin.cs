using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.SemanticKernel;

namespace AgenticDemo.Infrastructure.Plugins;

public sealed class SystemInfoPlugin
{
    [KernelFunction("get_system_health")]
    [Description("Gets current PC health info like CPU, RAM, and Disk space")]
    public string GetSystemHealth()
    {
        var drive = DriveInfo.GetDrives().First(d => d.IsReady);
        var freeSpaceGb = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
        var totalSpaceGb = drive.TotalSize / (1024 * 1024 * 1024);

        return $"OS: {RuntimeInformation.OSDescription}\n" +
               $"Architecture: {RuntimeInformation.ProcessArchitecture}\n" +
               $"Free Disk Space: {freeSpaceGb}GB / {totalSpaceGb}GB\n" +
               $"Process Memory: {Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024)}MB";
    }
}
