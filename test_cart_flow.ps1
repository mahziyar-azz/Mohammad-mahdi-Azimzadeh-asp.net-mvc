$res1 = Invoke-WebRequest -Method Post -Uri "http://localhost:11539/Home/AddToCart" -Body @{ productId = 1; quantity = 2 } -SessionVariable mySession
$res2 = Invoke-WebRequest -Method Get -Uri "http://localhost:11539/Home/Cart" -WebSession $mySession
if ($res2.Content -like "*quantity[*" -and $res2.Content -like "*value=`"2`"*") {
    Write-Host "TEST RESULT: SUCCESS"
} else {
    Write-Host "TEST RESULT: FAILED"
    Write-Host $res2.Content
}
