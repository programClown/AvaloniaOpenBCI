﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Serilog;
using ILogger = Serilog.ILogger;
using Process = System.Diagnostics.Process;

namespace AvaloniaOpenBCI.Helper;

/// <summary>
///     Allows processes to be automatically killed if this parent process unexpectedly quits.
///     This feature requires Windows 8 or greater. On Windows 7, nothing is done.
/// </summary>
/// <remarks>
///     References:
///     https://stackoverflow.com/a/4657392/386091
///     https://stackoverflow.com/a/9164742/386091
/// </remarks>
[SupportedOSPlatform("windows")]
public static class ProcessTracker
{
    // Windows will automatically close any open job handles when our process terminates.
    readonly private static IntPtr JobHandle;

    static ProcessTracker()
    {
        // This feature requires Windows 8 or later.
        if (Environment.OSVersion.Version < new Version(6, 2))
            return;

        // The job name is optional (and can be null) but it helps with diagnostics.
        //  If it's not null, it has to be unique. Use SysInternals' Handle command-line
        //  utility: handle -a ChildProcessTracker
        string jobName = "ProcessTracker" + Environment.ProcessId;
        JobHandle = CreateJobObject(IntPtr.Zero, jobName);

        var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION();

        // This is the key flag. When our process is killed, Windows will automatically
        //  close the job handle, and when that happens, we want the child processes to
        //  be killed, too.
        info.LimitFlags = JOBOBJECTLIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

        var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            BasicLimitInformation = info
        };

        int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
        IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
        try
        {
            Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

            if (!SetInformationJobObject(JobHandle,
                    JobObjectInfoType.ExtendedLimitInformation,
                    extendedInfoPtr,
                    (uint)length
                ))
            {
                throw new Win32Exception();
            }
        }
        finally
        {
            Marshal.FreeHGlobal(extendedInfoPtr);
        }
    }

    private static ILogger Logger => Log.Logger;

    /// <summary>
    ///     Add the process to be tracked. If our current process is killed, the child processes
    ///     that we are tracking will be automatically killed, too. If the child process terminates
    ///     first, that's fine, too.
    /// </summary>
    /// <param name="process"></param>
    public static void AddProcess(Process process)
    {
        if (JobHandle == IntPtr.Zero) return;
        Logger.Debug("Adding process '{Name}' [{Id}] to job object", process.ProcessName, process.Id);
        bool success = AssignProcessToJobObject(JobHandle, process.Handle);
        if (!success && !process.HasExited)
        {
            throw new Win32Exception();
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string name);

    [DllImport("kernel32.dll")]
    private static extern bool SetInformationJobObject(IntPtr job,
        JobObjectInfoType infoType,
        IntPtr lpJobObjectInfo,
        uint cbJobObjectInfoLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);
}

public enum JobObjectInfoType
{
    AssociateCompletionPortInformation = 7,
    BasicLimitInformation = 2,
    BasicUIRestrictions = 4,
    EndOfJobTimeInformation = 6,
    ExtendedLimitInformation = 9,
    SecurityLimitInformation = 5,
    GroupInformation = 11
}

[StructLayout(LayoutKind.Sequential)]
public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
{
    public Int64 PerProcessUserTimeLimit;
    public Int64 PerJobUserTimeLimit;
    public JOBOBJECTLIMIT LimitFlags;
    public UIntPtr MinimumWorkingSetSize;
    public UIntPtr MaximumWorkingSetSize;
    public UInt32 ActiveProcessLimit;
    public Int64 Affinity;
    public UInt32 PriorityClass;
    public UInt32 SchedulingClass;
}

[Flags]
public enum JOBOBJECTLIMIT : uint
{
    JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000
}

[StructLayout(LayoutKind.Sequential)]
public struct IO_COUNTERS
{
    public UInt64 ReadOperationCount;
    public UInt64 WriteOperationCount;
    public UInt64 OtherOperationCount;
    public UInt64 ReadTransferCount;
    public UInt64 WriteTransferCount;
    public UInt64 OtherTransferCount;
}

[StructLayout(LayoutKind.Sequential)]
public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
{
    public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
    public IO_COUNTERS IoInfo;
    public UIntPtr ProcessMemoryLimit;
    public UIntPtr JobMemoryLimit;
    public UIntPtr PeakProcessMemoryUsed;
    public UIntPtr PeakJobMemoryUsed;
}