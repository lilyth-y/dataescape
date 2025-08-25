# Contributing to Memory Fracture

Memory Fracture 프로젝트에 기여해주셔서 감사합니다! 이 문서는 프로젝트에 기여하는 방법을 안내합니다.

## 🚀 시작하기

### 개발 환경 설정
1. Unity 2022.3 LTS 설치
2. Git 설치
3. Visual Studio 2019/2022 또는 VS Code 설치

### 프로젝트 클론
```bash
git clone https://github.com/your-username/MemoryFracture.git
cd MemoryFracture
```

## 📝 기여 가이드라인

### 브랜치 전략
- `main`: 안정적인 릴리스 버전
- `develop`: 개발 중인 기능들
- `feature/*`: 새로운 기능 개발
- `bugfix/*`: 버그 수정
- `hotfix/*`: 긴급 수정

### 커밋 메시지 규칙
```
type(scope): description

[optional body]

[optional footer]
```

**타입:**
- `feat`: 새로운 기능
- `fix`: 버그 수정
- `docs`: 문서 수정
- `style`: 코드 스타일 변경
- `refactor`: 코드 리팩토링
- `test`: 테스트 추가/수정
- `chore`: 빌드 프로세스 또는 보조 도구 변경

**예시:**
```
feat(puzzle): 거울 반사 퍼즐에 새로운 난이도 추가
fix(player): 플레이어 이동 시 카메라 흔들림 수정
docs(readme): 설치 가이드 업데이트
```

### 코드 스타일
- C# 네이밍 컨벤션 준수
- Unity 코딩 표준 준수
- 주석은 한국어로 작성
- 클래스와 메서드에 XML 문서 주석 추가

## 🎮 게임 개발 가이드라인

### 새로운 퍼즐 추가
1. `Assets/Scripts/Puzzles/` 폴더에 새 퍼즐 스크립트 생성
2. `BasePuzzle` 클래스 상속
3. 필요한 추상 메서드 구현
4. 퍼즐 테스트 케이스 작성

### 새로운 챕터 추가
1. `Assets/Scenes/` 폴더에 새 씬 생성
2. 챕터별 환경 구축
3. 퍼즐 배치 및 연결
4. 스토리 요소 추가

### UI 요소 추가
1. `Assets/Scripts/UI/` 폴더에 새 UI 스크립트 생성
2. `UIManager`와 연결
3. 반응형 디자인 적용
4. 접근성 고려

## 🧪 테스트

### 단위 테스트
- `tests/` 폴더에 테스트 파일 작성
- NUnit 프레임워크 사용
- 모든 핵심 기능에 대한 테스트 작성

### 통합 테스트
- 게임 플레이 시나리오 테스트
- 퍼즐 완료 플로우 테스트
- 멀티플레이어 동기화 테스트

## 📋 Pull Request 가이드라인

### PR 생성 전 체크리스트
- [ ] 코드가 프로젝트 스타일 가이드에 맞는지 확인
- [ ] 모든 테스트가 통과하는지 확인
- [ ] 새로운 기능에 대한 테스트를 추가했는지 확인
- [ ] 문서를 업데이트했는지 확인
- [ ] 커밋 메시지가 규칙에 맞는지 확인

### PR 템플릿
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
```

## 🐛 버그 리포트

### 버그 리포트 템플릿
```markdown
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

## 💡 기능 요청

### 기능 요청 템플릿
```markdown
## 기능 설명
요청하는 기능에 대한 자세한 설명

## 사용 사례
이 기능이 어떻게 사용될지 설명

## 대안
고려한 다른 해결책들

## 추가 정보
스크린샷, 참고 자료 등
```

## 📞 연락처

- 이슈 트래커: [GitHub Issues](https://github.com/your-username/MemoryFracture/issues)
- 토론: [GitHub Discussions](https://github.com/your-username/MemoryFracture/discussions)

## 📄 라이선스

이 프로젝트에 기여함으로써, 귀하는 기여 내용이 프로젝트의 MIT 라이선스 하에 배포됨에 동의합니다.

---

**Memory Fracture** 개발팀을 지원해주셔서 감사합니다! 🎮