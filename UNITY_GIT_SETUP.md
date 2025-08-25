# Unity에서 GitHub 연결 설정 가이드

Memory Fracture 프로젝트를 Unity에서 GitHub와 연결하여 협업 개발 환경을 설정하는 방법을 안내합니다.

## 🚀 Unity에서 Git 설정

### 1. Unity 에디터에서 Git 활성화

1. **Unity 에디터 열기**
   - Unity Hub에서 MemoryFracture 프로젝트 열기
   - Unity 2022.3 LTS 버전으로 열기

2. **Version Control 설정**
   - `Edit` → `Project Settings` → `Editor`
   - `Version Control` → `Mode` → `Git` 선택
   - `Asset Serialization` → `Force Text` 선택

3. **Git 설정 확인**
   - `Edit` → `Project Settings` → `Editor`
   - `Version Control` 섹션에서 Git 설정 확인

### 2. Unity Git 플러그인 설정

#### Git LFS (Large File Storage) 설정
```bash
# 프로젝트 디렉토리에서
git lfs install
git lfs track "*.unity"
git lfs track "*.prefab"
git lfs track "*.mat"
git lfs track "*.png"
git lfs track "*.jpg"
git lfs track "*.wav"
git lfs track "*.mp3"
git add .gitattributes
git commit -m "Add Git LFS tracking for Unity assets"
```

#### Unity용 .gitattributes 파일 생성
```
# Unity assets
*.unity filter=lfs diff=lfs merge=lfs -text
*.prefab filter=lfs diff=lfs merge=lfs -text
*.mat filter=lfs diff=lfs merge=lfs -text
*.asset filter=lfs diff=lfs merge=lfs -text

# Textures
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.jpeg filter=lfs diff=lfs merge=lfs -text
*.tga filter=lfs diff=lfs merge=lfs -text
*.tif filter=lfs diff=lfs merge=lfs -text
*.tiff filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text

# Audio
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.ogg filter=lfs diff=lfs merge=lfs -text
*.aiff filter=lfs diff=lfs merge=lfs -text

# Models
*.fbx filter=lfs diff=lfs merge=lfs -text
*.obj filter=lfs diff=lfs merge=lfs -text
*.dae filter=lfs diff=lfs merge=lfs -text
*.3ds filter=lfs diff=lfs merge=lfs -text
*.dxf filter=lfs diff=lfs merge=lfs -text
*.skp filter=lfs diff=lfs merge=lfs -text

# Fonts
*.ttf filter=lfs diff=lfs merge=lfs -text
*.otf filter=lfs diff=lfs merge=lfs -text

# Scripts (text files)
*.cs text eol=crlf
*.shader text eol=crlf
*.cginc text eol=crlf
*.compute text eol=crlf
*.hlsl text eol=crlf
*.glsl text eol=crlf
*.glslinc text eol=crlf
*.json text eol=crlf
*.xml text eol=crlf
*.txt text eol=crlf
*.md text eol=crlf
*.yml text eol=crlf
*.yaml text eol=crlf
```

### 3. Unity에서 Git 사용하기

#### Git Integration 도구 사용
1. **메뉴 열기:** `Memory Fracture` → `Git Integration`
2. **Git 상태 확인:** "Git Status 확인" 버튼 클릭
3. **커밋 및 푸시:** 커밋 메시지 입력 후 "커밋 및 푸시" 버튼 클릭
4. **GitHub 저장소 열기:** "GitHub 저장소 열기" 버튼 클릭

#### Unity 에디터에서 직접 Git 사용
1. **Window** → **General** → **Git** (Unity 2022.3+)
2. **Git 패널에서:**
   - 변경된 파일 확인
   - 스테이징 영역에 추가
   - 커밋 메시지 작성
   - 커밋 및 푸시

### 4. 협업 개발 워크플로우

#### 브랜치 전략
```bash
# 새로운 기능 개발
git checkout -b feature/new-puzzle

# 버그 수정
git checkout -b bugfix/player-movement

# 개발 완료 후
git checkout main
git merge feature/new-puzzle
git push origin main
```

#### 커밋 메시지 규칙
```
type(scope): description

feat(puzzle): 거울 반사 퍼즐에 새로운 난이도 추가
fix(player): 플레이어 이동 시 카메라 흔들림 수정
docs(readme): 설치 가이드 업데이트
style(ui): 메인 메뉴 UI 개선
refactor(core): GameManager 리팩토링
test(puzzle): 퍼즐 테스트 케이스 추가
```

### 5. Unity 프로젝트 설정

#### 프로젝트 설정 파일
- `ProjectSettings/ProjectVersion.txt`: Unity 버전
- `ProjectSettings/ProjectSettings.asset`: 프로젝트 설정
- `Packages/manifest.json`: 패키지 의존성

#### 권장 Unity 설정
1. **Player Settings:**
   - `Company Name`: Memory Fracture Team
   - `Product Name`: Memory Fracture
   - `Version`: 0.1.0

2. **Quality Settings:**
   - 개발 중: Fast
   - 릴리스: Fantastic

3. **Graphics Settings:**
   - Universal Render Pipeline 사용
   - 모바일 최적화 설정

### 6. 자동화 스크립트

#### Unity 빌드 자동화
```csharp
// Assets/Scripts/Editor/BuildAutomation.cs
using UnityEditor;
using UnityEngine;

public class BuildAutomation
{
    [MenuItem("Memory Fracture/Build/Windows")]
    public static void BuildWindows()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetEnabledScenes();
        buildPlayerOptions.locationPathName = "Builds/MemoryFracture_Windows.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildResult result = report.summary.result;
        
        if (result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + buildPlayerOptions.locationPathName);
        }
        else
        {
            Debug.LogError("Build failed");
        }
    }
    
    private static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }
}
```

### 7. 문제 해결

#### 일반적인 문제들
1. **Git LFS 오류:**
   ```bash
   git lfs install
   git lfs pull
   ```

2. **Merge 충돌:**
   ```bash
   git status
   # 충돌 파일 수정 후
   git add .
   git commit -m "Resolve merge conflicts"
   ```

3. **Unity 메타 파일 충돌:**
   - Unity 에디터에서 `Assets` → `Reimport All`
   - 충돌된 메타 파일 삭제 후 재생성

#### Unity Git 설정 확인
1. **Version Control 모드:** Git으로 설정
2. **Asset Serialization:** Force Text로 설정
3. **Git 설정:** 올바른 사용자 정보 설정

### 8. 권장 사항

#### 개발 환경
- Unity 2022.3 LTS 사용
- Git LFS 설치 및 설정
- Visual Studio 2019/2022 또는 VS Code 사용

#### 협업 규칙
- 커밋 전 Unity 에디터에서 테스트
- 큰 파일은 Git LFS 사용
- 정기적인 브랜치 병합
- 코드 리뷰 필수

#### 백업 및 보안
- 정기적인 GitHub 백업
- 민감한 정보는 .gitignore에 추가
- API 키 등은 환경 변수 사용

---

**Memory Fracture** 프로젝트의 Unity-Git 연동이 완료되었습니다! 🎮✨