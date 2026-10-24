# Jeomseon Unity Localization

[English](Overview.en.md) · [설치](../README.md)

## 목적

Unity Localization의 LocalizedString과 String Table을 유지하면서 언어별 조회와 전용 Inspector 편집을 보완합니다. 번역 데이터 서비스 자체를 대체하지 않습니다.

## 사용 흐름

Unity Localization의 Locale·Table·Entry 구성 → LocalizedStringAttribute 필드 연결 → TryGetLocalizedString 또는 GetLocalizedStringByLocaleAsync 호출. namespace는 Jeomseon.Unity.Localization.Extensions입니다.

## 계약과 제한

비동기 언어별 조회는 초기화를 기다리고 빈 참조·없는 Locale에는 빈 문자열을 반환합니다. 취소 토큰 오버로드는 없습니다. Try 조회도 필요시 공식 StringDatabase의 동기 동작을 사용하므로 모든 호출이 비블로킹이라고 가정하지 마세요.

## 패키지 요구 사항

- Package ID: `com.jeomseon.unity.localization`
- Version: `0.5.2`
- Unity: `6000.6` / `0f1`
- `com.jeomseon.unity.core`: `0.3.2`
- `com.unity.editorcoroutines`: `1.0.0`
- `com.unity.localization`: `1.4.5`

## 샘플과 검증

Package Manager > Samples에서 다음 예제를 Import하세요. 포함된 Scene 또는 Editor 도구를 열고 샘플 README의 Setup 절차를 먼저 수행합니다.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [LocalizationSample](../Samples~/BasicUsage/LocalizationSample.unity)

[테스트 코드](../Tests)

소비 프로젝트 manifest의 `testables`에 패키지 ID를 추가하고 Unity Test Runner의 관련 어셈블리를 실행합니다. 위 링크는 테스트 위치이며 이번 문서 수정으로 새 테스트 실행을 주장하지 않습니다.

## 코드 지도

- [LocalizationExtensions](../Runtime/Extensions/LocalizationExtensions.cs)
- [LocalizedStringAttribute](../Runtime/LocalizedStringAttribute.cs)
- [LocalizedStringExtensionsForEditor](../Editor/Extensions/LocalizedStringExtensionsForEditor.cs)

[상세 설계](Design.md)
