# Microsoft Bot Builder Overview

Microsoft Bot Builder is a powerful framework for constructing bots that can handle both freeform interactions and more guided ones where the possibilities are explicitly shown to the user. It is easy to use and leverages C# to provide a natural way to write Bots.

High Level Features:
* Powerful dialog system with dialogs that are isolated and composable.  
* Built-in dialogs for simple things like Yes/No, strings, numbers, enumerations.  
* Built-in dialogs that utilize powerful AI frameworks like [LUIS](http://luis.ai)
* Bots are stateless which helps them scale.  
* Form Flow for automatically generating a Bot from a C# class for filling in the class and that supports help, navigation, clarification and confirmation.

[Get started with the Bot Builder!](http://docs.botframework.com/sdkreference/csharp/)

There are six samples in this directory.
* [Microsoft.Bot.Sample.SimpleEchoBot](SimpleEchoBot/) -- Bot Connector example done with the Bot Builder framework.
* [Microsoft.Bot.Sample.EchoBot](EchoBot/) -- Add state onto the previous example.
* [Microsoft.Bot.Sample.SimpleSandwichBot](SimpleSandwichBot/) -- FormFlow example of how easy it is to create a rich dialog with guided conversation, help and clarification. 
* [Microsoft.Bot.Sample.AnnotatedSandwichBot](AnnotatedSandwichBot/) -- Builds on the previous example to add attributes, messages, confirmation and business logic.
* [Microsoft.Bot.Sample.SimpleAlarmBot](SimpleAlarmBot/) -- Integration of http://luis.ai with the dialog system to set alarms.
* [Microsoft.Bot.Sample.PizzaBot](PizzaBot/) -- Integration of http://luis.ai with FormFlow.
* [//Build/ 2016](Build-2016/) -- Sample application used in the //Build/ 2016 session [Building a Conversational Bot: From 0 to 60](https://channel9.msdn.com/Events/Build/2016/B821) 