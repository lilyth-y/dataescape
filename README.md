# Memory Fracture

## 🎮 게임 개요
- **제목:** Memory Fracture (가제)
- **장르:** 2인 협동, 방탈출, 퍼즐, 미스터리/판타지
- **플랫폼:** PC (Steam), 모바일 (Android/iOS)
- **목표 플레이타임:** 첫 플레이 12~15시간, 스피드런 6~8시간

## 🌟 핵심 요소
- **협동:** 2인 플레이어 간 정보 공유와 동시 행동 필수
- **퍼즐 난이도:** 데이터 과학적 메타포를 활용한 복합 퍼즐
- **감정 몰입:** 정체성 미스터리와 신뢰/의심의 극대화
- **다중 엔딩:** 플레이어 행동과 선택에 따른 6가지 엔딩 분기

## 🏗️ 프로젝트 구조
```
MemoryFracture/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # 핵심 게임 시스템
│   │   │   ├── GameManager.cs
│   │   │   ├── PlayerController.cs
│   │   │   ├── GameBuilder.cs
│   │   │   └── GameRunner.cs
│   │   ├── Puzzles/        # 퍼즐 시스템
│   │   │   ├── BasePuzzle.cs
│   │   │   ├── MirrorReflectionPuzzle.cs
│   │   │   └── DataRestorationPuzzle.cs
│   │   ├── Networking/     # 멀티플레이어
│   │   │   └── NetworkManager.cs
│   │   └── UI/            # 사용자 인터페이스
│   │       ├── UIManager.cs
│   │       └── MainMenuController.cs
│   ├── Scenes/            # 게임 씬
│   │   ├── MainMenu.unity
│   │   ├── GameScene.unity
│   │   └── Chapter1_MirrorCorridor.unity
│   ├── Prefabs/           # 프리팹
│   └── Materials/         # 재질
├── ProjectSettings/        # Unity 프로젝트 설정
├── Packages/              # Unity 패키지
└── docs/                  # 게임 문서
```

## 🚀 개발 환경 설정

### 필수 요구사항
- Unity 2022.3 LTS 이상
- Git
- Visual Studio 2019/2022 또는 VS Code

### 설치 및 실행
1. **레포지토리 클론**
   ```bash
   git clone [repository-url]
   cd MemoryFracture
   ```

2. **Unity에서 프로젝트 열기**
   - Unity Hub 실행
   - "Open" 버튼 클릭
   - `MemoryFracture` 폴더 선택
   - Unity 2022.3 LTS 버전으로 열기

3. **게임 실행**
   - `Assets/Scenes/GameScene.unity` 씬 열기
   - Play 버튼 클릭

## 🎯 게임 조작법
- **WASD:** 플레이어 이동
- **마우스:** 시점 조작
- **E:** 상호작용 (퍼즐 시작)
- **ESC:** 일시정지
- **F1:** 게임 재시작
- **F2:** 디버그 정보 토글

## 🧩 퍼즐 시스템
### 챕터 1: 거울 복도
- **거울 반사 퍼즐:** 3개의 거울을 조정하여 빛을 목표물에 반사
- **협동 요소:** 두 플레이어가 동시에 거울을 조정해야 함
- **플래그 보상:** 진실 +1, 협동 +2

### 챕터 2: 데이터 아카이브
- **데이터 복원 퍼즐:** 손상된 데이터 조각들을 복원
- **동시 행동:** 두 플레이어가 동시에 데이터 조각 복원
- **플래그 보상:** 진실 +2, 협동 +3

## 🏷️ 플래그 시스템
게임의 6가지 핵심 플래그가 플레이어 행동에 따라 변화합니다:

- **진실 (Truth):** 진실을 추구하는 행동
- **망각 (Oblivion):** 기억을 잃어버리는 행동
- **희생 (Sacrifice):** 자신을 희생하는 행동
- **손상 (Corruption):** 데이터가 손상되는 행동
- **추적자 (Tracker):** 추적자에게 발각되는 행동
- **협동 (Cooperation):** 플레이어 간 협력하는 행동

## 🎬 엔딩 시스템
플래그 조합에 따른 6가지 엔딩:

1. **현실 귀환 (진엔딩):** 진실 ≥ 3, 손상 < 5
2. **거짓된 기억:** 망각 ≥ 3
3. **희생 엔딩:** 희생 ≥ 2, 진실 = 0
4. **공존 엔딩 (히든):** 완료 퍼즐 ≥ 15, 히든 퍼즐 ≥ 3
5. **루프 엔딩:** 손상 ≥ 5
6. **추적자 승리:** 추적자 ≥ 2

## 🤝 기여하기
1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 라이선스
이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자세한 내용은 `LICENSE` 파일을 참조하세요.

## 📞 연락처
- 프로젝트 링크: [https://github.com/your-username/MemoryFracture](https://github.com/your-username/MemoryFracture)

---
**Memory Fracture** - 기억의 미로에서 진실을 찾아라