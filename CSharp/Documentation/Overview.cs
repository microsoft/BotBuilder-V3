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
    /// In order to use the %Microsoft %Bot %Builder you should first follow the install steps in the 
    /// \ref gettingstarted page to setup your bot.  
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
    /// \until }
    /// 
    /// To use it your would do something like this:
    /// ~~~
    ///             Interactive(FormDialog.FromForm<AnnotatedSandwichOrder>(() => AnnotatedSandwichOrder.BuildForm())).GetAwaiter().GetResult();
    /// ~~~
    /// 
    /// \section troubleshooting_q_and_a Troubleshooting Q & A
    /// 
    /// If you have problems or suggestions, please visit our [support page](/support/).
    /// 
    ///
    ///
}

/// <summary>Root namespace for the %Microsoft %Bot %Connector %SDK.</summary>
namespace Microsoft.Bot.Connector { }

/// <summary>Root namespace for the %Microsoft %Bot %Builder %SDK.</summary>
namespace Microsoft.Bot.Builder { }

/// <summary>Core namespace for \ref dialogs and associated infrastructure.</summary>
/// <remarks>This is one of the core namespaces you should include in your code.</remarks>
namespace Microsoft.Bot.Builder.Dialogs { }

/// <summary>Namespace for internal \ref dialogs machinery that is not useful for most developers.</summary>
namespace Microsoft.Bot.Builder.Dialogs.Internals { }

/// <summary>Core namespace for \ref FormFlow and associated infrastructure.</summary>
/// <remarks>
/// If you want to use \ref FormFlow you should use include both the Microsoft.Bot.Builder.Dialogs namespace and this one.
/// </remarks>
namespace Microsoft.Bot.Builder.FormFlow { }

/// <summary>
/// Namespace for \ref FormFlow declaratively defined with JSON Schema.
/// </summary>
namespace Microsoft.Bot.Builder.FormFlow.Json { }

/// <summary>
/// Root namespace for the %Microsoft %Bot %Builder %Calling %SDK.
/// </summary>
namespace Microsoft.Bot.Builder.Calling { }

/// <summary>Namespace for \ref FormFlow advanced building blocks.</summary>
/// <remarks>
/// For most developers the building blocks in this namespace are not necessary.
/// The main place where you would use these building blocks is if you want to define a form
/// dynamically rather than through C# reflection.
/// </remarks>
namespace Microsoft.Bot.Builder.FormFlow.Advanced { }

/// <summary>Namespace for internal machinery that is not useful for most developers.</summary>
namespace Microsoft.Bot.Builder.Internals { }

/// <summary>Namespace for the internal fibers machinery that is not useful for most developers.</summary>
namespace Microsoft.Bot.Builder.Internals.Fibers { }

/// <summary>Namespace for the machinery needed to talk to http://luis.ai.</summary>
/// <remarks>This namespace is not useful for most developers.</remarks>
namespace Microsoft.Bot.Builder.Luis { }

/// <summary>
/// Namespace for resources.
/// </summary>
namespace Microsoft.Bot.Builder.Resource { }
