call npm install replace@0.3.0

rem ..\..\packages\autorest.0.16.0\tools\AutoRest -namespace Microsoft.Bot.Connector -input swagger\ConnectorApi.json -outputDirectory ConnectorAPI -AddCredentials -ClientName ConnectorClient
call autorest --input-file=.\Swagger\ConnectorAPI.json --csharp --namespace:Microsoft.Bot.Connector --output-folder=ConnectorAPI --add-credentials --override-client-name=ConnectorClient --use-datetimeoffset

cd ConnectorAPI
call ..\node_modules\.bin\replace "Microsoft.Bot.Connector.Models" "Microsoft.Bot.Connector" . -r --include="*.cs"
call ..\node_modules\.bin\replace "using Models;" ""  . -r --include="*.cs"
call ..\node_modules\.bin\replace "FromProperty" "From" . -r --include="*.cs"
call ..\node_modules\.bin\replace "fromProperty" "from" . -r --include="*.cs"
cd ..

rem call AutoRest --namespace Microsoft.Bot.Connector.Payments --input-file=swagger\Connector-Payments.json --output-folder=Payments --add-credentials --override-client=PaymentsClient --use-datetimeoffset
rem cd Payments\Models\
rem call ..\..\node_modules\.bin\replace "namespace Microsoft.Bot.Connector.Payments.Models" "namespace Microsoft.Bot.Connector.Payments" . -r --include="*.cs"
rem call ..\..\node_modules\.bin\replace "using Models;" "" . -r --include="*.cs"
rem call ..\..\node_modules\.bin\replace "using Microsoft.Rest;" "" . -r --include="*.cs"
rem rem call ..\..\node_modules\.bin\replace "using Microsoft.Rest.Serialization;" "" . -r --include="*.cs"
rem cd ..\..

rem ..\..\packages\autorest.0.16.0\tools\AutoRest -namespace Microsoft.Bot.Connector -input swagger\StateAPI.json -outputDirectory StateApi -AddCredentials -ClientName StateClient
call autorest --input-file=.\Swagger\StateAPI.json --csharp --namespace:Microsoft.Bot.Connector --output-folder=StateAPI --add-credentials --override-client-name=StateClient --use-datetimeoffset
cd StateAPI
erase /q Models\Error*.cs
call ..\node_modules\.bin\replace "Microsoft.Bot.Connector.Models" "Microsoft.Bot.Connector" . -r --include="*.cs"
call ..\node_modules\.bin\replace "using Models;" "" . -r --include="*.cs"
cd ..


@echo !!!!! Please review ConversationsExtensions.cs and BotStateExtensions.cs for custom throw code
pause