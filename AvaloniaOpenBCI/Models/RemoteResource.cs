using System;
using System.IO;

namespace AvaloniaOpenBCI.Models;

/// <summary>
///     Defines a remote downloadable resource.
/// </summary>
public readonly record struct RemoteResource
{
    public required Uri Url { get; init; }

    public Uri[]? FallbackUrls { get; init; }

    public string? FileNameOverride { get; init; }

    public string FileName
    {
        get => FileNameOverride ?? Path.GetFileName(Url.ToString());
    }

    public string? HashSha256 { get; init; }

    /// <summary>
    ///     Type info, for remote models this is <see cref="SharedFolderType" /> of the model.
    /// </summary>
    public object? ContextType { get; init; }

    public Uri? InfoUrl { get; init; }

    public string? LicenseType { get; init; }

    public Uri? LicenseUrl { get; init; }

    public string? Author { get; init; }

    public string? ByAuthor
    {
        get => string.IsNullOrEmpty(Author) ? null : $"by {Author}";
    }
}