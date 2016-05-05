@echo off
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
erase /s *.nupkg
msbuild /property:Configuration=release Microsoft.Bot.Builder.csproj 
msbuild /property:Configuration=release ..\tools\rview\rview.csproj
for /f "skip=1 tokens=3-6 delims=:. " %%i in ('version bin\release\Microsoft.Bot.Builder.dll') do (
	set version=%%i.%%j.%%k.%%l
	goto skip
)
:skip
..\packages\NuGet.CommandLine.3.4.3\tools\NuGet.exe pack Microsoft.Bot.Builder.nuspec -symbols -properties version=%version%
