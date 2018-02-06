$dictory = Split-Path -Parent $PSCommandPath
Set-Location $dictory
dotnet.exe MyClient.dll