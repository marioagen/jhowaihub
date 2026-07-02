$repoRoot = Split-Path $PSScriptRoot -Parent
$env:ConnectionStrings__TemplateConnection = 'Server=localhost,1433;Database=WoopiAiHub;User Id=sa;Password=Strong!WortePass99;TrustServerCertificate=True;'
$env:ConnectionStrings__Redis = 'localhost:6379'
$env:CORS = 'http://localhost:3000'
$env:JWT__Key = 'local-dev-jwt-secret-key-minimum-32-characters-long'
$env:EncryptionSettings__Key = 'local-dev-encryption-key-minimum-32-chars'
$env:Messaging__Brokers__RabbitMQ__UserName = 'guest'
$env:Messaging__Brokers__RabbitMQ__Password = 'guest'
$env:RefitExternalSettings__MarketPlaceBaseAddress = 'http://localhost:7047'
$env:KeyAccess = 'local-dev'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
Set-Location "$repoRoot\back-end\WoopiAiHub.Api"
dotnet run --launch-profile WoopiAiHub.Api
