namespace Microsoft.Bot.Builder.Dialogs
{
    /// \page dialogs %Dialogs
    /// \tableofcontents
    /// [LUIS]: http://luis.ai 
    /// [How to Setup LUIS]: http://aka.ms/bf-node-nl 
    /// 
    /// \section Overview
    /// %Dialogs model a conversational process, where the exchange of messages between bot and user
    /// is the primary channel for interaction with the outside world.  Each dialog is an abstraction that encapsulates
    /// its own state in a C# class that implements IDialog.  %Dialogs can be composed with other dialogs to maximize reuse,
    /// and a dialog context maintains a stack of dialogs active in the conversation.  A conversation composed of dialogs is
    /// portable across machines to make it possible to scale a bot implementation.  This conversation state (the stack of
    /// active dialogs and each dialog's state) is stored in the messages exchanged with the %Bot Connector, making the bot
    /// implementation stateless between requests. (Much like a web application that does not store session state in the
    /// web server's memory.)
    /// 
    /// The best way to understand this is to work through some examples.  The first example changes the code in the 
    /// %Bot Framework template to use dialogs from the %Bot %Builder.  The second example, \ref echoBot builds on that to
    /// add some simple state.  The final example \ref alarmBot uses the [LUIS] natural language framework and some of the 
    /// built-in system prompts.
    /// 
    /// \section simpleEcho Echo Bot
    /// This example starts with the bot you get by starting your bot the %Bot Framework template which includes code to 
    /// echo back what the user says.  
    /// In order to change the echo example to use the %Bot %Builder, we first need to import the required namespace:
    /// ~~~
    /// using Microsoft.Bot.Builder.Dialogs;
    /// ~~~
    /// 
    /// Next we need to add a C# class to represent our conversation. 
    /// You can do this by adding this class to your MessagesController.cs file:
    /// \dontinclude SimpleEchoBot/Controllers/MessagesController.cs
    /// \skip Serializable
    /// \until }
    /// \until }
    /// \until }
    /// 
    /// Finally we need to wire the class into your Post method like this:
    /// \dontinclude SimpleEchoBot/Controllers/MessagesController.cs
    /// \skip Post(
    /// \until HandleSystem
    /// \until }
    /// \until }
    /// 
    /// The method is marked async because the %Bot %Builder makes use of the C# facilities for handling asynchronous communication. 
    /// It returns a Task<Message> which is the reply to the passed in Message.  
    /// If there is an exception, the Task will contain the exception information. Within the Post method we call
    /// Conversation.SendAsync which is the root method for the %Bot %Builder SDK.  It follows the dependency
    /// inversion principle (https://en.wikipedia.org/wiki/Dependency_inversion_principle) and does the following steps:
    /// - Instantiate the required components.  
    /// - Deserialize the dialog state (the dialog stack and each dialog's state) from the %Bot Connector message.
    /// - Resume the conversation processes where the %Bot decided to suspend and wait for a message.
    /// - Queues messages to be sent to the user.
    /// - Serializes the updated dialog state in messages sent back to the user.
    /// 
    /// When your conversation first starts, there is no dialog state in the message so the delegate passed to Conversation.SendAsync
    /// will be used to construct an EchoDialog and it's StartAsync method will be called.  In this case, StartAsync  calls
    /// IDialogContext.Wait with the continuation delegate (our MessageReceivedAsync method) to call when there is a new message.  
    /// In the initial case there is an immediate message available (the one that launched the dialog) and it is immediately 
    /// passed To MessageReceivedAsync.  
    /// 
    /// Within MessageReceivedAsync we wait for the message to come in and then post our response and wait for the next message. 
    /// In this simple case the next message would again be processed by MessageReceivedAsync. 
    /// Every time we call IDialogContext.Wait our bot is suspended and can be restarted on any machine that receives the message.  
    /// 
    /// If you run and test this bot, it will behave exactly like the original one from the %Bot Framework template.  It is a
    /// little more complicated, but it allows you to compose together multiple dialogs into complex conversations without
    /// having to explicitly manage state.   
    /// 
    /// \section echoBot Echo Bot with State
    /// Now that we have an example of the %Bot %Builder framework we are going to build on it to add some dialog state and 
    /// some commands to control that state. We are going to number the responses and allow the command "reset" to reset the
    /// count.  All we need to do is to replace our EchoDialog with the one below. 
    /// \dontinclude EchoBot/EchoDialog.cs
    /// \skip Serializable
    /// \until AfterResetAsync(
    /// \until context.Wait
    /// \until }
    /// \until }
    /// 
    /// The first change you will notice is the addition of `private int count = 1;`.  This is the state we are persisting
    /// with this dialog on each message.  
    /// 
    /// In MessageReceivedAsync we have added check to see if the input was "reset" and if that is true we use the built-in
    /// Prompts.Confirm dialog to spawn a sub-dialog that asks the user if they are sure about resetting the count. The sub-dialog has its
    /// own private state and does not need to worry about interfering with the parent dialog.  When the sub dialog
    /// is done, it's result is then passed onto the AfterResetAync method.  In AfterResetAsync we check on the 
    /// response and perform the action including sending a message back to the user.  The final step is to do IDialogContext.Wait 
    /// with a continuation back to MessageReceivedAsync on the next message.
    /// 
    /// \section alarmBot Alarm Bot
    /// This example is more complex and shows how to integrate [LUIS] together with PromptDialog to create an alarm 
    /// system you can interact with through natural language.  
    /// In order to create a dialog that uses [LUIS] you need to create a class that derives from
    /// LuisDialog like this:
    /// \dontinclude SimpleAlarmBot/SimpleAlarmDialog.cs
    /// \skip LuisModel
    /// \until {
    /// 
    /// The parameters to the LuisModel attribute are the [LUIS] REST endpoint and key.  Here we have supplied
    /// some values that will work, but the endpoint may be throttled in which case you will need to setup your own endpoint 
    /// as described in [How to Setup LUIS].  
    /// 
    /// Within the SimpleAlarmDialog class any method with the name of the [LUIS] intent or marked with the LuisIntent attribute
    /// is called when that intent is matched.  For example here is the handler for turning off an
    /// alarm, i.e. the intent "builtin.intent.alarm.turn_off_alarm":
    /// \dontinclude SimpleAlarmBot/SimpleAlarmDialog.cs
    /// \skip builtin.intent.alarm.turn_off_alarm
    /// \until context.Wait
    /// \until }
    /// \until }
    /// Within the handler you can see the confirmation done using the built-in Prompt.Confirm dialog.  
    /// The confirm dialog will spawn a sub-dialog for verifying the alarm deletion. Here is the full
    /// example:
    /// \include SimpleAlarmBot/SimpleAlarmDialog.cs
    /// 
    /// \section IDialogContext
    /// All of the dialogs take in an IDialogContext, an interface that provides the services
    /// needed to save state and comunicate.  
    /// The interface is composed of three interfaces: Internals.IBotData, Internals.IDialogStack, and Internals.IBotToUser.
    ///
    /// Internals.IBotData represents access to the per user, conversation, and user in conversation state maintained
    /// by the %Bot Connector.  The per user state is useful for storing things about the user that cross
    /// conversations--for example the last sandwich order so that you can use that as the default 
    /// when ordering a sandwich. It is also possible to store such state in your own store
    /// and use the Message.From.Id as a key.  
    /// 
    /// Internals.IBotToUser provides methods to post messages to be sent to the user, according to some policy.  Some of these messages may be sent
    /// inline with the response to the web api method call, and some of these messages may be sent directly using the %Bot Connector client.
    /// Sending and receiving messages through the dialog context ensures the Internals.IBotData state is passed through the %Bot Connector.
    /// 
    /// Internals.IDialogStack provides methods to:
    /// - Call children dialogs and push the new child on the dialog stack.
    /// - Mark the current dialog as done and return a result to the calling dialog and pop the current dialog from the dialog stack.
    /// - Wait for a message from the user and suspend the conversation until the message arrives.
    /// The stack is usually automatically managed for you.
    /// 
    /// \section Conclusion
    /// Through this description we have seen how you can easily create stateless bots that can reuse dialog building blocks
    /// ranging from simple prompts to advanced natural language.  As a next step, you should explore \ref forms which 
    /// describes how the %Bot Builder framework can automatically build dialogs from a C# class you want the user to fill in.
}
