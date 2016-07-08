---
layout: page
title: Language
permalink: /en-us/bot-intelligence/language/
weight: 8240
parent1: Bot Intelligence
---

* TOC
{:toc}

## Summary
The Language APIs enable you to build smart bots that are able to understand and process natural language. This is a particularly important skill for bots as, in most cases, the interaction users have with bots is free-form. Thus, bots must be able to understand language the way people speak it - naturally and contextually. The Language APIs use powerful language models to determine what users want, identify concepts and entities in a given sentence, and ultimately allow bots to respond with the appropriate action. Furthermore, they support several text analytics capabilities, such as spell checking, sentiment detection, language modeling, and more, to extract more accurate and richer insights from text.   

## API Overview
There are 5 language APIs available in Cognitive Services to understand and process natural language:

- The [Language Understanding Intelligent Service (LUIS)](https://www.microsoft.com/cognitive-services/en-us/language-understanding-intelligent-service-luis){:target="_blank"} is able to process natural language using pre-built or custom-trained language models. 
- The [Text Analytics API](https://www.microsoft.com/cognitive-services/en-us/text-analytics-api){:target="_blank"}  detects sentiment, key phrases, topics, and language from text. 
- The [Bing Spell Check API](https://www.microsoft.com/cognitive-services/en-us/bing-spell-check-api){:target="_blank"}  provides powerful spell check capabilities, and is able to recognize the difference between names, brand names, and slang.
- The [Linguistic Analysis API](https://www.microsoft.com/cognitive-services/en-us/linguistic-analysis-api){:target="_blank"} uses advanced linguistic analysis algorithms to process text, and perform operations such as breaking down the structure of the text, or performing part-of-speech tagging and parsing. 
- The [Web Language Model (WebLM) API](https://www.microsoft.com/cognitive-services/en-us/web-language-model-api){:target="_blank"} can be used to automate a variety of natural language processing tasks, such as word frequency or next-word prediction, using advanced language modeling algorithms. 


## Example: Weather Bot
For our first example, we will build a weather bot that is able to understand and respond to various hypothetical commands, such as What's the weather like in Paris, What's the temperature next week in seattle...etc. The bot is using LUIS to identify the intent of the user, and reply with the appropriate prompt. 

To get started with LUIS, go to [LUIS.ai](www.luis.ai){:target="_blank"} and build your own custom language model. Our [Getting Started](https://www.microsoft.com/cognitive-services/en-us/luis-api/documentation/getstartedwithluis-basics){:target="_blank"} guide describes in details how to build your first model through the LUIS user interface, or programatically via the LUIS APIs. 
We encoourgae you to watch our [basic](https://www.youtube.com/watch?v=jWeLajon9M8&index=4&list=PLD7HFcN7LXRdHkFBFu4stPPeWJcQ0VFLx){:target="_blank"} video tutorial. 

To create the bot, we will use the [Bot Application .NET template](http://docs.botframework.com/connector/getstarted/#getting-started-in-net){:target="_blank"} as our starting point. After you create your project with the Bot Application template, add a class to handle the integration with your LUIS language model. 

To build LU model for our weather bot follow this [video](https://www.youtube.com/watch?v=39L0Gv2EcSk&index=5&list=PLD7HFcN7LXRdHkFBFu4stPPeWJcQ0VFLx).

{% highlight c# %}

[LuisModel("<YOUR_LUIS_APP_ID>", "<YOUR_LUIS_SUBSCRIPTION_KEY>")]
[Serializable]
public class TravelGuidDialog: LuisDialog<object>
{
    public const string Entity_location = "Location";
    
    [LuisIntent("")]
    public async Task None(IDialogContext context, LuisResult result)
    {
        string message = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
        await context.PostAsync(message);
        context.Wait(MessageReceived);
    }
    
    enum City { Paris, London, Seattle, Munich};

    [LuisIntent("GetWeather")]
    public async Task GetWeather(IDialogContext context, LuisResult result)
    {
        var obj = (IEnumerable<City>)Enum.GetValues(typeof(City));
        EntityRecommendation location;
        
        if (!result.TryFindEntity(Entity_location, out location))
        {
            PromptDialog.Choice(context, SelectCity, City, "In which city do you want to know the weather forecast?");
        }
        else
        {
            //Add code to retrieve the weather
            await context.PostAsync($"The weather in {location} is ");
            context.Wait(MessageReceived);
        }
    }

    private async Task SelectObject(SelectCity context, IAwaitable<City> city)
    {
        var message = string.Empty;
        switch (await city)
        {
            case City.Paris:
            case City.London:
            case City.Seattle:
            case City.Munich:
                message = $"The weather in {city} is ";
                break;
            default:
                message = $"Sorry!! I don't have know the weather in {city}";
                break;
        }
        await context.PostAsync(message);
        context.Wait(MessageReceived);
    }
}

{% endhighlight %}

Next, go to *MessagesController.cs*, and add the following namespaces. 

{% highlight c# %}

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

{% endhighlight %}
        
Finally, on the same file, replace the code in the Post task with the one below.  

{% highlight c# %}

    public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
    {
        ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
    
        if (activity == null || activity.GetActivityType() != ActivityTypes.Message)
        {
            //add code to handle errors, or non-messaging activities
        }
    
        await connector.Conversations.SendAsync(activity, () => new TravelGuidDialog()); 
        var response = Request.CreateResponse(HttpStatusCode.OK);
        return response;

    }

{% endhighlight %}

## Example: Emotional Bot
For our next example, we will use the Text Analytics API to determine the sentiment behind a user's message, i.e. whether it is positive or negative. The Text Analytics API returns a sentiment score between 0 and 1, where 0 is very negative and 1 is very positive. For example, if the user types "That was really helpful", the API will classify it with a highly positive score, whereas if he types "That didn't help at all", the API will return a negative score. The example that follows shows how the bot's response can be customized according to the sentiment score calculated by the Text Analytics API. For more information about the Text Analytics API, see the [C# and Python sample code](https://text-analytics-demo.azurewebsites.net/Home/SampleCode){:target="_blank"} for the service, or our [Getting Started guide](http://go.microsoft.com/fwlink/?LinkID=760860){:target="_blank"}.

For this example, we will use the [Bot Application .NET template](http://docs.botframework.com/connector/getstarted/#getting-started-in-net){:target="_blank"} as our starting point. Note that the *Newtonsoft.JSON* package is also required, which can be obtained via NuGet. After you create your project with the Bot Application template, you will create some classes to hold the input and output from the API. Create a new C# class file (*TextAnalyticsCall.cs*) with the following code. The class will serve as our model for the JSON input/output of the Text Analytics API.    

{% highlight c# %}

using System.Collections.Generic;

// Classes to store the input for the sentiment API call
public class BatchInput
{
    public List<DocumentInput> documents { get; set; }
}
public class DocumentInput
{
    public double id { get; set; }
    public string text { get; set; }
}

// Classes to store the result from the sentiment analysis
public class BatchResult
{
    public List<DocumentResult> documents { get; set; }
}
public class DocumentResult
{
    public double score { get; set; }
    public string id { get; set; }
}
    
{% endhighlight %}

Next, go to *MessagesController.cs* and add the following namespaces. 

{% highlight c# %}

using System.Collections.Generic;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;

{% endhighlight %}

Finally, replace the code in the Post task with the one in the code snippet below. The code receives the user message, calls the sentiment analysis endpoint and responds accordingly to the user. 

{% highlight c# %}

public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
{
    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

    if (activity == null || activity.GetActivityType() != ActivityTypes.Message)
    {
        //add code to handle errors, or non-messaging activities
    }
    
    const string apiKey = "<YOUR API KEY FROM MICROSOFT.COM/COGNITIVE>"; 
    string queryUri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey); 
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    BatchInput sentimentInput = new BatchInput();

    sentimentInput.documents = new List<DocumentInput>();
    sentimentInput.documents.Add(new DocumentInput()
    {
        id = 1,
        text = activity.Text
    });

    var sentimentJsonInput = JsonConvert.SerializeObject(sentimentInput);
    byte[] byteData = Encoding.UTF8.GetBytes(sentimentJsonInput);
    var content = new ByteArrayContent(byteData);
    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    var sentimentPost = await client.PostAsync(queryUri, content);
    var sentimentRawResponse = await sentimentPost.Content.ReadAsStringAsync();
    var sentimentJsonResponse = JsonConvert.DeserializeObject<BatchResult>(sentimentRawResponse);
    double sentimentScore = sentimentJsonResponse.documents[0].score;

    var replyMessage = activity.CreateReply();
    replyMessage.Recipient = activity.From;
    replyMessage.Type = ActivityTypes.Message;

    if (sentimentScore > 0.7)
    {
        replyMessage.Text = $"That's great to hear!";
    }
    else if (sentimentScore < 0.3)
    {
        replyMessage.Text = $"I'm sorry to hear that...";
    }
    else
    {
        replyMessage.Text = $"I see...";
    }

    await connector.Conversations.ReplyToActivityAsync(replyMessage);
    var response = Request.CreateResponse(HttpStatusCode.OK);
    return response;
}

{% endhighlight %}
