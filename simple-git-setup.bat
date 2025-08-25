@echo off
echo Unity Git 설정을 간단하게 진행합니다...
cd /d "C:\startingup\games\MemoryFracture"

echo.
echo 1. .gitattributes 파일 추가...
git add .gitattributes

echo.
echo 2. 새로운 파일들 추가...
git add .

echo.
echo 3. Unity Git 설정 커밋...
git commit -m "Add Unity Git integration tools and documentation"

echo.
echo 4. GitHub에 푸시...
git push origin main

echo.
echo Unity Git 설정이 완료되었습니다!
echo.
echo Unity에서 다음 설정을 해주세요:
echo 1. Edit > Project Settings > Editor
echo 2. Version Control > Mode > Git 선택
echo 3. Asset Serialization > Force Text 선택
echo.
pause