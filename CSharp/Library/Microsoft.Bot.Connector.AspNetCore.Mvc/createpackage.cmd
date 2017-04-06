@echo off
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Connector.AspNetCore.Mvc*nupkg
msbuild /property:Configuration=release Microsoft.Bot.Connector.AspNetCore.Mvc.csproj
msbuild /property:Configuration=release ..\Microsoft.Bot.Connector.AspNetCore.Mvc.NetFramework\Microsoft.Bot.Connector.AspNetCore.Mvc.NetFramework.csproj
msbuild /property:Configuration=release ..\Microsoft.Bot.Connector.Common\Microsoft.Bot.Connector.Common.csproj

for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Connector.AspNetCore.Mvc.dll).FileVersionInfo.FileVersion"') do set version=%%v
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Connector.Common.dll).FileVersionInfo.FileVersion"') do set connectorCommonVersion=%%v
..\..\packages\NuGet.CommandLine.3.5.0\tools\NuGet.exe pack Microsoft.Bot.Connector.AspNetCore.Mvc.nuspec -symbols -properties version=%version%;connectorCommonVersion=%connectorCommonVersion% -OutputDirectory ..\nuget
