namespace Microsoft.Bot.Builder.Dialogs
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
    /// <li><a href="namespaces.html"><b>Namespaces</b></a></li>
    /// <li><a href="annotated.html"><b>Classes</b></a></li>
    /// <li><a href="files.html"><b>Source Files</b></a></li>
    /// </ul>    
    /// 
    /// \section overview Overview
    /// 
    /// %Microsoft %Bot %Builder is a powerful framework for constructing bots that can handle
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
    /// * Open source found on http://github.com/Microsoft/botbuilder.
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
    /// At this point your project has the builder installed and is ready to use it.  If you want to understand how
    /// to create and use dialogs, see \ref dialogs or if you would like to have a dialog automatically constructed see \ref forms.
    /// 
    /// \section troubleshooting_q_and_a Troubleshooting Q and A
    /// 
    /// If your question isn't answered here visit our [support page](/support/).
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