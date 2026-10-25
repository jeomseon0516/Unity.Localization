# Localization 사용과 경계

[English](Design.en.md) · [설치](../README.md)

## 공식 패키지와의 관계

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

## 편집부터 실행까지

1. 공식 Localization Settings와 Locale을 준비합니다.
2. `[LocalizedString]` 필드에서 Table/Entry를 지정하거나 Drawer의 생성 기능을 사용합니다.
3. Play Mode에서 현재 언어 또는 지정 언어의 문자열을 조회합니다.
4. 게임 UI의 실시간 Locale 변경 구독은 공식 Localization 계약과 소비자 수명에 맞춰 관리합니다.

타입명 기반 Table 명명, 편집 언어 변경, Entry 생성·검색은
[샘플 절차](../Samples~/BasicUsage/README.md)에 설명돼 있습니다. Setup 메뉴는 소비 프로젝트의
Settings/Table/Entry를 생성하므로 테스트 프로젝트나 저장한 Scene에서 실행하세요.
Edit Mode의 AvailableLocales 상태는 Play Mode와 다를 수 있어 런타임 조회 검증은 Play에서 수행합니다.

## 코드와 테스트 읽기

[런타임 확장](../Runtime/Extensions/LocalizationExtensions.cs)으로 실패/초기화 계약을 확인한 뒤
[Drawer](../Editor/LocalizedStringDrawer.cs)의 편집 경로,
[Editor 조회 도구](../Editor/Extensions/LocalizedStringExtensionsForEditor.cs)를 읽습니다.
namespace는 `Jeomseon.Unity.Localization`과 `.Extensions`/`.Editor`입니다.
샘플은 정상 조회, 빈 참조, 없는 Locale, Entry key와 진단 정보를 각각 확인합니다.
