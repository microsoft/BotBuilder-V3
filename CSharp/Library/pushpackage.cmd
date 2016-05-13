@if "%1" == "" goto  :needpackage
@if "%2" == "" goto :needfeed
..\packages\NuGet.CommandLine.3.4.3\tools\nuget push %1 -Source https://fuselabs.pkgs.visualstudio.com/DefaultCollection/_packaging/packages/nuget/v3/index.json -ApiKey %2
@goto end

:needpackage
@echo "You need to pass a package as first parameter"
@goto end

:needfeed
@echo "You need to pass a feed name, either: fuse or fusesymbols"

:end