---
layout: page
title: Embed the Chat Control
permalink: /qnamaker/embed-chat-control/
weight: 1250
parent1: Bot Creation Tools
parent2: QnA Maker
---

## Overview

Microsoft’s chat widget is a messaging application that allows you to chat with the bot built using Microsoft BotFramework. Each bot created using the BotFramework has this chat widget in it's Bot page. The chat widget can be embedded in any website, allowing you to have your bot available on anywhere.

The Chat widget supports [Markdown](https://en.wikipedia.org/wiki/Markdown).

## Flow
![Chat widget Overview](/images/chatwidget-overview.png)

## Step 1 : Get your bot secret key
1.	Go to [https://dev.botframework.com/#/bots](https://dev.botframework.com/#/bots)
2.	Click on your bot
3.	Look for “Web Chat” in the Channels section

![Chat widget channel](/images/chatwidget-channel.png)

Click on Edit for Web chat and press Generate Web Chat Secret

![Chat widget Token](/images/chatwidget-token.PNG)

Copy the generated secret and embed tag and press “I’m done configuring Web Chat”

## Step 2 : Embed the chat widget in your website

Chat widget can be embedded in 2 ways

### Option 1 - Embed the chat widget in your website using secret
<ol>
<li>The Web Chat secret you got from Step 1 is the unique identifier for your bot. Using this secret to embed the chat control will disclose the bot secret and can be used by anyone to embed chat widget for your bot on any website</li>
<li>If you want to use secret for chat widget, paste the embed tag from Step 1 in your website and adjust the styling for the iframe element as per the requirement. Example:</li>

{% highlight html %}

    <iframe src="https://webchat.botframework.com/embed/<AppId>?s=<secret>"></iframe>

{% endhighlight %}

<li>If you do not want your bot secret to be disclosed, use Option </li>
</ol>

### Option 2 - Embed the chat widget in your website using token

<ol>
<li>You can embed chat widget using token if you want your bot only on your specific websites</li>
<li>You can generate your bot token using the Web Chat secret from Steps1 as below
<ul>
<li>Issue a POST request to "https://webchat.botframework.com/api/conversations" passing the Web Chat secret as the Authorization header</li>
<li>Parse the json response and get the </li>
<li>Example code in javascript –</li>
</ul>
</li>

{% highlight javascript %}

    var token = null;
    $.ajax({
    type: 'POST',
    url: 'https://webchat.botframework.com/api/conversations',
    headers: { 'Authorization': 'BOTCONNECTOR ' + secret }
    }).done(result => {
    token = result.token;
    })

{% endhighlight %}
<li>The generated token can be used on only website for only one session. You need to automate above step for every invocation of your website</li>
<li>Embed the modified iframe tag in your website and adjust the styling for the iframe element as per the requirement. To use the token instead of the secret, use the IFrame like below:</li>
</ol>

{% highlight html %}

    <iframe src="https://webchat.botframework.com/embed/<AppId>?t=<token>"></iframe>

{% endhighlight %}

## Step 3 : Chat with your Bot

This is how the chat widget looks like with below iframe styling

{% highlight html %}

    <iframe style="height:480px; width:402px" src="https://webchat.botframework.com/embed/qnatestbot?t=<token>"></iframe>

{% endhighlight %}

![Chat widget Client](/images/chatwidget-client.png)

