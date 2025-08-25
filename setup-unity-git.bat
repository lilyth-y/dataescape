@echo off
echo Unity Git 설정을 시작합니다...
cd /d "C:\startingup\games\MemoryFracture"

echo.
echo 1. Git LFS 설치 및 설정...
git lfs install

echo.
echo 2. Git LFS 추적 설정...
git lfs track "*.unity"
git lfs track "*.prefab"
git lfs track "*.mat"
git lfs track "*.asset"
git lfs track "*.png"
git lfs track "*.jpg"
git lfs track "*.wav"
git lfs track "*.mp3"
git lfs track "*.fbx"
git lfs track "*.obj"

echo.
echo 3. .gitattributes 파일 추가...
git add .gitattributes

echo.
echo 4. Unity Git 설정 커밋...
git add .
git commit -m "Setup Unity Git integration with LFS"

echo.
echo 5. GitHub에 푸시...
git push origin main

echo.
echo Unity Git 설정이 완료되었습니다!
echo.
echo 다음 단계:
echo 1. Unity Hub에서 프로젝트 열기
echo 2. Edit > Project Settings > Editor에서 Version Control을 Git으로 설정
echo 3. Asset Serialization을 Force Text로 설정
echo 4. Memory Fracture > Git Integration 메뉴 사용
echo.
pause