---
layout: page
title:  Securing your bot
permalink: /connector/authorization/
weight: 211
parent1: Bot Connector SDK
---

Some developers want to ensure that their bot's endpoint can only be called by the Bot Connector.

To do this you should
* configure your endpoint to only use HTTPS
* accept your AppId/AppSecret using basic authorization

If the Basic auth headers are missing you should return a 401 asking for basic authentication like this

{% highlight C# %}
    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
    actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", host));
{% endhighlight %}

### BotAuthenticationAttribute
To make it easy for our C# developers we have created an attribute which does this for your method or controller.

To use with the AppId and AppSecret coming from the web.config

{% highlight C# %}
    [BotAuthentication()]
    public class MessagesController : ApiController
    {
    }
{% endhighlight %}

Or you can pass in the appId appSecret to the attribute directly:

{% highlight C# %}
    [BotAuthentication("..appId...","...appSecret...")]
    public class MessagesController : ApiController
    {
    }
{% endhighlight %}



