---
layout: page
title:  REST API (auth)
permalink: /connector/calling-api/
weight: 231
parent1: Bot Connector SDK
parent2: Library Reference
---

Our API is a simple REST interface and is accessible from any language which can construct
a HTTPS request.

All calls to our API needs to be secured with the following information.

### Ocp-Apim-Subscription-Key Header
You need to add a Ocp-Apim-Subscription-Key header with value which is the **AppSecret** from 
your bot registration on the portal.

### Https and Basic Auth
Our API is only available via HTTPS, and in that secure channel you need to pass us your
AppId/AppSecret as the user/password for basic auth.

### Json
All bodies for API calls are json (application/json) content-types.

### Example C\# code

{% highlight C# %}
string appId="...";
string appSecret="...";
request.Headers.Add("Ocp-Apim-Subscription-Key", appSecret);
var byteArray = Encoding.ASCII.GetBytes($"{appId}:{appSecret}");
request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
{% endhighlight %}

