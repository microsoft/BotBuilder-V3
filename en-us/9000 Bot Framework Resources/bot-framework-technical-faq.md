---
layout: page
title: Bot Framework Technical FAQ
permalink: /en-us/technical-faq/
weight: 9350
parent1: Bot Framework Resources
---

* TOC
{:toc}

## My bot is stuck!  How can I reset the conversation?

You can use the command "/deleteprofile" to delete the User/PrivateConversation bot data bag state and reset your bot.  Note, some channels interpret slash-commands natively, so it may be necessary to send the command with a space in front (" /deleteprofile")

## How can I reset the Bot Framework Emulator's settings?

You can remove the emulator.service file in %temp% with "erase %temp%\emulator.service".

## How can I troubleshoot my Bot?

### Try your Bot's web service in the emulator first.

The emulator allows local debugging of your Bot logic, before deploying to the cloud.

1. Using a blank "Microsoft App Id" and "Microsoft App Password" in the emulator and web.config (while keeping the BotAuthentication attribute) will allow you to remove authentication as a possible issue.
2. Since the exchange of chat messages will likely result in several HTTPS method calls, it's important to make sure that they all succeed.  You can see the result of every HTTPS method call by selecting the different items in the list view on the left-hand side of the emulator window.

### Deploy your code to Azure next.

