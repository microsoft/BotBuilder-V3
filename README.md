# V3 Deprecation Notification

Microsoft Bot Framework SDK V4 was released in September 2018, and since then we have shipped a few dot-release improvements. As announced previously, the V3  SDK is being retired with final long-term support ending on December 31st, 2019.
Accordingly, there will be no more development in this repo. **Existing V3 bot workloads will continue to run without interruption. We have no plans to disrupt any running workloads**.

We highly recommend that you start migrating your V3 bots to V4. In order to support this migration we have produced migration documentation and will provide extended support for migration initiatives (via standard channels such as Stack Overflow and Microsoft Customer Support).

For more information please refer to the following references:
* Migration Documentation: https://aka.ms/v3v4-bot-migration
* End of lifetime support announcement: https://aka.ms/bfmigfaq
* Primary V4 Repositories to develop Bot Framework bots
  * [Botbuilder for dotnet](https://github.com/microsoft/botbuilder-dotnet)
  * [Botbuilder for JS](https://github.com/microsoft/botbuilder-js) 
* QnA Maker Libraries were replaced with the following V4 libraries:
  * [Libraries for dotnet](https://github.com/Microsoft/botbuilder-dotnet/tree/master/libraries/Microsoft.Bot.Builder.AI.QnA)
  * [Libraries for JS](https://github.com/Microsoft/botbuilder-js/blob/master/libraries/botbuilder-ai/src/qnaMaker.ts)
* Azure Libraries were replaced with the following V4 libraries:
  * [Botbuilder for JS Azure](https://github.com/Microsoft/botbuilder-js/tree/master/libraries/botbuilder-azure)
  * [Botbuilder for dotnet Azure](https://github.com/Microsoft/botbuilder-dotnet/tree/master/libraries/Microsoft.Bot.Builder.Azure)


# Bot Builder SDK 

If you are new to the Bot Builder SDK, we strongly encourage you to build your bot using the [v4 SDK](https://github.com/Microsoft/botbuilder).  

This repo contains version 3.

The Bot Builder SDK enables you to build bots that support different types of interactions with users. You can design conversations in your bot to be freeform. Your bot can also have more guided interactions where it provides the user choices or actions. The conversation can use simple text or more complex rich cards that contain text, images, and action buttons. You can add natural language interactions and questions and answers, which let your users interact with your bots in a natural way.

![Bot Framework](https://botframework.blob.core.windows.net/web/images/bot-framework.png)

The Bot Builder includes a set of [command line tools](https://github.com/microsoft/botbuilder-tools) to streamline end-to-end conversation centric development experience, and an [emulator](https://github.com/microsoft/botframework-emulator) for debugging your bot locally or in the cloud. 

You can create a bot with Bot Builder v3 SDK using your favorite language: 
- [.NET](https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-3.0)
- [JavaScript](https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-3.0)

## Documentation
Visit azure.com for the primary [Azure Bot Service documentation page](https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-3.0) to learn about building bots using Bot Builder. There is additional documentation on the SDK, oriented towards contributors. The v3 SDK currently supports two programing language: 
- [.NET](https://github.com/Microsoft/BotBuilder-v3/tree/master/CSharp)
- [JavaScript](https://github.com/Microsoft/BotBuilder-V3/tree/master/Node)

## Samples
Bot builder SDK v3 includes samples for all supported languages:
- [.NET](https://github.com/Microsoft/BotBuilder-Samples/tree/v3-sdk-samples/CSharp)
- [JavaScript](https://github.com/Microsoft/BotBuilder-Samples/tree/v3-sdk-samples/Node)

## Questions and Help 
If you have questions about Bot Builder SDK v3 or using Azure Bot Service, we encourage you to reach out to the community and Azure Bot Service dev team for help.
- For questions which fit the Stack Overflow format ("how does this work?"), we monitor the both [Azure-bot-service](https://stackoverflow.com/questions/tagged/azure-bot-service) and [bot framework](https://stackoverflow.com/questions/tagged/botframework) tags (search [both](https://stackoverflow.com/questions/tagged/azure-bot-service+or+botframework))
- You can also tweet/follow [@msbotframework](https://twitter.com/msbotframework) 

While we do our best to help out on a timely basis, we don't have any promise around the above resources. If you need an SLA on support from us, it's recommended you invest in an [Azure Support plan](https://azure.microsoft.com/en-us/support/options/).

## Issues and feature requests 
We track functional issues and features asks for and Bot Builder and Azure Bot Service in a variety of locations. If you have found an issue or have a feature request, please submit an issue to the below repositories.

|Item|Description|Link|
|----|-----|-----|
|SDK v3 (.NET and JS)| core bot runtime, abstractions, prompts, dialogs, FormFlow, etc. | [File an issue](https://github.com/Microsoft/BotBuilder-V3/issues) |
|Documentation | Docs for Bot Builder and Azure Bot Service | [File an issue](https://github.com/Microsoft/BotBuilder-V3/issues)|
|CLI tools| MSBot, chatdown, ludown, LUIS, LUISGen, QnA Maker, dispatch  | [File an issue](https://github.com/microsoft/botbuilder-tools/issues)|
|Emulator| view transcripts, connect to services, debug your bot | [File an issue](https://github.com/Microsoft/BotFramework-Emulator/issues)| 

## Helpful links
### GitHub repositories 
- [SDK v3 (.NET and node)](https://github.com/Microsoft/BotBuilder-V3/)
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

Get started quickly with our samples:

* Bot Builder samples [GitHub repo](https://github.com/Microsoft/BotBuilder-Samples)
* More samples are available within the SDK [C#](https://github.com/Microsoft/BotBuilder/tree/master/CSharp/Samples), [Node.js](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples)

Join the conversation on **[Gitter](https://gitter.im/Microsoft/BotBuilder)**.

See all the support options **[here](https://docs.microsoft.com/en-us/bot-framework/resources-support)**.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
