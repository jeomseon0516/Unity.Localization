#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;
using Jeomseon.Collections;
using Object = UnityEngine.Object;

namespace Jeomseon.Unity.Localization.Editor
{
    internal sealed class EntryAdvancedDropdown : AdvancedDropdown
    {
        private sealed class EntryRenamePopupWindow : PopupWindowContent
        {
            private readonly SharedTableData.SharedTableEntry _targetEntry;
            private readonly SharedTableData _tableData;
            private string _entryName;

            public override Vector2 GetWindowSize()
            {
                return new(300f, 200f);
            }

            public override VisualElement CreateGUI()
            {
                var root = new VisualElement();

                root.Add(new Label("Rename")
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        unityTextAlign = TextAnchor.MiddleCenter
                    }
                });

                var entryNameField = new TextField
                {
                    value = _entryName
                };
                entryNameField.RegisterValueChangedCallback(evt => _entryName = evt.newValue);
                root.Add(entryNameField);

                root.Add(new Button(() =>
                {
                    _tableData.RenameKey(_targetEntry.Id, _entryName);
                    editorWindow.Close();
                })
                {
                    text = "Modify"
                });

                return root;
            }

            public EntryRenamePopupWindow(SharedTableData tableData, SharedTableData.SharedTableEntry targetEntry)
            {
                _targetEntry = targetEntry;
                _tableData = tableData;
                _entryName = _targetEntry.Key;
            }
        }

        private sealed class EntryItem : AdvancedDropdownItem
        {
            public SharedTableData.SharedTableEntry Entry { get; }

            public EntryItem(string name, SharedTableData.SharedTableEntry entry) : base(name)
            {
                Entry = entry;
            }
        }

        public SerializedProperty TargetProp { get; set; } = null;

        public event Action<SharedTableData.SharedTableEntry> OnEntrySelected;
        public event Action<SharedTableData.SharedTableEntry> OnCreatedEntry;

        private SharedTableData _tableData;
        private StringTableCollection _tableCollection;

        public EntryAdvancedDropdown(AdvancedDropdownState state, StringTableCollection tableCollection, SharedTableData tableData) : base(state)
        {
            _tableCollection = tableCollection;
            _tableData = tableData;
            minimumSize = new(200, 300);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new("Entries");

            // 기존 엔트리 목록 추가
            List<SharedTableData.SharedTableEntry> entries = _tableData.Entries;
            entries.Select(entry => new EntryItem(entry.Key, entry)).ForEach(root.AddChild);
            // 구분선 추가
            root.AddSeparator();

            // 새로운 엔트리 생성 옵션 추가
            AdvancedDropdownItem createNewEntryItem = new("Create New Entry...");
            root.AddChild(createNewEntryItem);

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is EntryItem entryItem)
            {
                if (TargetProp is not null)
                {
                    TargetProp.stringValue = entryItem.Entry.Key;
                    TargetProp.serializedObject.ApplyModifiedProperties();
                }
                // 기존 엔트리 선택됨
                OnEntrySelected?.Invoke(entryItem.Entry);
            }
            else
            {
                CreateNewEntry();
            }
        }

        private void CreateNewEntry()
        {
            const string EntryKey = "NewEntryKey";
            int count = _tableData.Entries.Count(entry => entry.Key.StartsWith(EntryKey)) + 1;
            // 새로운 엔트리 키 입력 받기
            string key = $"{EntryKey}_{count}";

            // 엔트리 추가
            SharedTableData.SharedTableEntry newEntry = _tableData.AddKey(key);

            // 변경사항 저장
            EditorUtility.SetDirty(_tableData);

            foreach (StringTable table in _tableCollection.StringTables)
            {
                table.AddEntry(newEntry.Id, "");
                EditorUtility.SetDirty(table);
            }

            // 에셋 데이터베이스 저장
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(EditorLocaleText.Tr(
                $"엔트리 '{key}'가 테이블 '{_tableCollection.TableCollectionName}'에 생성되었습니다.",
                $"Entry '{key}' was created in table '{_tableCollection.TableCollectionName}'."));
            OnCreatedEntry?.Invoke(newEntry);

            if (TargetProp is not null)
            {
                TargetProp.stringValue = newEntry.Key;
                TargetProp.serializedObject.ApplyModifiedProperties();
            }

            // 기존 엔트리 선택됨
            OnEntrySelected?.Invoke(newEntry);
        }
    }
}
#endif
