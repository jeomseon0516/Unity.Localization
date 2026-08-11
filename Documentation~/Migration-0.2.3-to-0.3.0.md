# Migration: 0.2.3 → 0.3.0

## `LocalizedStringOption`/`ILocalizable` 제거

워크스페이스 전체에 소비처가 없던 미사용 wrapper를 제거했습니다. `LocalizedStringOption.TrySetOption`이
하던 일(대상 GameObject에 `LocalizeStringEvent`를 붙이거나 재사용하고, `StringReference`를 설정한 뒤
`OnUpdateString`에 콜백을 구독)은 Unity의 `LocalizeStringEvent` 컴포넌트를 직접 사용하면 동일하게
처리됩니다.

```csharp
// 0.2.3
public sealed class Label : MonoBehaviour
{
    [SerializeField] private LocalizedStringOption _option;

    private void OnEnable()
    {
        _option.TrySetOption(this, text => _text.text = text, out _);
    }
}

// 0.3.0
public sealed class Label : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent _localizeStringEvent;

    private void OnEnable()
    {
        _localizeStringEvent.OnUpdateString.AddListener(text => _text.text = text);
        _localizeStringEvent.RefreshString();
    }
}
```

`ILocalizable.StringOption`을 구현하던 타입이 있었다면 `LocalizedString` 필드를 직접 노출하거나,
Inspector에서 `LocalizeStringEvent`를 조합하는 방식으로 대체하세요.

## `GetLocalizedStringByLocale(string)` → `GetLocalizedStringByLocaleAsync(string)`

기존 API는 `LocalizationSettings.InitializationOperation.WaitForCompletion()`을 사용해 WebGL에서
지원되지 않고 메인 스레드를 블로킹했습니다. `Awaitable<string>`을 반환하는 비동기 버전으로
교체하세요.

```csharp
// 0.2.3
string text = localizedString.GetLocalizedStringByLocale("ko");

// 0.3.0
string text = await localizedString.GetLocalizedStringByLocaleAsync("ko");
```

호출부가 `async`로 전환하기 어려운 동기 컨텍스트(예: 기존 동기 콜백)에 있다면, 초기화가 이미 끝난
상태에서만 호출하도록 보장하거나 별도의 캐싱 전략을 고려하세요. `WaitForCompletion` 기반 동기 대기는
더 이상 이 API에서 제공하지 않습니다.

## `TryGetLocalizedString(this StringTable, string)` 반환 타입 변경

이름과 다르게 `string`을 반환하며 존재하지 않는 entry에서 `NullReferenceException`을 던지던 결함을
수정해, `bool`을 반환하고 결과를 `out string`으로 전달하는 표준 Try 패턴으로 바꿨습니다.

```csharp
// 0.2.3 (결함 있음 — 존재하지 않는 entry에서 NullReferenceException)
string text = table.TryGetLocalizedString("greeting");

// 0.3.0
if (table.TryGetLocalizedString("greeting", out string text))
{
    // text 사용
}
```
