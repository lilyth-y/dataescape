Write-Host "Unity Git 설정을 시작합니다..." -ForegroundColor Green
Set-Location "C:\startingup\games\MemoryFracture"

Write-Host "`n1. .gitattributes 파일 추가..." -ForegroundColor Yellow
git add .gitattributes

Write-Host "`n2. 새로운 파일들 추가..." -ForegroundColor Yellow
git add .

Write-Host "`n3. Unity Git 설정 커밋..." -ForegroundColor Yellow
git commit -m "Add Unity Git integration tools and documentation"

Write-Host "`n4. GitHub에 푸시..." -ForegroundColor Yellow
git push origin main

Write-Host "`nUnity Git 설정이 완료되었습니다!" -ForegroundColor Green
Write-Host "`nUnity에서 다음 설정을 해주세요:" -ForegroundColor Cyan
Write-Host "1. Edit > Project Settings > Editor" -ForegroundColor White
Write-Host "2. Version Control > Mode > Git 선택" -ForegroundColor White
Write-Host "3. Asset Serialization > Force Text 선택" -ForegroundColor White
Write-Host "`nMemory Fracture > Git Integration 메뉴를 사용하여 Git 작업을 할 수 있습니다." -ForegroundColor Cyan

Read-Host "`n계속하려면 Enter를 누르세요"