using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AvaloniaOpenBCI.Models;
using AvaloniaOpenBCI.Models.Progress;
using AvaloniaOpenBCI.Services;
using Microsoft.Win32;
using Serilog;

namespace AvaloniaOpenBCI.Helper;

[SupportedOSPlatform("windows")]
public class WindowsPrerequisiteHelper : IPrerequisiteHelper
{
    private const string VcRedistDownloadUrl = "https://aka.ms/vs/16/release/vc_redist.x64.exe";
    private readonly ISettingsManager _settingsManager;

    private static ILogger Logger => Log.Logger;
    private string HomeDir => _settingsManager.LibraryDir;

    private string VcRedistDownloadPath => Path.Combine(HomeDir, "vcredist.x64.exe");

    private string AssetsDir => Path.Combine(HomeDir, "Assets");
    private string SevenZipPath => Path.Combine(AssetsDir, "7za.exe");

    private string PythonDir => Path.Combine(AssetsDir, "Python310");

    public async Task UnpackResourcesIfNecessary(IProgress<ProgressReport>? progress = null)
    {
        // Array of (asset_uri, extract_to)
        var assets = new[] { (Assets.SevenZipExecutable, AssetsDir), (Assets.SevenZipLicense, AssetsDir) };

        progress?.Report(new ProgressReport(0, message: "Unpacking resources", isIndeterminate: true));

        Directory.CreateDirectory(AssetsDir);
        foreach ((AvaloniaResource asset, string extractDir) in assets)
        {
            await asset.ExtractToDir(extractDir);
        }

        progress?.Report(new ProgressReport(1, message: "Unpacking resources", isIndeterminate: false));
    }

    [SupportedOSPlatform("windows")]
    public async Task InstallVcRedistIfNecessary(IProgress<ProgressReport>? progress = null)
    {
        RegistryKey registry = Registry.LocalMachine;
        RegistryKey? key = registry.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\X64", false);

        if (key != null)
        {
            uint buildId = Convert.ToUInt32(key.GetValue("Bld"));
            if (buildId >= 30139) { }
        }

        // Logger.Information("Installing VC Redist");
        // progress?.Report(
        //     new ProgressReport(
        //         0.5f,
        //         isIndeterminate: true,
        //         type: ProgressType.Generic,
        //         message: "Installing prerequisites..."
        //     )
        // );
        //
        // var process = ProcessRunner.StartAnsiProcess(
        //     VcRedistDownloadPath,
        //     "/install /quiet /norestart"
        // );
        //
        // await process.WaitForExitAsync();
        // progress?.Report(
        //     new ProgressReport(
        //         1f,
        //         message: "Visual C++ install complete",
        //         type: ProgressType.Generic
        //     )
        // );
        //
        // File.Delete(VcRedistDownloadPath);
    }
}
