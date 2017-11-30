@echo off
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Connector*nupkg
msbuild /property:Configuration=release /p:DefineConstants="NET45" Microsoft.Bot.Connector.NetFramework.csproj 
msbuild /property:Configuration=release ..\Microsoft.Bot.Connector.NetCore\Microsoft.Bot.Connector.NetCore.csproj 
msbuild /property:Configuration=release ..\Microsoft.Bot.Connector.Standard\Microsoft.Bot.Connector.Standard.csproj 
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Connector.dll).FileVersionInfo.FileVersion"') do set version=%%v
..\..\packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe pack Microsoft.Bot.Connector.nuspec -symbols -properties version=%version% -OutputDirectory ..\nuget
