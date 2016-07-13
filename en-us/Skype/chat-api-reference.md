---
layout: page
title: Skype Chat API Reference
permalink: /en-us/skype/chat/
weight: 5020
parent1: Skype bots
---

* TOC
{:toc}

## Authentication
### Inbound
{:.no_toc}

All calls to the Skype REST API should supply a Microsoft Online OAuth2 token. This token can be obtained by issuing a POST call to login.microsoft.online.com with parameters passed in the request body (x-www-form-urencoded).

* The Grant Type should be “client_credentials”
* The **scope** should be https://graph.microsoft.com/.default
* The **client** is the MSA App ID, and client_secret is corresponding secret.

    POST /common/oauth2/v2.0/token HTTP/1.1
    Host: login.microsoftonline.com
    Content-Type: application/x-www-form-urlencoded
    client_id=<MSAAppId>f&client_secret=<secret>&grant_type=client_credentials&scope=https%3A%2F%2Fgraph.microsoft.com%2F.default

The obtained token should be passed in the Authorization header with a Bearer auth scheme.

    Authorization: Bearer <oauth2-token>

For more information on obtaining an OAuth2 token see: https://azure.microsoft.com/en-us/documentation/articles/active-directory-v2-protocols-oauth-code/.

###	Outbound
{:.no_toc}
With all calls from Skype to a bot application Skype will supply a JWT token. We strongly recommend that your bot application verifies the token to make sure it is Skype issuing this call.

1. Your bot should periodically monitor the OpenId metadata endpoint https://api.aps.skype.com/v1/keys and retrieve a list of valid signing certificates with assigned key ids (kid). The recommended poll interval is one day.

2. Once your bot  receives the signed JWT, it should parse the first part of the JWT which contains information about signature algorithm (X509SHA2) and “kid” identifier corresponding to X509 certificate from OpenId metadata.

3. Your bot should then use the correct public key for the “kid” cert to verify a signature over JWT content and compare the resultant byte array against the signature in the JWT token.

4. Once the signature is verified your bot will need to verify other properties of token: the **audience** should match the bot MSA App Id, the **token** should not be expired and the **issuer** should match issuer published in OpenId metadata.

## Skype chat REST API

###	Domain names
{:.no_toc}
* **apis.skype.com or api.skype.net** A production domain that may contain all recent changes.
* **df-apis.skype.com or df-api.skype.net** A production pre-release domain containing the most recent changes.

###	Versioning
{:.no_toc}
Activities APIs have ‘v3’ version identifier.

###	Error Model
{:.no_toc}
On any error, Skype will respond with a body containing the following error model:

{% highlight json %}
{
   “error” : 
    {
         “code” : “string”,
         “message” : “string” 
    }
}
{% endhighlight %}

###	HTTP Redirects
{:.no_toc}
The Skype Bot platform uses HTTP redirection where appropriate. Redirect responses will have a Location header field, which contains the URL of the resource where the client should repeat the requests.

* Bots should assume any request might result in a redirection. 
* Bots should follow HTTP redirects.

Status code|description
**302**|The request should be repeated verbatim to the URL specified in the Location header field but clients should continue to use the original URL for future requests.
**301 or 307**|The request should be repeated verbatim to the URL specified in the Location header field preserving the HTTP method as originally sent.

You may use other redirection status codes (in accordance with the HTTP 1.1 spec).

###	ContextId header
{:.no_toc}
The ContextId header provided by server in all messages response is a generated unique string identifier of each client request, so it can differentiate user requests in logs and can be used for troubleshooting. E.g.

    HTTP/1.1 200 OK
    ContextId: tcid=8695243588097561400,server=CO2SCH020010627
    Date: Mon, 21 Mar 2016 15:26:03 GMT

##	Activities

### POST /v3/conversations/<conversationId>/activities/
{:.no_toc}

#### Purpose
{:.no_toc}
POST sends activities to conversation stream. Here conversationId matches to the identity of either a user of a group conversation. Body should a message model.

#### Response
{:.no_toc}
HTTP status codes:
Code|Description
201 Created|The request has been fulfilled and resulted in a new resource.
400 Bad Request|The request can't be fulfilled because of bad syntax.
401 Unauthorized|The authentication information is not provided or is invalid.
403 Forbidden|The provided credentials do not grant the client permission to access the resource.  For example: a recognized user attempted to access restricted content.

## Outgoing web hooks
{:.no_toc}
Outgoing web hooks are how Bots get notifications about new messages and other events. When your bot registers in an APS service, it provides a callback URL used by Skype Platform API services for notifications.

Supported notifications are:
<ul>**New message notification** The message sent to the bot (1:1 or via group conversation).</ul>
<ul>**New attachment notification** The attachment sent to bot (1:1 or via group conversation).</ul>
<ul>**Conversation event**</ul>

Notifications are sent in case:
<ul>Members are added or removed from the conversation.</ul>
<ul>The conversation's topic name changed.</ul>
<ul>A contact was added to or removed from the bot's contact list.</ul>

POST is issued (with json-formatted body) for all notifications.

### Response from Bot
{:.no_toc}
The expected response is a “201 Created” without body content. 
Redirects will be ignored. Operations will time out after 5 seconds.

