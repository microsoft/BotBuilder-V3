@if "%1" == "" goto  :needpackage
..\..\packages\NuGet.CommandLine.3.4.3\tools\nuget.exe push -Source "packages" -ApiKey VSTS %1
@goto end

:needpackage
@echo "You need to pass a package as first parameter"
@goto end

:end