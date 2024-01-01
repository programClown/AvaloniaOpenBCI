using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaOpenBCI.ViewModels.Base;

public partial class ConsoleProgressViewModel : ProgressViewModel
{
    [ObservableProperty]
    private bool _closeWhenFinished;

    public ConsoleViewModel Console { get; } = new();
}