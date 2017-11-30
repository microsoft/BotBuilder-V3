@echo off
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
erase Microsoft.Bot.Connector*nupkg
msbuild /property:Configuration=release Microsoft.Bot.Connector.Falcon.csproj
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Connector.dll).FileVersionInfo.FileVersion"') do set version=%%v
..\..\packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe pack Microsoft.Bot.Connector.nuspec -symbols -properties version=%version% -OutputDirectory .