Test your connection to the Bot Framework in the [developer dashboard](https://docs.botframework.com/en-us/csharp/builder/sdkreference/gettingstarted.html#testing).  If your bot web service encounters issues as you move from running on your development machine to the cloud, you might consider using this guide: [Troubleshoot a web app in Azure App Service using Visual Studio](https://azure.microsoft.com/en-us/documentation/articles/web-sites-dotnet-troubleshoot-visual-studio/). 

### Finally, try your Bot on Skype.

This will help you validate the end-to-end user experience.

### Consider adding channels with additional authentication requirements.

Once channels like Skype work, you can try additional channels like DirectLine or Web chat.

## How can I troubleshoot my Bot's authentication?

Take a look at [Troubleshooting Bot Framework Authentication](/en-us/support/troubleshooting-bot-framework-authentication/).

## How can I troubleshoot my Bot using the C# Bot Builder SDK?

### Look for exceptions.

In Visual Studio 2015, go to <code>Debug|Windows|Exception Settings</code> and select the "Break When Thrown" checkbox next to "Common Language Runtime Exceptions".  You may also see diagnostics output in your "Output Window" when there are thrown or unhandled exceptions.

### Look at the call stack.

In Visual Studio, you can choose whether you're debugging [Just My Code](https://msdn.microsoft.com/en-us/library/dn457346.aspx) or not.  Seeing the full call stack may provide more insight into any issues.

### Ensure all dialog methods end with a plan to handle the next message.

All IDialog methods should complete with IDialogStack.Call, IDialogStack.Wait, or IDialogStack.Done.  These IDialogStack Call/Wait/Done methods are exposed through the IDialogContext passed to every IDialog method.  Calling IDialogStack.Forward and using the system prompts through the PromptDialog static methods will call one of these methods in their implementation.

### Ensure that all dialogs are serializable.

This can be as simple as using the [Serializable] attribute on your IDialog implementations.  But beware that anonymous method closures are not serializable if they reference their outside environment to capture variables.  We also support a reflection-based serialization surrogate to help serialize types not marked as serializable.

## What is the model for how user messages relate to HTTPS method calls?

When the user sends a message over a channel, the Bot Framework web service will make an HTTPS post to your Bot's web service endpoint.  During this HTTPS method call, your Bot's code may send zero, one, or many messages back to the user on that channel.  Each one of these messages is a separate HTTPS post back to the Bot Framework.

## How can I guarantee message delivery order?

The Bot Framework will preserve message ordering to the extent possible, such that if you wait for the completion of the http operation to send message A before initiating another http operation to send message B, we will respect the ordering that A comes before B .  In general, however, you cannot guarantee message delivery order, as delivery is done by the channel, and the channel may reorder messages.  For example, you have likely seen email and text messages being delivered out of order.  You might choose to put a time delay between your messages as a mitigation.

## What is the difference between the "Bot Url" and "Emulator Url" in the emulator?

The "Bot Url" is the web service url that executes your Bot's code.  The "Emulator Url" is a *different* web service url that emulates the Bot Framework Connector service.  They should not be the same.  Generally, you will want to leave the "Emulator Url" as the default value, and update the "Bot Url" to the url for your Bot web service.


## What is the difference between the Connector and Builder library in the SDK?

The Connector library is the exposition of the REST API.  The Builder library adds the conversational dialog programming model and other features (e.g. prompts, waterfalls, chains, guided form filling) and access to cognitive services (e.g. LUIS).

## How can I get access to the user id?

SMS and email will provide the user id raw in the from.Id field.  In Skype we give you a unique ID for the user which is different from the Skype ID. If you need to connect to an existing account you can use a sign in card and implement your own oauth flow to connect the user ID to your own service's user ID.

## How can I limit access to my bot to a pre-determined list of users?

Some channels provide unscoped addresses. SMS and email are examples. The ID appears raw in the from.Id feld.

Other channels give you either scoped or tenanted addresses (e.g. Skype, Facebook, Slack), and they typically do so in a way that prevents the bot from predicting the user’s ID ahead of time.  For these channels, you need to authenticate the user on your own (via a login link or shared secret) before you know whether or not they are authorized to use the bot.

## How can I intercept all messages between the user and my bot?

In C#, you can provide implementations of the IPostToBot and IBotToUser interfaces to the Autofac dependency injection container.  In Node, you can use middleware for much the same purpose.

## Why are my Facebook user names not showing anymore?

Did you change your Facebook password?  This will invalidate the access token, and you will have to update the Facebook channel registration.

## Why doesn't the Typing activity do anything?

Some channels don't support transient typing updates in their client.

## Why are parts of my message text being dropped?

The Bot Framework and many Channels interpret text as Markdown.  Check to see if your text uses reserved Markdown characters.

## How can I use authenticated services from my Bot?

1. the [AuthBot NuGet](https://www.nuget.org/packages/AuthBot) library is extremely helpful for Azure Active Directory authentication.
2. the [Builder GitHub samples](https://github.com/Microsoft/BotBuilder/tree/master/CSharp/Samples) has examples for Facebook authentication.

## How can I reference non-serializable services from my C# dialogs?

There are a few options:

1. don't store that dependency so it won't be serialized (probably not helpful, but it's an option)
2. use the reflection serialization surrogate (still may not work and risks serializing too much)
3. resolve the dependency through Autofac and [FiberModule.Key_DoNotSerialize](https://github.com/Microsoft/BotBuilder/blob/master/CSharp/Library/Fibers/FiberModule.cs#L59) (cleanest solution)
4. Use [NonSerialized](https://msdn.microsoft.com/en-us/library/system.nonserializedattribute(v=vs.110).aspx) and [OnDeserialized](https://msdn.microsoft.com/en-us/library/system.runtime.serialization.ondeserializedattribute(v=vs.110).aspx) attributes to restore the dependency on deserialization (simplest solution)

## What is the difference between "proactive" and "reactive"?

The term "reactive" means the "normal" situation, where the user sends the first message in a conversation to the bot and the bot responds to that message (the conversation is created by the user's initial message).  The term "proactive" means there isn’t an initial message from the user to the bot, so the bot must create the conversation.

## What is an ETag?  How does it relate to bot data bag storage?

An [ETag](https://en.wikipedia.org/wiki/HTTP_ETag) is a mechanism for [optimistic concurrency control](https://en.wikipedia.org/wiki/Optimistic_concurrency_control).  The bot data bag storage uses ETags to prevent conflicting updates to the data.  If you encounter an ETag "precondition failed" http error, it means that there were multiple "read-modify-write" sequences executing concurrently for that bot data bag.

The dialog stack and state are stored in these bot data bags.  For example, you might see the ETag precondition failed error if your bot is still processing a previous message when it receives a new message for that conversation.

## Is there a limit on the amount of data I can store using the State API?

Yes, each state store (e.g. User store, Conversation store, etc.) may be up to 32kb [see Bot State API](https://docs.botframework.com/en-us/restapi/state/).

## What are the possible machine-readable resolutions of the LUIS builtin date, time, duration, and set entities?

There is a list of examples available in the [Pre-built entities section](https://www.luis.ai/Help/#PreBuiltEntities) of the LUIS documentation.

## How can I use more than the maximum number of LUIS intents?

You might consider splitting up your model and calling the LUIS service in series or parallel.

## How can I use more than one LUIS model?

Both the Node and C# versions of the SDK support calling multiple LUIS models from a single LUIS intent dialog.

There are few caveats to keep in mind:

1. Using multiple LUIS models assumes the LUIS models have non-overlapping sets of intents.
2. Using multiple LUIS models assumes the scores from different models are comparable, to select the “best matched intent” across multiple models.
3. Using multiple LUIS models means that if an intent matches one model, it will also strongly match the "none" intent of the other models.  You can avoid selecting the "none" intent in this situation (the Node SDK will automatically scale down the score for "none" intents to avoid this issue).

## Where can I get more help on LUIS?

You can access LUIS experts at the [Language Understanding Intelligent Service Forum](https://social.msdn.microsoft.com/forums/azure/en-US/home?forum=LUIS).

## What are some community-authored dialogs?

* [AuthBot](https://www.nuget.org/packages/AuthBot) - Azure Active Directory authentication
* [BestMatchDialog](http://www.garypretty.co.uk/2016/08/01/bestmatchdialog-for-microsoft-bot-framework-now-available-via-nuget/) - regular expression-based dispatch of user text to dialog methods

## Why is my Kik bot replying <code>I'm sorry, I can't talk right now.</code>?

Be default bots in development on Kik are allowed 50 subscribers. After 50 unique users have chatted with your bot, any new user attempting to chat will get the <code>I'm sorry, I can't talk right now</code>. This is mentioned in Kik's [Getting Started](https://dev.kik.com/#/docs/getting-started).

## Where can I get more help?

The [GitHub issues](https://github.com/Microsoft/BotBuilder/issues) has an active forum.  [Gitter](https://gitter.im/Microsoft/BotBuilder) has active discussions.  [Stack Overflow](http://stackoverflow.com/questions/tagged/botframework) has a list of tagged Bot Framework questions.
