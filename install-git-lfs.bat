@echo off
echo Git LFS 설치를 시작합니다...

echo.
echo 1. Git LFS 다운로드 및 설치...
winget install GitHub.GitLFS

echo.
echo 2. 설치 확인...
git lfs version

echo.
echo 3. Git LFS 초기화...
git lfs install

echo.
echo Git LFS 설치가 완료되었습니다!
echo Unity Git 설정을 계속하려면 setup-unity-git.bat를 실행하세요.
pause