# Jeomseon Unity Localization

[한국어](README.md) | English

Preserves Unity LocalizedString and String Tables while adding locale lookup helpers and dedicated Inspector editing. It does not replace the localization data service.

## Quick start

Configure Unity Locales/Tables/Entries, assign the attributed LocalizedString, then call TryGetLocalizedString or GetLocalizedStringByLocaleAsync. Import Jeomseon.Unity.Localization.Extensions.

Configure a LocalizedString in the Inspector after setting up official Localization Settings, Locales, and String Tables. The snippet belongs inside a MonoBehaviour; runtime lookups are verified in Play Mode.

```csharp
using Jeomseon.Unity.Localization.Extensions;
using UnityEngine;
using UnityEngine.Localization;

[SerializeField, Jeomseon.Unity.Localization.LocalizedString]
private LocalizedString message;

private async Awaitable PrintEnglishAsync()
{
    string text = await message.GetLocalizedStringByLocaleAsync("en");
    Debug.Log(text);
}
```

[Complete sample](Samples~/BasicUsage/LocalizationSample.cs)

## Contracts and limitations

Async locale lookup waits for initialization and returns an empty string for empty references or absent Locales. No cancellation-token overload exists. Try lookup uses the official synchronous StringDatabase path; not every helper is nonblocking.

## Requirements

- Unity `6000.6.0f1`
- Package: `com.jeomseon.unity.localization` `0.5.2`
- `com.jeomseon.unity.core`: `0.3.2`
- `com.unity.editorcoroutines`: `1.0.0`
- `com.unity.localization`: `1.4.5`

## Installation

Merge this into your project's `Packages/manifest.json`; preserve existing dependencies and registries.

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.jeomseon"
      ]
    }
  ],
  "dependencies": {
    "com.jeomseon.unity.localization": "0.5.2"
  }
}
```

Or use **Install package from git URL** in Package Manager:

```text
https://github.com/jeomseon0516/Unity.Localization.git#v0.5.2
```

## Design and behavior

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

### Editing to execution

Configure official Settings/Locales, assign Table/Entry in a LocalizedString-attributed field or use
the Drawer creation UI, then query in Play Mode. Manage live Locale subscriptions using the official
Localization API and consumer lifetimes.

The [sample](Samples~/BasicUsage/README.md) covers type-named Tables, editor language, Entry
creation/search, and setup. Its Setup menu creates Settings/Tables/Entries in the consumer project;
use a test project or saved Scene. Edit Mode AvailableLocales can differ from Play Mode, so verify
runtime lookup in Play.

Read [runtime helpers](Runtime/Extensions/LocalizationExtensions.cs) for failure/initialization
contracts, then [Drawer](Editor/LocalizedStringDrawer.cs) and
[Editor helpers](Editor/Extensions/LocalizedStringExtensionsForEditor.cs).
Namespaces are Jeomseon.Unity.Localization and its Extensions/Editor namespaces. Sample checks cover
successful lookup, empty references, missing Locales, Entry keys, and diagnostics.

## API usage and details

### Behavior contract

- `GetLocalizedStringByLocaleAsync(string localeCode)` returns `Awaitable<string>`. If
  Localization has not finished initializing, it awaits asynchronously instead of blocking the
  main thread.
- `TryGetLocalizedString(this StringTable, string entryName, out string)` returns `false` and an
  empty string when the entry does not exist, instead of throwing.

## Samples

Import the desired entry from **Package Manager > Samples**. Use the included Scene/Editor tool and complete its Setup step first, if required.

- [Basic Usage](Samples~/BasicUsage/README.md)
  - [LocalizationSample](Samples~/BasicUsage/LocalizationSample.unity)

## Tests

[Test sources](Tests)

Add the package ID to manifest `testables`, then execute its relevant EditMode/PlayMode assemblies in Unity Test Runner.

## Further reading

[Complete source map and package metadata](Documentation~/Overview.en.md)
- [Changelog](CHANGELOG.md)

## License

[License](LICENSE.md)
