@ECHO OFF
IF /i [%1] == [docs] GOTO :docs
IF /i [%1] == [npm] GOTO :npm
ECHO usage:
ECHO To compile docs type "build-library.cmd docs"
ECHO To just prepare the node module for publishing type "build-library.cmd npm"
ECHO To prepare and publish the node module type "build-library.cmd npm publish"
GOTO :end

:docs
ECHO [COMPILING DOCS]
typedoc --includeDeclarations --module amd --out doc index.d.ts --theme .\botframework --hideGenerator --name "BotBuilder SDK Reference Library" --readme none
GOTO :end

:npm
ECHO [PREPARING NPM]
IF NOT EXIST target md target
XCOPY /Q /Y package.json target
XCOPY /Q /Y index.d.ts target 
IF /i NOT [%2] == [publish] GOTO :end
ECHO:
ECHO [PUBLISHING NPM]
GOTO :end

:end