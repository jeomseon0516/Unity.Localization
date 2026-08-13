using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Jeomseon.Unity.Localization.Extensions
{
    public static class LocalizationExtensions
    {
        public static bool TryGetLocalizedString(this LocalizedString localizedString, out string localizedText)
        {
            if (localizedString.IsEmpty || !LocalizationSettings.HasSettings)
            {
                localizedText = string.Empty;
                return false;
            }

            return TryGetLocalizedString(
                localizedString,
                LocalizationSettings.StringDatabase,
                LocalizationSettings.SelectedLocale,
                out localizedText);
        }

        public static bool TryGetLocalizedString(this StringTable table, string entryName, out string localizedText)
        {
            StringTableEntry entry = table.GetEntry(entryName);
            if (entry is null)
            {
                localizedText = string.Empty;
                return false;
            }

            localizedText = entry.GetLocalizedString();
            return true;
        }

        /* Awaitable을 사용하는 이유: LocalizationSettings.InitializationOperation.WaitForCompletion()은
         * WebGL에서 지원되지 않고 메인 스레드를 블로킹합니다. 초기화가 끝나지 않은 상태에서 호출되면
         * 비동기로 대기합니다.
         */
        public static async Awaitable<string> GetLocalizedStringByLocaleAsync(this LocalizedString localizedString, string localeCode)
        {
            if (localizedString.IsEmpty || !LocalizationSettings.HasSettings) return string.Empty;

            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                await LocalizationSettings.InitializationOperation.Task;
            }

            return await GetLocalizedStringByLocaleAsync(
                localizedString,
                LocalizationSettings.StringDatabase,
                LocalizationSettings.AvailableLocales,
                localeCode);
        }

        internal static bool TryGetLocalizedString(
            LocalizedString localizedString,
            LocalizedStringDatabase stringDatabase,
            Locale locale,
            out string localizedText)
        {
            // Try 패턴 계약: 실패(false) 시 out 값은 null이 아니라 항상 string.Empty를 보장합니다.
            // GetLocalizedString은 엔트리를 찾지 못하면 null을 반환할 수 있어 그대로 대입하면 안 됩니다.
            string result = stringDatabase.GetLocalizedString(
                localizedString.TableReference,
                localizedString.TableEntryReference,
                locale);

            localizedText = result ?? string.Empty;
            return result is not null;
        }

        internal static async Awaitable<string> GetLocalizedStringByLocaleAsync(
            LocalizedString localizedString,
            LocalizedStringDatabase stringDatabase,
            ILocalesProvider localesProvider,
            string localeCode)
        {
            Locale locale = localesProvider.GetLocale(new LocaleIdentifier(localeCode));
            if (locale is null) return string.Empty;

            return await stringDatabase.GetLocalizedStringAsync(
                localizedString.TableReference,
                localizedString.TableEntryReference,
                locale).Task ?? string.Empty;
        }

        public static string GetEntryKeyName(this LocalizedString localizedString)
        {
            if (localizedString.IsEmpty) return string.Empty;

            if (localizedString.TableEntryReference.ReferenceType == TableEntryReference.Type.Name)
            {
                return localizedString.TableEntryReference.Key;
            }
            
            LocalizedDatabase<StringTable, StringTableEntry>.TableEntryResult t =
                LocalizationSettings.StringDatabase.GetTableEntry(
                    localizedString.TableReference,
                    localizedString.TableEntryReference);
            
            return t.Entry?.Key ?? string.Empty;

        }
    }
}
