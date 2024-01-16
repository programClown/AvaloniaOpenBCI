using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.Controls;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.Models;
using AvaloniaOpenBCI.Models.FileInterfaces;
using AvaloniaOpenBCI.ViewModels.Base;
using AvaloniaOpenBCI.Views;
using AvaloniaOpenBCI.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaOpenBCI.ViewModels.Dialogs;

[View(typeof(ImageViewerDialog))]
public partial class ImageViewerViewModel : ContentDialogViewModelBase
{
    [ObservableProperty]
    private Bitmap? _bitmap;

    [ObservableProperty]
    private LocalImageFile? _localImageFile;

    [ObservableProperty]
    private bool _isFooterEnabled;

    [ObservableProperty]
    private string? _fileNameText;

    [ObservableProperty]
    private string? _fileSizeText;

    [ObservableProperty]
    private string? _imageSizeText;

    public event EventHandler<DirectionalNavigationEventArgs>? NavigationRequested;

    [RelayCommand]
    private void NavigateNext()
    {
        NavigationRequested?.Invoke(this, DirectionalNavigationEventArgs.Down);
    }

    [RelayCommand]
    private void NavigatePrevious()
    {
        NavigationRequested?.Invoke(this, DirectionalNavigationEventArgs.Up);
    }

    partial void OnLocalImageFileChanged(LocalImageFile? value)
    {
        if (value?.ImageSize is { IsEmpty: false } size)
        {
            ImageSizeText = $"{size.Width} x {size.Height}";
        }
    }

    [RelayCommand]
    private async Task CopyImage(Bitmap? image)
    {
        if (image is null || !Compat.IsWindows)
            return;

        await Task.Run(() =>
        {
            if (Compat.IsWindows)
            {
                WindowsClipboard.SetBitmap(image);
            }
        });
    }

    /// <summary>
    /// Get the bitmap
    /// </summary>
    public async Task<Bitmap?> GetBitmapAsync()
    {
        if (Bitmap is not null)
            return Bitmap;

        var loader = ImageLoader.AsyncImageLoader;

        // Use local file path if available, otherwise remote URL
        var path = LocalImageFile?.AbsolutePath;
        if (path is null)
            return null;

        // Load the image
        Bitmap = await loader.ProvideImageAsync(path).ConfigureAwait(false);
        return Bitmap;
    }

    public BetterContentDialog GetDialog()
    {
        var margins = new Thickness(64, 32);

        var mainWindowSize = App.Services.GetRequiredService<MainWindow>()?.ClientSize;
        var dialogSize = new global::Avalonia.Size(
            Math.Floor((mainWindowSize?.Width * 0.6 ?? 1000) - margins.Horizontal()),
            Math.Floor((mainWindowSize?.Height ?? 1000) - margins.Vertical())
        );

        var dialog = new BetterContentDialog
        {
            MaxDialogWidth = dialogSize.Width,
            MaxDialogHeight = dialogSize.Height,
            ContentMargin = margins,
            FullSizeDesired = true,
            IsFooterVisible = false,
            CloseOnClickOutside = true,
            ContentVerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = new ImageViewerDialog
            {
                Width = dialogSize.Width,
                Height = dialogSize.Height,
                DataContext = this
            }
        };

        return dialog;
    }
}
