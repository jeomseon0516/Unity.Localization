using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;
using Jeomseon.Events;

namespace Jeomseon.LocalizationExtensions
{
    /// <summary>
    /// 현재 사용하지 않습니다.
    /// </summary>
    [System.Serializable]
    public sealed class LocalizedStringOption
    {
        public string EntryKeyName => _localizedString.GetEntryKeyName();

        [SerializeField]
        private LocalizedString _localizedString;

        /* TODO(P1-01, api): Unity Localization의 LocalizeStringEvent로 직접 대체할 수 있는 동작과
         * 이 래퍼에서만 제공해야 하는 추가 기능을 구분한 뒤 유지 여부를 결정합니다.
         */
        public bool TrySetOption(MonoBehaviour monoBehaviour, UnityAction<string> onChangedText, out LocalizeStringEvent localizeStringEvent)
        {
            if (string.IsNullOrEmpty(EntryKeyName) || !monoBehaviour) // .. 모노비하이비어가 없다면..
            {
                localizeStringEvent = null;
                return false;
            }

            if (!monoBehaviour.TryGetComponent(out localizeStringEvent)) // .. 
            {
                localizeStringEvent = getLocalizeStringEvent(monoBehaviour.gameObject, _localizedString);
            }
            else
            {
                if (localizeStringEvent.StringReference != _localizedString)
                {
                    localizeStringEvent = getLocalizeStringEvent(monoBehaviour.gameObject, _localizedString);
                }
            }

            localizeStringEvent.OnUpdateString.AddListener(onChangedText);
            localizeStringEvent.OnUpdateString.SetPersistentListenerState(UnityEventCallState.EditorAndRuntime);
            localizeStringEvent.RefreshString();

            return true;

            static LocalizeStringEvent getLocalizeStringEvent(GameObject go, LocalizedString localizedString)
            {
                LocalizeStringEvent localizeStringEvent = go.AddComponent<LocalizeStringEvent>();
                localizeStringEvent.StringReference = localizedString;

                return localizeStringEvent;
            }
        }

        public LocalizedStringOption(TableReference tableReference, TableEntryReference entryReference)
        {
            _localizedString = new(tableReference, entryReference);
        }
    }
}
