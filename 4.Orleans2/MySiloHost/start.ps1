$dictory = Split-Path -Parent $PSCommandPath
Set-Location $dictory
dotnet.exe MySiloHost.dll