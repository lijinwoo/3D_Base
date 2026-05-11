# Notion / 태그 / 릴리즈 노트 동기화 (Phase 3~6)

## 목적
MVP 로드맵의 **문서 ↔ Notion ↔ Git 태그**를 Phase 3 이후에도 동일한 패턴으로 유지합니다.

<<<<<<< HEAD
> **2026-05 업데이트:** 부모 Notion 페이지 `35537e0b48d8816a85c3c747f658050a`를 **Hybrid Single-player RPG** 기준으로 갱신했습니다(Roguelite/Wave 중심 서술 제거, Encounter/Quest/Save 반영).

=======
>>>>>>> 29fbfa50cf419c0d688f39bdbd0021df496917ec
## 기준 Notion 페이지

| 용도 | Page ID | 비고 |
|------|---------|------|
| 전체 로드맵 (부모) | `35537e0b48d8816a85c3c747f658050a` | 하위에 Phase 3~6 수업 페이지를 추가 |
| Phase 1 수업 | `35537e0b48d881d3a77def3b281a485a` | 기존 |
| Phase 2 수업 | `35537e0b48d881e08216ec4e08c62210` | 기존 |

## Phase 3~6 수업 페이지에 넣을 최소 목차

1. **목표 / Done 기준** — [MVP_Roadmap_Phase_and_Release_Management.md](MVP_Roadmap_Phase_and_Release_Management.md) 해당 Phase 절
2. **Validation Scene** — 에디터 메뉴 이름 + `.unity` 경로
3. **핵심 스크립트 경로** — `Assets/Scripts/Gameplay/...`
4. **Known Issue / Risk** — 프로젝트 `Documentation`에 기록된 항목 링크

## Git 태그 (Phase 완료 시)

로드맵 규칙에 따라 원격에 태그를 밀어 넣습니다.

```bash
git tag mvp-phase3
git push origin mvp-phase3
# Phase 4~6도 동일 패턴: mvp-phase4 … mvp-phase6
```

## 릴리즈 노트

- `phase/*` → `develop` 병합 PR에 **Validation Scene 스모크 결과**를 첨부합니다.
- `mvp-1.0.0` 전 `release/mvp-1.0.0` 브랜치에서 Addressables·Pool·AI 회귀를 한 번 더 수행합니다.

## 로컬 문서 갱신 체크리스트

- [ ] `MVP_Roadmap_Phase_and_Release_Management.md`의 Phase별 Done/Risk 반영
- [ ] `Unity3D_Course_Notion_and_Git.md`의 Phase 3~6 Notion URL 채움(페이지 생성 후)
- [ ] 본 파일의 Page ID 테이블과 실제 Notion 트리가 일치하는지 확인
