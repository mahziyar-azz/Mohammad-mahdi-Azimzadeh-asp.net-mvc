$res1 = Invoke-WebRequest -Method Post -Uri "http://localhost:11539/Home/Login" -Body @{ email = "admin@example.com"; password = "admin123" } -SessionVariable mySession
$res2 = Invoke-WebRequest -Method Get -Uri "http://localhost:11539/Admin" -WebSession $mySession
if ($res2.Content -like "*sidebar-menu*" -and $res2.Content -like "*/Admin/Products*") {
    Write-Host "TEST RESULT: SUCCESS"
} else {
    Write-Host "TEST RESULT: FAILED"
    Write-Host $res2.Content
}
