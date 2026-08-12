# Migration: 0.3.1 → 0.4.0

## 확장 메서드 namespace 변경

`Jeomseon.LocalizationExtensions` → `Jeomseon.Localization.Extensions`(Runtime),
`Jeomseon.Localization.Extensions.Editor`(Editor)로 변경했습니다. 패키지의 기본 domain 계층
(`Jeomseon.Localization`)과 일치시키기 위한 정리이며, 호환 alias는 제공하지 않습니다.

```csharp
// 0.3.1
using Jeomseon.LocalizationExtensions;

// 0.4.0
using Jeomseon.Localization.Extensions;
```

Editor 전용 확장(`MonitorSpecificLocaleEntry` 등)을 사용하던 코드는 `Jeomseon.Localization.Extensions.Editor`로
`using`을 갱신하세요.

## 동작 수정 (API 시그니처 변경 없음)

다음 항목은 공개 API 시그니처는 그대로지만 실제 동작이 바뀌었으므로 함께 기록합니다.

- `LocalizedStringDrawer`로 설정한 `LocalizedString` 필드가 Inspector에는 값이 보여도 실제로는
  빈 참조(`ReferenceType.Empty`)로 저장되어 런타임 조회가 항상 빈 문자열을 반환하던 결함을
  수정했습니다. 기존에 이 결함을 겪은 필드는 Inspector에서 해당 필드를 한 번 펼치면 자동 복구됩니다.
- `TryGetLocalizedString(this LocalizedString, out string)`은 이제 실패 시 `out` 값으로 항상
  `string.Empty`를 반환합니다(과거에는 `null`일 수 있었습니다).
- `GetLocalizedStringByLocaleAsync`는 요청한 Locale을 찾지 못하거나 결과가 없을 때 `null` 대신
  빈 문자열을 반환합니다.
- Edit Mode에서 `TryGetLocalizedString`/`GetLocalizedStringByLocaleAsync`가 값을 찾지 못하는 것은
  Unity Localization의 `LocalesProvider`가 Play Mode 진입 시에만 Locale Preload를 트리거하는
  설계 때문입니다(이 패키지의 결함이 아닙니다). 두 메서드는 Play Mode에서 확인하세요.
