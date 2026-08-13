#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Jeomseon.Unity.Localization.Editor
{
    internal enum EditorLocaleOverride
    {
        Auto = 0,
        Korean = 1,
        English = 2,
        Japanese = 3
    }

    // UnityEditor.LocalizationDatabase/L10n은 Unity 자체 내장 UI 문자열 번역용 internal API라
    // 패키지 코드에서 접근할 수 없습니다(protection level 오류로 확인). 기본값은 OS 언어
    // (Application.systemLanguage)를 따르고, Preferences의 EditorLocaleSettingsProvider에서
    // EditorLocaleOverride로 재정의할 수 있습니다. 지원 언어는 호출부마다 다를 수 있으므로
    // (예: 어떤 패키지는 일본어 문구가 없음), Tr 오버로드에 없는 언어가 현재 언어로 해석되면
    // 항상 영어로 fallback합니다.
    internal static class EditorLocaleText
    {
        public const string OverrideEditorPrefsKey = "Jeomseon.Localization.EditorLocaleOverride";

        public static EditorLocaleOverride Override
        {
            get => (EditorLocaleOverride)EditorPrefs.GetInt(OverrideEditorPrefsKey, (int)EditorLocaleOverride.Auto);
            set => EditorPrefs.SetInt(OverrideEditorPrefsKey, (int)value);
        }

        public static string Tr(string korean, string english)
        {
            return Tr(korean, english, null);
        }

        public static string Tr(string korean, string english, string japanese)
        {
            return ResolveLanguage() switch
            {
                EditorLocaleOverride.Korean => korean,
                EditorLocaleOverride.Japanese when !string.IsNullOrEmpty(japanese) => japanese,
                _ => english
            };
        }

        private static EditorLocaleOverride ResolveLanguage()
        {
            if (Override != EditorLocaleOverride.Auto)
            {
                return Override;
            }

            return Application.systemLanguage switch
            {
                SystemLanguage.Korean => EditorLocaleOverride.Korean,
                SystemLanguage.Japanese => EditorLocaleOverride.Japanese,
                _ => EditorLocaleOverride.English
            };
        }
    }
}
#endif
