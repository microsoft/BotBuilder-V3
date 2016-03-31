---
layout: page
title:  Securing your bot
permalink: /connector/authorization/
weight: 211
parent1: Bot Connector
parent2: Configure
---

Some developers want to ensure that their bot's endpoint can only be called by the Bot Connector.

To do this you should

* configure your endpoint to only use HTTPS
* accept basic authorization with User: AppId Password: AppSecret 

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



### Debugging access issues

If you register your endpoint with HTTP, we will NOT use basic Auth because if we did that we would be exposing in the
clear your appId/appSecret combo.

The problem is that our samples are written to do basic auth with AppId/AppSecret, so if you deploy a server using HTTP
the default code will be *checking for a value which will not be sent.*

You should either:

* register as HTTPS and Check for basic auth appId/appSecret

or
 
* register as HTTP and **disable basic auth** 
 