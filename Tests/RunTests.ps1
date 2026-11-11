param([string]$Server='localhost\SQLEXPRESS')
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$testDatabaseName='AutoCarePro_Test_'+[Guid]::NewGuid().ToString('N').Substring(0,12)
$oldConnection=$env:AUTOCARE_CONNECTION_STRING
Push-Location $workspace
try {
    New-Item -ItemType Directory -Force (Join-Path $PSScriptRoot 'artifacts') | Out-Null
    $setupPath=Join-Path $PSScriptRoot 'artifacts/setup.sql'
    $setup=Get-Content 'VehicleServiceCenterManagementSystem/Database/SetupDatabase.sql' -Raw
    $setup.Replace('VehicleServiceCenterDB',$testDatabaseName) | Set-Content -LiteralPath $setupPath
    & sqlcmd -S $Server -E -C -b -i $setupPath
    if($LASTEXITCODE -ne 0){throw 'Test database creation failed.'}
    & sqlcmd -S $Server -E -C -b -d $testDatabaseName -i 'VehicleServiceCenterManagementSystem/Database/UpgradeDatabase.sql'
    if($LASTEXITCODE -ne 0){throw 'Test database migration failed.'}
    & dotnet build Tests/AutoCarePro.Tests.csproj
    if($LASTEXITCODE -ne 0){throw 'Test build failed.'}
    $env:AUTOCARE_CONNECTION_STRING="Server=$Server;Database=$testDatabaseName;Integrated Security=True;Connect Timeout=10;"
    & dotnet Tests/bin/Debug/net8.0-windows/AutoCarePro.Tests.dll
    if($LASTEXITCODE -ne 0){throw 'Integration tests failed.'}
    Write-Output "Test database retained for inspection: $testDatabaseName"
}
finally { $env:AUTOCARE_CONNECTION_STRING=$oldConnection; Pop-Location }
