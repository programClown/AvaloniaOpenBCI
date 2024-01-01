using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;

namespace AvaloniaOpenBCI.Extensions;

public static class TextMateExtensions
{
    public static IGrammar LoadGrammarFromStream(
        this Registry registry,
        Stream stream,
        int? initialLanguage = default,
        Dictionary<string, int>? embeddedLanguages = default)
    {
        IRawGrammar rawGrammar;
        using (var sr = new StreamReader(stream))
        {
            rawGrammar = GrammarReader.ReadGrammarSync(sr);
        }

        FieldInfo? locatorField = typeof(Registry).GetField("locator", BindingFlags.Instance | BindingFlags.NonPublic);
        var locator = (IRegistryOptions)locatorField!.GetValue(registry)!;

        var injections = locator.GetInjections(rawGrammar.GetScopeName());

        FieldInfo? syncRegistryField =
            typeof(Registry).GetField("syncRegistry", BindingFlags.Instance | BindingFlags.NonPublic);
        var syncRegistry = (SyncRegistry)syncRegistryField!.GetValue(registry)!;

        syncRegistry.AddGrammar(rawGrammar, injections);
        return registry.GrammarForScopeName(
            rawGrammar.GetScopeName(),
            initialLanguage ?? 0,
            embeddedLanguages ?? new Dictionary<string, int>());
    }
}