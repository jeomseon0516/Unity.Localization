using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Jeomseon.Samples.Localization.Editor
{
    internal static class LocalizationSampleTableSetup
    {
        // LocalizationSample.message는 [LocalizedString]에 tableName을 지정하지 않았으므로
        // LocalizedStringDrawer가 테이블 이름을 컴포넌트 타입 이름으로 자동 고정합니다.
        private const string TableName = "LocalizationSample";
        private const string EntryKey = "Greeting";

        private static readonly SystemLanguage[] SampleLanguages = { SystemLanguage.English, SystemLanguage.Korean };

        [MenuItem("Jeomseon/Tool/Localization/Setup Basic Usage Sample")]
        private static void Setup()
        {
            string sampleFolder = GetSampleFolder();
            EnsureLocalizationSettings(sampleFolder);

            if (LocalizationEditorSettings.GetStringTableCollection(TableName))
            {
                Debug.Log($"[PASS] Active Localization Settings and '{TableName}' table are ready. / " +
                    $"활성 Localization Settings와 '{TableName}' 테이블이 준비되었습니다.");
                return;
            }

            if (LocalizationEditorSettings.GetLocales().Count == 0)
            {
                CreateSampleLocales(sampleFolder);
            }

            StringTableCollection collection = LocalizationEditorSettings.CreateStringTableCollection(TableName, sampleFolder);
            SharedTableData.SharedTableEntry entry = collection.SharedData.AddKey(EntryKey);

            foreach (StringTable table in collection.StringTables)
            {
                table.AddEntry(entry.Id, GetDefaultValue(table.LocaleIdentifier));
                EditorUtility.SetDirty(table);
            }

            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(collection.SharedData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PASS] Created table '{TableName}' with entry '{EntryKey}'. Open " +
                $"LocalizationSample.unity, select the entry in the message field's Inspector " +
                $"(\"Select Entry\" button), then enter Play Mode. / 테이블 '{TableName}'과 엔트리 " +
                $"'{EntryKey}'를 생성했습니다. LocalizationSample.unity를 열어 message 필드 Inspector에서 " +
                $"방금 만든 Entry를 선택(\"엔트리 선택\" 버튼)한 뒤 Play Mode로 확인해 주세요.");
        }

        private static void EnsureLocalizationSettings(string sampleFolder)
        {
            // is not null(참조 비교)이 아니라 UnityEngine.Object의 암시적 bool 변환을 씁니다.
            // ActiveLocalizationSettings는 EditorBuildSettings에 저장된, 프로젝트 전체에 걸쳐
            // 유지되는 config object 참조입니다. 예전 Sample 버전(0.2.3)이 재임포트로 제거되면서
            // 그 폴더에 있던 Settings 자산도 함께 삭제되면, 이 참조는 "존재하지만 실제로는 파괴된
            // 오브젝트"(Unity의 fake-null)가 됩니다. is not null은 그런 경우도 true로 판단해 새
            // Settings를 만들지 않고 건너뛰어, Play Mode 진입 시
            // "There is no active LocalizationSettings" 예외로 이어졌습니다.
            if (LocalizationEditorSettings.ActiveLocalizationSettings)
            {
                return;
            }

            string settingsPath = $"{sampleFolder}/Localization Settings.asset";
            LocalizationSettings settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(settingsPath);
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                settings.name = "Localization Settings";
                AssetDatabase.CreateAsset(settings, settingsPath);
            }

            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        // 프로젝트에 Locale이 하나도 없을 때만 예시로 en/ko Locale을 생성합니다. 기존 Locale이
        // 하나라도 있으면 이 메서드는 호출되지 않으므로, 이미 구성된 프로젝트의 Locale 목록을
        // 건드리지 않습니다.
        private static void CreateSampleLocales(string sampleFolder)
        {
            string localeFolder = $"{sampleFolder}/Locale";
            if (!AssetDatabase.IsValidFolder(localeFolder))
            {
                AssetDatabase.CreateFolder(sampleFolder, "Locale");
            }

            foreach (SystemLanguage language in SampleLanguages)
            {
                Locale locale = Locale.CreateLocale(language);
                AssetDatabase.CreateAsset(locale, $"{localeFolder}/{language}.asset");
                LocalizationEditorSettings.AddLocale(locale, false);
            }

            Debug.Log($"[PASS] Created sample Locales ({string.Join(", ", SampleLanguages)}) at " +
                $"'{localeFolder}' — this project had none. / 이 프로젝트에 Locale이 없어 예시로 " +
                $"({string.Join(", ", SampleLanguages)}) Locale을 '{localeFolder}'에 생성했습니다.");
        }

        private static string GetDefaultValue(LocaleIdentifier localeIdentifier)
        {
            return localeIdentifier.Code switch
            {
                "ko" => "안녕하세요, Localization Sample!",
                "en" => "Hello, Localization Sample!",
                _ => ""
            };
        }

        private static string GetSampleFolder([CallerFilePath] string sourceFilePath = "")
        {
            string editorFolder = Path.GetDirectoryName(sourceFilePath);
            string basicUsageFolder = Path.GetDirectoryName(editorFolder);
            return FileUtil.GetProjectRelativePath(basicUsageFolder);
        }
    }
}
