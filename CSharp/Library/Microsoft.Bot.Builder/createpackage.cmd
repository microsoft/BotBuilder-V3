@echo off
echo *** Building Microsoft.Bot.Builder
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Builder*.nupkg
msbuild /property:Configuration=release /p:DefineConstants="NET45" ..\Microsoft.Bot.Connector.NetFramework\Microsoft.Bot.Connector.csproj
msbuild /property:Configuration=release ..\Microsoft.Bot.Builder.Autofac\Microsoft.Bot.Builder.Autofac.csproj
msbuild /property:Configuration=release Microsoft.Bot.Builder.csproj
msbuild /property:Configuration=release ..\..\tools\rview\rview.csproj
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.dll).FileVersionInfo.FileVersion"') do set builderVersion=%%v
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Connector.Common.dll).FileVersionInfo.FileVersion"') do set connectorCommonVersion=%%v
for /f %%v in ('powershell -noprofile "(Get-Command ..\Microsoft.Bot.Connector.NetFramework\bin\release\Microsoft.Bot.Connector.dll).FileVersionInfo.FileVersion"') do set connectorVersion=%%v
..\..\packages\NuGet.CommandLine.3.5.0\tools\NuGet.exe pack Microsoft.Bot.Builder.Common.nuspec -symbols -properties version=%builderVersion%;connectorCommon=%connectorCommonVersion% -OutputDirectory ..\nuget
..\..\packages\NuGet.CommandLine.3.5.0\tools\NuGet.exe pack Microsoft.Bot.Builder.nuspec -properties version=%builderVersion%;connector=%connectorVersion% -OutputDirectory ..\nuget
echo *** Finished building Microsoft.Bot.Builder
