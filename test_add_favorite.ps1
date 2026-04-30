# Script test API addFavorite
# Chạy trong PowerShell

$url = "https://bridged-shindig-feminize.ngrok-free.dev/vfg-api/api.php?action=addFavorite"

$body = @{
    userId = 1
    foodId = 1
} | ConvertTo-Json

Write-Host "Testing addFavorite API..." -ForegroundColor Yellow
Write-Host "URL: $url" -ForegroundColor Cyan
Write-Host "Body: $body" -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri $url -Method Post -Body $body -ContentType "application/json"
    Write-Host "`nResponse:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 10
} catch {
    Write-Host "`nError:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    Write-Host "`nResponse Body:" -ForegroundColor Red
    Write-Host $_.ErrorDetails.Message
}
