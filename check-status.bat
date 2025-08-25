@echo off
cd /d "C:\startingup\games\MemoryFracture"
echo === Git Status ===
git status
echo.
echo === Remote Info ===
git remote -v
echo.
echo === Branch Info ===
git branch -v
echo.
echo === Last Commit ===
git log --oneline -1
echo.
echo === File Count ===
git ls-files | find /c /v ""
pause