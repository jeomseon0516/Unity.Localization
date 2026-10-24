# Jeomseon Unity Localization

한국어 | [English](README.en.md)

Unity Localization의 LocalizedString과 String Table을 유지하면서 언어별 조회와 전용 Inspector 편집을 보완합니다. 번역 데이터 서비스 자체를 대체하지 않습니다.

## 빠르게 시작하기

Unity Localization의 Locale·Table·Entry 구성 → LocalizedStringAttribute 필드 연결 → TryGetLocalizedString 또는 GetLocalizedStringByLocaleAsync 호출. namespace는 Jeomseon.Unity.Localization.Extensions입니다.

공식 Localization Settings·Locale·String Table을 준비하고 Inspector에서 LocalizedString을 연결합니다. 다음 코드는 MonoBehaviour 내부의 선언·메서드 예이며, 조회는 Play Mode에서 확인합니다.

```csharp
using Jeomseon.Unity.Localization.Extensions;
using UnityEngine;
using UnityEngine.Localization;

[SerializeField, Jeomseon.Unity.Localization.LocalizedString]
private LocalizedString message;

private async Awaitable PrintEnglishAsync()
{
    string text = await message.GetLocalizedStringByLocaleAsync("en");
    Debug.Log(text);
}
```

[전체 실행 샘플](Samples~/BasicUsage/LocalizationSample.cs)

## 계약과 주의사항

비동기 언어별 조회는 초기화를 기다리고 빈 참조·없는 Locale에는 빈 문자열을 반환합니다. 취소 토큰 오버로드는 없습니다. Try 조회도 필요시 공식 StringDatabase의 동기 동작을 사용하므로 모든 호출이 비블로킹이라고 가정하지 마세요.

## 요구 사항

- Unity `6000.6.0f1`
- 패키지: `com.jeomseon.unity.localization` `0.5.2`
- `com.jeomseon.unity.core`: `0.3.2`
- `com.unity.editorcoroutines`: `1.0.0`
- `com.unity.localization`: `1.4.5`

## 설치

프로젝트 `Packages/manifest.json`의 기존 dependencies와 registry를 유지하면서 다음 항목을 병합합니다.

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.jeomseon"
      ]
    }
  ],
  "dependencies": {
    "com.jeomseon.unity.localization": "0.5.2"
  }
}
```

또는 Package Manager의 **Install package from git URL**을 사용합니다:

```text
https://github.com/jeomseon0516/Unity.Localization.git#v0.5.2
```

## 설계와 동작

### 공식 패키지와의 관계

Locale, String Table, Entry, LocalizedString과 로드는 Unity Localization이 담당합니다.
이 패키지는 LocalizedStringAttribute의 Inspector 편집과 조회 편의를 추가합니다.
번역 서비스, 번역 자동화, 별도 문자열 저장소를 만들지 않습니다.

| API | 의미 | 실패 시 |
| --- | --- | --- |
| `TryGetLocalizedString(LocalizedString, out string)` | 현재 Locale 문자열 조회 | 빈 참조/설정 없음은 false와 빈 문자열 |
| `TryGetLocalizedString(StringTable, string, out string)` | 준비된 Table에서 key 조회 | Entry 없음은 false와 빈 문자열 |
| `GetLocalizedStringByLocaleAsync` | 지정 언어 조회, 초기화 비동기 대기 | 빈 참조/설정 없음/Locale 없음은 빈 문자열 |
| `GetEntryKeyName` | 이름 또는 ID 참조에서 key 조회 | 빈 참조는 빈 문자열 |

위 Try API는 모든 잘못된 인자를 예외 없이 처리하는 범용 방어 함수가 아닙니다. null 객체를
전달하지 마세요. 또한 동기 StringDatabase 조회를 사용하므로 Try라는 이름이 비동기·비블로킹을
의미하지 않습니다. 비동기 언어별 조회에는 현재 취소 토큰 매개변수가 없습니다.

### 편집부터 실행까지

1. 공식 Localization Settings와 Locale을 준비합니다.
2. `[LocalizedString]` 필드에서 Table/Entry를 지정하거나 Drawer의 생성 기능을 사용합니다.
3. Play Mode에서 현재 언어 또는 지정 언어의 문자열을 조회합니다.
4. 게임 UI의 실시간 Locale 변경 구독은 공식 Localization 계약과 소비자 수명에 맞춰 관리합니다.

타입명 기반 Table 명명, 편집 언어 변경, Entry 생성·검색은
[샘플 절차](Samples~/BasicUsage/README.md)에 설명돼 있습니다. Setup 메뉴는 소비 프로젝트의
Settings/Table/Entry를 생성하므로 테스트 프로젝트나 저장한 Scene에서 실행하세요.
Edit Mode의 AvailableLocales 상태는 Play Mode와 다를 수 있어 런타임 조회 검증은 Play에서 수행합니다.

### 코드와 테스트 읽기

[런타임 확장](Runtime/Extensions/LocalizationExtensions.cs)으로 실패/초기화 계약을 확인한 뒤
[Drawer](Editor/LocalizedStringDrawer.cs)의 편집 경로,
[Editor 조회 도구](Editor/Extensions/LocalizedStringExtensionsForEditor.cs)를 읽습니다.
namespace는 `Jeomseon.Unity.Localization`과 `.Extensions`/`.Editor`입니다.
샘플은 정상 조회, 빈 참조, 없는 Locale, Entry key와 진단 정보를 각각 확인합니다.

## API 사용과 상세 설명

### 포함 기능

- `LocalizedString` 런타임 확장
- `Jeomseon.Unity.Localization.LocalizedStringAttribute`
- String Table과 Entry를 편집하는 전용 PropertyDrawer

### 동작 계약

- `GetLocalizedStringByLocaleAsync(string localeCode)`는 `Awaitable<string>`을 반환합니다.
  Localization 초기화가 끝나지 않았다면 메인 스레드를 블로킹하지 않고 비동기로 대기합니다.
- `TryGetLocalizedString(this StringTable, string entryName, out string)`은 entry가 없으면
  예외 대신 `false`와 빈 문자열을 반환합니다.

## 샘플 실행

**Package Manager > Samples**에서 예제를 Import합니다. 포함된 Scene 또는 Editor 도구를 열고 필요한 Setup 메뉴를 먼저 실행합니다.

- [Basic Usage](Samples~/BasicUsage/README.md)
  - [LocalizationSample](Samples~/BasicUsage/LocalizationSample.unity)

## 테스트

[테스트 코드](Tests)

manifest의 `testables`에 패키지 ID를 추가하고 Unity Test Runner의 관련 EditMode/PlayMode 어셈블리를 실행합니다.

## 추가 문서

[전체 코드 지도와 패키지 구성](Documentation~/Overview.md)
- [Changelog](CHANGELOG.md)

## 라이선스

[License](LICENSE.md)
