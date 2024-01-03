using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AvaloniaOpenBCI.Models;
using AvaloniaOpenBCI.Models.FileInterfaces;
using AvaloniaOpenBCI.Models.Progress;
using AvaloniaOpenBCI.Services;
using Serilog;

namespace AvaloniaOpenBCI.Helper;

[SupportedOSPlatform("macos")]
[SupportedOSPlatform("linux")]
public class UnixPrerequisiteHelper : IPrerequisiteHelper
{
    readonly private ISettingsManager settingsManager;


    public UnixPrerequisiteHelper(
        ISettingsManager settingsManager
    )
    {
        this.settingsManager = settingsManager;
    }

    private static ILogger Logger => Log.Logger;

    private DirectoryPath HomeDir => settingsManager.LibraryDir;
    private DirectoryPath AssetsDir => HomeDir.JoinDir("Assets");

    public async Task UnpackResourcesIfNecessary(IProgress<ProgressReport>? progress = null)
    {
        // Array of (asset_uri, extract_to)
        var assets = new[]
        {
            (Assets.SevenZipExecutable, AssetsDir), (Assets.SevenZipLicense, AssetsDir)
        };

        progress?.Report(
            new ProgressReport(0, message: "Unpacking resources", isIndeterminate: true)
        );

        Directory.CreateDirectory(AssetsDir);
        foreach ((AvaloniaResource asset, DirectoryPath extractDir) in assets)
        {
            await asset.ExtractToDir(extractDir);
        }

        progress?.Report(
            new ProgressReport(1, message: "Unpacking resources", isIndeterminate: false)
        );
    }

    [UnsupportedOSPlatform("Linux")]
    [UnsupportedOSPlatform("macOS")]
    public Task InstallVcRedistIfNecessary(IProgress<ProgressReport>? progress = null) =>
        throw new PlatformNotSupportedException();
}