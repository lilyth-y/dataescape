@echo off
cd /d "C:\startingup\games\MemoryFracture"
echo Current directory: %CD%
git remote -v
git branch
git push -u origin main
echo Push completed!
pause