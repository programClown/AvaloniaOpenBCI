using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using AvaloniaOpenBCI.Extensions;
using AvaloniaOpenBCI.Models;
using AvaloniaOpenBCI.Styles;
using System;
using System.IO;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace AvaloniaOpenBCI.Helper;

public static class TextEditorConfigs
{
    public static void Configure(TextEditor editor, TextEditorPreset preset)
    {
        switch (preset)
        {
            case TextEditorPreset.Prompt:
                break;

            case TextEditorPreset.Console:
                break;

            case TextEditorPreset.None:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
        }
    }

    private static void ConfigForPrompt(TextEditor editor)
    {
        const ThemeName themeName = ThemeName.DimmedMonokai;
        var registryOptions = new RegistryOptions(themeName);

        var registry = new Registry(registryOptions);

        using Stream stream = Assets.ImagePromptLanguageJson.Open();
        IGrammar promptGrammer = registry.LoadGrammarFromStream(stream);

        // Load theme
        IRawTheme theme = GetCustomTheme();

        //Setup editor
        TextEditorOptions? editorOptions = editor.Options;
        editorOptions.ShowColumnRulers = true;
        editorOptions.EnableTextDragDrop = true;
        editorOptions.ExtendSelectionOnMouseUp = true;
        //Config hyperlinks
        editorOptions.EnableHyperlinks = true;
        editorOptions.RequireControlModifierForHyperlinkClick = true;
        editor.TextArea.TextView.LinkTextForegroundBrush = Brushes.Coral;
        editor.TextArea.SelectionBrush = ThemeColors.EditorSelectionBrush;

        TextMate.Installation? installation = editor.InstallTextMate(registryOptions);

        // Set the _textMateRegistry property
        installation.SetPrivateField("_textMateRegistry", registry);

        installation.SetGrammar(promptGrammer.GetScopeName());

        installation.SetTheme(theme);
    }

    private static void ConfigForConsole(TextEditor editor)
    {
        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        // Config hyperlinks
        editor.TextArea.Options.EnableHyperlinks = true;
        editor.TextArea.Options.RequireControlModifierForHyperlinkClick = false;
        editor.TextArea.TextView.LinkTextForegroundBrush = Brushes.Coral;

        TextMate.Installation? textMate = editor.InstallTextMate(registryOptions);
        string? scope = registryOptions.GetScopeByLanguageId("log");

        if (scope is null)
        {
            throw new InvalidOperationException("Scope is null");
        }

        textMate.SetGrammar(scope);
        textMate.SetTheme(registryOptions.LoadTheme(ThemeName.DarkPlus));

        editor.Options.ShowBoxForControlCharacters = false;
    }

    private static IRawTheme GetThemeFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return ThemeReader.ReadThemeSync(reader);
    }

    private static IRawTheme GetCustomTheme()
    {
        using Stream stream = Assets.ThemeMatrixDarkJson.Open();
        return GetThemeFromStream(stream);
    }
}