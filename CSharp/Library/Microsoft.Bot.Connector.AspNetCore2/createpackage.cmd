@echo off
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Connector.aspnetcore.2.*.nupkg
erase /s *.nupkg
msbuild /property:Configuration=release Microsoft.Bot.Connector.AspNetCore2.csproj 
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\netstandard2.0\Microsoft.Bot.Connector.AspNetCore.dll).FileVersionInfo.FileVersion"') do set version=%%v
..\..\packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe pack Microsoft.Bot.Connector.AspNetCore2.nuspec -symbols -properties -Version=%version% -OutputDirectory ..\nuget
