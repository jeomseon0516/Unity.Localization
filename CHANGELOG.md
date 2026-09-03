# 변경 기록

## [0.5.1] - 2026-09-03

- Unity 최소 버전을 `6000.5.7f1` → `6000.6.0f1`로 상향했습니다. 코드·API 변경은 없습니다.

## [0.5.0] - 2026-08-13

- **(Breaking)** Runtime/Editor 네임스페이스를 패키지 및 경로 규칙에 맞춰
  `Jeomseon.Unity.Localization[.Extensions]`와
  `Jeomseon.Unity.Localization.Editor[.Extensions]`로 변경하고 중복된 `Localization/Editor`
  폴더 계층을 평탄화했습니다. 이전 네임스페이스 호환 별칭은 제공하지 않습니다.

## [0.4.0] - 2026-08-11

- **(중요)** `LocalizationSampleTableSetup.EnsureLocalizationSettings`가
  `LocalizationEditorSettings.ActiveLocalizationSettings is not null`(참조 비교)로 기존 Settings
  존재 여부를 판단해, 참조된 자산이 삭제된 "fake-null"(Unity가 파괴된 오브젝트를 진짜 `null`이
  아니라 겉보기엔 non-null인 참조로 유지하는 경우) 상태에서도 이미 존재한다고 판단해 새 Settings를
  만들지 않고 건너뛰던 결함을 수정했습니다. UPM Sample 폴더가 `0.2.3` → `0.3.1`로 재임포트되며 그
  안에 있던 `Localization Settings.asset`이 함께 삭제됐는데 `EditorBuildSettings`의 config object
  참조는 남아 있었던 것이 실제 재현 사례였고, 그 결과 Play Mode 진입 시
  `LocalizedString.StringChanged` 구독(`LocalizationSample.OnEnable`)에서 "There is no active
  LocalizationSettings" 예외가 발생했습니다. `is not null`/`is null`을 `UnityEngine.Object`의
  암시적 bool 변환(`if (obj)`/`if (!obj)`)으로 교체했습니다. TestProject의 실제
  `ProjectSettings/EditorBuildSettings.asset`에 `com.unity.localization.settings` config object가
  전혀 등록돼 있지 않은 것으로 재현을 확인했습니다.
- **(중요, 문서화)** `TryGetLocalizedString`/`GetLocalizedStringByLocaleAsync`가 Edit Mode
  ContextMenu 테스트에서 간헐적으로 빈 값을 반환하는 원인을 `Unity.Localization.dll`을
  디컴파일해 확인했습니다. 기본 `LocalesProvider.Locales` getter는
  `PlaymodeState.IsPlayingOrWillChangePlaymode`가 `true`일 때만 Locale Addressables Preload를
  트리거합니다(`AsyncOperationUtility.SynchronousLoad`가 내부적으로 `WaitForCompletion`을 쓰는
  것과 별개로, Preload 자체가 Edit Mode에서는 시작되지 않음). 즉 Edit Mode에서는 참조
  (`TableReference`/`TableEntryReference`)가 완전히 정상이어도 `LocalizationSettings.AvailableLocales`가
  비어 있을 수 있어 두 메서드가 실패할 수 있습니다. 이는 이 패키지의 결함이 아니라 Unity
  Localization 자체의 설계이며, `GetEntryKeyName`이 안정적인 이유는 Name 참조를
  `LocalizationSettings` 없이 즉시 반환하기 때문입니다. `Basic Usage` Sample README와
  ContextMenu 실패 메시지에 Play Mode 요구사항을 명시했습니다.
- `Basic Usage` Sample에 `진단 정보 확인`에 `Application.isPlaying`과 로드된
  `AvailableLocales` 개수를 추가하고, `TryGetLocalizedString`/`GetLocalizedStringByLocaleAsync`
  확인 메서드가 Edit Mode에서 실패하면 위 원인을 명시하는 메시지를 출력하도록 고쳤습니다.
- `Basic Usage` Sample의 `Jeomseon.Unity.Localization.Samples.BasicUsage.asmdef`에
  `Unity.ResourceManager` 참조가 빠져 있어 `LocalizationSample.cs`가 `AsyncOperationHandle<>`을
  참조하는 순간(신규 진단 ContextMenu) `CS0012`로 컴파일이 깨지던 결함을 수정했습니다. Runtime
  asmdef는 이미 이 참조를 갖고 있었지만 Unity asmdef 참조는 전이되지 않습니다.
- `Basic Usage` Sample에 `진단 정보 확인` `ContextMenu`를 추가했습니다. `message.IsEmpty`,
  `TableReference`/`TableEntryReference`의 `ReferenceType`·`Key`·`KeyId`,
  `LocalizationSettings.HasSettings`/`SelectedLocale`/초기화 상태를 한 번에 로그로 출력해, 세
  조회 메서드 중 어디가 왜 실패하는지 재현 없이 바로 확인할 수 있습니다. `TryGetLocalizedString`
  확인 메서드의 실패 로그도 항상 "Entry 미지정"으로 단정하던 것을 `IsEmpty`/`HasSettings`/기타로
  원인을 구분해 출력하도록 고쳤습니다.
