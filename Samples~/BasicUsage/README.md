# Localization 기본 예제

`LocalizationSample.unity` Scene에 `LocalizationSample` 컴포넌트가 이미 부착된 GameObject가 포함돼 있습니다.

`message` 필드는 `[LocalizedString]` attribute가 붙어 있어 Unity 기본 LocalizedString Drawer 대신
이 패키지의 `LocalizedStringDrawer`가 그려집니다. 테이블 이름이 자동으로 컴포넌트 타입 이름
(`LocalizationSample`)으로 고정되므로, 프로젝트에 해당 String Table Collection이 아직 없어도
Inspector에서 바로 생성할 수 있습니다.

Sample에는 Editor 전용 설정 스크립트도 포함돼 있습니다. `Jeomseon/Tool/Localization/Setup Basic
Usage Sample` 메뉴를 실행하면 `LocalizationSample` 테이블과 `Greeting` Entry를 자동으로 생성합니다.
활성 Localization Settings가 없는 새 프로젝트라면 Sample 폴더에 Settings 자산을 만들고 프로젝트의
활성 Settings로 등록합니다.
프로젝트에 Locale이 하나도 없으면(완전히 새 프로젝트) 예시로 `English`/`Korean` Locale까지 자동
생성한 뒤 계속 진행합니다 — 기존 Locale이 하나라도 있으면 그 목록은 건드리지 않고 그대로 씁니다.
아래 확인 절차의 2번을 수동으로 하는 대신 이 메뉴 한 번으로 대체할 수 있습니다.

## 확인 절차

1. `LocalizationSample.unity`를 엽니다.
2. `Localization Sample` GameObject의 `message` 필드를 펼칩니다(Foldout).
   - 테이블 `LocalizationSample`이 아직 없다면 `테이블 생성` 버튼으로 원하는 폴더에 생성합니다
     (또는 위 `Jeomseon/Tool/Localization/Setup Basic Usage Sample` 메뉴로 한 번에 생성).
   - Entry Key 입력란에 임의의 키(예: `Greeting`)를 입력해 Entry를 만들고, 로케일별 값 입력란에
     번역 문자열을 입력합니다.
3. Play Mode로 진입해 Console에서 `현지화 문자열: ...` 로그가 출력되는지 확인합니다.
4. `Localization Sample` GameObject를 선택하고 Inspector 우클릭 메뉴(또는 컴포넌트 컨텍스트 메뉴)에서
   아래 3개 항목을 각각 실행해 로그를 확인합니다. **`TryGetLocalizedString`/
   `GetLocalizedStringByLocaleAsync`는 Play Mode에서 실행하세요** — Unity Localization의 기본
   `LocalesProvider`는 Play Mode(또는 진입 예정)일 때만 Locale Addressables Preload를 트리거하므로
   (`PlaymodeState.IsPlayingOrWillChangePlaymode` 내부 가드), Edit Mode에서는 참조가 정상이어도
   `AvailableLocales`가 비어 있어 실패할 수 있습니다. 이는 이 패키지의 결함이 아니라 Unity
   Localization 자체의 동작입니다.
   - `Entry Key 확인`: `Entry Key: ...` 로그가 2번에서 지정한 Entry의 Key와 일치하는지 확인합니다
     (`GetEntryKeyName` 예제). Name 참조를 즉시 반환하므로 Edit Mode에서도 항상 안정적입니다.
   - `TryGetLocalizedString 확인`: Entry가 지정된 상태에서는 성공 로그, `message`를 비워두면 실패
     로그가 출력되는지 확인합니다(`TryGetLocalizedString` 예제).
   - `GetLocalizedStringByLocaleAsync 확인 (en)`: 프로젝트에 `en` 로케일이 있고 해당 로케일의 값을
     입력했다면 그 값이, 없다면 빈 문자열이 로그로 출력되는지 확인합니다
     (`GetLocalizedStringByLocaleAsync` 예제).
   - 셋 중 하나라도 예상과 다르면 `진단 정보 확인`을 실행해 `IsEmpty`, `TableReference`/
     `TableEntryReference`의 `ReferenceType`·`Key`, `LocalizationSettings`의
     `HasSettings`/`SelectedLocale`/초기화 상태, `IsPlaying`, 로드된 `AvailableLocales` 개수를 한
     번에 확인합니다.
5. `Edit/Unity > Preferences > Jeomseon > Localization`에서 `Editor Language`를 `Korean`/`English`로
   전환하며 `LocalizedStringDrawer`의 HelpBox·버튼 텍스트(예: 테이블이 없을 때 `테이블 생성`/
   `Create Table`)가 즉시 바뀌는지 확인합니다. `Auto`는 OS 언어(`Application.systemLanguage`)를
   따르며, 지원하지 않는 언어는 영어로 표시됩니다.
