# Jeomseon Unity Localization

[한국어](Overview.md) · [Installation](../README.en.md)

## Purpose

Preserves Unity LocalizedString and String Tables while adding locale lookup helpers and dedicated Inspector editing. It does not replace the localization data service.

## Usage flow

Configure Unity Locales/Tables/Entries, assign the attributed LocalizedString, then call TryGetLocalizedString or GetLocalizedStringByLocaleAsync. Import Jeomseon.Unity.Localization.Extensions.

## Contracts and limitations

Async locale lookup waits for initialization and returns an empty string for empty references or absent Locales. No cancellation-token overload exists. Try lookup uses the official synchronous StringDatabase path; not every helper is nonblocking.

## Package requirements

- Package ID: `com.jeomseon.unity.localization`
- Version: `0.5.2`
- Unity: `6000.6` / `0f1`
- `com.jeomseon.unity.core`: `0.3.2`
- `com.unity.editorcoroutines`: `1.0.0`
- `com.unity.localization`: `1.4.5`

## Samples and verification

Import the following from Package Manager > Samples. Open the included Scene or Editor tool; follow any Setup step in its sample README.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [LocalizationSample](../Samples~/BasicUsage/LocalizationSample.unity)

[Test sources](../Tests)

Add the package ID to manifest `testables`, then run the relevant Unity Test Runner assemblies. These links describe available tests, not a new successful test run.

## Source map

- [LocalizationExtensions](../Runtime/Extensions/LocalizationExtensions.cs)
- [LocalizedStringAttribute](../Runtime/LocalizedStringAttribute.cs)
- [LocalizedStringExtensionsForEditor](../Editor/Extensions/LocalizedStringExtensionsForEditor.cs)

[Detailed design](Design.en.md)
