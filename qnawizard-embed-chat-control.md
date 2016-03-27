---
layout: page
title: Embed the Chat Control
permalink: /qnamaker/embed-chat-control/
weight: 1250
parent1: Bot Creation Tools
parent2: QnA Maker
---
###Overview

Microsoft’s chat widget is a messaging application that allows you to chat with the bot built using Microsoft BotFramework. Each bot created using the BotFramework has this chat widget in it's Bot page. The chat widget can be embedded in any website, allowing you to have your bot available on anywhere. 

###Flow
![Chat widget Overview](/images/chatwidget-overview.png)

###Step 1 : Get your bot secret key
1.	Go to https://https://dev.botframework.com/#/bots
2.	Click on your bot
3.	Look for “Web Chat” in the Channels section

![Chat widget channel](/images/chatwidget-channel.png)

Click on Edit for Web chat and press Generate Web Chat Secret

![Chat widget Token](/images/chatwidget-token.png)

Copy the generated secret and embed tag and press “I’m done configuring Web Chat”

###Step 2 : Embed the chat widget in your website

Chat widget can be embedded in 2 ways
####Option 1 - Embed the chat widget in your website using secret
1.	The Web Chat secret you got from Step 1 is the unique identifier for your bot. Using this secret to embed the chat control will disclose the bot secret and can be used by anyone to embed chat widget for your bot on any website
2.	If you want to use secret for chat widget, paste the embed tag from Step 1 in your website and adjust the styling for the iframe element as per the requirement. Example:
`<iframe src='https://ic-webchat-scratch.azurewebsites.net/embed/<AppId>?s=<secret>'></iframe>`
3.	If you do not want your bot secret to be disclosed, use Option 2

####Option 2 - Embed the chat widget in your website using token
1.	You can embed chat widget using token if you want your bot only on your specific websites
2.	You can generate your bot token using the Web Chat secret from Steps1 as below
  *	Issue a POST request to "https://ic-webchat-scratch.azurewebsites.net/api/conversations" passing the Web Chat secret as the Authorization header
  *	Parse the json response and get the token
  *	Example code in javascript –
  
        ````var token = null;
        $.ajax({
        type: 'POST',
        url: 'https://ic-webchat-scratch.azurewebsites.net/api/conversations',
        headers: { 'Authorization': 'BOTCONNECTOR ' + secret }
        }).done(result => {
        token = result.token;
        })````

3.	The generated token can be used on only website for only one session. You need to automate above step for every invocation of your website
4.	Embed the modified iframe tag in your website and adjust the styling for the iframe element as per the requirement. To use the token instead of the secret, use the IFrame like below:

`<iframe src='https://ic-webchat-scratch.azurewebsites.net/embed/<AppId>?t=<token>'></iframe>`

###Step 3 : Chat with your Bot
This is how the chat widget looks like with below iframe styling

`<iframe style= “height:480px ; width:402px” src='https://ic-webchat-scratch.azurewebsites.net/embed/qnatestbot?t=<token>'> </iframe>`

![Chat widget Client](/images/chatwidget-client.png)

