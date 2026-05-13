# Local Documentation Workspace Guide

## 목적
- 이 `Documentation/` 폴더는 **Git 공유 대상이 아닌 로컬 전용 작업 공간**입니다.
- 브랜치를 변경해도 동일한 문서 자산을 유지하면서, 공통 작업과 브랜치 한정 작업을 분리 기록합니다.

## 폴더 구조
- `Documentation/common/`
  - 모든 브랜치에서 공통으로 참조하는 기준 문서
- `Documentation/branches/<branch-name>/`
  - 특정 브랜치에서만 유효한 구현 로그, 실험 기록, 임시 체크리스트

## 운영 규칙
1. 공통 정책/설계 원칙은 `common/`에 기록합니다.
2. 구현 상세/트러블슈팅은 현재 브랜치 하위 폴더에만 기록합니다.
3. 브랜치 머지 후에도 남겨야 하는 내용만 `common/`으로 승격합니다.
4. 공유가 필요한 문서는 별도 공유 경로(예: Notion 또는 추적되는 Docs 저장소)로 이관합니다.

## 브랜치별 작업 구분 템플릿
- `Goal`: 브랜치 목표
- `Implemented`: 구현 완료 항목
- `Validation`: 검증 결과
- `Pending`: 잔여 작업 및 리스크
