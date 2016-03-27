---
layout: page
title: Prompts
permalink: /builder/node/dialogs/Prompts/
weight: 631
parent1: Bot Builder for Node.js
parent2: Dialogs
---

* TOC
{:toc}

## Collecting Input
Bot Builder comes with a number of built-in prompts that can be used to collect input from a user.  

|**Prompt Type**     | **Description**                                   
| -------------------| ---------------------------------------------
|[Prompts.text](#promptstext) | Asks the user to enter a string of text.      
|[Prompts.confirm](#promptsconfirm) | Asks the user to confirm an action.  
|[Prompts.number](#promptsnumber) | Asks the user to enter a number.
|[Prompts.time](#promptstime) | Asks the user for a time or date.
|[Prompts.choice](#promptschoice) | Asks the user to choose from a list of choices.       

These built-in prompts are implemented as a [Dialog](/builder/node/dialogs/overview/) so they’ll return the users response through a call to [session.endDialog()](/sdkreference/nodejs/classes/_botbuilder_d_.session.html#enddialog). Any [DialogHandler](/builder/node/dialogs/overview/#dialog-handlers) can receive the result of a dialog but [waterfalls](/builder/node/dialogs/overview/#waterfall) tend to be the simplest way to handle a prompt result.  

Prompts return to the caller an [IPromptResult](/sdkreference/nodejs/interfaces/_botbuilder_d_.ipromptresult.html). The users response will be contained in the [results.response](/sdkreference/nodejs/interfaces/_botbuilder_d_.ipromptresult.html#reponse) field and may be null. There are a number of reasons for the response to be null. The built-in prompts let the user cancel an action by saying something like ‘cancel’ or ‘nevermind’ which will result in a null response. Or the user may fail to enter a properly formatted response which can also result in a null response. The exact reason can be determined by examining the [ResumeReason](/sdkreference/nodejs/enums/_botbuilder_d_.resumereason.html) returned in [result.resumed](/sdkreference/nodejs/interfaces/_botbuilder_d_.ipromptresult.html#resumed).

### Prompts.text()
The [Prompts.text()](/sdkreference/nodejs/classes/_botbuilder_d_.prompts.html#text) method prompts the user for a string of text.

### Prompts.confirm()

### Prompts.number()

### Prompts.time()

### Prompts.choice()


## Dialog Actions
Dialog actions offer shortcuts to implementing common actions. The [DialogAction](/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html) class provides a set of static methods that return a closure which can be passed to anything that accepts a dialog handler. This includes but is not limited to [DialogCollection.add()](/sdkreference/nodejs/classes/_botbuilder_d_.dialogcollection.html#add), [CommandDialog.matches()](/sdkreference/nodejs/classes/_botbuilder_d_.commanddialog.html#matches), and [LuisDialog.on()](/sdkreference/nodejs/classes/_botbuilder_d_.luisdialog.html#on).