---
layout: 'redirect'
permalink: /en-us/node/builder/calling/prompts/
redir_to: 'https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-dialog-prompt
sitemap: false
---
* TOC
{:toc}

## Collecting Input
Bot Builder comes with a number of built-in prompts that can be used to collect input from a user.  

|**Prompt Type**     | **Description**                                   
| -------------------| ---------------------------------------------
|[Prompts.choice](#promptschoice) | Asks the user to choose from a list of choices.       
|[Prompts.digits](#promptsdigits) | Asks the user to enter a sequence of digits.      
|[Prompts.confirm](#promptsconfirm) | Asks the user to confirm an action.  
|[Prompts.record](#promptsrecord) | Asks the record a message.
|[Prompts.action](#promptsaction) | Sends a raw [action](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.iaction) to the calling service and lets the bot manually process its outcome.

These built-in prompts are implemented as a [Dialog](/en-us/node/builder/chat/dialogs/) so they’ll return the users response through a call to [session.endDialogWithresult()](/en-us/node/builder/calling-reference/classes/_botbuilder_d_.callsession#enddialogwithresult). Any [DialogHandler](/en-us/node/builder/chat/dialogs/#dialog-handlers) can receive the result of a dialog but [waterfalls](/en-us/node/builder/chat/dialogs/#waterfall) tend to be the simplest way to handle a prompt result.  

Prompts return to the caller an [IPromptResult](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ipromptresult.html). The users response will be contained in the [results.response](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ipromptresult.html#reponse) field and may be null should the user fail to input a proper response. 

### Prompts.choice()
The [Prompts.choice()](/en-us/node/builder/calling-reference/classes/_botbuilder_d_.prompts.html#choice) method asks the user to pick an option from a list. The users response will be returned as an [IPromptChoiceResult](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ipromptchoiceresult.html). The list of choices is passed to the prompt as an array of [IRecognitionChoice](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.irecognitionchoice) objects.

You can configure the prompt to use speech recognition to recognize the callers choice: 

{% highlight JavaScript %}
calling.Prompts.choice(session, "Which department? support, billing, or claims?", [
    { name: 'support', speechVariation: ['support', 'customer service'] },
    { name: 'billing', speechVariation: ['billing'] },
    { name: 'claims', speechVariation: ['claims'] }
]);
{% endhighlight %}

Or DTMF input using Skypes diling pad:

{% highlight JavaScript %}
calling.Prompts.choice(session, "Which department? Press 1 for support, 2 for billing, or 3 for claims", [
    { name: 'support', dtmfVariation: '1' },
    { name: 'billing', dtmfVariation: '2' },
    { name: 'claims', dtmfVariation: '3' }
]);
{% endhighlight %}

Or both speech recognition and DTMF:

{% highlight JavaScript %}
bot.dialog('/departmentMenu', [
    function (session) {
        calling.Prompts.choice(session, "Which department? Press 1 for support, 2 for billing, 3 for claims, or star to return to previous menu.", [
            { name: 'support', dtmfVariation: '1', speechVariation: ['support', 'customer service'] },
            { name: 'billing', dtmfVariation: '2', speechVariation: ['billing'] },
            { name: 'claims', dtmfVariation: '3', speechVariation: ['claims'] },
            { name: '(back)', dtmfVariation: '*', speechVariation: ['back', 'previous'] }
        ]);
    },
    function (session, results) {
        if (results.response !== '(back)') {
            session.beginDialog('/' + results.response.entity + 'Menu');
        } else {
            session.endDialog();
        }
    },
    function (session) {
        // Loop menu
        session.replaceDialog('/departmentMenu');
    }
]);
{% endhighlight %}

The users choice is returned as an [IFindMatchResult](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ifindmatchresult) similar to chat bots and the choices name will be assigned to the [response.entity](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ifindmatchresult#entity) property.

### Prompts.digits()
The [Prompts.digits()](/en-us/node/builder/calling-reference/classes/_botbuilder_d_.prompts.html#digits) method asks the user to enter a sequence of digits followed by an optional stop tone. The users response will be returned as an [IPromptDigitsResult](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ipromptdigitsresult.html). 

{% highlight JavaScript %}
calling.Prompts.digits(session, "Please enter your account number followed by pound.", 10, { stopTones: ['#'] });
{% endhighlight %}

### Prompts.confirm()
The [Prompts.confirm()](/en-us/node/builder/calling-reference/classes/_botbuilder_d_.prompts.html#confirm) method asks the user to confirm some action. This prompt builds on the choices prompt by calling it with a standard set of yes & no choices. The user can reply by saying a range of responses or they can press 1 for yes or 2 for no.  The users response will be returned as an [IPromptConfirmResult](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ipromptconfirmresult.html). 

{% highlight JavaScript %}
calling.Prompts.confirm(session, "Would you like to end the call?");
{% endhighlight %}

### Prompts.record()
The [Prompts.record()](/en-us/node/builder/calling-reference/classes/_botbuilder_d_.prompts.html#record) method asks the user to record a message. This prompt builds on the choices prompt by calling it with a standard set of yes & no choices. The recorded message will be returned as an [IPromptRecordResult](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ipromptrecordresult.html) and the recorded audio will be available off that object as a _{Buffer}_.

{% highlight JavaScript %}
calling.Prompts.record(session, "Please leave a message after the beep.");
{% endhighlight %}

### Prompts.action()
The [Prompts.action()](/en-us/node/builder/calling-reference/classes/_botbuilder_d_.prompts.html#action) method lets you send the calling service a raw [action](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.iaction) object. The outcome will be returned as an [IPromptActionResult](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ipromptactionresult.html) for manual processing by your bot. 

In general, you shouldn’t ever need to call this prompt but one scenario where you might is if you wanted to send a playPrompt action that plays a file to the user and you’d like to keep the call active so you can take another action once that completes.  The normal [session.send()](/en-us/node/builder/calling-reference/classes/_botbuilder_d_.callsession#send) method you’d use to play a file will automatically end the call if that playPrompt action isn’t followed by a recognize or record action so this gives you a way of dynamically chaining play prompts together. You might do this if you want to play the user hold music or silence while you periodically check for some long running operation to complete.
 

