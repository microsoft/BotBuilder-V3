namespace Microsoft.Bot.Builder.Connector
{
    /**
\page stateapi %Bot State Service

A key to good bot design is to 
- make the web service stateless so that it can be scaled
- make it track context of a conversation.

Since all bots have these requirements the %Bot Framework has a service for storing bot state. This lets your bot track things 
like _what was the last question I asked them?_. 

\section contextualproperties Useful properties for tracking state
Every Activity has several properties which are useful for tracking state.

| **Property**                  | **Description**                                    | **Use cases**                                                
|------------------------------ |----------------------------------------------------|----------------------------------------------------------
| **From**                      | A Users's address on a channel (ex: email address) | Remembering context for a user on a channel                 
| **Conversation**              | A unique id for a conversation                     | Remembering context all users in a conversation    
| **From + Conversation**       | A user in a conversation                           | Remembering context for a user in a conversation   

You can use these keys to store information in your own database as appropriate to your needs.

\section botstateapi State Methods
The %Bot State service exposes the following methods 

| **Method**                       | **Scoped to**              | **Use cases**                                                
|----------------------------------|----------------------------| ------
| **GetUserData()**                | User                       | Remembering context object with a user
| **GetConversationData()**        | Conversation               | Remembering context object with a conversation
| **GetPrivateConversationData()** | User & Conversation        | Remembering context object with a person in a conversation
| **SetUserData()**                | User                       | Remembering context object with a user
| **SetConversationData()**        | Conversation               | Remembering context object with a conversation
| **SetPrivateConversationData()** | User & Conversation        | Remembering context object with a person in a conversation
| **DeleteStateForUser()**         | User                       | When the user requests data be deleted or removes the %bot contact

When your %bot sends a reply you simply set your object in one of the BotData records properties and it will be persisted and
played back to you on future messages when the context is the same. Your bot may store data for a user, a conversation, or a single
user within a conversation (called "private" data). Each payload may be up to 32 kilobytes in size. The data may be removed by 
the bot or upon a user's request, e.g. if the user requests the channel to inform the bot (and therefore, the %Bot Framework) 
to delete the user's data.

> NOTE: If the record doesn't exist, it will return a new BotData() record with a null .Data field and an ETag = "*", so that is suitable for
> changing and passing back to be saved

\section stateclient Creating State Client
The default state client is stored in central service. For some channel ids you may want to use a state API hosted in the channel itself
(for example with the "emulator" channel) so that state can be stored in a compliant store which the channel supplies.

We have provided a helper method on the Activity object which makes it easy to get an appropriate StateClient for a given message.
~~~{.cs}
StateClient stateClient = activity.GetStateClient();
~~~

\section getsetproperties Get/SetProperty Methods
The C# library has helper methods called SetProperty() and GetProperty() which make it easy to get and set any type
of data from a BotData record, including complex objects.

Setting typed data
~~~{.cs}
BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
userData.SetProperty<bool>("SentGreeting", true);
await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
~~~

Getting typed data
~~~{.cs}
BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
if (userData.GetProperty<bool>("SentGreeting"))
        ... do something ...;
~~~

Example of setting a complex type

~~~{.cs}
BotState botState = new BotState(stateClient);
BotData botData = new BotData(eTag: "*");
botData.SetProperty<BotState>("UserData", myUserData);
BotData response = await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, botData);
~~~

Getting a complex type
~~~{.cs}
MyUserData addedUserData = new MyUserData();
BotData botData = await botState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
myUserData = botData.GetProperty<MyUserData>("UserData");
~~~

\section concurrency Concurrency
These botData objects will fail to be stored if another instance of your bot has changed the object already.
    
Example of using the REST API client library:
~~~{.cs}
try
{
    // get the user data object
    BotData userData = await botState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

    // modify it...
    userData.Data = ...modify...;

    // save it
    await botState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
}
catch (HttpOperationException err)
{
    // handle precondition failed error if someone else has modified your object
}
~~~

    **/
}
