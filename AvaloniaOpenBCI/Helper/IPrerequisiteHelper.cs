using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AvaloniaOpenBCI.Models.Progress;

namespace AvaloniaOpenBCI.Helper;

public interface IPrerequisiteHelper
{
    Task UnpackResourcesIfNecessary(IProgress<ProgressReport>? progress = null);

    [SupportedOSPlatform("Windows")]
    Task InstallVcRedistIfNecessary(IProgress<ProgressReport>? progress = null);
}