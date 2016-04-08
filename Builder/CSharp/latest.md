---
layout: page
title: Releases
permalink: /builder/node/libraries/CSharp/latest/
weight: 560
parent1: Bot Builder for C#
---

Microsoft Bot Builder is a powerful framework for constructing bots that can handle both freeform interactions and more guided ones where the possibilities are explicitly shown to the user. It is easy to use and leverages C# to provide a natural way to write bots.

High Level Features:

* Powerful dialog system with dialogs that are isolated and composable.
* Built-in dialogs for simple things like Yes/No, strings, numbers, enumerations.
* Built-in dialogs that utilize powerful AI frameworks like LUIS.
* Bots are stateless which helps them scale.
* FormFlow for automatically generating a bot from a C# class for filling in the class and that supports help, navigation, clarification and confirmation.
* SDK source code is found on http://github.com/Microsoft/botbuilder.

## Install
To install Microsoft.Bot.Builder, run the following command in the [Package Manager Console](http://docs.nuget.org/consume/package-manager-console)

    PM> Install-Package Microsoft.Bot.Builder

## Release Notes
The framework is still in preview mode so developers should expect breaking changes in future versions of the framework. A list of current issues can be found on our [GitHub Repository](https://github.com/Microsoft/BotBuilder/issues).

### v1.0.1
* Fixed LuisDialog to handle null score returned by Luis. This is because of a behavior change in Cortana pre-built apps by [Luis](http://luis.ai)
* Updated nuspec with better description 
* Added error resilient context store

Link to updated [Microsoft.Bot.Builder nuget](https://www.nuget.org/packages/Microsoft.Bot.Builder/1.0.1)
