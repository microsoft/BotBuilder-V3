namespace Microsoft.Bot.Builder
{

    ///
    /// \mainpage 
    ///
    /// \section getting_started Getting Started
    /// 
    /// \subsection contents Table of Contents
    /// <ul style="list-style-type: none;">
    /// <li>\ref overview </li>
    /// <li>\ref install </li>
    /// <li>\ref dialogs </li>
    /// <li>\ref forms </li>
    /// <li>\ref Examples</li>
    /// <li><a href="namespaces.html"><b>Namespaces</b></a></li>
    /// <li><a href="annotated.html"><b>Classes</b></a></li>
    /// <li><a href="files.html"><b>Source Files</b></a></li>
    /// </ul>    
    /// 
    /// \section overview Overview
    /// 
    /// %Microsoft Bot Builder is a powerful framework for constructing bots that can handle
    /// both freeform interactions and more guided ones where the possibilities are explicitly 
    /// shown to the user.  It is easy to use and leverages C# to provide a natural way to 
    /// write Bots.
    /// 
    /// High Level Features:
    /// * Powerful dialog system with dialogs that are isolated and composable.  
    /// * Built-in dialogs for simple things like Yes/No, strings, numbers, enumerations.  
    /// * Built-in dialogs that utilize powerful AI frameworks like <a href="http://luis.ai">LUIS</a>.  
    /// * Bots are stateless which helps them scale.  
    /// * Form Flow for automatically generating a Bot from a C# class for filling in the class and that supports help, navigation, clarification and confirmation.
    /// 
    /// \section install Install
    /// 
    /// In order to use the Microsoft Bot Builder you should first follow the install steps in the 
    /// <a href="http://aka.ms/bf-getting-started-in-c">Getting Started with Bot Connector</a> page to setup your bot.  
    /// In order to use the framework you need to:
    /// 1. Right-click on your project and select "Manage NuGet Packages".  
    /// 2. In the "Browse" tab, type "Microsoft.Bot.Builder".  
    /// 3. Click the "Install" button and accept the changes.  
    /// 
    /// At this point your project has the builder installed and is ready to use it.  In order to change the echo example
    /// to use the Bot Builder, we need to add a C# class to represent our conversation and its state.  
    /// You can do this by adding this class to your MessagesController.cs file:
    /// \dontinclude SimpleEchoBot/SimpleEchoBot.cs
    /// \skip Serializable
    /// \until }
    /// \until }
    /// \until }
    /// 
    /// Next we need to wire this class into your Post method like this:
    ///  \dontinclude SimpleEchoBot/SimpleEchoBot.cs
    /// \skip Post(
    /// \until }
    /// \until }
    /// 
    /// The method is marked async because the Bot Framework makes use of the C# facilities for handling asynchronous communication. 
    /// When a message comes in, CompositionRoot.SendAsync is given the message and a delegate for constructing an EchoDialog instance to handle
    /// the message.  CompositionRoot will start the EchoDialog by calling StartAsync which then immediately waits for the next message.  The
    /// IDialogContext.Wait call is given the delegate name to call on the next 
    /// by calling StartAsync where it immediately waits for a message.  to our new dialog.  
    /// 
    /// If you run and test this bot, it will behave exactly like the original one from the Bot Framework template.  Looking at the
    /// code this doesn't look like progress--we have added more lines to achieve exactly the same effect.  We are doing this to lay 
    /// a foundation for handling richer conversations. One of the challenges of handling more complex conversations is in managing the
    /// state of the conversation and the transitions between different phases.  The Bot Builder makes this easier by using dialogs as a unit
    /// of state, isolation and composability.  By introducing the EchoDialog class we have a place to record the state of the dialog. 
    /// This state is then included with every message passed off to the Bot Framework Service.  (Which is why the class is marked as serializable.)  
    /// By making our Bot stateless then a conversation can be moved between machines because of machine crashes or to improve scalability 
    /// without affecting the conversation.  
    /// 
    /// 
    /// 
    /// when writing anything that involves a richer conversation This definitely
    /// looks
    /// 
    /// \section dialogs Dialogs
    /// 
    /// \subsection Overview
    /// Dialogs model a conversational process, where the exchange of messages between bot and user
    /// is the primary channel for interaction with the outside world.  Each dialog is an abstraction that encapsulates
    /// its own state in a C# class that implements IDialog<T>.  Dialogs can be composed with other dialogs to maximize reuse,
    /// and a dialog context maintains a stack of dialogs active in the conversation.  A conversation composed of dialogs is
    /// portable across machines to make it possible to scale a bot implementation.  This conversation state (the stack of
    /// active dialogs and each dialog's state) is stored in the messages exchanged with the %Bot Connector, making the bot
    /// implementation stateless between requests (much like an web application that does not store session state in the
    /// web server's memory).
    /// 
    /// \subsection Execution Flow
    /// Conversation.SendAsync is the top level method for the %Bot Builder SDK.  This composition root follows the dependency
    /// inversion principle (https://en.wikipedia.org/wiki/Dependency_inversion_principle), and performs this work:
    /// 
    /// - instantiates the required components
    /// - deserializes the dialog state (the dialog stack and each dialog's state) from the %Bot Connector message
    /// - resumes the conversation processes where the %Bot decided to suspend and wait for a message
    /// - queues messages to be sent to the user
    /// - serializes the updated dialog state in the messages to be sent to the user
    /// 
    /// CompositionRoot.SendAsync<T> takes as arguments
    /// - the incoming Message from the user (as delivered by the %Bot Connector), and
    /// - a factory method to create the root IDialog<T> dialog for your %Bot's implementation
    /// 
    /// and returns an inline Message to send back to the user through the %Bot Connector.  The factory method is invoked
    /// for new conversations only, because existing conversations have the dialog stack and state serialized in the %Bot data.
    /// 
    /// \subsection IDialog
    /// The IDialog<T> interface provides a single IDialog<T>.StartAsync method that serves as the entry point to the dialog.
    /// The StartAsync method takes an argument and the dialog context.  Your IDialog<T> implementation must be serializable if
    /// you expect to suspend that dialog's execution to collect more Messages from the user.
    /// 
    /// \subsection IDialogContext
    /// The IDialogContext interface is composed of three interfaces: IBotData, IDialogStack, and IBotToUser.
    ///
    /// IBotData represents access to the per user, conversation, and user in conversation state maintained
    /// by the %Bot Connector.
    /// 
    /// IDialogStack provides methods to
    /// - call children dialogs (and push the new child on the dialog stack),
    /// - mark the current dialog as done to return a result to the calling dialog (and pop the current dialog from the dialog stack), and
    /// - wait for a message from the user and suspend the conversation.
    /// 
    /// IBotToUser provides methods to post messages to be sent to the user, according to some policy.  Some of these messages may be sent
    /// inline with the response to the web api method call, and some of these messages may be sent directly using the %Bot Connector client.
    /// Sending and receiving messages through the dialog context ensures the IBotData state is passed through the %Bot Connector.
    /// 
    /// \section forms Form Flow
    /// \ref dialogs are very powerful and flexible, but handling a guided conversation like ordering a sandwich
    /// can require a lot of effort.  At each point in the dialog, there are many possibilities for what happens
    /// next.  You may need to clarify an ambiguity, provide help, go back or show progress so far.  
    /// In order to simplify building guided conversations the framework provides a very powerful 
    /// dialog building block known as a form.  A form sacrifices some of the flexibility provided by dialogs, 
    /// but in a way that requires much less effort.  Even better, you can combine forms and other kinds of dialogs
    /// like a <a href="http://luis.ai">LUIS</a> dialog to get the best of both worlds.  The dialog guides the user through filling in the form
    /// while provding help and guidance along the way.
    /// 
    /// The clearest way to understand this is to take a look at the SimpleSandwichBot sample. 
    /// In that sample you define the form you want using C# classes, fields and properties. 
    /// That is all you need to do to get a pretty good dialog that supports help, clarification, status and 
    /// navigation without any more effort.  Once you have that dialog you can make use of simple annotations
    /// to improve your bot in a straightforward way.  
    /// 
    /// \subsection fields Forms and Fields
    /// A form is made up of fields that you want to fill in through a conversation with the user.  
    /// The simplest way to describe a form is through a C# class.  
    /// Within a class, a "field" is any public field or property with one of the following types:
    /// * Integral -- sbyte, byte, short, ushort, int, uint, long, ulong
    /// * Floating point -- float, double
    /// * String
    /// * DateTime
    /// * Enum
    /// * List of enum
    /// 
    /// Any of the data types can also be nullable which is a good way to model that the field does not have a value.
    /// If a field is based on an enum and it is not nullable, then the 0 value in the enum is considered to be null and you should start your enumeration at 1.
    /// Any other fields, properties or methods are ignored by the form code.
    /// It is also possible to define a form directly by implementing Advanced.IField or using Form.Advanced.Field and populating the dictionaries within it. 
    /// 
    /// \subsection patterns Pattern Language
    /// One of the keys to creating a Bot is being able to generate text that is clear and
    /// meaningful to the bot user.  This framework supports a pattern language with  
    /// elements that can be filled in at runtime.  Everything in a pattern that is not surrounded by curly braces
    /// is just passed straight through.  Anything in curly braces is substitued with values to make a string that can be
    /// shown to the user. Once substitution is done, some additional processing to remove double spaces and
    /// use the proper form of a/an is also done.
    /// 
    /// Possible curly brace pattern elements are outline in the table below.  Within a pattern element, "<field>" refers to the  path within your form class to get
    /// to the field value.  So if I had a class with a field named "Size" you would refer to the size value with the pattern element {Size}.  
    /// "..." within a pattern element means multiple elements are allowed.
    /// 
    /// Pattern Element | Description
    /// --------------- | -----------
    /// {} | Value of the current field.
    /// {&} | Description of the current field.
    /// {<field>} | Value of a particular field. 
    /// {&<field>} | Description of a particular field.
    /// {\|\|} | Show the current choices for enumerated fields.
    /// {[<field> ...]} | Create a list with all field values together utilizing Form.TemplateBase.Separator and Form.TemplateBase.LastSeparator to separate the individual values.
    /// {*} | Show one line for each active field with the description and current value.
    /// {*filled} | Show one line for each active field that has an actual value with the description and current value.
    /// {<format>} | A regular C# format specifier that refers to the nth arg.  See Form.TemplateUsage to see what args are available.
    /// {?<textOrPatternElement>...} | Conditional substitution.  If all referred to pattern elements have values, the values are substituted and the whole expression is used.
    ///
    /// Patterns are used in Form.Prompt and Form.Template annotations.  
    /// Form.Prompt defines a prompt to the user for a particular field or confirmation.  
    /// Form.Template is used to automatically construct prompts and other things like help.
    /// There is a built-in set of templates defined in Form.FormConfiguration.Templates.
    /// A good way to see examples of the pattern language is to look at the templates defined there.
    /// A Form.Prompt can be specified by annotating a particular field or property or implicitly defined through Form.IField<T>.Field.
    /// A default Form.Template can be overridden on a class or field basis.  
    /// Both prompts and templates support the formatting parameters outlined below.
    /// 
    /// Usage | Description
    /// ------|------------
    /// Form.TemplateBase.AllowDefault | When processing choices using {\|\|} controls whether the current value should be showed as a choice.
    /// Form.TemplateBase.AllowNumbers | When processing choices using {\|\|} controls whether or not you can enter numbers for choices. If set to false, you should also set Form.TemplateBase.ChoiceFormat.
    /// Form.TemplateBase.ChoiceFormat | When processing choices using {\|\|} controls how each choice is formatted. {0} is the choice number and {1} the choice description.
    /// Form.TemplateBase.ChoiceStyle | When processing choices using {\|\|} controls whether the choices are presented in line or per line.
    /// Form.TemplateBase.Feedback | For Form.Prompt only controls feedback after user entry.
    /// Form.TemplateBase.FieldCase | Controls case normalization when displaying a field description.
    /// Form.TemplateBase.LastSeparator | When lists are constructed for {[]} or in line choices from {\|\|} provides the separator before the last item.
    /// Form.TemplateBase.Separator | When lists are constructed for {[]} or in line choices from {\|\|} provides the separator before every item except the last.
    /// Form.TemplateBase.ValueCase | Controls case normalization when displaying a field value.
    /// 
    /// \section usage Usage
    /// 
    /// To use Microsoft Bot Builder ...
    /// 
    /// \section Updates
    /// 
    /// With the above release, we are updating bot builder to 
    /// 
    /// -# Have a better session management
    /// 
    /// \section running_in_azure Running in Azure
    /// 
    /// \section troubleshooting_q_and_a Troubleshooting Q and A
    /// 
    /// If your question isn't answered here, try:
    /// 
    /// 
    /// -----------------
    /// \b Question: I have a problem with the builder who should I contact?
    /// 
    /// \b Answer: contact fuse labs
    /// 
    /// \tableofcontents
    ///
    ///
}