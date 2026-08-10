using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Jeomseon.LocalizationExtensions
{
    public static class LocalizationExtensions
    {
        public static bool TryGetLocalizedString(this LocalizedString localizedString, out string localizedText)
        {
            if (localizedString.IsEmpty)
            {
                localizedText = "";
                return false;
            }

            localizedText = localizedString.GetLocalizedString();
            return true;
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
            if (localizedString.IsEmpty) return string.Empty;

            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                await LocalizationSettings.InitializationOperation.Task;
            }

            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            return await localizedString.GetLocalizedStringAsync(locale).Task;
        }

        public static string GetEntryKeyName(this LocalizedString localizedString)
        {
            if (!string.IsNullOrEmpty(localizedString.TableReference))
            {
                LocalizedDatabase<StringTable, StringTableEntry>.TableEntryResult t =
                    LocalizationSettings.StringDatabase.GetTableEntry(
                        localizedString.TableReference,
                        localizedString.TableEntryReference);
                return t.Entry?.Key ?? "";
            }

            return "";
        }
    }
}
