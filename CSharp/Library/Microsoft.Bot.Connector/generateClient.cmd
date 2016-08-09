rd /s /q Client
rd /s /q NodeJS
rd /s /q Azure.NodeJS

..\..\packages\autorest.0.16.0\tools\AutoRest -namespace Microsoft.Bot.Connector -input swagger\ConnectorApi.json -outputDirectory ConnectorAPI -AddCredentials -ClientName ConnectorClient
..\..\packages\autorest.0.16.0\tools\AutoRest -namespace Microsoft.Bot.Connector -input swagger\StateAPI.json -outputDirectory StateApi -AddCredentials -ClientName StateClient

cd ConnectorAPI
..\..\..\rep -r -find:"Microsoft.Bot.Connector.Models" -replace:"Microsoft.Bot.Connector" *.cs
..\..\..\rep -r -find:"using Models;" -replace:"" *.cs
..\..\..\rep -r -find:FromProperty -replace:From *.cs
..\..\..\rep -r -find:fromProperty -replace:from *.cs
cd ..
cd StateAPI
..\..\..\rep -r -find:"Microsoft.Bot.Connector.Models" -replace:"Microsoft.Bot.Connector" *.cs
..\..\..\rep -r -find:"using Models;" -replace:"" *.cs
cd ..
@echo !!!!! Please review ConversationsExtensions.cs and BotStateExtensions.cs for custom throw code
pause