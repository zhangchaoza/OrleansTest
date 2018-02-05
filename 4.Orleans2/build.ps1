# $PSCommandPath = $MyInvocation.MyCommand.Definition
# $PSCommandPath
$dictory = Split-Path -Parent $PSCommandPath
Set-Location $dictory
dotnet.exe build -c Debug -r win-x64

"完成，任意键退出"  ;
Read-Host | Out-Null ;
Exit