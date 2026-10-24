# Localization usage and boundaries

[한국어](Design.md) · [Installation](../README.en.md)

Unity Localization owns Locales, String Tables, Entries, LocalizedString, and loading. This package
adds an attributed Inspector and lookup conveniences, not a translation service or separate data store.

| API | Meaning | Failure behavior |
| --- | --- | --- |
| `TryGetLocalizedString(LocalizedString, out string)` | Current-Locale lookup | Empty reference/missing settings: false and empty string |
| `TryGetLocalizedString(StringTable, string, out string)` | Lookup in a prepared Table | Missing Entry: false and empty string |
| `GetLocalizedStringByLocaleAsync` | Specific-Locale lookup with async initialization | Empty reference/settings/Locale: empty string |
| `GetEntryKeyName` | Resolve key from name/ID reference | Empty reference: empty string |

Try methods are not universal exception guards; pass non-null receivers. Synchronous StringDatabase
lookup means Try is not a nonblocking guarantee. Async locale lookup currently has no cancellation-token
parameter.

## Editing to execution

Configure official Settings/Locales, assign Table/Entry in a LocalizedString-attributed field or use
the Drawer creation UI, then query in Play Mode. Manage live Locale subscriptions using the official
Localization API and consumer lifetimes.

The [sample](../Samples~/BasicUsage/README.md) covers type-named Tables, editor language, Entry
creation/search, and setup. Its Setup menu creates Settings/Tables/Entries in the consumer project;
use a test project or saved Scene. Edit Mode AvailableLocales can differ from Play Mode, so verify
runtime lookup in Play.

Read [runtime helpers](../Runtime/Extensions/LocalizationExtensions.cs) for failure/initialization
contracts, then [Drawer](../Editor/LocalizedStringDrawer.cs) and
[Editor helpers](../Editor/Extensions/LocalizedStringExtensionsForEditor.cs).
Namespaces are Jeomseon.Unity.Localization and its Extensions/Editor namespaces. Sample checks cover
successful lookup, empty references, missing Locales, Entry keys, and diagnostics.
