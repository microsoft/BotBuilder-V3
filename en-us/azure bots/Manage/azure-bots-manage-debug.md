---
layout: page
parent1: Azure Bot Service
weight: 13025
permalink: /en-us/azure-bot-service/manage/debug/
parent2: Manage
title: Debugging your bot
---

 
The new Azure Bot Service bots are built on the Azure Functions Serverless Architecture, which provides a great pay-for-what-you-use, automatically scaling pricing model.

In the Azure Bot Service model, your bot's code starts in Azure, and then you can use the continuous integration features to take it offline and work with your favorite tool chain.  In this article we'll cover:


* TOC
{:toc}

## Debugging for Node.js Bots 

First we need to set up your environment.  We'll need:

1.  A local copy of your Azure Bot Service code - see the article on [Setting up Continuous Integration](/en-us/azure-bot-service/manage/setting-up-continuous-integration/)
2.  The Bot Framework Emulator for your platform ([Mac, Linux, Windows](https://docs.botframework.com/en-us/downloads/))
3.  Node, or a code editor such as Visual Studio Code ([Mac, Linux, Windows](https://code.visualstudio.com))

The simplest path to running locally is to start your bot in Node, and then
connect to it from the Bot Framework Emulator. You'll need to set an environment
variable before starting:

On a Mac, using the default Empty Bot repo layout:

[![](/en-us/images/azure-bots/mac-azureservice-debug-config.png)](/en-us/images/azure-bots/mac-azureservice-debug-config.png)

At this point the bot is running locally.  Copy the endpoint the bot is running at into the clipboard.  Now start the Bot Framework Emulator, and paste the URL into the address bar:

[![](/en-us/images/azure-bots/mac-azureservice-emulator-config.png)](/en-us/images/azure-bots/mac-azureservice-emulator-config.png)

You don't need security for local debugging, so leave the Microsoft App Id and Microsoft App Password fields blank, and hit **Connect**.  You now should be able to type a message to your bot in the lower left box labeled “Type your
message...”

[![](/en-us/images/azure-bots/mac-azureservice-debug-emulator.png)](/en-us/images/azure-bots/mac-azureservice-debug-emulator.png)


And you're good to go; you can send messages to and receive messages from your bot, and get detailed understanding of the message traffic from the Log and the Inspector.  You can also see the logs from the node runtime in your terminal
window:

[![](/en-us/images/azure-bots/mac-azureservice-debug-logging.png)](/en-us/images/azure-bots/mac-azureservice-debug-logging.png)

### Debugging with Visual Studio Code

If you need more than visual inspection and logs to diagnose your bot, you can use a local debugger such as Visual Studio Code.  All of the steps are the same, but instead of running the node runtime, you'll start the VS Code debugger.

To get started, load VS Code, and then open the folder that your repo is in.

[![](/en-us/images/azure-bots/mac-azureservice-debug-vs-config.png)](/en-us/images/azure-bots/mac-azureservice-debug-vs-config.png)

Switch to the debugging view, and hit go.  The first time it will ask you to pick a runtime engine to run your code; I'll pick node.

[![](/en-us/images/azure-bots/mac-azureservice-debug-vsruntime.png)](/en-us/images/azure-bots/mac-azureservice-debug-vsruntime.png)

After this, depending on whether or not you've synced the repo or changed any of it, you may get asked to configure the launch.json file.  When you do, you'll need code telling the template that you're going to work with the emulator.  Add
this to the configurations section:

{% highlight json %}
"env": {
    "NODE\_ENV": "development"
}
{% endhighlight %}

[![](/en-us/images/azure-bots/mac-azureservice-debug-launchjson.png)](/en-us/images/azure-bots/mac-azureservice-debug-launchjson.png)

Once that's done, save the launch.json file and hit go again.  Your bot should be running in the VS Code environment with Node.  You can open the debug console to see logging output, and set breakpoints as needed.

[![](/en-us/images/azure-bots/mac-azureservice-debug-vsrunning.png) ](/en-us/images/azure-bots/mac-azureservice-debug-vsrunning.png))

To interact with the bot, use the Bot Framework Emulator again.  Copy the endpoint from the debug console in VS code, and connect to it in the emulator. Start chatting with your bot and you should hit your breakpoints as appropriate.

[![](/en-us/images/azure-bots/mac-azureservice-debug-vsbreakpoint.png)](/en-us/images/azure-bots/mac-azureservice-debug-vsbreakpoint.png)

And that's it. VS Code is great as well as you can make locally within the editor during debugging, update your repo, and because you're using Azure Bot Service with continuous integration turned on, your bot in the cloud will
automatically pick up and start running your changes.

## Debugging C\# Bots built using the Azure Bot Service on Windows

The C\# environment in the Azure Bot Service has more in common with Node.JS than a typical C\# app as they require a runtime host, much like the Node engine.  In Azure, the runtime is part of the hosting environment in the cloud,
but on your desktop we'll need to replicate that environment locally.  This debugging configuration also only supports Windows.

We'll need a few things to get started:

1.  A local copy of your Azure Bot Service code - see the article on [Setting up Continuous Integration](/en-us/azure-bot-service/manage/setting-up-continuous-integration/)
2.  The Bot Framework Emulator ([Download](https://docs.botframework.com/en-us/downloads/))
3.  The Azure Functions CLI: ([Download](https://www.npmjs.com/package/azure-functions-cli))
4.  DotNet CLI ([Download](https://github.com/dotnet/cli))

and if you want breakpoint debugging in Visual Studio 15

5.  Visual Studio 15 - Community Edition will work fine ([Download](https://www.visualstudio.com/downloads/))
6.  The Command Task Runner Visual Studio Extension ([Download](https://visualstudiogallery.msdn.microsoft.com/e6bf6a3d-7411-4494-8a1e-28c1a8c4ce99))

<div class="docs-text-note"><strong>Note:</strong> VS Code is not currently supported, but stay tuned</div>

Once you've installed the tools above, you have everything you need to debug your C\# bot locally.  

First, open a command prompt to your repository directory to the folder where your project.json file lives.  Issue the command **dotnet restore** to restore the various packages referenced in your bot.

[![](/en-us/images/azure-bots/csharp-azureservice-debug-envconfig.png)](/en-us/images/azure-bots/csharp-azureservice-debug-envconfig.png)

From here you can immediately start your bot running locally. Change directory to the folder with project.json in it, and run debughost.cmd.  Your bot will be loaded and running and you should see the output of your log calls:

[![](/en-us/images/azure-bots/csharp-azureservice-debug-debughost.png)](/en-us/images/azure-bots/csharp-azureservice-debug-debughost.png)

After it's running, note the endpoint in the console, and then start the Bot Framework Emulator. Paste in the endpoint that the debughost is listening on (including /api/EmptyBot in the case of the empty bot template).

[![](/en-us/images/azure-bots/mac-azureservice-emulator-config.png)](/en-us/images/azure-bots/mac-azureservice-emulator-config.png)

You don't need security for local debugging, so leave the Microsoft App Id and Microsoft App Password fields blank, and hit **Connect**.  You now should be able to type a message to your bot in the lower left box labeled “Type your
message...”

[![](/en-us/images/azure-bots/csharp-azureservice-debug-debughost-logging.png)](/en-us/images/azure-bots/csharp-azureservice-debug-debughost-logging.png)

You may also want to do breakpoint debugging in Visual Studio 2015.  To do this, stop the DebugHost.cmd script, and load the solution for your project (included as part of the repo) in Visual Studio. Then click on the Task Runner Explorer tab in the bottom of your Visual Studio window.

[![](/en-us/images/azure-bots/csharp-azureservice-debug-vsopen.png)](/en-us/images/azure-bots/csharp-azureservice-debug-vsopen.png)

You can see in the bottom pane in the Task Runner Explorer the bot loading up in the debug host environment.  From here your bot is live, and if you switch over to the emulator, you can talk to it and both see responses as well as logged output in the Task Runner Explorer.

[![](/en-us/images/azure-bots/csharp-azureservice-debug-logging.png)](/en-us/images/azure-bots/csharp-azureservice-debug-logging.png)

You can also set breakpoints for your bot.  They will only get hit once you hit **Start** in the Visual Studio environment, which will attach to the Azure Function host (func command from the Azure Functions CLI).  Chat with your bot again in the emulator and you should hit your breakpoint.

<div class="docs-text-note"><strong>Note:</strong> If you can't set your breakpoint successfully, likely you have a syntax error in your code. Look for compile errors in the Task Runner Explorer window after trying to talk to your Bot for a clue.</div>

[![](/en-us/images/azure-bots/csharp-azureservice-debug-breakpoint.png)](/en-us/images/azure-bots/csharp-azureservice-debug-breakpoint.png)

The steps above will cover most scenarios.  There are additional cases for example, in the case of the **proactive template**, these steps will enable debugging the bot, you'll have to do some additional work though to enable the queue storage used between the trigger function and the bot function.




