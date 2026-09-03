# Jeomseon Unity Localization

Unity Localization을 위한 런타임 확장, `LocalizedStringAttribute` 및 전용 PropertyDrawer를 제공합니다.

## OpenUPM으로 설치

프로젝트의 `Packages/manifest.json`에 OpenUPM scoped registry를 한 번 등록합니다.

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

## Git URL로 설치

Unity Package Manager의 `Install package from git URL`에 다음 주소를 사용합니다.

```text
https://github.com/jeomseon0516/Unity.Localization.git#v0.5.2
```

## 리팩토링 방침

Unity가 제공하는 동등 기능과 비교해 대체 가능한 코드는 소스의 한글 TODO 주석과 CHANGELOG의 Unreleased 항목에서 추적합니다.

## 포함 기능

- `LocalizedString` 런타임 확장
- `Jeomseon.Localization.LocalizedStringAttribute`
- String Table과 Entry를 편집하는 전용 PropertyDrawer

## 동작 계약

- `GetLocalizedStringByLocaleAsync(string localeCode)`는 `Awaitable<string>`을 반환합니다.
  Localization 초기화가 끝나지 않았다면 메인 스레드를 블로킹하지 않고 비동기로 대기합니다.
- `TryGetLocalizedString(this StringTable, string entryName, out string)`은 entry가 없으면
  예외 대신 `false`와 빈 문자열을 반환합니다.
