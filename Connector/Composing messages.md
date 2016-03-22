---
layout: page
title: Composing Messages
permalink: /connector/composing-messages
weight: 220
parent1: Bot Connector SDK
---

# Composing Messages
A message is the object that is used to communicate between a user and a bot.

# Text and Language 
Most of the time these are the only properties you need to worry about.  A person sent you some text, or your bot is sending some text back.

| Property    | Description                               | Example   
| ------------|-------- ----------------------------------| ----------
| **Text**  | A text payload in markdown syntax which will be rendered as appropriate on each channel| Hello, how are you?
| **Language** | (Optional) an optional [language code](https://msdn.microsoft.com/en-us/library/hh456380.aspx) | "en"

If all you do is give simple one line text responses you don't have to read any further.

# The Text property is **Markdown**
The text property is actually expressed in markdown. This allows each channel to render the markdown as appropriate.

The markdown that is supported:

|Style          | Markdown                                                               |Description                              | Example                                                             
|---------------| -----------------------------------------------------------------------|-----------------------------------------| ------ -------------------------------------------------------------
|**Bold**           | \*\*text\*\*                                                           | make the text bold                      | **text**                                                            
|**Italic**         | \*text\*                                                               | make the text italic                    | *text*
|**Header1-5**      | # H1                                                                   | Mark a line as a header                 | # H1                                                       
|**Strikethrough**  | \~\~text\~\~                                                           | make the text strikethrough             | ~~text~~                                                            
|**Hr**             | \-\-\-                                                                 | insert a horizontal rule                |                                                                    |   
|**Unordered list** | \*                                                                     | Make an unordered list item             | * text                                             
|**Ordered list**   | 1.                                                                     | Make an ordered list item starting at 1 | 1. text                                                          
|**Pre**            | \`text\`                                                               | Preformatted text (can be inline)                 | `text`                                                              
|**Block quote**    | \> text                                                                | quote a section of text                 | > text                                                              
|**link**           | \[bing](http://bing.com)                                               | create a hyperlink with title           | [bing](http://bing.com)                                             
|**image link**     | \![duck]\(http://aka.ms/Fo983c) | link to an image                        | ![duck](http://aka.ms/Fo983c)

> Not all channels can represent all markdown fields.  As appropriate channels will fallback to a reasonable approximation.
> If you are communicating with a channel which supports fixed width fonts or tables you can use standard table 
> markdown, but because many channels (such as SMS) do not have a known display width and/or support fixed width fonts it 
> is not possible to render a table properly on all channels.      

# Attachments
The Attachments property is an array of Attachment objects which allow you to send and receive images and other content.
The primary fields for an Attachment object are:

| Name        | Description                               | Example   
| ------------|-------- ----------------------------------| ----------
|**ContentType** | The contentType of the ContentUrl property| image/png
|**ContentUrl**  | A link to content of type ContentType     | http://somedomain.com/cat.jpg 
|**Content**     | An embedded object of type contentType    | If contentType = Location then this could be an object that represents the location

> When images are sent by a user to the bot they will come in as attachments with a ContentType and ContentUrl pointing to the image.  

Some channels allow you to represent a card responses made up of a title, link, description and image. The
Attachments data structure allows you to include "Card" metadata allowing a channel to build a proper card response

| Name         | Description                               | Example                                                                                                                                                                      
| ------------ |-------------------------------------------| -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
|**Title**        | The title of a card                       | World's Cutest Dogs                                                                                                                                                          
| **TitleLink**    | Link for the title                        | https://www.youtube.com/watch?v=dA1UlMVLhnU                                                                                                                                  
| **Text**         |Description of the card                    | Meet 17 of the Cutest Dogs in the World. These adorable puppies are from all over the world and while they pose no threat to you physically, they could threaten your wallet.
| **ThumbnailUrl** | A small image to display on card          | http://i.ytimg.com/vi/dA1UlMVLhnU/mqdefault.jpg                                                                                                                              

# ChannelData Property
As you can see above the default message gives you a pretty rich pallete to describe your response in way that allows your message to "just work" across
a variety of channels.  Most of the heavy lifting is done by the channel adapter, adapating your message to the way it is expressed on that channel.

If you want to be able to take advantage of special features or concepts for a channel we provide a way for you to send native 
metadata to that channel giving you much deeper control over how your bot interacts on a channel.  The way you do this is to pass 
extra properties via the *ChannelData* property. 

Go to [Custom Channel Messages](/connector/custom-channeldata) for more detailed description of what each channel enables via the CustomData field.

