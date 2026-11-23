#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
                activateHandler = (_, rootElement) => BuildSettings(rootElement),
                keywords = new[] { "Localization", "Language", "언어", "로케일" }
            };
        }

        private static void BuildSettings(VisualElement rootElement)
        {
            rootElement.Clear();

            // activateHandler는 guiHandler와 달리 제목을 그리지 않으므로 Settings 창 IMGUI 제목과 같은 형태로 직접 추가합니다.
            var titleLabel = new Label("Localization")
            {
                style =
                {
                    fontSize = 19,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginLeft = 10,
                    marginTop = 1,
                    marginBottom = 10
                }
            };
            titleLabel.AddToClassList("title");
            rootElement.Add(titleLabel);

            var content = new VisualElement
            {
                style =
                {
                    marginLeft = 10,
                    marginRight = 10
                }
            };
            rootElement.Add(content);

            var languageField = new EnumField(EditorLocaleText.Tr("Editor 언어", "Editor Language"), EditorLocaleText.Override);
            languageField.RegisterValueChangedCallback(evt =>
            {
                EditorLocaleText.Override = (EditorLocaleOverride)evt.newValue;

                // guiHandler는 매 프레임 라벨을 다시 번역했으므로, 언어가 바뀌면 다음 틱에 화면을 다시 구성합니다.
                rootElement.schedule.Execute(() => BuildSettings(rootElement));
            });
            content.Add(languageField);

            content.Add(new HelpBox(EditorLocaleText.Tr(
                "Auto는 OS 언어(Application.systemLanguage)를 따릅니다.",
                "Auto follows the OS language (Application.systemLanguage)."), HelpBoxMessageType.None));
        }
    }
}
#endif
