using Avalonia.Platform;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.Models.FileInterfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace AvaloniaOpenBCI.Models;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public readonly record struct AvaloniaResource(
    Uri UriPath,
    UnixFileMode WriteUnixFileMode = UnixFileMode.None)
{
    public AvaloniaResource(string uriPath, UnixFileMode writeUnixFileMode = UnixFileMode.None)
        : this(new Uri(uriPath), writeUnixFileMode)
    {
    }

    /// <summary>
    ///     File name component of the Uri path.
    /// </summary>
    public string FileName
    {
        get => Path.GetFileName(UriPath.ToString());
    }

    /// <summary>
    ///     File path relative to the 'Assets' folder.
    /// </summary>
    public Uri RelativeAssetPath
    {
        get => new Uri("avares://AvaloniaOpenBCI/Assets/").MakeRelativeUri(UriPath);
    }

    /// <summary>
    ///     Open a stream to this resource
    /// </summary>
    /// <returns></returns>
    public Stream Open()
    {
        return AssetLoader.Open(UriPath);
    }

    /// <summary>
    ///     Extract this resource to a target file path
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="overwrite"></param>
    public async Task ExtractTo(FilePath outputPath, bool overwrite = true)
    {
        if (outputPath.Exists)
        {
            // Skip if not overwritting
            if (!overwrite)
            {
                return;
            }
            // Otherwise delete the file
            await outputPath.DeleteAsync();
        }

        Stream stream = AssetLoader.Open(UriPath);
        await using FileStream fileStream = File.Create(outputPath);
        await stream.CopyToAsync(fileStream);
        //Write permissions
        if (!Compat.IsWindows && Compat.IsUnix && WriteUnixFileMode != UnixFileMode.None)
        {
            File.SetUnixFileMode(outputPath, WriteUnixFileMode);
        }
    }

    /// <summary>
    ///     Extract this resource to the output directory
    /// </summary>
    /// <param name="outputDir"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    public Task ExtractToDir(DirectoryPath outputDir, bool overwrite = true)
    {
        return ExtractTo(outputDir.JoinFile(FileName), overwrite);
    }
}