using System.IO;

namespace AvaloniaOpenBCI.Models.FileInterfaces;

public partial class FilePath
{
    public FilePath WithName(string fileName)
    {
        if (Path.GetDirectoryName(FullPath) is { } directory
            && !string.IsNullOrWhiteSpace(directory))
        {
            return new FilePath(directory, fileName);
        }

        return new FilePath(fileName);
    }
}