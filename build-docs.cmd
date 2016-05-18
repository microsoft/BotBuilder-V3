@ECHO OFF
IF /i [%1] == [local] GOTO :local
IF /i [%1] == [fuseserver01] GOTO :fuseserver01
ECHO usage:
ECHO To build the Jekyll site and debug locally "build-docs local"
ECHO To build the jekyll site and copy it to http://fuseserver01/ "build-docs fuseserver01"
GOTO :end

:local
cls
ECHO [Building docs and serving them locally]
ECHO [Wait until the localhost url is ready (it might take a while), then copy the url and visit it with the browser]
call bundle exec jekyll serve --watch --incremental
GOTO :end

:fuseserver01
cls
ECHO [Building docs and copying them on http://fuseserver01/]
call bundle exec jekyll build
robocopy _site "\\fuseserver01\wwwroot" /mir
cls
ECHO [Your site is now ready at http://fuseserver01/]
pause
GOTO :end

:end