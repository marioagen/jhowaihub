# Sobe infra (Docker), API e frontend para desenvolvimento local.
$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent

$env:Path = "$env:USERPROFILE\.dotnet\tools;C:\Program Files\nodejs;C:\Program Files\dotnet;" + $env:Path

Write-Host ">> Subindo infraestrutura (SQL Server, Redis, RabbitMQ)..."
Push-Location $repoRoot
docker compose up -d
Pop-Location

Write-Host ">> Iniciando mock do Marketplace em http://localhost:7047 ..."
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "cd '$repoRoot\scripts\local-marketplace-mock'; dotnet run"
) -WindowStyle Normal

Start-Sleep -Seconds 3

$env:ConnectionStrings__TemplateConnection = "Server=localhost,1433;Database=WoopiAiHub;User Id=sa;Password=Strong!WortePass99;TrustServerCertificate=True;"
$env:ConnectionStrings__Redis = "localhost:6379"
$env:CORS = "http://localhost:3000"
$env:JWT__Key = "local-dev-jwt-secret-key-minimum-32-characters-long"
$env:EncryptionSettings__Key = "local-dev-encryption-key-minimum-32-chars"
$env:Messaging__Brokers__RabbitMQ__UserName = "guest"
$env:Messaging__Brokers__RabbitMQ__Password = "guest"
$env:RefitExternalSettings__MarketPlaceBaseAddress = "http://localhost:7047"
$env:KeyAccess = "local-dev"
$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host ">> Iniciando API em https://localhost:7045 ..."
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    @"
`$env:ConnectionStrings__TemplateConnection = 'Server=localhost,1433;Database=WoopiAiHub;User Id=sa;Password=Strong!WortePass99;TrustServerCertificate=True;'
`$env:ConnectionStrings__Redis = 'localhost:6379'
`$env:CORS = 'http://localhost:3000'
`$env:JWT__Key = 'local-dev-jwt-secret-key-minimum-32-characters-long'
`$env:EncryptionSettings__Key = 'local-dev-encryption-key-minimum-32-chars'
`$env:Messaging__Brokers__RabbitMQ__UserName = 'guest'
`$env:Messaging__Brokers__RabbitMQ__Password = 'guest'
`$env:RefitExternalSettings__MarketPlaceBaseAddress = 'http://localhost:7047'
`$env:KeyAccess = 'local-dev'
`$env:ASPNETCORE_ENVIRONMENT = 'Development'
cd '$repoRoot\back-end\WoopiAiHub.Api'
dotnet run --launch-profile WoopiAiHub.Api
"@
) -WindowStyle Normal

Start-Sleep -Seconds 3

Write-Host ">> Iniciando frontend em http://localhost:3000 ..."
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "cd '$repoRoot\front-end\vueapp'; npm run dev"
) -WindowStyle Normal

Write-Host ""
Write-Host "Pronto. Acesse:"
Write-Host "  Frontend: http://localhost:3000"
Write-Host "  API/Swagger: https://localhost:7045/swagger (ou http://localhost:5215/swagger)"
Write-Host "  Marketplace mock: http://localhost:7047 (obrigatorio para login)"
Write-Host "  Usuario teste: test.admin@woopi.local / Test@123456 (tenant: local)"
Write-Host ""
Write-Host "Se o login falhar no browser por certificado HTTPS, use VUE_APP_BASE_URL_API=http://localhost:5215 no .env"
