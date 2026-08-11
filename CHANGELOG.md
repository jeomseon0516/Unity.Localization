# 변경 기록

## [Unreleased]

## [0.2.2] - 2026-07-29

- Runtime·Editor·Samples 어셈블리의 `rootNamespace`와 소스 파일 위치를 namespace에 맞게 정리했습니다.

## [0.2.1] - 2026-07-29

- LocalizedString 변경을 확인하는 `Basic Usage` 샘플을 추가했습니다.

## [0.2.0] - 2026-07-29

- LocalizedStringAttribute와 전용 PropertyDrawer를 Localization 패키지로 이동했습니다.
- Localization 전용 타입의 namespace를 `Jeomseon.Localization`으로 정리했습니다.

## [0.3.0] - 2026-08-11

- **(Breaking)** 워크스페이스 전체에 소비처가 없던 `LocalizedStringOption`/`ILocalizable`을
  제거했습니다. `LocalizeStringEvent`로 완전히 대체되는 미사용 wrapper였습니다.
- **(Breaking)** `LocalizationExtensions.GetLocalizedStringByLocale(string)`을 제거하고
  `GetLocalizedStringByLocaleAsync(string)`(`Awaitable<string>`)로 교체했습니다. 기존 API는
  WebGL에서 지원되지 않고 메인 스레드를 블로킹하는 `WaitForCompletion`을 사용했습니다.
- **(Breaking)** `LocalizationExtensions.TryGetLocalizedString(this StringTable, string)`이
  이름과 다르게 `bool`을 반환하지 않고 존재하지 않는 entry에서 `NullReferenceException`을
  던지던 결함을 `bool` + `out string` Try 패턴으로 수정했습니다.
- Editor 전용 `LocalizedStringDrawer`의 정적 로케일 이벤트 구독과 Sample의 listener 수명을
  검토했습니다. 별도 수정이 필요한 누수는 없었습니다.
- `Basic Usage` Sample에 `LocalizationSample` 컴포넌트가 이미 부착된 `LocalizationSample.unity`
  Scene을 추가했습니다. 기존에는 Scene 자산 없이 README로 GameObject 생성·컴포넌트 부착을
  안내만 하고 있어 `AGENTS.md`의 샘플 정책(Scene 자산 필수)을 충족하지 못했습니다.
- 워크스페이스 명명 규칙에 맞춰 `LocalizationSample`의 `[SerializeField] private` 필드를
  `_camelCase`에서 `camelCase`로 정리하고(`[FormerlySerializedAs]`로 기존 이름 보존),
  `EntryAdvancedDropdown`의 `SCREAMING_SNAKE_CASE` 로컬 상수를 `PascalCase`로 정리했습니다.
  공개 API 변경은 없습니다.

## [0.1.0] - 2026-07-29

- JeomseonScriptPack의 관련 모듈을 독립 UPM 패키지로 분리했습니다.


## [0.2.3] - 2026-08-05

- Unity 6000.5.7f1을 최소 지원 버전으로 상향했습니다.
