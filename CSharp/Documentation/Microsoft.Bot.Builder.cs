using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Form;

///
/// \mainpage 
///
/// \section overview Overview
/// 
/// <b>Microsoft Bot Builder</b>
/// 
/// Microsoft Bot Builder is a powerful framework for constructing bots that can handle
/// both freeform interactions and more guided ones where the possibilities are explicitly 
/// shown to the user.  The bots that are built are stateless which helps ensure that they
/// can scale.  
/// 
/// \section contents Table of Contents
/// 
/// <ul style="list-style-type: none;">
/// <li>\ref key_concepts </li>
/// <li>\ref usage </li>
/// <li><a href="namespaces.html"><b>Namespaces</b></a></li>
/// <li><a href="annotated.html"><b>Classes</b></a></li>
/// <li><a href="files.html"><b>Source Files</b></a></li>
/// </ul>
/// 
/// \section features High Level Features
/// 
/// - 
/// 
/// 
/// \page key_concepts Key Concepts
/// \section dialogs Dialogs
/// 
/// \section forms Form Flow
/// \ref Dialogs are very powerful and flexible, but handling a guided conversation like ordering a sandwich
/// can require a lot of effort.  At each point in the dialog, there are many possibilities for what happens
/// next.  You may need to clarify an ambiguity, provide help, go back or show progress so far.  
/// In order to simplify building guided conversations the framework provides a very powerful 
/// dialog building block known as a form.  A form sacrifices some of the flexibility provided by dialogs, 
/// but in a way that requires much less effort.  Even better, you can combine forms and other kinds of dialogs
/// like a LUIS dialog to get the best of both worlds.  The dialog guides the user through filling in the form
/// while provding help and guidance along the way.
/// 
/// The clearest way to understand this is to take a look at the SimpleSandwichBot sample. 
/// In that sample you define the form you want using C# classes, fields and properties. 
/// That is all you need to do to get a pretty good dialog that supports help, clarification, status and 
/// navigation without any more effort.  Once you have that dialog you can make use of simple annotations
/// to improve your bot in a straightforward way.  
/// 
/// \section patterns Pattern Language
/// One of the keys to creating a Bot is being able to generate text that is clear and
/// meaningful to the bot user.  This framework supports a pattern language with  
/// elements that can be filled in at runtime.  Everything in a pattern that is not surrounded by curly braces
/// is just passed straight through.  Anything in curly braces is substitued with values to make a string that can be
/// shown to the user. Once substitution is done some additional processing to remove double spaces and
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
/// {[<field> ...]} | Create a list with all field values together utilizing Microsoft.Bot.Builder.Form.TemplateBase.Separator and Microsoft.Bot.Builder.Form.TemplatBase.LastSeparator to separate the individual values.
/// {*} | Show one line for each active field with the description and current value.
/// {*filled} | Show one line for each active field that has an actual value with the description and current value.
/// {<format>} | A regular C# format specifier that refers to the nth arg.  See Microsoft.Bot.Builder.Form.TemplateUsage to see what args are available.
/// {?<textOrPatternElement>...} | Conditional substitution.  If all referred to pattern elements have values, the values are substituted and the whole expression is used.
///
/// Patterns are used in Microsoft.Bot.Builder.Form.Prompt and Microsoft.Bot.Builder.Form.Template annotations.  
/// Microsoft.Bot.Builder.Form.Prompt defines a prompt to the user for a particular field or confirmation.  
/// Microsoft.Bot.Builder.Form.Template is used to automatically construct prompts and other things like help.
/// There is a built-in set of templates defined in Microsoft.Bot.Builder.Form.FormConfiguration.Templates.
/// A good way to see examples of the pattern language is to look at the templates defined there.
/// A Microsoft.Bot.Builder.Form.Prompt can be specified by annotating a particular field or property or implicitly defined through Microsoft.Bot.Builder.Form.IField<T>.Field.
/// A default Microsoft.Bot.Builder.Form.Template can be overridden on a class or field basis.  
/// Both prompts and templates support the formatting parameters outlined below.
/// 
/// Usage | Description
/// ------|------------
/// Microsoft.Bot.Builder.Form.TemplateBase.AllowDefault | When processing choices using {\|\|} controls whether the current value should be showed as a choice.
/// Microsoft.Bot.Builder.Form.TemplateBase.AllowNumbers | When processing choices using {\|\|} controls whether or not you can enter numbers for choices. If set to false, you should also set Microsoft.Bot.Builder.Form.TemplateBase.ChoiceFormat.
/// Microsoft.Bot.Builder.Form.TemplateBase.ChoiceFormat | When processing choices using {\|\|} controls how each choice is formatted. {0} is the choice number and {1} the choice description.
/// Microsoft.Bot.Builder.Form.TemplateBase.ChoiceStyle | When processing choices using {\|\|} controls whether the choices are presented in line or per line.
/// Microsoft.Bot.Builder.Form.TemplateBase.Feedback | For Microsoft.Bot.Builder.Form.Prompt only controls feedback after user entry.
/// Microsoft.Bot.Builder.Form.TemplateBase.FieldCase | Controls case normalization when displaying a field description.
/// Microsoft.Bot.Builder.Form.TemplateBase.LastSeparator | When lists are constructed for {[]} or in line choices from {\|\|} provides the separator before the last item.
/// Microsoft.Bot.Builder.Form.TemplateBase.Separator | When lists are constructed for {[]} or in line choices from {\|\|} provides the separator before every item except the last.
/// Microsoft.Bot.Builder.Form.TemplateBase.ValueCase | Controls case normalization when displaying a field value.
/// 
/// \page usage Usage
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
