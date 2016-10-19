@echo off
echo *** Building Microsoft.Bot.Builder.Calling
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Builder.Calling*nupkg
msbuild /property:Configuration=release Microsoft.Bot.Builder.Calling.csproj
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.dll).FileVersionInfo.FileVersion"') do set builder=%%v
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.Calling.dll).FileVersionInfo.FileVersion"') do set version=%%v
..\..\packages\NuGet.CommandLine.3.4.3\tools\NuGet.exe pack Microsoft.Bot.Builder.Calling.nuspec -symbols -properties version=%version%;builder=%builder% -OutputDirectory ..\nuget
echo *** Finished building Microsoft.Bot.Builder.Calling

