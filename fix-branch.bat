@echo off
cd /d "C:\startingup\games\MemoryFracture"
echo Current branch: 
git branch
echo Renaming master to main...
git branch -M main
echo New branch name:
git branch
echo Pushing to GitHub...
git push -u origin main
echo Done!
pause