#if UNITY_EDITOR
using UnityEditor;

namespace Jeomseon.Unity.Localization.Editor
{
    internal static class EditorLocaleSettingsProvider
    {
        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Preferences/Jeomseon/Localization", SettingsScope.User)
            {
                label = "Localization",
                guiHandler = _ =>
                {
                    EditorGUI.BeginChangeCheck();
                    EditorLocaleOverride selected = (EditorLocaleOverride)EditorGUILayout.EnumPopup(
                        EditorLocaleText.Tr("Editor 언어", "Editor Language"),
                        EditorLocaleText.Override);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorLocaleText.Override = selected;
                    }

                    EditorGUILayout.HelpBox(EditorLocaleText.Tr(
                        "Auto는 OS 언어(Application.systemLanguage)를 따릅니다.",
                        "Auto follows the OS language (Application.systemLanguage)."), MessageType.None);
                },
                keywords = new[] { "Localization", "Language", "언어", "로케일" }
            };
        }
    }
}
#endif
