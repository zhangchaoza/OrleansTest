# $PSCommandPath = $MyInvocation.MyCommand.Definition
# $PSCommandPath
$dictory = Split-Path -Parent $PSCommandPath
Set-Location $dictory
dotnet.exe publish -c Release -r win-x64
dotnet.exe publish -c Release

"完成，任意键退出"  ;
Read-Host | Out-Null ;
Exit