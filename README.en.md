# Jeomseon Unity Localization

Runtime extensions, `LocalizedStringAttribute`, and editor tooling for Unity Localization.

## Behavior contract

- `GetLocalizedStringByLocaleAsync(string localeCode)` returns `Awaitable<string>`. If
  Localization has not finished initializing, it awaits asynchronously instead of blocking the
  main thread.
- `TryGetLocalizedString(this StringTable, string entryName, out string)` returns `false` and an
  empty string when the entry does not exist, instead of throwing.
