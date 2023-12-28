using System;
using System.IO;

namespace AvaloniaOpenBCI.Models.FileInterfaces;

public class FileSystemPath : IEquatable<FileSystemPath>, IEquatable<string>, IFormattable
{
    public FileSystemPath(string path)
    {
        FullPath = path;
    }

    public FileSystemPath(FileSystemPath path) : this(path.FullPath)
    {
    }

    public FileSystemPath(params string[] paths) : this(Path.Combine(paths))
    {
    }

    public string FullPath { get; }

    public bool Equals(FileSystemPath? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return FullPath == other.FullPath;
    }

    public bool Equals(string? other) => string.Equals(FullPath, other);

    /// <inheritdoc />
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(format, formatProvider);

    /// <summary>
    ///     Overridable IFormattable.ToString method.
    ///     By default, returns <see cref="FullPath" />.
    /// </summary>
    public virtual string ToString(string? format, IFormatProvider? formatProvider) => FullPath;

    public override string ToString() => FullPath;

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            FileSystemPath path => Equals(path),
            string path => Equals(path),
            _ => false
        };
    }

    public override int GetHashCode() => FullPath.GetHashCode();

    // Implicit conversions to and from string
    public static implicit operator string(FileSystemPath path) => path.FullPath;

    public static implicit operator FileSystemPath(string path) => new(path);
}