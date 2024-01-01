using Avalonia.Platform;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;

namespace AvaloniaOpenBCI;

public static class Assets
{
    private const UnixFileMode Unix755 =
        UnixFileMode.UserRead |
        UnixFileMode.UserWrite |
        UnixFileMode.UserExecute |
        UnixFileMode.GroupRead |
        UnixFileMode.GroupWrite |
        UnixFileMode.OtherExecute |
        UnixFileMode.OtherRead |
        UnixFileMode.OtherExecute;

    public static AvaloniaResource AppIcon { get; } = new("avares://AvaloniaOpenBCI/Assets/app-icon.ico");

    public static Uri NoImage { get; } = new("avares://AvaloniaOpenBCI/Assets/noimage.png");

    public static AvaloniaResource ThemeMatrixDarkJson
    {
        get => new("avares://AvaloniaOpenBCI/Assets/ThemeMatrixDark.json");
    }

    public static AvaloniaResource ImagePromptLanguageJson
    {
        get => new("avares://AvaloniaOpenBCI/Assets/ImagePrompt.tmLanguage.json");
    }

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public static AvaloniaResource SevenZipExecutable
    {
        get => Compat.Switch(
            (PlatformKind.Windows, new AvaloniaResource("avares://AvaloniaOpenBCI/Assets/win-x64/7za.exe")),
            (
                PlatformKind.Linux | PlatformKind.X64,
                new AvaloniaResource("avares://AvaloniaOpenBCI/Assets/linux-x64/7zzs", Unix755)
            ),
            (
                PlatformKind.MacOS | PlatformKind.Arm,
                new AvaloniaResource("avares://AvaloniaOpenBCI/Assets/macos-arm64/7zz", Unix755)
            )
        );
    }

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public static AvaloniaResource SevenZipLicense
    {
        get => Compat.Switch(
            (PlatformKind.Windows, new AvaloniaResource("avares://AvaloniaOpenBCI/Assets/win-x64/7za - LICENSE.txt")),
            (
                PlatformKind.Linux | PlatformKind.X64,
                new AvaloniaResource("avares://AvaloniaOpenBCI/Assets/linux-x64/7zzs - LICENSE.txt")
            ),
            (
                PlatformKind.MacOS | PlatformKind.Arm,
                new AvaloniaResource("avares://AvaloniaOpenBCI/Assets/macos-arm64/7zz - LICENSE.txt")
            )
        );
    }

    [SupportedOSPlatform("windows")]
    public static IEnumerable<(AvaloniaResource resource, string relativePath)> PyModuleVenv
    {
        get => FindAssets("win-x64/venv/");
    }

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public static RemoteResource PythonDownloadUrl
    {
        get => Compat.Switch(
            (
                PlatformKind.Windows | PlatformKind.X64,
                new RemoteResource
                {
                    Url = new Uri("https://www.python.org/ftp/python/3.10.11/python-3.10.11-embed-amd64.zip"),
                    HashSha256 = "608619f8619075629c9c69f361352a0da6ed7e62f83a0e19c63e0ea32eb7629d"
                }
            ),
            (
                PlatformKind.Linux | PlatformKind.X64,
                new RemoteResource
                {
                    Url = new Uri(
                        "https://github.com/indygreg/python-build-standalone/releases/download/20230507/cpython-3.10.11+20230507-x86_64-unknown-linux-gnu-install_only.tar.gz"
                    ),
                    HashSha256 = "c5bcaac91bc80bfc29cf510669ecad12d506035ecb3ad85ef213416d54aecd79"
                }
            ),
            (
                PlatformKind.MacOS | PlatformKind.Arm,
                new RemoteResource
                {
                    Url = new Uri(
                        "https://github.com/indygreg/python-build-standalone/releases/download/20230507/cpython-3.10.11+20230507-aarch64-apple-darwin-install_only.tar.gz"
                    ),
                    HashSha256 = "8348bc3c2311f94ec63751fb71bd0108174be1c4def002773cf519ee1506f96f"
                }
            )
        );
    }

    /// <summary>
    ///     Yield AvaloniaResources given a relative directory path within the 'Assets' folder.
    /// </summary>
    public static IEnumerable<(AvaloniaResource resource, string relativePath)> FindAssets(string relativeAssetPath)
    {
        var baseUrl = new Uri("avares://AvaloniaOpenBCI/Assets/");
        var targetUri = new Uri(baseUrl, relativeAssetPath);
        var files = AssetLoader.GetAssets(targetUri, null);
        foreach (Uri file in files)
        {
            yield return (new AvaloniaResource(file), targetUri.MakeRelativeUri(file).ToString());
        }
    }
}