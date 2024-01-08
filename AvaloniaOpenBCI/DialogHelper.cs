using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using AvaloniaOpenBCI.Controls;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.Models;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Markdown.Avalonia;
using Refit;
using Serilog;
using Serilog.Events;
using TextMateSharp.Grammars;

namespace AvaloniaOpenBCI;

public static class DialogHelper
{
    private static ILogger Logger
    {
        get => Log.Logger;
    }

    public static BetterContentDialog CreateTextEntryDialog(
        string title,
        string description,
        IReadOnlyList<TextBoxField> textFields
    )
    {
        return CreateTextEntryDialog(title, new MarkdownScrollViewer { Markdown = description }, textFields);
    }

    /// <summary>
    ///     Create a generic textbox entry content dialog
    /// </summary>
    public static BetterContentDialog CreateTextEntryDialog(
        string title,
        string description,
        string imageSource,
        IReadOnlyList<TextBoxField> textFields
    )
    {
        var markdown = new MarkdownScrollViewer { Markdown = description };
        var image = new BetterAdvancedImage((Uri?)null)
        {
            Source = imageSource,
            Stretch = Stretch.UniformToFill,
            StretchDirection = StretchDirection.Both,
            HorizontalAlignment = HorizontalAlignment.Center,
            MaxWidth = 400
        };

        Grid.SetRow(markdown, 0);
        Grid.SetRow(image, 1);

        var grid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
            Children = { markdown, image }
        };

