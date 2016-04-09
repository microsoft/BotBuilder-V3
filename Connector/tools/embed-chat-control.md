---
layout: page
title: Embed the Chat Control
permalink: /connector/embed-chat-control/
weight: 250
parent1: Bot Connector
parent2: Tools
---

## Overview

The Bot Framework chat widget allows you to chat with your bot through the Bot Connector. The chat widget may be enabled on the bot's landing page in the directory, and you may embed the web chat control in your own page.

The Chat widget supports [Markdown](https://en.wikipedia.org/wiki/Markdown) and images.

## Flow
![Chat widget Overview](/images/chatwidget-overview.png)

## Step 1 - Get your bot secret key
1.	Go to [https://dev.botframework.com/#/bots](https://dev.botframework.com/#/bots)
2.	Click on your bot
3.	Look for “Web Chat” in the Channels section

![Chat widget channel](/images/chatwidget-channel.png)

Click on Edit for Web chat and press Generate Web Chat Secret

![Chat widget Token](/images/chatwidget-token.PNG)

Copy the generated secret and embed tag and press “I'm done configuring Web Chat”

## Step 2 - Embed the chat widget in your website

Chat widget can be embedded in 2 ways:

### Option 1 - Keep your secret hidden, exchange your secret for a token, and generate the embed

Use this option if you can make a server-to-server call to exchange your web chat secret for a temporary token,
and if you want to discourage other websites from embedding your bot in their pages.

Note that this does not prevent another website from embedding your bot in their pages, but it makes it substantially
harder than option 2 below.

To exchange your secret for a token and generate the embed:

<ol>
<li>Issue a server-to-server GET request to "https://webchat.botframework.com/api/tokens" and pass your web chat secret as the Authorization header</li>
<li>The call will return a token good for one conversation. If you want to start a new conversation, you must generate a new token.
<li>Embed the modified iframe tag in your website and adjust the styling for the iframe element as per the requirement. To use the token instead of the secret, use the IFrame like below:</li>
</ol>

{% highlight html %}

    <iframe src="https://webchat.botframework.com/embed/YOUR_BOT_ID?t=YOUR_TOKEN_HERE"></iframe>

{% endhighlight %}

### Option 2 - Embed the chat widget in your website using secret

Use this option if you are OK with other websites embedding your bot into their page. This option provides no protection against
other developers copying your embed code.

If you do not want other websites to host web chat with your bot, option 1 makes it more difficult to do so.

To embed your bot in your web site by include your secret on your web page:

<ol>
<li>Copy the iframe embed code from the channel. See Step 1, above. Example:</li>

{% highlight html %}

    <iframe src="https://webchat.botframework.com/embed/YOUR_BOT_ID?s=YOUR_SECRET_HERE"></iframe>

{% endhighlight %}

<li>Replace YOUR_SECRET_HERE with the embed secret from the same page.</li>
</ol>

## Step 3 - Style the chat control

You may change the chat control's size by adding height and width to the style element.

{% highlight html %}

    <iframe style="height:480px; width:402px" src="https://webchat.botframework.com/embed/YOUR_BOT_ID?t=YOUR_TOKEN_HERE"></iframe>

{% endhighlight %}

![Chat widget Client](/images/chatwidget-client.png)

