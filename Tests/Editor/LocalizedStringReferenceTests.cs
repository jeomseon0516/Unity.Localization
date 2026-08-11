using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jeomseon.Localization.Extensions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using LocalizationExtensionMethods = Jeomseon.Localization.Extensions.LocalizationExtensions;

namespace Jeomseon.Tests.Localization
{
    public sealed class LocalizedStringReferenceTests
    {
        [Serializable]
        private sealed class MemoryLocalesProvider : ILocalesProvider
        {
            public List<Locale> Locales { get; } = new();

            public Locale GetLocale(LocaleIdentifier id)
            {
                return Locales.Find(locale => locale.Identifier == id);
            }

            public void AddLocale(Locale locale)
            {
                Locales.Add(locale);
            }

            public bool RemoveLocale(Locale locale)
            {
                return Locales.Remove(locale);
            }
        }

        [Serializable]
        private sealed class MemoryStringTableProvider : ITableProvider, IDisposable
        {
            private readonly List<UnityEngine.Object> _createdObjects = new();

            public AsyncOperationHandle<TTable> ProvideTableAsync<TTable>(string tableCollectionName, Locale locale)
                where TTable : LocalizationTable
            {
                if (typeof(TTable) != typeof(StringTable) || tableCollectionName != TableName)
                {
                    return default;
                }

                SharedTableData sharedData = ScriptableObject.CreateInstance<SharedTableData>();
                sharedData.TableCollectionName = TableName;

                StringTable table = ScriptableObject.CreateInstance<StringTable>();
                table.SharedData = sharedData;
                table.LocaleIdentifier = locale.Identifier;
                table.AddEntry(EntryKey, LocalizedText);

                _createdObjects.Add(table);
                _createdObjects.Add(sharedData);
                return Addressables.ResourceManager.CreateCompletedOperation(table as TTable, null);
            }

            public void Dispose()
            {
                foreach (UnityEngine.Object createdObject in _createdObjects)
                {
                    UnityEngine.Object.DestroyImmediate(createdObject);
                }

                _createdObjects.Clear();
            }
        }

        private sealed class LocalizedStringHolder : ScriptableObject
        {
            [SerializeField] private LocalizedString message = new();
        }

        private const string TableName = "MemoryTable";
        private const string EntryKey = "Greeting";
        private const string LocaleCode = "en";
        private const string LocalizedText = "Hello from memory";

        private Locale _locale;
        private MemoryLocalesProvider _localesProvider;
        private LocalizedStringDatabase _stringDatabase;
        private MemoryStringTableProvider _tableProvider;

        [SetUp]
        public void SetUp()
        {
            _locale = Locale.CreateLocale(LocaleCode);
            _localesProvider = new();
            _localesProvider.AddLocale(_locale);

            _tableProvider = new();
            _stringDatabase = new()
            {
                TableProvider = _tableProvider
            };
        }

        [TearDown]
        public void TearDown()
        {
            (_stringDatabase as IDisposable)?.Dispose();
            _tableProvider?.Dispose();
            UnityEngine.Object.DestroyImmediate(_locale);
        }

        [Test]
        public void SerializedProperty_BoxedValueRequiresApplyToPersistReference()
        {
            LocalizedStringHolder holder = ScriptableObject.CreateInstance<LocalizedStringHolder>();
            try
            {
                SerializedObject serializedObject = new(holder);
                SerializedProperty property = serializedObject.FindProperty("message");
                LocalizedString localizedString = (LocalizedString)property.boxedValue;

                localizedString.SetReference(TableName, EntryKey);
                property.boxedValue = localizedString;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                LocalizedString persisted = (LocalizedString)property.boxedValue;
                Assert.That(persisted.IsEmpty, Is.False);
                Assert.That(persisted.TableReference.ReferenceType, Is.EqualTo(TableReference.Type.Name));
                Assert.That(persisted.TableEntryReference.ReferenceType, Is.EqualTo(TableEntryReference.Type.Name));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void GetEntryKeyName_ReturnsConfiguredEntryKey()
        {
            LocalizedString localizedString = new(TableName, EntryKey);

            Assert.That(localizedString.GetEntryKeyName(), Is.EqualTo(EntryKey));
        }

        [Test]
        public void TryGetLocalizedString_ReturnsSelectedLocaleText()
        {
            LocalizedString localizedString = new(TableName, EntryKey);

            bool succeeded = LocalizationExtensionMethods.TryGetLocalizedString(
                localizedString,
                _stringDatabase,
                _locale,
                out string localizedText);

            Assert.That(succeeded, Is.True);
            Assert.That(localizedText, Is.EqualTo(LocalizedText));
        }

        [Test]
        public async Task GetLocalizedStringByLocaleAsync_ReturnsRequestedLocaleText()
        {
            LocalizedString localizedString = new(TableName, EntryKey);

            string localizedText = await LocalizationExtensionMethods.GetLocalizedStringByLocaleAsync(
                localizedString,
                _stringDatabase,
                _localesProvider,
                LocaleCode);

            Assert.That(localizedText, Is.EqualTo(LocalizedText));
        }
    }
}
