using Jeomseon.LocalizationExtensions;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

namespace Jeomseon.Samples.Localization
{
    public sealed class LocalizationSample : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("_message")] private LocalizedString message;

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
    }
}
