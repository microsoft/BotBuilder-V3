namespace Microsoft.Bot.Builder
{

    ///
    /// \mainpage 
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
    /// * Form Flow for automatically generating a Bot from a C# class that supports help, navigation, clarification and confirmation.
    /// 
    /// \section contents Table of Contents
    /// 
    /// <ul style="list-style-type: none;">
    /// <li>\ref dialogs </li>
    /// <li>\ref forms </li>
    /// <li>\ref examples</li>
    /// <li><a href="namespaces.html"><b>Namespaces</b></a></li>
    /// <li><a href="annotated.html"><b>Classes</b></a></li>
    /// <li><a href="files.html"><b>Source Files</b></a></li>
    /// </ul>
    /// 
    /// \section dialogs Dialogs
    /// TBD
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