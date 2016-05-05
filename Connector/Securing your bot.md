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


### Implementing your own caller validation
To implement your own caller validation you are simply implementing basic authentication check against values in the 
the header.

1. Verify authorization schema is basic
2. Verify base64 decoded value of the authorization parameter is the AppId and AppSecret for your bot 

Sample C# code:

{% highlight C# %}
    if (req?.Headers?.Authorization?.Scheme != "Basic" || req?.Headers?.Authorization?.Parameter == null)
        return req.CreateResponse(HttpStatusCode.Unauthorized);

    string[] parts = Encoding.Default.GetString(Convert.FromBase64String(req?.Headers?.Authorization?.Parameter)).Split(':').Select(s => s.Trim()).ToArray();
    if ((parts.Length != 2) || (parts[0] != AppId ) || (parts[1] != AppSecret ))
        return req.CreateResponse(HttpStatusCode.Forbidden);
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

> Remember, if you use HTTP you are operating your web service open to the web.
 