# GitHub 저장소 설정 가이드

Memory Fracture 프로젝트를 GitHub에 업로드하는 방법을 안내합니다.

## 🚀 GitHub 저장소 생성

### 1. GitHub에서 새 저장소 생성
1. GitHub.com에 로그인
2. 우측 상단의 "+" 버튼 클릭 → "New repository"
3. 저장소 설정:
   - **Repository name:** `MemoryFracture`
   - **Description:** `2인 협동 미스터리 퍼즐 게임 - 기억의 미로에서 진실을 찾아라`
   - **Visibility:** Public (또는 Private)
   - **Initialize this repository with:** 체크하지 않음
4. "Create repository" 클릭

### 2. 로컬 저장소와 연결
```bash
# 프로젝트 디렉토리로 이동
cd C:\startingup\games\MemoryFracture

# 원격 저장소 추가
git remote add origin https://github.com/YOUR_USERNAME/MemoryFracture.git

# 메인 브랜치를 main으로 변경 (필요한 경우)
git branch -M main

# 첫 번째 푸시
git push -u origin main
```

## 📋 저장소 설정

### 1. 저장소 정보 설정
GitHub 저장소 페이지에서:
- **About 섹션:** 프로젝트 설명, 웹사이트, 토픽 추가
- **Topics:** `unity`, `game`, `puzzle`, `multiplayer`, `mystery`, `csharp`

### 2. README 커스터마이징
- 프로젝트 로고 추가
- 스크린샷/게임플레이 영상 추가
- 설치 및 실행 가이드 업데이트

### 3. 이슈 템플릿 설정
`.github/ISSUE_TEMPLATE/` 폴더에 템플릿 파일들 생성:

#### bug_report.md
```markdown
---
name: 버그 리포트
about: 버그를 발견했을 때 사용
title: '[BUG] '
labels: 'bug'
assignees: ''
---

## 버그 설명
간단하고 명확한 버그 설명

## 재현 단계
1. 
2. 
3. 

## 예상 동작
어떻게 동작해야 하는지 설명

## 실제 동작
실제로 어떻게 동작하는지 설명

## 환경 정보
- OS: 
- Unity 버전: 
- 게임 버전: 

## 추가 정보
스크린샷, 로그 등 추가 정보
```

#### feature_request.md
```markdown
---
name: 기능 요청
about: 새로운 기능을 제안할 때 사용
title: '[FEATURE] '
labels: 'enhancement'
assignees: ''
---

## 기능 설명
요청하는 기능에 대한 자세한 설명

## 사용 사례
이 기능이 어떻게 사용될지 설명

## 대안
고려한 다른 해결책들

## 추가 정보
스크린샷, 참고 자료 등
```

### 4. Pull Request 템플릿 설정
`.github/pull_request_template.md` 파일 생성:

```markdown
## 변경 사항
- 

## 테스트
- [ ] 단위 테스트 통과
- [ ] 통합 테스트 통과
- [ ] 수동 테스트 완료

## 체크리스트
- [ ] 코드 리뷰 완료
- [ ] 문서 업데이트
- [ ] 테스트 추가

## 관련 이슈
Closes #

## 스크린샷 (UI 변경 시)
```

## 🔧 GitHub Actions 설정

### 1. CI/CD 파이프라인 설정
`.github/workflows/unity-ci.yml` 파일 생성:

```yaml
name: Unity CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
      with:
        lfs: true
        
    - name: Cache Library
      uses: actions/cache@v3
      with:
        path: Library
        key: Library-${{ hashFiles('Assets/**', 'Packages/**', 'ProjectSettings/**') }}
        restore-keys: |
          Library-
          
    - name: Unity - Builder
      uses: game-ci/unity-builder@v2
      env:
        UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
      with:
        targetPlatform: StandaloneWindows64
        
    - name: Upload Build
      uses: actions/upload-artifact@v3
      with:
        name: Build
        path: build
```

### 2. Secrets 설정
GitHub 저장소 Settings → Secrets and variables → Actions에서:
- `UNITY_LICENSE`: Unity 라이선스 키

## 📊 프로젝트 관리

### 1. 프로젝트 보드 설정
- **To Do:** 할 일
- **In Progress:** 진행 중
- **Review:** 리뷰 대기
- **Done:** 완료

### 2. 마일스톤 설정
- **v0.1.0:** 기본 게임 시스템
- **v0.2.0:** 챕터 1 완성
- **v0.3.0:** 멀티플레이어 구현
- **v1.0.0:** 완전한 게임

### 3. 라벨 설정
- `bug`: 버그
- `enhancement`: 기능 개선
- `documentation`: 문서
- `good first issue`: 초보자용
- `help wanted`: 도움 필요
- `priority: high`: 높은 우선순위
- `priority: medium`: 중간 우선순위
- `priority: low`: 낮은 우선순위

## 🎮 릴리스 관리

### 1. 릴리스 노트 템플릿
```markdown
## 🎉 새로운 기능
- 

## 🐛 버그 수정
- 

## 🔧 개선사항
- 

## 📝 문서
- 

## 🧪 테스트
- 

## 🔄 변경사항
- 

## 📋 알려진 이슈
- 
```

### 2. 자동 릴리스 설정
GitHub Actions에서 자동 릴리스 워크플로우 설정

## 📈 프로젝트 통계

### 1. GitHub Insights 활용
- **Traffic:** 저장소 방문자 통계
- **Contributors:** 기여자 분석
- **Commits:** 커밋 활동 분석

### 2. 외부 도구 연동
- **Discord:** 개발자 커뮤니티
- **Trello:** 작업 관리
- **Figma:** UI/UX 디자인

## 🔐 보안 설정

### 1. 보안 정책 설정
`.github/SECURITY.md` 파일 생성:

```markdown
# 보안 정책

## 지원 버전

| 버전 | 지원 여부 |
| --- | --- |
| 1.0.x | ✅ |
| < 1.0 | ❌ |

## 취약점 신고

보안 취약점을 발견하셨다면:
1. **즉시 신고:** security@memoryfracture.com
2. **공개 신고 금지:** 공개적으로 신고하지 마세요
3. **상세 정보 제공:** 재현 단계, 영향 범위 등

## 보상 프로그램

중요한 보안 취약점 발견 시 보상이 제공될 수 있습니다.
```

## 📞 지원

GitHub 저장소 설정에 문제가 있으시면:
- **이슈 생성:** GitHub Issues에서 질문
- **토론 참여:** GitHub Discussions에서 논의
- **문서 확인:** 이 가이드와 CONTRIBUTING.md 참조

---

**Memory Fracture** 프로젝트가 GitHub에서 성공적으로 관리되기를 바랍니다! 🚀