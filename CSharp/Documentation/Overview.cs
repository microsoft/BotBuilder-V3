namespace Microsoft.Bot.Builder.Dialogs
{
    /// [Getting Started with Bot Connector]: http://aka.ms/bf-getting-started-in-c
    /// [LUIS]: http://luis.ai 
    /// 
    /// \mainpage Getting Started
    /// 
    /// \section overview Overview
    /// 
    /// %Microsoft %Bot %Builder is a powerful framework for constructing bots that can handle
    /// both freeform interactions and more guided ones where the possibilities are explicitly 
    /// shown to the user.  It is easy to use and leverages C# to provide a natural way to 
    /// write bots.
    /// 
    /// High Level Features:
    /// * Powerful dialog system with dialogs that are isolated and composable.  
    /// * Built-in dialogs for simple things like Yes/No, strings, numbers, enumerations.  
    /// * Built-in dialogs that utilize powerful AI frameworks like [LUIS].  
    /// * Bots are stateless which helps them scale.  
    /// * \ref forms for automatically generating a bot from a C# class for filling in the class and that supports help, navigation, clarification and confirmation.  
    /// * SDK source code is found on http://github.com/Microsoft/botbuilder.
    /// 
    /// \section install Install
    /// 
    /// In order to use the Microsoft Bot Builder you should first follow the install steps in the 
    /// [Getting Started with Bot Connector] page to setup your bot.  
    /// In order to use the framework you need to:
    /// 1. Right-click on your project and select "Manage NuGet Packages".  
    /// 2. In the "Browse" tab, type "Microsoft.Bot.Builder".  
    /// 3. Click the "Install" button and accept the changes.  
    /// 
    /// At this point your project has the builder installed and is ready to use it.  If you want to understand how
    /// to create and use dialogs, see \ref dialogs or if you would like to have a dialog automatically constructed see \ref forms.
    /// 
    /// \section debugging Debugging
    /// 
    /// In order to debug your bot, you can either use the %Bot Framework Emulator as described in [Getting Started with Bot Connector] 
    /// or you can create a console app with a command loop like this:
    /// \dontinclude FormTest/Program.cs
    /// \skip Interactive
    /// \until while
    /// \until }
    /// 
    /// To use it your would do something like this:
    /// ~~~
    ///             Interactive(FormDialog.FromForm<AnnotatedSandwichOrder>(() => AnnotatedSandwichOrder.BuildForm()));
    /// ~~~
    /// 
    /// \section troubleshooting_q_and_a Troubleshooting Q & A
    /// 
    /// If your question isn't answered here visit our [support page](/support/).
    /// 
    /// -----------------
    /// \b Question: I have a problem with the builder who should I contact?
    /// 
    /// \b Answer: contact FUSE Labs
    /// 
    /// \tableofcontents
    ///
    ///
}