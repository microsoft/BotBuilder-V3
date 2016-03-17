@ECHO OFF
IF /i [%1] == [docs] GOTO :docs
IF /i [%1] == [npm] GOTO :npm
ECHO usage:
ECHO To compile docs type "build-library.cmd docs"
GOTO :end

:docs
ECHO [COMPILING DOCS]
typedoc --includeDeclarations --module amd --out doc ..\src\botbuilder.d.ts --theme .\botframework --hideGenerator --name "BotBuilder SDK Reference Library" --readme none
GOTO :end

:end