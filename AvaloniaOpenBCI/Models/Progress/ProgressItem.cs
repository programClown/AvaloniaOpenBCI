using System;

namespace AvaloniaOpenBCI.Models.Progress;

public record ProgressItem(
    Guid ProgressId,
    string Name,
    ProgressReport Progress,
    bool Failed = false
);