using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Collections;
using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.Models;
using AvaloniaOpenBCI.ViewModels.Base;
using AvaloniaOpenBCI.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaOpenBCI.ViewModels.Dialogs;

[View(typeof(EnvVarsDialog))]
public partial class EnvVarsViewModel : ContentDialogViewModelBase
{
    [ObservableProperty]
    private string _title = "环境变量";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(EnvVarsView))]
    private ObservableCollection<EnvVarKeyPair> _envVars = new();

    public DataGridCollectionView EnvVarsView => new(EnvVars);

    [RelayCommand]
    private void AddRow()
    {
        EnvVars.Add(new EnvVarKeyPair());
    }

    [RelayCommand]
    private void RemoveSelectedRow(int selectedIndex)
    {
        try
        {
            EnvVars.RemoveAt(selectedIndex);
        }
        catch (ArgumentOutOfRangeException)
        {
            Debug.WriteLine($"RemoveSelectedRow: Index {selectedIndex} out of range");
        }
    }
}
