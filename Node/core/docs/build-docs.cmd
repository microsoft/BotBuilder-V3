@ECHO OFF
ECHO [COMPILING DOCS]
typedoc --includeDeclarations --module amd --out doc ..\lib\botbuilder.d.ts --theme .\botframework --hideGenerator --name "Bot Builder SDK Chat Reference Library" --readme none
