#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.Localization.Editor
{
    [CustomPropertyDrawer(typeof(LocalizedStringAttribute))]
    internal sealed class LocalizedStringDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, StringTableCollection> TableCache = new();
        private static EntryAdvancedDropdown _entryDropdown = null;
        private static List<Locale> _localesCache;

        static LocalizedStringDrawer()
        {
            // 정적 생성자는 도메인 로드당 최초 1회만 실행되고 Domain Reload마다 정적 상태가
            // 초기화되므로, 별도 해제 없이 구독해도 중복 등록되지 않습니다.
            LocalizationEditorSettings.EditorEvents.LocaleAdded += OnLocaleChanged;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved += OnLocaleChanged;
        }

        private static void OnLocaleChanged(Locale locale)
        {
            // 로케일 캐시 무효화
            _localesCache = null;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            LocalizedStringAttribute attr = (LocalizedStringAttribute)attribute;
            string labelText = preferredLabel ?? property.displayName;

            if (property.propertyType != SerializedPropertyType.Generic || property.type != nameof(LocalizedString))
            {
                return CreateLabelRow(labelText, EditorLocaleText.Tr(
                    "LocalizedStringAttribute는 LocalizedString 타입에만 사용 가능합니다.",
                    "LocalizedStringAttribute can only be used on a LocalizedString field."));
            }

            var root = new VisualElement();
            var foldout = new Foldout
            {
                text = labelText,
                value = property.isExpanded
            };
            // IMGUI판은 indentLevel을 0으로 두고 내용을 레이블과 같은 x에 그리므로, 내용은 Foldout 자식이 아닌 형제로 둡니다.
            var content = new VisualElement();
            root.Add(foldout);
            root.Add(content);

            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.target != foldout)
                {
                    return;
                }

                property.isExpanded = evt.newValue;
                Rebuild();
            });

            // Undo·외부 변경·엔트리 선택 등 참조 문자열이 바뀌면 IMGUI판의 매 프레임 재계산에 대응해 내용을 다시 구성합니다.
            SerializedProperty tableNamePropForTracking = property.FindPropertyRelative("m_TableReference").FindPropertyRelative("m_TableCollectionName");
            SerializedProperty tableEntryKeyPropForTracking = property.FindPropertyRelative("m_TableEntryReference").FindPropertyRelative("m_Key");
            root.TrackPropertyValue(tableNamePropForTracking, _ => Rebuild());
            root.TrackPropertyValue(tableEntryKeyPropForTracking, _ => Rebuild());

            Action<Locale> onLocaleChanged = _ => Rebuild();
            root.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                LocalizationEditorSettings.EditorEvents.LocaleAdded += onLocaleChanged;
                LocalizationEditorSettings.EditorEvents.LocaleRemoved += onLocaleChanged;
            });
            root.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                LocalizationEditorSettings.EditorEvents.LocaleAdded -= onLocaleChanged;
                LocalizationEditorSettings.EditorEvents.LocaleRemoved -= onLocaleChanged;
            });

            Rebuild();
            return root;

            void Rebuild()
            {
                content.Clear();
                foldout.SetValueWithoutNotify(property.isExpanded);

                if (!property.isExpanded)
                {
                    content.style.display = DisplayStyle.None;
                    return;
                }

                content.style.display = DisplayStyle.Flex;
                BuildExpandedContent(content, property, attr, Rebuild);
            }
        }

        private static void BuildExpandedContent(VisualElement content, SerializedProperty property, LocalizedStringAttribute attr, Action rebuild)
        {
            SerializedProperty tableReferenceProp = property.FindPropertyRelative("m_TableReference");
            SerializedProperty tableEntryReferenceProp = property.FindPropertyRelative("m_TableEntryReference");
            SerializedProperty tableNameProp = tableReferenceProp.FindPropertyRelative("m_TableCollectionName");
            SerializedProperty tableEntryKeyProp = tableEntryReferenceProp.FindPropertyRelative("m_Key");

            // OnGUI와 같이 구성 시작 시점 스냅샷과 끝난 뒤 값을 비교해 참조 동기화 여부를 판단합니다.
            string originalTableName = tableNameProp.stringValue;
            string originalEntryKey = tableEntryKeyProp.stringValue;

            string tableName;

            if (!attr.CanSelectTable)
            {
                tableName = attr.TableName;

                if (string.IsNullOrEmpty(tableName))
                {
                    tableName = property.serializedObject.targetObject.GetType().Name;
                    if (tableNameProp.stringValue != tableName)
                    {
                        tableNameProp.stringValue = tableName;
                    }
                }
            }
            else
            {
                tableName = tableNameProp.stringValue;

                // 매 입력마다 화면을 다시 구성하면 포커스를 잃으므로 입력 확정(Enter·포커스 해제) 시 반영합니다.
                var tableNameField = new TextField("Table Name")
                {
                    value = tableName,
                    isDelayed = true
                };
                tableNameField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue != tableNameProp.stringValue)
                    {
                        ModifyReference(property, tableNameProp, tableEntryKeyProp, () => tableNameProp.stringValue = evt.newValue, rebuild);
                    }
                });
                content.Add(tableNameField);
            }

            string entryKey = attr.EntryKey;
            if (string.IsNullOrEmpty(entryKey))
            {
                entryKey = tableEntryKeyProp.stringValue;
            }

            StringTableCollection tableCollection = GetTableCollection(tableName);
            SharedTableData.SharedTableEntry sharedTableEntry = null;
            if (tableCollection)
            {
                sharedTableEntry = GetSharedTableEntry(tableCollection, entryKey);
                string collectionName = tableCollection.SharedData.TableCollectionName;
                if (tableNameProp.stringValue != collectionName)
                {
                    tableNameProp.stringValue = collectionName;
                }

                if (sharedTableEntry is null && !string.IsNullOrEmpty(attr.EntryKey))
                {
                    sharedTableEntry = tableCollection.SharedData.AddKey(entryKey);
                    EditorUtility.SetDirty(tableCollection);
                    EditorUtility.SetDirty(tableCollection.SharedData);
                }
            }

            SerializedProperty tableEntryIdProp = tableEntryReferenceProp.FindPropertyRelative("m_KeyId");
            // OnGUI와 같은 이유로 이름 참조로 저장합니다.
            const long EntryId = 0;
            if (tableEntryIdProp.longValue != EntryId)
            {
                tableEntryIdProp.longValue = EntryId;
            }

            if (!tableCollection)
            {
                content.Add(new HelpBox(EditorLocaleText.Tr(
                    $"테이블 '{tableName}'이(가) 존재하지 않습니다.",
                    $"Table '{tableName}' does not exist."), HelpBoxMessageType.Error));

                if (!string.IsNullOrEmpty(tableName))
                {
                    string targetTableName = tableName;
                    content.Add(new Button(() => PromptCreateStringTable(targetTableName, rebuild))
                    {
                        text = EditorLocaleText.Tr("테이블 생성", "Create Table")
                    });
                }
            }
            else if (sharedTableEntry is null)
            {
                content.Add(new HelpBox(EditorLocaleText.Tr(
                    $"엔트리 '{entryKey}'이(가) 테이블 '{tableName}'에 존재하지 않습니다.",
                    $"Entry '{entryKey}' does not exist in table '{tableName}'."), HelpBoxMessageType.Error));
            }
            else
            {
                List<Locale> locales = GetLocales();

                if (string.IsNullOrEmpty(attr.EntryKey))
                {
                    content.Add(CreateEntryKeyRow(property, tableNameProp, tableEntryKeyProp, tableCollection.SharedData, sharedTableEntry, rebuild));
                }

                Color optionBackgroundColor = ColorUtility.TryParseHtmlString("#413a4f", out Color color) ? color : Color.black;

                foreach (Locale locale in locales)
                {
                    StringTable stringTable = tableCollection.GetTable(locale.Identifier) as StringTable;

                    if (stringTable)
                    {
                        StringTableEntry entry = stringTable.GetEntry(sharedTableEntry.Id) ?? stringTable.AddEntry(sharedTableEntry.Id, "");

                        content.Add(new Label(locale.LocaleName));

                        var optionBox = new VisualElement
                        {
                            style =
                            {
                                backgroundColor = optionBackgroundColor
                            }
                        };

                        var isSmartToggle = new Toggle("Is Smart")
                        {
                            value = entry.IsSmart
                        };
                        isSmartToggle.RegisterValueChangedCallback(evt =>
                        {
                            if (evt.newValue != entry.IsSmart)
                            {
                                entry.IsSmart = evt.newValue;
                                EditorUtility.SetDirty(stringTable);
                                EditorUtility.SetDirty(tableCollection);
                            }
                        });
                        optionBox.Add(isSmartToggle);

                        var valueField = new TextField
                        {
                            multiline = true,
                            value = entry.Value,
                            style =
                            {
                                height = EditorGUIUtility.singleLineHeight * 4,
                                whiteSpace = WhiteSpace.Normal
                            }
                        };
                        valueField.RegisterValueChangedCallback(evt =>
                        {
                            if (evt.newValue != entry.Value)
                            {
                                entry.Value = evt.newValue;
                                EditorUtility.SetDirty(stringTable);
                                EditorUtility.SetDirty(tableCollection);
                            }
                        });
                        optionBox.Add(valueField);

                        content.Add(optionBox);
                    }
                    else
                    {
                        content.Add(CreateLabelRow(locale.LocaleName, EditorLocaleText.Tr(
                            "해당 로케일의 테이블이 존재하지 않습니다.",
                            "No table exists for this locale.")));
                    }
                }
            }

            if (tableCollection && string.IsNullOrEmpty(attr.EntryKey))
            {
                string targetTableName = tableName;
                Button selectEntryButton = null;
                selectEntryButton = new Button(() =>
                {
                    _entryDropdown = new(new(), GetTableCollection(targetTableName), tableCollection.SharedData);
                    // TargetProp 대신 선택 이벤트에서 스냅샷 비교 후 참조를 동기화합니다.
                    _entryDropdown.OnEntrySelected += selectedEntry =>
                        ModifyReference(property, tableNameProp, tableEntryKeyProp, () => tableEntryKeyProp.stringValue = selectedEntry.Key, rebuild);
                    _entryDropdown.Show(selectEntryButton.worldBound);
                })
                {
                    text = EditorLocaleText.Tr("엔트리 선택", "Select Entry")
                };
                content.Add(selectEntryButton);
            }

            SyncLocalizedStringReference(property, tableNameProp, tableEntryKeyProp, originalTableName, originalEntryKey);

            // IMGUI는 Inspector가 OnGUI 뒤에 변경을 반영하지만 UI Toolkit 경로는 직접 반영해야 합니다(Undo 기록 포함).
            if (property.serializedObject.hasModifiedProperties)
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private static VisualElement CreateEntryKeyRow(
            SerializedProperty property,
            SerializedProperty tableNameProp,
            SerializedProperty tableEntryKeyProp,
            SharedTableData tableData,
            SharedTableData.SharedTableEntry sharedTableEntry,
            Action rebuild)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };

            // 매 입력마다 키를 바꾸면 화면 재구성으로 포커스를 잃으므로 입력 확정 시 반영합니다.
            var keyField = new TextField("Entry Key")
            {
                value = sharedTableEntry.Key,
                isDelayed = true,
                style =
                {
                    flexGrow = 1,
                    flexBasis = 0,
                    marginRight = EditorGUIUtility.standardVerticalSpacing
                }
            };
            keyField.RegisterValueChangedCallback(evt =>
            {
                string key = evt.newValue;
                if (key != sharedTableEntry.Key && !string.IsNullOrEmpty(key) && !tableData.Entries.Any(entry => entry != sharedTableEntry && entry.Key == key))
                {
                    ModifyReference(property, tableNameProp, tableEntryKeyProp, () =>
                    {
                        tableData.RenameKey(sharedTableEntry.Key, key);
                        tableEntryKeyProp.stringValue = key;
                        EditorUtility.SetDirty(tableData);
                    }, rebuild);
                }
                else
                {
                    keyField.SetValueWithoutNotify(sharedTableEntry.Key);
                }
            });
            row.Add(keyField);

            row.Add(new Button(() =>
            {
                ModifyReference(property, tableNameProp, tableEntryKeyProp, () =>
                {
                    tableData.RemoveKey(sharedTableEntry.Key);
                    tableEntryKeyProp.stringValue = "";
                    EditorUtility.SetDirty(tableData);
                }, rebuild);
            })
            {
                text = "Delete",
                style =
                {
                    flexGrow = 1,
                    flexBasis = 0
                }
            });

            return row;
        }

        private static VisualElement CreateLabelRow(string label, string message)
        {
            // EditorGUI.LabelField(label, message)의 2열 배치를 기본 필드 USS 클래스로 맞춥니다.
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            row.AddToClassList(BaseField<string>.ussClassName);

            var labelElement = new Label(label);
            labelElement.AddToClassList(BaseField<string>.labelUssClassName);
            row.Add(labelElement);
            row.Add(new Label(message));
            return row;
        }

        private static void ModifyReference(
            SerializedProperty property,
            SerializedProperty tableNameProp,
            SerializedProperty tableEntryKeyProp,
            Action modify,
            Action rebuild)
        {
            string originalTableName = tableNameProp.stringValue;
            string originalEntryKey = tableEntryKeyProp.stringValue;

            modify();
            SyncLocalizedStringReference(property, tableNameProp, tableEntryKeyProp, originalTableName, originalEntryKey);

            if (property.serializedObject.hasModifiedProperties)
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            rebuild();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LocalizedStringAttribute attr = (LocalizedStringAttribute)attribute;

            if (property.propertyType != SerializedPropertyType.Generic || property.type != nameof(LocalizedString))
            {
                EditorGUI.LabelField(position, label.text, EditorLocaleText.Tr(
                    "LocalizedStringAttribute는 LocalizedString 타입에만 사용 가능합니다.",
                    "LocalizedStringAttribute can only be used on a LocalizedString field."));
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 현재 위치 저장
            Rect currentPosition = position;

            // 레이블 그리기
            currentPosition.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(currentPosition, property.isExpanded, label);

            // 다음 줄로 이동
            currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                SerializedProperty tableReferenceProp = property.FindPropertyRelative("m_TableReference");
                SerializedProperty tableEntryReferenceProp = property.FindPropertyRelative("m_TableEntryReference");
                SerializedProperty tableNameProp = tableReferenceProp.FindPropertyRelative("m_TableCollectionName");
                SerializedProperty tableEntryKeyProp = tableEntryReferenceProp.FindPropertyRelative("m_Key");

                // 이번 OnGUI 호출에서 실제로 참조가 바뀌었는지는 아래의 개별 수정 지점마다 플래그를
                // 챙기는 대신, 시작 시점 스냅샷과 끝난 뒤 값을 비교해 판단합니다. Rename/Delete처럼
                // 새로 추가되는 수정 경로가 플래그를 빠뜨려도 이 비교가 항상 놓치지 않습니다.
                string originalTableName = tableNameProp.stringValue;
                string originalEntryKey = tableEntryKeyProp.stringValue;

                // 테이블 이름 및 엔트리 키 설정
                string tableName = "";

                if (!attr.CanSelectTable)
                {
                    tableName = attr.TableName;

                    if (string.IsNullOrEmpty(tableName))
                    {
                        tableName = tableNameProp.stringValue;
                        if (string.IsNullOrEmpty(tableName))
                        {
                            tableName = property.serializedObject.targetObject.GetType().Name;
                            tableNameProp.stringValue = tableName;
                        }
                        else
                        {
                            if (tableName != property.serializedObject.targetObject.GetType().Name)
                            {
                                tableName = property.serializedObject.targetObject.GetType().Name;
                                tableNameProp.stringValue = tableName;
                            }
                        }
                    }
                }
                else
                {
                    tableName = tableNameProp.stringValue;

                    tableName = EditorGUI.TextField(currentPosition, "Table Name", tableName);
                    currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    if (tableName != tableNameProp.stringValue)
                    {
                        tableNameProp.stringValue = tableName;
                    }
                }

                string entryKey = attr.EntryKey;
                if (string.IsNullOrEmpty(entryKey))
                {
                    entryKey = tableEntryKeyProp.stringValue;
                }

                // 테이블 컬렉션 가져오기
                StringTableCollection tableCollection = GetTableCollection(tableName);
                // 엔트리 가져오기
                SharedTableData.SharedTableEntry sharedTableEntry = null;
                if (tableCollection)
                {
                    sharedTableEntry = GetSharedTableEntry(tableCollection, entryKey);
                    string collectionName = tableCollection.SharedData.TableCollectionName;
                    if (tableNameProp.stringValue != collectionName)
                    {
                        tableNameProp.stringValue = collectionName;
                    }

                    if (sharedTableEntry is null && !string.IsNullOrEmpty(attr.EntryKey))
                    {
                        sharedTableEntry = tableCollection.SharedData.AddKey(entryKey);
                        EditorUtility.SetDirty(tableCollection);
                        EditorUtility.SetDirty(tableCollection.SharedData);
                    }
                }

                SerializedProperty tableEntryIdProp = tableEntryReferenceProp.FindPropertyRelative("m_KeyId");
                // Entry 이름을 이미 알고 있으므로 이름 참조로 저장합니다. ID 참조는 Runtime
                // String Database가 로드되기 전에는 Key를 해석할 수 없어 Edit Mode 조회가
                // 불필요하게 데이터베이스 초기화 상태에 종속됩니다.
                const long EntryId = 0;
                if (tableEntryIdProp.longValue != EntryId)
                {
                    tableEntryIdProp.longValue = EntryId;
                }

                if (!tableCollection)
                {
                    EditorGUI.HelpBox(currentPosition, EditorLocaleText.Tr(
                        $"테이블 '{tableName}'이(가) 존재하지 않습니다.",
                        $"Table '{tableName}' does not exist."), MessageType.Error);
                    currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    // 테이블 생성 버튼 제공
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        if (GUI.Button(new(currentPosition.x, currentPosition.y, currentPosition.width, EditorGUIUtility.singleLineHeight), EditorLocaleText.Tr("테이블 생성", "Create Table")))
                        {
                            PromptCreateStringTable(tableName);
                        }
                        currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                else if (sharedTableEntry is null)
                {
                    EditorGUI.HelpBox(currentPosition, EditorLocaleText.Tr(
                        $"엔트리 '{entryKey}'이(가) 테이블 '{tableName}'에 존재하지 않습니다.",
                        $"Entry '{entryKey}' does not exist in table '{tableName}'."), MessageType.Error);
                    currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                else
                {
                    // 로케일 목록 가져오기
                    List<Locale> locales = GetLocales();

                    if (string.IsNullOrEmpty(attr.EntryKey))
                    {
                        SharedTableData tableData = tableCollection.SharedData;

                        float halfCurrentPositionWidth = currentPosition.width * 0.5f;
                        Rect keyTextRect = new(currentPosition.x, currentPosition.y, halfCurrentPositionWidth - EditorGUIUtility.standardVerticalSpacing, currentPosition.height);
                        Rect deleteButtonRect = new(currentPosition.x + halfCurrentPositionWidth, currentPosition.y, halfCurrentPositionWidth, currentPosition.height);
                        string key = EditorGUI.TextField(keyTextRect, "Entry Key", sharedTableEntry.Key);
                        if (key != sharedTableEntry.Key && !string.IsNullOrEmpty(key) && !tableData.Entries.Any(entry => entry != sharedTableEntry && entry.Key == key))
                        {
                            tableData.RenameKey(sharedTableEntry.Key, key);
                            tableEntryKeyProp.stringValue = key;
                            EditorUtility.SetDirty(tableData);
                        }

                        if (GUI.Button(deleteButtonRect, "Delete"))
                        {
                            tableData.RemoveKey(sharedTableEntry.Key);
                            tableEntryKeyProp.stringValue = "";
                            EditorUtility.SetDirty(tableData);
                        }

                        currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }

                    foreach (Locale locale in locales)
                    {
                        // 해당 로케일의 테이블 가져오기
                        StringTable stringTable = tableCollection.GetTable(locale.Identifier) as StringTable;

                        if (stringTable)
                        {
                            // 엔트리 가져오기
                            // 엔트리가 없을 경우 생성
                            StringTableEntry entry = stringTable.GetEntry(sharedTableEntry.Id) ?? stringTable.AddEntry(sharedTableEntry.Id, "");

                            EditorGUI.LabelField(currentPosition, locale.LocaleName);
                            currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            // 번역 값 표시 및 편집
                            Rect optionRect = new(currentPosition.x, currentPosition.y, currentPosition.width, EditorGUIUtility.singleLineHeight * 5 + EditorGUIUtility.standardVerticalSpacing * 2);
                            EditorGUI.DrawRect(optionRect, ColorUtility.TryParseHtmlString("#413a4f", out Color color) ? color : Color.black);

                            bool isSmart = EditorGUI.Toggle(currentPosition, "Is Smart", entry.IsSmart);
                            currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                            string newValue = EditorGUI.TextArea(
                                new(currentPosition.x, currentPosition.y, currentPosition.width, EditorGUIUtility.singleLineHeight * 4),
                                entry.Value);
                            currentPosition.y += EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing;

                            if (isSmart != entry.IsSmart)
                            {
                                entry.IsSmart = isSmart;
                                EditorUtility.SetDirty(stringTable);
                                EditorUtility.SetDirty(tableCollection);
                            }

                            // 값이 변경되었을 경우 업데이트
                            if (newValue != entry.Value)
                            {
                                entry.Value = newValue;
                                EditorUtility.SetDirty(stringTable);
                                EditorUtility.SetDirty(tableCollection);
                            }
                        }
                        else
                        {
                            // 해당 로케일의 테이블이 없을 경우
                            currentPosition.height = EditorGUIUtility.singleLineHeight;
                            EditorGUI.LabelField(currentPosition, locale.LocaleName, EditorLocaleText.Tr(
                                "해당 로케일의 테이블이 존재하지 않습니다.",
                                "No table exists for this locale."));
                        }
                    }
                }

                if (tableCollection && string.IsNullOrEmpty(attr.EntryKey))
                {
                    Rect buttonRect = new(currentPosition.x, currentPosition.y, currentPosition.width, EditorGUIUtility.singleLineHeight);
                    // 엔트리 생성 버튼 제공
                    if (GUI.Button(buttonRect, EditorLocaleText.Tr("엔트리 선택", "Select Entry")))
                    {
                        _entryDropdown = new(new(), GetTableCollection(tableName), tableCollection.SharedData)
                        {
                            TargetProp = tableEntryKeyProp
                        };
                        _entryDropdown.Show(buttonRect);
                    }

                    currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                SyncLocalizedStringReference(property, tableNameProp, tableEntryKeyProp, originalTableName, originalEntryKey);
            }

            // 인덴트 복원
            EditorGUI.indentLevel = indent;

            // 프로퍼티 종료
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = 0f;

            // 기본 레이블 높이
            totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                LocalizedStringAttribute attr = (LocalizedStringAttribute)attribute;

                string tableName = attr.TableName ?? property.FindPropertyRelative("m_TableReference").FindPropertyRelative("m_TableCollectionName").stringValue;
                string entryKey = attr.EntryKey ?? property.FindPropertyRelative("m_TableEntryReference").FindPropertyRelative("m_Key").stringValue;

                StringTableCollection tableCollection = GetTableCollection(tableName);
                SharedTableData.SharedTableEntry sharedTableEntry = null;

                if (attr.CanSelectTable)
                {
                    totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                if (tableCollection)
                {
                    sharedTableEntry = GetSharedTableEntry(tableCollection, entryKey);
                }

                if (!tableCollection)
                {
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(attr.EntryKey))
                    {
                        totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }

                    if (sharedTableEntry is not null)
                    {
                        // 로케일 목록 가져오기
                        List<Locale> locales = GetLocales();
                        totalHeight += locales.Sum(_ => EditorGUIUtility.singleLineHeight * 6 + EditorGUIUtility.standardVerticalSpacing * 3);
                    }
                }

                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return totalHeight;
        }

        private static void SyncLocalizedStringReference(
            SerializedProperty property,
            SerializedProperty tableNameProp,
            SerializedProperty tableEntryKeyProp,
            string originalTableName,
            string originalEntryKey)
        {
            // TableReference/TableEntryReference의 ReferenceType은 [SerializeField]가 아니라
            // ISerializationCallbackReceiver.OnAfterDeserialize()에서 m_TableCollectionName/
            // m_Key로부터 파생되는 값이라, 위에서 raw SerializedProperty만 바꿔서는 반영이
            // 보장되지 않습니다(ReferenceType이 Empty로 남아 런타임 조회가 항상 빈 값을 반환하는
            // 원인). tableEntryKeyProp의 현재 값(초기 선택·Rename·Delete·엔트리 선택 드롭다운
            // 전부 이 필드로 수렴합니다)을 최종 값으로 사용해 ApplyModifiedProperties로 raw
            // 값을 반영한 뒤 boxedValue로 실제 LocalizedString을 가져와 C# 프로퍼티 대입(암시적
            // 변환 연산자)으로 ReferenceType까지 확실히 맞춥니다.
            string currentEntryKey = tableEntryKeyProp.stringValue;
            bool referenceChanged = tableNameProp.stringValue != originalTableName || currentEntryKey != originalEntryKey;

            LocalizedString boxedLocalizedString = (LocalizedString)property.boxedValue;
            bool requiresReferenceRepair = !string.IsNullOrEmpty(currentEntryKey) && boxedLocalizedString.IsEmpty;
            if (referenceChanged || requiresReferenceRepair)
            {
                boxedLocalizedString.TableReference = tableNameProp.stringValue;

                if (string.IsNullOrEmpty(currentEntryKey))
                {
                    boxedLocalizedString.TableEntryReference = default(TableEntryReference);
                }
                else
                {
                    boxedLocalizedString.TableEntryReference = currentEntryKey;
                }

                property.boxedValue = boxedLocalizedString;
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private static void PromptCreateStringTable(string tableName, Action onCreated = null)
        {
            // 테이블 생성 로직
            string folderPath = EditorUtility.OpenFolderPanel(EditorLocaleText.Tr(
                "테이블을 생성할 폴더를 선택하세요.",
                "Select a folder to create the table in."), "Assets", "") + $"/{tableName}/";
            if (!string.IsNullOrEmpty(folderPath))
            {
                folderPath = FileUtil.GetProjectRelativePath(folderPath);
                if (string.IsNullOrEmpty(folderPath))
                {
                    Debug.LogError(EditorLocaleText.Tr(
                        "선택한 폴더가 프로젝트 내에 없습니다.",
                        "The selected folder is not inside the project."));
                }
                else
                {
                    CreateStringTable(tableName, folderPath, onCreated);
                }
            }
        }

        private static StringTableCollection GetTableCollection(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) return null;

            if (TableCache.TryGetValue(tableName, out StringTableCollection tableCollection))
            {
                return tableCollection;
            }

            tableCollection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (tableCollection)
            {
                TableCache[tableName] = tableCollection;
            }

            return tableCollection;
        }

        private static SharedTableData.SharedTableEntry GetSharedTableEntry(StringTableCollection tableCollection, string entryKey)
        {
            if (!tableCollection || string.IsNullOrEmpty(entryKey)) return null;

            // 캐시를 사용하지 않고 직접 엔트리 가져오기
            SharedTableData sharedTableData = tableCollection.SharedData;
            return sharedTableData.GetEntry(entryKey);
        }

        private static List<Locale> GetLocales()
        {
            _localesCache ??= new(LocalizationEditorSettings.GetLocales());
            return _localesCache;
        }

        private static void CreateStringTable(string tableName, string folderPath, Action onCreated = null)
        {
            // EditorApplication.delayCall을 사용하여 다음 에디터 사이클로 작업을 미룸
            EditorApplication.delayCall += () =>
            {
                // 테이블 컬렉션 생성
                StringTableCollection collection = LocalizationEditorSettings.CreateStringTableCollection(tableName, folderPath);

                // 에셋 데이터베이스 저장 및 갱신
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 캐시 업데이트
                TableCache[tableName] = collection;

                Debug.Log(EditorLocaleText.Tr(
                    $"테이블 '{tableName}'이(가) '{folderPath}'에 생성되었습니다.",
                    $"Table '{tableName}' was created at '{folderPath}'."));

                onCreated?.Invoke();
            };
        }
    }
}
#endif
