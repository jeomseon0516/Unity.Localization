# Localization 로드맵

우선순위: `P0` 결함·안전성 → `P1` 핵심 구조 → `P2` API·성능 → `P3` 장기 확장

## 작업 순서

1. **P0-01 — 동기 대기 제거 검토 (완료)**
   - Runtime `GetLocalizedStringByLocale`의 `WaitForCompletion`(WebGL 미지원, 메인 스레드
     블로킹)을 제거하고 `GetLocalizedStringByLocaleAsync`(`Awaitable<string>`)로 교체했습니다.
   - Editor 전용 `MonitorSpecificLocaleEntry`의 `WaitForCompletion`은 Player 플랫폼 제약과
     무관하므로 그대로 유지합니다.
2. **P1-01 — LocalizeStringEvent와 중복되는 래퍼 정리 (완료)**
   - 워크스페이스 전체에서 소비처가 없던 `LocalizedStringOption`/`ILocalizable`을 제거했습니다.
     `LocalizeStringEvent`로 완전히 대체 가능한 미사용 wrapper였습니다.
3. **P1-02 — Listener 수명 관리 (완료)**
   - 남아 있는 구독 지점(`LocalizedStringDrawer`의 정적 로케일 이벤트, Sample의
     `LocalizedString.StringChanged`)을 검토했습니다. 정적 구독은 Domain Reload당 1회만
     실행되는 정적 생성자 기반이라 중복 등록되지 않고, Sample 구독은 `OnEnable`/`OnDisable`로
     짝을 맞춰 해제합니다. 별도 수정이 필요한 누수는 없었습니다.
4. **P2-01 — Locale·Entry 오류 모델 (완료)**
   - `TryGetLocalizedString(this StringTable, string)`이 이름과 달리 `bool`을 반환하지 않고
     존재하지 않는 entry에서 `NullReferenceException`을 던지던 결함을 `bool` + `out string`
     Try 패턴으로 수정했습니다. 무관한 디버그 `Comment` 로그 부수효과도 제거했습니다.
