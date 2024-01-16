using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.Models.FileInterfaces;

namespace AvaloniaOpenBCI.Models;

/// <summary>
/// Represents a locally indexed image file.
/// </summary>
public record LocalImageFile
{
    public required string AbsolutePath { get; init; }

    /// <summary>
    /// Creation time of the file.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Last modified time of the file.
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; init; }

    /// <summary>
    /// Dimensions of the image
    /// </summary>
    public System.Drawing.Size? ImageSize { get; init; }

    /// <summary>
    /// File name of the relative path.
    /// </summary>
    public string FileName => Path.GetFileName(AbsolutePath);

    /// <summary>
    /// File name of the relative path without extension.
    /// </summary>
    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(AbsolutePath);

    public (string? Parameters, string? ParametersJson, string? SMProject, string? ComfyNodes) ReadMetadata()
    {
        using var stream = new FileStream(AbsolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream);

        var parameters = ImageMetadata.ReadTextChunk(reader, "parameters");
        var parametersJson = ImageMetadata.ReadTextChunk(reader, "parameters-json");
        var smProject = ImageMetadata.ReadTextChunk(reader, "smproj");
        var comfyNodes = ImageMetadata.ReadTextChunk(reader, "prompt");

        return (
            string.IsNullOrEmpty(parameters) ? null : parameters,
            string.IsNullOrEmpty(parametersJson) ? null : parametersJson,
            string.IsNullOrEmpty(smProject) ? null : smProject,
            string.IsNullOrEmpty(comfyNodes) ? null : comfyNodes
        );
    }

    public static LocalImageFile FromPath(FilePath filePath)
    {
        // Get metadata
        using var stream = filePath.Info.OpenRead();
        using var reader = new BinaryReader(stream);

        var imageSize = ImageMetadata.GetImageSize(reader);

        var metadata = ImageMetadata.ReadTextChunk(reader, "parameters-json");

        filePath.Info.Refresh();

        return new LocalImageFile
        {
            AbsolutePath = filePath,
            CreatedAt = filePath.Info.CreationTimeUtc,
            LastModifiedAt = filePath.Info.LastWriteTimeUtc,
            ImageSize = imageSize
        };
    }

    public static readonly HashSet<string> SupportedImageExtensions = new() { ".png", ".jpg", ".jpeg", ".webp" };
}
