# $PSCommandPath = $MyInvocation.MyCommand.Definition
# $PSCommandPath
$dictory = Split-Path -Parent $PSCommandPath
Set-Location $dictory
$dictory
dotnet.exe publish -c release -r win-x64 --self-contained

"完成，任意键退出"  ;
Read-Host | Out-Null ;
Exit