# Bot Builder SDK 

The Bot Builder SDK enables you to build bots that support different types of interactions with users. You can design conversations in your bot to be freeform. Your bot can also have more guided interactions where it provides the user choices or actions. The conversation can use simple text or more complex rich cards that contain text, images, and action buttons. You can add natural language interactions and questions and answers, which let your users interact with your bots in a natural way.

![Bot Framework](https://botframework.blob.core.windows.net/web/images/bot-framework.png)

The Bot Builder includes a set of [command line tools](https://github.com/microsoft/botbuilder-tools) to streamline end-to-end conversation centric development experience, and an [emulator](https://github.com/microsoft/botframework-emulator) for debugging your bot locally or in the cloud. 

## Get started with Bot Builder v4 (Preview) 

*Bot Builder SDK v4 is the latest SDK for building bot applications. It is in **Preview** state. Production bots should continue to be developed using the **v3 SDK** - [csharp](CSharp), [node](Node).*

It is easy to build your first Bot. You can create a bot with [Azure Bot Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart?view=azure-bot-service-4.0). Click [here](https://account.azure.com/signup) if you need a trial Azure subscription. 

You can create a bot with Bot Builder SDK using your favorite language: 
- [.NET](https://docs.microsoft.com/en-us/azure/bot-service/dotnet/bot-builder-dotnet-sdk-quickstart?view=azure-bot-service-4.0)
- [JavaScript](https://docs.microsoft.com/en-us/azure/bot-service/javascript/bot-builder-javascript-quickstart?view=azure-bot-service-4.0)
- [Python](https://docs.microsoft.com/en-us/azure/bot-service/python/bot-builder-python-quickstart?view=azure-bot-service-4.0)
- [Java](https://docs.microsoft.com/en-us/azure/bot-service/java/bot-builder-java-quickstart?view=azure-bot-service-4.0)

## Documentation
Visit azure.com for the primary [Azure Bot Service documentation page](https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0) to learn about building bots using Bot Builder. There is additional documentation on the SDK, oriented towards contributors. The SDK currently supports four programing language: 
- [.NET](https://github.com/Microsoft/botbuilder-dotnet/wiki)
- [JavaScript](https://github.com/microsoft/botbuilder-js/wiki)
- [Python](https://github.com/Microsoft/botbuilder-python/wiki/Overview)
- [Java](https://github.com/Microsoft/botbuilder-java/wiki)

## Samples
Bot builder SDK v4 (preview) includes samples for all supported languages:
-  [.NET](https://github.com/Microsoft/botbuilder-dotnet/tree/master/samples-final)
-  [JavaScript](https://github.com/Microsoft/botbuilder-js/tree/master/samples)
 - [Python](https://github.com/Microsoft/botbuilder-python/tree/master/samples)
- [Java](https://github.com/Microsoft/botbuilder-java/tree/master/samples)

## Questions and Help 
If you have questions about Bot Builder SDK v3 or v4 Preview or using Azure Bot Service, we encourage you to reach out to the community and Azure Bot Service dev team for help.
- For questions which fit the Stack Overflow format ("how does this work?"), we monitor the both [Azure-bot-service](https://stackoverflow.com/questions/tagged/azure-bot-service) and [bot framework](https://stackoverflow.com/questions/tagged/botframework) tags (search [both](https://stackoverflow.com/questions/tagged/azure-bot-service+or+botframework))
- You can also tweet/follow [@msbotframework](https://twitter.com/msbotframework) 

While we do our best to help out on a timely basis, we don't have any promise around the above resources. If you need an SLA on support from us, it's recommended you invest in an [Azure Support plan](https://azure.microsoft.com/en-us/support/options/).

## Issues and feature requests 
We track functional issues and features asks for and Bot Builder and Azure Bot Service in a variety of locations. If you have found an issue or have a feature request, please submit an issue to the below repositories.

|Item|Description|Link|
|----|-----|-----|
|SDK v3 (.NET and JS)| core bot runtime, abstractions, prompts, dialogs, FormFlow, etc. | [File an issue](https://github.com/Microsoft/BotBuilder/issues) |
|SDK v4 .net| core bot runtime for .NET, connectors, middleware, dialogs, prompts, LUIS and QnA| [File an issue](https://github.com/Microsoft/botbuilder-dotnet/tree/master/libraries) |
|SDK v4 JavaScript| core bot runtime for JavaScript, connectors, middleware, dialogs, prompts, LUIS and QnA | [File an issue](https://github.com/Microsoft/botbuilder-js/issues) |
|SDK v4 Python| core bot runtime for Python, connectors, middleware, dialogs, prompts, LUIS and QnA | [File an issue](https://github.com/Microsoft/botbuilder-python/issues) |
|SDK v4 Java| core bot runtime for Java, connectors, middleware, dialogs, prompts, LUIS and QnA | [File an issue]( https://github.com/Microsoft/botbuilder-java/issues)|
|Documentation | Docs for Bot Builder and Azure Bot Service | [File an issue](https://github.com/Microsoft/BotBuilder/issues)|
|CLI tools| MSBot, chatdown, ludown, LUIS, LUISGen, QnA Maker, dispatch  | [File an issue](https://github.com/microsoft/botbuilder-tools/issues)|
|Emulator| view transcripts, connect to services, debug your bot | [File an issue](https://github.com/Microsoft/BotFramework-Emulator/issues)| 

## Helpful links
### GitHub repositories 
- [SDK v3 (.NET and node)](https://github.com/Microsoft/BotBuilder/tree/master/CSharp)
- [SDK v4 - .NET](https://github.com/Microsoft/botbuilder-dotnet)
- [SDK v4 - JavaScript](https://github.com/Microsoft/botbuilder-js)
- [SDK v4 - Python](https://github.com/Microsoft/botbuilder-python)
- [SDK v4 - Java](https://github.com/Microsoft/botbuilder-java)
- [Bot Builder tools](https://github.com/Microsoft/botbuilder-tools)
- [Bot Builder Emulator](https://github.com/Microsoft/BotFramework-Emulator) 

### Documentation 
- [SDK v3](https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-3.0)
- [SDK v4](https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0)

## Adding intelligence to your bot
Your bot can provide a great conversational experience without using any Azure Cognitive Services. You can increase your customers' delight with adding a more natural interaction using one or multiple Azure Cognitive Services.  The following are common services integrated to bots: 
- [LUIS](https://www.luis.ai)
- [QnA Maker](https://www.qnamaker.ai/)
- [Speech](https://azure.microsoft.com/services/cognitive-services/directory/speech/)
- [Personality Chat](https://github.com/Microsoft/BotBuilder-PersonalityChat) - Handles Small-Talk/Chitchat for any bot, in line with a distinct personality.
- all [Azure Cognitive Services](https://azure.microsoft.com/services/cognitive-services/)

## Bot Builder SDK v3
Production bots should continue to be developed using the [v3 SDK](https://github.com/Microsoft/BotBuilder/tree/master/CSharp).

**[Review the documentation](http://docs.microsoft.com/en-us/bot-framework)** to get started with the Bot Builder SDK!

Get started quickly with our samples:

* Bot Builder samples [GitHub repo](https://github.com/Microsoft/BotBuilder-Samples)
* More samples are available within the SDK [C#](https://github.com/Microsoft/BotBuilder/tree/master/CSharp/Samples), [Node.js](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples)

Join the conversation on **[Gitter](https://gitter.im/Microsoft/BotBuilder)**.

See all the support options **[here](https://docs.microsoft.com/en-us/bot-framework/resources-support)**.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
