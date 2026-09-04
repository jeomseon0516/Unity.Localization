# Jeomseon Unity Localization

Runtime extensions, `LocalizedStringAttribute`, and editor tooling for Unity Localization.

## Install via OpenUPM

Register the OpenUPM scoped registry once in your project's `Packages/manifest.json`.

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

## Install via Git URL

Enter the following URL in Unity Package Manager's `Install package from git URL`.

```text
https://github.com/jeomseon0516/Unity.Localization.git#v0.5.2
```

## Behavior contract

- `GetLocalizedStringByLocaleAsync(string localeCode)` returns `Awaitable<string>`. If
  Localization has not finished initializing, it awaits asynchronously instead of blocking the
  main thread.
- `TryGetLocalizedString(this StringTable, string entryName, out string)` returns `false` and an
  empty string when the entry does not exist, instead of throwing.
