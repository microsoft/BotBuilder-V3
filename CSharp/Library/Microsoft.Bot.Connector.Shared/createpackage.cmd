@echo off
echo *** Building Microsoft.Bot.Connector.Common
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Connector.Common*nupkg
msbuild /property:Configuration=release Microsoft.Bot.Connector.Common.csproj
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Connector.Common.dll).FileVersionInfo.FileVersion"') do set version=%%v
..\..\packages\NuGet.CommandLine.3.5.0\tools\NuGet.exe pack Microsoft.Bot.Connector.Common.nuspec -symbols -properties version=%version% -OutputDirectory ..\nuget
echo *** Finished building Microsoft.Bot.Connector.Common
