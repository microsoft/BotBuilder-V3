# Microsoft Bot Builder Overview

Microsoft Bot Builder is a powerful framework for constructing bots that can handle both freeform interactions and more guided ones where the possibilities are explicitly shown to the user. It is easy to use and leverages C# to provide a natural way to write Bots.

High Level Features:
* Powerful dialog system with dialogs that are isolated and composable.  
* Built-in dialogs for simple things like Yes/No, strings, numbers, enumerations.  
* Built-in dialogs that utilize powerful AI frameworks like [LUIS](http://luis.ai)
* Bots are stateless which helps them scale.  
* Form Flow for automatically generating a Bot from a C# class for filling in the class and that supports help, navigation, clarification and confirmation.

[Get started with the Bot Builder!](http://docs.botframework.com/sdkreference/csharp/)

The code itself uses Nuget which restores all needed files when built.  If you want to build the documentation, 
you will need to install [Doxygen](http://www.stack.nl/~dimitri/doxygen/), [GraphViz](http://graphviz.org/) and [Mscgen](http://www.mcternan.me.uk/mscgen/)
Here are step by step instructions:

1. Download and install from the [Doxygen Windows Installer](http://ftp.stack.nl/pub/users/dimitri/doxygen-1.8.11-setup.exe).
2. Download and install from the [GraphViz Windows Installer](http://graphviz.org/pub/graphviz/stable/windows/graphviz-2.38.msi)
3. Download and install from the [Mscgen Windows Installer](http://www.mcternan.me.uk/mscgen/software/mscgen_0.20.exe)

If versions have changed you can find the latest through the core pages above, although you may need to update the Doxygen config file with the appropriate version of tools.

If you want to do localization you should also install the [Multilingual App Toolkit](https://developer.microsoft.com/en-us/windows/develop/multilingual-app-toolkit) which
allows you to edit the localization files and make use of tools like automatic translation.
