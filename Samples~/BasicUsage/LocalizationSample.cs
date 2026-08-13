using Jeomseon.Unity.Localization;
using Jeomseon.Unity.Localization.Extensions;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;

namespace Jeomseon.Samples.Localization
{
    public sealed class LocalizationSample : MonoBehaviour
    {
        // canSelectTable을 지정하지 않으면 테이블 이름이 컴포넌트 타입 이름("LocalizationSample")으로
        // 고정되고, Inspector에 테이블이 없을 때 "테이블 생성" 버튼이 표시됩니다. 이 attribute 없이
        // 기본 LocalizedString 필드로 두면 Unity 기본 Drawer가 대신 그려져 이 패키지가 제공하는
        // 테이블/엔트리 생성 UI를 확인할 수 없습니다.
        [SerializeField, FormerlySerializedAs("_message"), LocalizedString] private LocalizedString message;

        private const string SampleLocaleCode = "en";

        private void OnEnable()
        {
            message.StringChanged += OnStringChanged;
        }

        private void OnDisable()
        {
            message.StringChanged -= OnStringChanged;
        }

        private static void OnStringChanged(string value)
        {
            Debug.Log($"현지화 문자열: {value}");
        }

        [ContextMenu("Entry Key 확인")]
        private void PrintEntryKeyName()
        {
            Debug.Log($"Entry Key: {message.GetEntryKeyName()}");
        }

        [ContextMenu("TryGetLocalizedString 확인")]
        private void PrintTryGetLocalizedString()
        {
            if (message.TryGetLocalizedString(out string localizedText))
            {
                Debug.Log($"TryGetLocalizedString 성공: {localizedText}");
            }
            else if (message.IsEmpty)
            {
                Debug.Log("TryGetLocalizedString 실패: LocalizedString이 비어 있습니다(Entry 미지정).");
            }
            else if (!LocalizationSettings.HasSettings)
            {
                Debug.Log("TryGetLocalizedString 실패: 활성 Localization Settings가 없습니다.");
            }
            else if (!Application.isPlaying)
            {
                // Unity Localization의 기본 LocalesProvider는 Play Mode(또는 진입 예정)일 때만
                // Addressables Locale Preload를 트리거합니다(LocalesProvider.Locales 내부의
                // PlaymodeState.IsPlayingOrWillChangePlaymode 가드). Edit Mode에서는 Locale
                // 목록이 비어 있을 수 있어 참조가 정상이어도 값을 못 찾을 수 있습니다.
                Debug.Log("TryGetLocalizedString 실패: 참조는 정상이지만 Edit Mode에서는 Unity " +
                    "Localization이 Locale Preload를 건너뛸 수 있습니다. Play Mode에서 다시 " +
                    "실행하거나 \"진단 정보 확인\"으로 AvailableLocales 상태를 확인해 주세요.");
            }
            else
            {
                Debug.Log("TryGetLocalizedString 실패: 참조는 있지만 선택된 Locale의 테이블에서 값을 찾지 못했습니다. " +
                    "\"진단 정보 확인\"으로 SelectedLocale과 TableEntryReference를 확인해 주세요.");
            }
        }

        [ContextMenu("GetLocalizedStringByLocaleAsync 확인 (en)")]
        private async void PrintLocalizedStringByLocaleAsync()
        {
            string localizedText = await message.GetLocalizedStringByLocaleAsync(SampleLocaleCode);
            if (string.IsNullOrEmpty(localizedText) && !Application.isPlaying)
            {
                Debug.Log($"'{SampleLocaleCode}' 로케일 문자열: (비어 있음) — Edit Mode에서는 Unity " +
                    "Localization이 Locale Preload를 건너뛸 수 있습니다. Play Mode에서 다시 실행해 주세요.");
            }
            else
            {
                Debug.Log($"'{SampleLocaleCode}' 로케일 문자열: {localizedText}");
            }
        }

        [ContextMenu("진단 정보 확인")]
        private void PrintDiagnostics()
        {
            bool hasSettings = LocalizationSettings.HasSettings;
            string selectedLocale = hasSettings
                ? LocalizationSettings.SelectedLocale != null ? LocalizationSettings.SelectedLocale.ToString() : "null"
                : "N/A (활성 Settings 없음)";
            string initializationDone = hasSettings
                ? LocalizationSettings.InitializationOperation.IsDone.ToString()
                : "N/A (활성 Settings 없음)";
            // LocalesProvider.Locales는 Play Mode가 아니면 Addressables Preload를 트리거하지
            // 않으므로, Edit Mode에서 0이어도 정상입니다(버그 아님).
            string availableLocaleCount = hasSettings
                ? LocalizationSettings.AvailableLocales.Locales.Count.ToString()
                : "N/A (활성 Settings 없음)";

            Debug.Log(
                $"[진단] IsEmpty={message.IsEmpty}, " +
                $"TableReference=(ReferenceType={message.TableReference.ReferenceType}, " +
                $"TableCollectionName='{message.TableReference.TableCollectionName}'), " +
                $"TableEntryReference=(ReferenceType={message.TableEntryReference.ReferenceType}, " +
                $"Key='{message.TableEntryReference.Key}', KeyId={message.TableEntryReference.KeyId}), " +
                $"HasSettings={hasSettings}, SelectedLocale={selectedLocale}, " +
                $"InitializationDone={initializationDone}, " +
                $"IsPlaying={Application.isPlaying}, AvailableLocalesLoaded={availableLocaleCount}");
        }
    }
}