        return CreateTextEntryDialog(title, description, textFields);
    }

    public static BetterContentDialog CreateTextEntryDialog(
        string title,
        Control content,
        IReadOnlyList<TextBoxField> textFields
    )
    {
        Dispatcher.UIThread.VerifyAccess();

        var stackPanel = new StackPanel { Spacing = 4 };

        Grid.SetRow(content, 0);
        Grid.SetRow(stackPanel, 1);

        var grid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) },
            Children = { content, stackPanel }
        };

        grid.Loaded += (_, _) =>
        {
            // Focus first TextBox
            var firstTextBox = stackPanel
                .Children.OfType<StackPanel>()
                .FirstOrDefault()
                .FindDescendantOfType<TextBox>();

            firstTextBox!.Focus();
            firstTextBox.CaretIndex = firstTextBox.Text?.LastIndexOf('.') ?? 0; //光标位置
        };

        // Disable primary button if any textboxes are invalid
        var primaryCommand = new RelayCommand(
            delegate { },
            () =>
            {
                int invalidCount = textFields.Count(field => !field.IsValid);
                Logger.Debug($"Checking can execute: {invalidCount} invalid fields");
                return invalidCount == 0;
            }
        );

        // Create textboxes
        foreach (TextBoxField field in textFields)
        {
            var label = new TextBlock { Text = field.Label, Margin = new Thickness(0, 0, 0, 4) };

            var textBox = new TextBox
            {
                [!TextBox.TextProperty] = new Binding("TextProperty"),
                Watermark = field.Watermark,
                DataContext = field
            };

            if (!string.IsNullOrEmpty(field.InnerLeftText))
            {
                textBox.InnerLeftContent = new TextBlock
                {
                    Text = field.InnerLeftText,
                    Foreground = Brushes.Gray,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0, -4, 0)
                };
            }

            stackPanel.Children.Add(new StackPanel { Spacing = 4, Children = { label, textBox } });

            // When IsValid property changes, update invalid count and primary button
            field.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(TextBoxField.IsValid))
                {
                    primaryCommand.NotifyCanExecuteChanged();
                }
            };

            // Set initial value
            textBox.Text = field.Text;

            // See if initial value is valid
            try
            {
                field.Validator?.Invoke(field.Text);
            }
            catch (Exception)
            {
                field.IsValid = false;
            }
        }

        return new BetterContentDialog
        {
            Title = title,
            Content = grid,
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            IsPrimaryButtonEnabled = true,
            PrimaryButtonCommand = primaryCommand,
            DefaultButton = ContentDialogButton.Primary
        };
    }

    /// <summary>
    ///     Create a generic dialog for showing a markdown document
    /// </summary>
    public static BetterContentDialog CreateMarkdownDialog(
        string markdown,
        string? title = null,
        TextEditorPreset editorPreset = default
    )
    {
        Dispatcher.UIThread.VerifyAccess();

        var viewer = new MarkdownScrollViewer { Markdown = markdown };

        // Apply syntax highlighting to code blocks if preset is provided
        if (editorPreset != default)
        {
            using IDisposable _ = CodeTimer.StartDebug();

            int appliedCount = 0;

            if (viewer.GetLogicalDescendants().FirstOrDefault()?.GetLogicalDescendants() is { } stackDescendants)
            {
                foreach (TextEditor editor in stackDescendants.OfType<TextEditor>())
                {
                    TextEditorConfigs.Configure(editor, editorPreset);

                    editor.FontFamily = "Cascadia Code,Consolas,Menlo,Monospace";
                    editor.Margin = new Thickness(0);
                    editor.Padding = new Thickness(4);
                    editor.IsEnabled = false;

                    if (editor.GetLogicalParent() is Border border)
                    {
                        border.BorderThickness = new Thickness(0);
                        border.CornerRadius = new CornerRadius(4);
                    }

                    appliedCount++;
                }
            }

            Logger.Write(
                appliedCount > 0 ? LogEventLevel.Debug : LogEventLevel.Warning,
                $"Applied syntax highlighting to {appliedCount} code blocks"
            );
        }

        return new BetterContentDialog
        {
            Title = title,
            Content = viewer,
            CloseButtonText = "取消",
            IsPrimaryButtonEnabled = false
        };
    }

    /// <summary>
    ///     Create a dialog fot displaying an ApiException
    /// </summary>
    public static BetterContentDialog CreateApiExceptionDialog(ApiException exception, string? title = null)
    {
        Dispatcher.UIThread.VerifyAccess();

        // Setup text editor
        var textEditor = new TextEditor
        {
            IsReadOnly = true,
            WordWrap = true,
            Options = { ShowColumnRulers = false, AllowScrollBelowDocument = false }
        };
        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        textEditor.InstallTextMate(registryOptions).SetGrammar(registryOptions.GetScopeByLanguageId("json"));

        var mainGrid = new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(16),
            Children =
            {
                new TextBlock
                {
                    Text = $"{(int)exception.StatusCode} - {exception.ReasonPhrase}",
                    FontSize = 18,
                    FontWeight = FontWeight.Medium,
                    Margin = new Thickness(0, 8)
                },
                textEditor
            }
        };

        var dialog = new BetterContentDialog
        {
            Title = title,
            Content = mainGrid,
            CloseButtonText = "取消",
            IsPrimaryButtonEnabled = false
        };

        // try to deserialize to json element
        if (exception.Content != null)
        {
            try
            {
                // Deserialize to json element then re-serialize to ensure indentation
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(
                    exception.Content,
                    new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        ReadCommentHandling = JsonCommentHandling.Skip
                    }
                );
                string formatted = JsonSerializer.Serialize(
                    jsonElement,
                    new JsonSerializerOptions { WriteIndented = true }
                );

                textEditor.Document.Text = formatted;
            }
            catch (JsonException)
            {
                // Otherwise just add the content as a code block
                textEditor.Document.Text = exception.Content;
            }
        }

        return dialog;
    }

    /// <summary>
    ///     Create a dialog for displaying json
    /// </summary>
    public static BetterContentDialog CreateJsonDialog(string json, string? title = null, string? subTitle = null)
    {
        Dispatcher.UIThread.VerifyAccess();

        // Setup text editor
        var textEditor = new TextEditor
        {
            IsReadOnly = true,
            WordWrap = true,
            Options = { ShowColumnRulers = false, AllowScrollBelowDocument = false }
        };
        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        textEditor.InstallTextMate(registryOptions).SetGrammar(registryOptions.GetScopeByLanguageId("json"));

        var mainGrid = new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(16),
            Children = { textEditor }
        };

        if (subTitle is not null)
        {
            mainGrid.Children.Insert(
                0,
                new TextBlock
                {
                    Text = subTitle,
                    FontSize = 18,
                    FontWeight = FontWeight.Medium,
                    Margin = new Thickness(0, 8)
                }
            );
        }

        var dialog = new BetterContentDialog
        {
            Title = title,
            Content = mainGrid,
            CloseButtonText = "关闭",
            PrimaryButtonText = "复制",
            IsPrimaryButtonEnabled = false
        };

        // Try to deserialize to json element
        try
        {
            // Deserialize to json element then re-serialize to ensure indentation
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(
                json,
                new JsonSerializerOptions { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip }
            );
            string formatted = JsonSerializer.Serialize(
                jsonElement,
                new JsonSerializerOptions { WriteIndented = true }
            );

            textEditor.Document.Text = formatted;
        }
        catch (JsonException)
        {
            // Other just add the content as a code block
            textEditor.Document.Text = json;
        }

        dialog.PrimaryButtonCommand = new AsyncRelayCommand(async () =>
        {
            // Copy the json to clipboard
            IClipboard clipboard = App.Clipboard;
            await clipboard.SetTextAsync(textEditor.Document.Text);
        });

        return dialog;
    }

    public static TaskDialog CreateTaskDialog(string title, string description)
    {
        Dispatcher.UIThread.VerifyAccess();

        var content = new StackPanel
        {
            Children =
            {
                new TextBlock
                {
                    Margin = new Thickness(0, 2, 0, 8),
                    FontSize = 20,
                    FontWeight = FontWeight.DemiBold,
                    Text = title,
                    TextWrapping = TextWrapping.WrapWithOverflow
                },
                new TextBlock { Text = description, TextWrapping = TextWrapping.WrapWithOverflow }
            }
        };

        return new TaskDialog
        {
            Title = title,
            Content = content,
            XamlRoot = App.VisualRoot
        };
    }
}

/// Text fields
public sealed class TextBoxField : INotifyPropertyChanged
{
    private bool _isValid;

    // Label above the textbox
    public string Label { get; init; } = string.Empty;

    // Actual text value
    public string Text { get; set; } = string.Empty;

    // Watermark text
    public string Watermark { get; init; } = string.Empty;

    // Inner left value
    public string? InnerLeftText { get; init; }

    /// <summary>
    ///     Validation action on text changes. Throw exception if invalid.
    /// </summary>
    public Action<string>? Validator { get; init; }

    public string TextProperty
    {
        get => Text;
        [DebuggerStepThrough]
        set
        {
            try
            {
                Validator?.Invoke(value);
            }
            catch (Exception e)
            {
                IsValid = false;
                throw new DataValidationException(e.Message);
            }
            Text = value;
            IsValid = true;
            OnPropertyChanged();
        }
    }

    public bool IsValid
    {
        get => Validator == null || _isValid;
        set
        {
            _isValid = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
