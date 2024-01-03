namespace AvaloniaOpenBCI.Services;

public interface ISettingsManager
{
    bool IsPortableMode { get; }
    string? LibraryDirOverride { set; }
    string LibraryDir { get; }
    bool IsLibraryDirSet { get; }
    string DatabasePath { get; }
    string ModelsDirectory { get; }
    string DownloadsDirectory { get; }

    bool FirstLaunchSetupComplete { get; set; }

    int? Version { get; set; }
    string? Theme { get; set; }
    string? Language { get; set; }
}