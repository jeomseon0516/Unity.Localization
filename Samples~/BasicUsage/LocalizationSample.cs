using Jeomseon.LocalizationExtensions;
using UnityEngine;
using UnityEngine.Localization;

namespace Jeomseon.Samples.Localization
{
    public sealed class LocalizationSample : MonoBehaviour
    {
        [SerializeField] private LocalizedString _message;

        private void OnEnable()
        {
            _message.StringChanged += OnStringChanged;
        }

        private void OnDisable()
        {
            _message.StringChanged -= OnStringChanged;
        }

        private static void OnStringChanged(string value)
        {
            Debug.Log($"현지화 문자열: {value}");
        }

        [ContextMenu("Entry Key 확인")]
        private void PrintEntryKeyName()
        {
            Debug.Log($"Entry Key: {_message.GetEntryKeyName()}");
        }
    }
}
