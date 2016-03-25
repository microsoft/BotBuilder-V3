erase /s *.nupkg
..\packages\NuGet.CommandLine.3.3.0\tools\NuGet.exe pack Microsoft.Bot.Builder.csproj -symbols -build -properties Configuration=release
