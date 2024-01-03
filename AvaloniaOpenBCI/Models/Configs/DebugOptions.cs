﻿namespace AvaloniaOpenBCI.Models.Configs;

public class DebugOptions
{
    /// <summary>
    ///     Sets up LiteDB to use a temporary database file on each run
    /// </summary>
    public bool TempDatabase { get; set; }

    /// <summary>
    ///     Always show the one-click install page on launch
    /// </summary>
    public bool ShowOneClickInstall { get; set; }

    /// <summary>
    ///     Override the default update manifest url
    /// </summary>
    public string? UpdateManifestUrl { get; set; }
}