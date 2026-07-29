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

        [ContextMenu("Localization 확장 타입 확인")]
        private void PrintExtensionType()
        {
            Debug.Log(typeof(LocalizedStringOption).FullName);
        }
    }
}
