@echo off
cd /d "C:\startingup\games\MemoryFracture"
git init
git config user.name "Memory Fracture Developer"
git config user.email "developer@memoryfracture.com"
git add .
git commit -m "Initial commit: Memory Fracture game project setup"
echo Git repository setup complete!
pause