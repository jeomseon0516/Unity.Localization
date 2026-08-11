# Localization 기본 예제

`LocalizationSample.unity` Scene에 `LocalizationSample` 컴포넌트가 이미 부착된 GameObject가 포함돼 있습니다.

## 확인 절차

1. `LocalizationSample.unity`를 엽니다.
2. Inspector에서 `Localization Sample` GameObject의 `_message`(LocalizedString) 필드에 프로젝트의
   String Table Collection과 Entry를 지정합니다. 이 값은 프로젝트마다 다른 실제 Localization
   Settings/String Table 자산을 가리켜야 하므로 Sample에 미리 채워둘 수 없습니다.
3. Play Mode로 진입해 Console에서 `현지화 문자열: ...` 로그가 출력되는지 확인합니다.
4. `Localization Sample` GameObject를 선택하고 Inspector 우클릭 메뉴(또는 컴포넌트 컨텍스트 메뉴)에서
   `Entry Key 확인`을 실행해 `Entry Key: ...` 로그가 지정한 Entry의 Key와 일치하는지 확인합니다.
