using System;
using System.IO;
using System.Runtime.InteropServices;
using Serilog;

namespace AvaloniaOpenBCI.Helper;

public static class SystemInfo
{
    public const long Gibibyte = 1024 * 1024 * 1024;
    public const long Mebibyte = 1024 * 1024;

    [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
    public static extern bool ShouldUseDarkMode();

    public static long? GetDiskFreeSpaceBytes(string path)
    {
        try
        {
            var drive = new DriveInfo(path);
            return drive.AvailableFreeSpace;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "GetDiskFreeSpaceBytes");
        }

        return null;
    }
}