- Claude 세션에서 Codex 세션 산출물을 재검토해 발견한 결함을 수정했습니다.
  - `LocalizedStringExtensionsForEditor.MonitorSpecificLocaleEntry`(공개 API)가 항상 빈 값을
    보고하던 결함을 수정했습니다. Drawer가 Entry를 이름 참조로 저장하도록 바뀌면서
    `TableEntryReference.KeyId`가 항상 `0`이 되는데, 이 메서드는 여전히 `KeyId`로만 조회해
    Inspector로 설정한 필드에서는 절대 값을 찾지 못했습니다. `ReferenceType`에 맞춰 이름/ID
    조회를 모두 지원하도록 고쳤습니다.
  - `LocalizedStringDrawer`의 참조 변경 감지를 개별 수정 지점마다 수동으로 플래그를 세우는
    방식에서, OnGUI 시작 시점 스냅샷과 종료 시점 값을 비교하는 방식으로 바꿨습니다. 기존
    방식은 Entry Key 인라인 rename·Delete 버튼 두 경로에서 플래그를 빠뜨려, boxedValue 기반
    `ReferenceType` 복구가 실행되지 않고 원래 버그와 같은 조건(raw 필드만 갱신)에 노출돼
    있었습니다.
  - `LocalizationExtensions.TryGetLocalizedString(this LocalizedString, out string)`이 조회
    실패 시 `out` 값으로 `null`을 반환할 수 있던 것을 항상 `string.Empty`를 보장하도록
    고쳤습니다(StringTable 오버로드와 계약 통일).
  - `Basic Usage` Sample의 `LocalizationSampleTableSetup.cs`에 `using UnityEngine.Localization.Settings;`가
    누락되어 있던 컴파일 오류를 수정했습니다. UPM Sample은 Import 시 스냅샷으로 복사되고
    `Samples~` 원본 수정이 자동 반영되지 않는 특성상, 이 오류는 마지막 재검증 이후 원본에만
    추가된 `EnsureLocalizationSettings` 코드에 들어 있어 실제 Unity 컴파일로 확인되지 않은
    상태였습니다.
  - `Runtime/LocalizationExtensions/`, `Editor/LocalizationExtensions/Editor/` 폴더를 각각
    `Runtime/Localization/Extensions/`, `Editor/Localization/Extensions/Editor/`로 옮겨
    namespace 변경(`Jeomseon.Localization.Extensions[.Editor]`) 이후에도 남아 있던 폴더 경로·
    namespace 불일치를 정리했습니다(AGENTS.md 코드 구조 규칙).
  - `LocalizedStringDrawer`·`EntryAdvancedDropdown`·`LocalizedStringExtensionsForEditor`의
    `private`/`private static` 메서드(`onLocaleChanged`, `getTableCollection`,
    `getSharedTableEntry`, `getLocales`, `createStringTable`, `createNewEntry`,
    `iEMonitorEntryCoroutine`)를 워크스페이스 명명 규칙(메서드 PascalCase)에 맞춰 정리했습니다.
    `LocalizedStringDrawer`의 로컬 `const long entryId`도 `EntryId`로 정리했습니다.
  - AGENTS.md 명명 규칙에 "`static readonly` 필드는 접근 제한자와 무관하게 PascalCase" 규칙을
    추가하고, `LocalizedStringDrawer._tableCache`를 `TableCache`로 정리했습니다
    (`LocalizedStringExtensionsForEditor.LocaleCache`, `LocalizationSampleTableSetup.SampleLanguages`는
    이미 이 규칙을 따르고 있었습니다).
- **(Breaking)** 확장 메서드 namespace를 `Jeomseon.LocalizationExtensions`에서 패키지의 기본
  도메인 계층과 일치하는 `Jeomseon.Localization.Extensions`로 변경했습니다. Editor 전용 확장도
  `Jeomseon.Localization.Extensions.Editor`로 이동했으며 기존 namespace 호환 alias는 제공하지
  않습니다.
- **(중요)** `LocalizedStringDrawer`가 `LocalizedString.TableReference`/`TableEntryReference`를
  raw `SerializedProperty`(`m_TableCollectionName`, `m_KeyId`)로만 채우고 `ReferenceType`을
  갱신하지 않아, Inspector에는 값이 채워진 것처럼 보여도 실제 `LocalizedString`은 항상 빈
  참조(`ReferenceType.Empty`)로 저장되어 `GetLocalizedString`/`GetEntryKeyName`/
  `TryGetLocalizedString` 등 모든 런타임 조회가 값이 실제로 존재해도 항상 빈 문자열을 반환하던
  결함을 수정했습니다. `TableReference`/`TableEntryReference`의 `ReferenceType`은
  `[SerializeField]`가 아니라 `ISerializationCallbackReceiver.OnAfterDeserialize()`에서
  파생되는 값이라 raw 필드만 바꿔서는 반영되지 않는다는 것을 리플렉션으로 확인했고,
  `SerializedProperty.boxedValue`로 실제 `LocalizedString`을 가져와 C# 프로퍼티 대입(암시적
  변환 연산자)으로 `ReferenceType`까지 맞추도록 수정했습니다.
