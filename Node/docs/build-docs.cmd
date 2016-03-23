@ECHO OFF
ECHO [COMPILING DOCS]
typedoc --includeDeclarations --module amd --out doc ..\src\botbuilder.d.ts --theme .\botframework --hideGenerator --name "Bot Builder SDK Reference Library" --readme none
