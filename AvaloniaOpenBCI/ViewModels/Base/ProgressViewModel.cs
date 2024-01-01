using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaOpenBCI.ViewModels.Base;

public partial class ProgressViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _description;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsProgressVisible))]
    private bool _isIndeterminate;

    [ObservableProperty]
    private double _maximum = 300;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsTextVisible))]
    private string? _text;


    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsProgressVisible))]
    private double _value;

    public virtual bool IsProgressVisible
    {
        get => Value > 0 || IsIndeterminate;
    }

    public virtual bool IsTextVisible
    {
        get => !string.IsNullOrWhiteSpace(Text);
    }

    public void ClearProgress()
    {
        Value = 0;
        Text = null;
        IsIndeterminate = false;
    }
}