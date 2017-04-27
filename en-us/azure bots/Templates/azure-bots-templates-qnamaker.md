---
layout: 'redirect'
permalink: /en-us/azure-bot-service/templates/qnamaker/
redir_to: 'https://docs.microsoft.com/en-us/bot-framework/azure/azure-bot-service-template-question-and-answer'
sitemap: false
---
The question and answer bot template shows how to use the [QnA Maker](https://qnamaker.ai) tool to quickly create an FAQ Bot. One of the basic requirements in writing your own bot service is to seed it with questions and answers. In many cases, the questions and answers already exist in content like FAQ URLs/documents, etc. [QnA Maker](https://qnamaker.ai) lets you ingest your existing FAQ content and expose it as an HTTP endpoint.

When you create the template, Azure Bot Service lets you either select an existing knowledge base you may have created from the [QnA Maker](https://qnamaker.ai) portal, or creates an empty knowledge base.

The routing of the message is identical to the one presented in the [Basic bot template](/en-us/azure-bot-service/templates/basic/), please refer to that document for more info.

Most messages will have a Message activity type, and will contain the text and attachments that the user sent. If the message’s activity type is Message, the template posts the message to **BasicQnAMakerDialog** in the context of the current message (see BasicQnAMakerDialog.csx). 

<div id="thetabs1">
    <ul>
        <li data-lang="csharp"><a href="#tab11">C#</a></li>
        <li data-lang="node"><a href="#tab12">Node.js</a></li>
    </ul>

    <div id="tab11">

{% highlight csharp %}
        switch (activity.GetActivityType())
        {
            case ActivityTypes.Message:
                await Conversation.SendAsync(activity, () => new BasicQnAMakerDialog());
                break;
{% endhighlight %}
    </div>
    <div id="tab12">
{% highlight JavaScript %}
bot.dialog('/', BasicQnAMakerDialog);
{% endhighlight %}
	</div>  
</div>

The **BasicQnAMakerDialog** object inherits from the QnAMakerDialog object. The **QnAMakerDialog** object contains the **StartAsync** and **MessageReceived** methods. When the dialog’s instantiated, the dialog’s StartAsync method runs and calls IDialogContext.Wait with the continuation delegate that’s called when there is a new message. In the initial case, there is an immediate message available (the one that launched the dialog) and the message is immediately passed to the **MessageReceived** method (in the **QnAMakerDialog** object).

The **MessageReceived** method calls your QnA Maker service and returns the response to the user.

QnAMaker Dialog is distributed in a separate NuGet package called **Microsoft.Bot.Builder.CognitiveServices** for C# and npm module called **botbuilder-cognitiveservices** for Node.js.

The following parameters are passed when invoking the QnA Maker service.

* Subscription Key - Each registered user on [QnA Maker](https://qnamaker.ai) is assigned an unique subscription key for metering.
* Knowledge Base ID - Each knowledge base created is assigned a unique subscription key by the tool.
* Default Message (optional) - Message to show if there is no match in the knowledge base.
* Score Threshold (optional) - Threshold value of the match confidence score returned by the service. It ranges from 0-1. This is useful in controlling the relevance of the responses.

<div id="thetabs2">
    <ul>
        <li data-lang="csharp"><a href="#tab21">C#</a></li>
        <li data-lang="node"><a href="#tab22">Node.js</a></li>
    </ul>

    <div id="tab21">
	
{% highlight csharp %}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;

//Inherit from the QnAMakerDialog
[Serializable]
public class BasicQnAMakerDialog : QnAMakerDialog
{        
	//Parameters to QnAMakerService are:
	//Compulsory: subscriptionKey, knowledgebaseId, 
	//Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]
	public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute(Utils.GetAppSetting("QnASubscriptionKey"), Utils.GetAppSetting("QnAKnowledgebaseId"), "No good match in FAQ.", 0.5)))
	{}
}{% endhighlight %}

    </div>
    <div id="tab22">
{% highlight JavaScript %}
// QnA Maker Dialogs

var recognizer = new cognitiveservices.QnAMakerRecognizer({
	knowledgeBaseId: 'set your kbid here', 
	subscriptionKey: 'set your subscription key here'});

var BasicQnAMakerDialog = new cognitiveservices.QnAMakerDialog({ 
	recognizers: [recognizer],
	defaultMessage: 'No good match in FAQ.',
	qnaThreshold: 0.5});
{% endhighlight %}

    </div>  
</div>	

### Resources
* [Bot Builder Samples GitHub Repo](https://github.com/Microsoft/BotBuilder-Samples)
* [Bot Builder SDK C# Reference](https://docs.botframework.com/en-us/csharp/builder/sdkreference/)
* [Bot Builder SDK](https://github.com/Microsoft/BotBuilder-Samples)
* [QnA Maker documentation](https://qnamaker.ai/Documentation){:target="_blank"}
