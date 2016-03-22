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
/// Possible curly brace pattern elements are outline in the table below.
/// 
/// <list type="table">
/// <listheader>
/// <term>Pattern Elements</term>
/// <description>Description</description>
/// </listheader>
/// <item>
///     <term>{}</term>
///     <description>Value of the current field.</description>
/// </item>
/// <item>
///     <term>{&amp;}</term>
///     <description>Description of the current field.</description>
/// </item>
/// <item>
///     <term>{&lt;field&rt;</term>
///     <description>Value of a particular field.</description>
/// </item>
/// <item>
///     <term>{&amp;&lt;field&gt;}</term>
///     <description>Description of a particulsar field.</description>
/// </item>
/// <item>
///     <term>{||}</term>
///     <description>Show the current choices for enumerated fields.</description>
/// </item>
/// <item>
///     <term>{[{&lt;field&gt;}...</term>
///     <description>Create a list with all field values together.</description>
/// </item>
/// <item>
///     <term>{*}</term>
///     <description>Show the status of all the active fields in a form.</description>
/// </item>
/// <item>
///     <term>{*filled}</term>
///     <description>Show the status of all active fields with a current value.</description>
/// </item>
/// <item>
///     <term>{&lt;format&gt;</term>
///     <description>A regular C# format specifier that refers to the nth arg.  See <see cref="TemplateUsage"/> to see what args are available.</description></item>
/// <item>
///     <term>{?&lt;text or pattern element&gt;>...}</term>
///     <description>Conditional substitution.  If all referred to pattern elements have values, the values are substituted and the whole expression is used.</description>
/// </item>
/// </list>
///
/// Patterns are used in <see cref="Prompt"/> and <see cref="Template"/> annotations.  
/// <see cref="Prompt"/> defines a prompt to the user for a particular field or confirmation.  
/// <see cref="Template"/> is used to automatically construct prompts and other things like help.
/// There is a built-in set of templates defined in <see cref="FormConfiguration.Templates"/>.
/// A good way to see examples of the pattern language is to look at the templates defined there.
/// A <see cref="Prompt"/> can be specified by annotating a particular field or property or implicitly defined through <see cref="IField&lt;T&gt;.Field</see>]]>"/>.
/// A default <see cref="Template"/> can be overridden on a class or field basis.  
/// Both prompts and templates support the formatting parameters outlined below.
/// 
/// <list type="table">
/// <item>
///     <term><see cref="TemplateBase.AllowDefault"/></term>
///     <description>When processing choices using {||} controls whether the current value should be showed as a choice.</description>
/// </item>
/// <item>
///     <term><see cref="TemplateBase.AllowNumbers"/></term>
///     <description>When processing choices using {||} controls whether or not you can enter numbers for choices. If set to false, you should also set <see cref="TemplateBase.ChoiceFormat"/>.</description>
/// </item>
/// <item>
///     <term><see cref="TemplateBase.ChoiceFormat"/></term>
///     <description>When processing choices using {||} controls how each choice is formatted. {0} is the choice number sand {1} the choice description.</description>
/// </item>
/// <item>
///     <term><see cref="TemplateBase.ChoiceStyle"/></term>
///     <description>When processing choices using {||} controls whether the choices are presented in line or per line.</description>
/// </item>
/// <item>
///     <term><see cref="TemplateBase.Feedback"/></term>
///     <description>For <see cref="Prompt"/> only controls feedback after user entry.</description>
/// </item>
/// <item>
///     <term><see cref="TemplateBase.FieldCase>"/></term>
///     <description>Controls case normalization when displaying a field description.</description>
/// </item>
/// <item>
///     <term><see cref="TemplateBase.LastSeparator"/></term>}
///     <description>When lists are constructed for {[]} or in line choices from {||} provides the separator before the last item.</description>
/// </item>
/// <item>
///     <term><see cref="TemplateBasse.Separator"/></term>
///     <description>When lists are constructed for {[]} or in line choices from {||} provides the separator before every item except the last.</description>
/// </item>
/// /// <item>
///     <term><see cref="TemplateBase.ValueCase>"/></term>
///     <description>Controls case normalization when displaying a field value.</description>
/// </item>
/// </list>
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