- `Basic Usage` Sample의 `LocalizationSample.message` 필드에 `[LocalizedString]` attribute가
  누락되어 있어 이 패키지의 `LocalizedStringDrawer`(테이블/엔트리 생성 UI) 대신 Unity 기본
  LocalizedString Drawer가 그려지고, 프로젝트에 String Table Collection이 없으면 Inspector에서
  설정할 방법이 없어 출력이 항상 비어 있던 결함을 수정했습니다.
- `Basic Usage` Sample에 `TryGetLocalizedString`, `GetLocalizedStringByLocaleAsync` 예제
  `ContextMenu`를 추가했습니다. 기존에는 `GetEntryKeyName` 예제만 있어 나머지 2개 확장 메서드를
  Scene에서 확인할 방법이 없었습니다.
- `LocalizedStringDrawer`·`EntryAdvancedDropdown`의 HelpBox·버튼·로그 텍스트가 한국어로
  고정되어 있던 것을 `EditorLocaleText`로 정리했습니다. 기본값은 OS 언어
  (`Application.systemLanguage`)를 따르고, `Preferences > Jeomseon > Localization`
  (`EditorLocaleSettingsProvider`)에서 한국어/영어로 override할 수 있습니다. 지원하지 않는
  언어는 영어로 fallback됩니다. `UnityEditor.LocalizationDatabase`/`L10n`은 Unity 내장 UI
  문자열 전용 internal API라 사용할 수 없어 자체 구현했습니다.
- `Basic Usage` Sample에 Editor 전용 설정 스크립트(`LocalizationSampleTableSetup`,
  `Jeomseon/Tool/Localization/Setup Basic Usage Sample`)를 추가했습니다. `LocalizationSample`
  테이블과 `Greeting` Entry를 자동 생성해, `message` 필드 설정을 위해 매번 Inspector에서 수동으로
  테이블을 만들 필요가 없습니다. 프로젝트에 Locale이 하나도 없는 완전히 새 프로젝트라면 예시로
  `English`/`Korean` Locale까지 함께 생성합니다(기존 Locale이 있는 프로젝트는 건드리지 않습니다).
  활성 `LocalizationSettings`가 없으면 Settings 자산도 생성·등록해, Edit Mode와 Runtime 조회가
  실제 String Database 및 Locale provider를 사용하도록 보장합니다.
- `LocalizedStringDrawer`가 `boxedValue`에 설정한 참조를 실제 Scene/Prefab 객체에 반영하도록 참조가
  변경되거나 과거 데이터를 복구할 때만 `ApplyModifiedProperties()`를 호출합니다. 호출이 없으면
  Inspector 메모리에는 보이지만 Scene YAML에 `message`가 기록되지 않는 회귀를 TestProject에서
  확인했습니다.
- Drawer가 Entry를 ID가 아닌 이름으로 저장하도록 변경했습니다. 이름을 이미 알고 있는 Editor 경로에서
  ID를 저장하면 Runtime String Database가 로드되기 전 `GetEntryKeyName`이 Key를 해석할 수 없습니다.
  `GetEntryKeyName`은 이름 참조를 즉시 반환하고, 외부 소비자의 기존 ID 참조는 데이터베이스 조회
  fallback을 유지합니다.
- `GetLocalizedStringByLocaleAsync`는 요청 Locale을 찾지 못하거나 Unity Localization이 `null` 결과를
  반환할 때 공개 계약대로 `null` 대신 빈 문자열을 반환합니다.
- 활성 `LocalizationSettings`가 없을 때 `TryGetLocalizedString`이 이름과 달리 첫 호출에서 예외를
  던지던 문제를 수정했습니다. Settings 부재 시 `false`와 빈 문자열을 반환하며,
  `GetLocalizedStringByLocaleAsync`도 초기화 진입 전에 빈 문자열을 반환합니다.
- Editor 테스트를 실제 프로젝트의 Localization Settings, Locale, Sample Table 자산에 의존하지 않는
  메모리 테스트로 전환했습니다. 테스트 전용 `ILocalesProvider`/`ITableProvider`와 메모리
  `Locale`/`StringTable`을 사용하며 조회 핵심에는 database/provider를 직접 주입합니다. 정적
  `LocalizationSettings`나 AssetDatabase 파일을 생성·교체하지 않습니다.

## [0.3.1] - 2026-08-11

- `README.md` 설치 안내의 Git URL 태그를 `v0.2.1`에서 `v0.3.0`으로 갱신했습니다(누락됐던 부분).
- `0.3.0`의 Breaking 변경 3건(`LocalizedStringOption`/`ILocalizable` 제거,
  `GetLocalizedStringByLocale` → `GetLocalizedStringByLocaleAsync`,
  `TryGetLocalizedString` Try 패턴 전환)에 대한 마이그레이션 가이드를
  `Documentation~/Migration-0.2.3-to-0.3.0.md`로 추가했습니다.

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
