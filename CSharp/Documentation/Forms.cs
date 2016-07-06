namespace Microsoft.Bot.Builder.FormFlow
{
    /// \page forms %FormFlow 
    /// [LUIS]: http://luis.ai
    /// [Describe]: @ref DescribeAttribute 
    /// [Numeric]: @ref NumericAttribute 
    /// [Pattern]: @ref PatternAttribute
    /// [Optional]: @ref OptionalAttribute 
    /// [Prompt]: @ref PromptAttribute 
    /// [Template]: @ref TemplateAttribute 
    /// [Terms]: @ref TermsAttribute 
    /// [EntityRecommendation.Type]: @ref Microsoft.Bot.Builder.Luis.EntityRecommendation.Type 
    /// [EntityRecommendation.Entity]: @ref Microsoft.Bot.Builder.Luis.EntityRecommendation.Entity 
    /// [LuisDialog]: @ref Microsoft.Bot.Builder.Dialogs.LuisDialog 
    /// [AllowDefault]: @ref Advanced.TemplateBaseAttribute.AllowDefault 
    /// [ChoiceCase]: @ref Advanced.TemplateBaseAttribute.ChoiceCase
    /// [ChoiceLastSeparator]: @ref Advanced.TemplateBaseAttribute.ChoiceLastSeparator
    /// [ChoiceSeparator]: @ref Advanced.TemplateBaseAttribute.ChoiceSeparator
    /// [ChoiceFormat]: @ref Advanced.TemplateBaseAttribute.ChoiceFormat 
    /// [ChoiceParens]: @ref Advanced.TemplateBaseAttribute.ChoiceParens
    /// [ChoiceStyle]: @ref Advanced.TemplateBaseAttribute.ChoiceStyle 
    /// [Feedback]: @ref Advanced.TemplateBaseAttribute.Feedback 
    /// [FieldCase]: @ref Advanced.TemplateBaseAttribute.FieldCase 
    /// [LastSeparator]: @ref Advanced.TemplateBaseAttribute.LastSeparator 
    /// [Separator]: @ref Advanced.TemplateBaseAttribute.Separator 
    /// [ValueCase]: @ref Advanced.TemplateBaseAttribute.ValueCase
    /// [MAT]: https://developer.microsoft.com/en-us/windows/develop/multilingual-app-toolkit
    /// [XLF]: https://en.wikipedia.org/wiki/XLIFF
    /// [CurrentUICulture]: https://msdn.microsoft.com/en-us/library/system.threading.thread.currentuiculture(v=vs.110).aspx
    /// [CurrentCulture]: https://msdn.microsoft.com/en-us/library/system.threading.thread.currentculture(v=vs.110).aspx
    /// [JSON Schema]: http://json-schema.org/documentation.html
    /// [JObject]: http://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm
    /// [Microsoft.Bot.Builder.FormFlow.Json]: https://www.nuget.org/packages/Microsoft.Bot.Builder.FormFlow.Json/
    /// 
    /// \section Overview
    /// \ref dialogs are very powerful and flexible, but handling a guided conversation like ordering a sandwich
    /// can require a lot of effort.  At each point in the dialog, there are many possibilities for what happens
    /// next.  You may need to clarify an ambiguity, provide help, go back or show progress so far.  
    /// In order to simplify building guided conversations the framework provides a powerful 
    /// dialog building block known as %FormFlow.  %FormFlow sacrifices some of the flexibility provided by dialogs, 
    /// but in a way that requires much less effort.  Even better, you can combine %FormFlow generate dialogs and other kinds of dialogs
    /// like a [LuisDialog] to get the best of both worlds.  
    /// A %FormFlow dialog guides the user through filling in the form while provding help and guidance along the way.
    /// 
    /// The clearest way to understand this is to take a look at the \ref simpleSandwichBot sample. 
    /// In that sample you define the form you want using C# classes, fields and properties. 
    /// That is all you need to do to get a pretty good dialog that supports help, clarification, status and 
    /// navigation without any more effort.  Once you have that base you can make use of C# attributes and FormBuilder
    /// to improve your bot in a straightforward way as shown in the \ref annotatedSandwich example.
    /// 
    /// \section fields Forms and Fields
    /// %FormFlow starts with the idea of a form--a collection of fields that you want to fill in through a conversation with the user.  
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
    /// Any other fields, properties or methods are ignored by the %FormFlow code.  In order to handle a list of complex
    /// objects, you need to create a form for the top level C# class and also one for the complex object.  You can
    /// use the \ref dialogs system to compose the forms together.  
    /// It is also possible to define a form directly by implementing Advanced.IField or using Advanced.Field and populating the dictionaries within it. 
    /// In order to better understand FormFlow and its capabilities we will work through two examples, \ref simpleSandwichBot where 
    /// everything is automatically generated and \ref annotatedSandwich where the form is extensively customized.
    /// 
    /// \section simpleSandwichBot Simple Sandwich Bot
    /// As an example of %FormFlow in action, we will outline a simple sandwich ordering form that will be elaborated in \ref annotatedSandwich
    /// to show various features.  To start with %FormFlow you need to create a C# class to define the form you want to fill in. 
    /// Like this:
    /// \include SimpleSandwichBot/sandwich.cs
    /// You can see how we import the core Microsoft.Bot.Builder.FormFlow namespace and then define our class.
    /// Included in the class is a static method BuildForm that uses a FormBuilder to build your form.  There
    /// are lots of things you can do with the form builder, but we will cover that later and here we just 
    /// define a simple welcome message.
    /// 
    /// In order to connect your form to the bot framework you need to add it to your controller like this:
    /// \dontinclude SimpleSandwichBot/Controllers/MessagesController.cs
    /// \skip MakeRootDialog
    /// \until Post
    /// \until }
    /// 
    /// The combination of your C# class and connecting it to the %Bot Framework is enough to automatically create a conversation.  
    /// Here is an example interaction that demonstrates some of the features offered by %FormFlow.  A <b>&gt;</b> symbol shows 
    /// where a response is expected from the user and any input that was entered at that point in the dialog.
    /// ~~~{.txt}
    ///  Please select a sandwich
    ///  1. BLT
    ///  2. Black Forest Ham
    ///  3. Buffalo Chicken
    ///  4. Chicken And Bacon Ranch Melt
    ///  5. Cold Cut Combo
    ///  6. Meatball Marinara
    ///  7. Oven Roasted Chicken
    ///  8. Roast Beef
    ///  9. Rotisserie Style Chicken
    ///  10. Spicy Italian
    ///  11. Steak And Cheese
    ///  12. Sweet Onion Teriyaki
    ///  13. Tuna
    ///  14. Turkey Breast
    ///  15. Veggie
    ///  >
    /// ~~~ 
    /// 
    /// Here you can see the field SandwichOrder.Sandwich being filled in.  First off you can see the automatically generated
    /// prompt "Please select a sandwich".  The word "sandwich" came from the name of the field.  The SandwichOptions enumeration provided the 
    /// choices that make up the list.  Each enumeration was broken into words based on changes in case and underscores.  
    /// 
    /// Now what are the possible responses?  If you ask for "help" you can see this:
    /// ~~~{.txt}
    /// > help
    /// * You are filling in the sandwich field.Possible responses:
    /// * You can enter a number 1-15 or words from the descriptions. (BLT, Black Forest Ham, Buffalo Chicken, Chicken And Bacon Ranch Melt, Cold Cut Combo, Meatball Marinara, Oven Roasted Chicken, Roast Beef, Rotisserie Style Chicken, Spicy Italian, Steak And Cheese, Sweet Onion Teriyaki, Tuna, Turkey Breast, and Veggie)
    /// * Back: Go back to the previous question.
    /// * Help: Show the kinds of responses you can enter.
    /// * Quit: Quit the form without completing it.
    /// * Reset: Start over filling in the form. (With defaults from your previous entries.)
    /// * Status: Show your progress in filling in the form so far.
    /// * You can switch to another field by entering its name. (Sandwich, Length, Bread, Cheese, Toppings, and Sauce).
    /// ~~~  
    /// 
    /// As described in th help, you can respond to this prompt by responding with the number of the choice you want, or you can also use the words that
    /// are found in the choice descriptions.  There are also a number of commands that let you back up a step, get help, quit, start over 
    /// or get the progress so far.  Let's enter "2" to select "Black Forest Ham".
    /// ~~~{.txt}
    ///  Please select a sandwich
    ///  1. BLT
    ///  2. Black Forest Ham
    ///  3. Buffalo Chicken
    ///  4. Chicken And Bacon Ranch Melt
    ///  5. Cold Cut Combo
    ///  6. Meatball Marinara
    ///  7. Oven Roasted Chicken
    ///  8. Roast Beef
    ///  9. Rotisserie Style Chicken
    ///  10. Spicy Italian
    ///  11. Steak And Cheese
    ///  12. Sweet Onion Teriyaki
    ///  13. Tuna
    ///  14. Turkey Breast
    ///  15. Veggie
    /// > 2
    /// Please select a length(1. Six Inch, 2. Foot Long)
    /// > 
    /// ~~~
    /// 
    /// Now we get the next prompt which is for the SandwichOrder.Length property.  If you wanted to back up to check
    /// on your change, you could type 'back' like this:
    /// ~~~{.txt}
    /// > back
    /// Please select a sandwich(current choice: Black Forest Ham)
    ///  1. BLT
    ///  2. Black Forest Ham
    ///  3. Buffalo Chicken
    ///  4. Chicken And Bacon Ranch Melt
    ///  5. Cold Cut Combo
    ///  6. Meatball Marinara
    ///  7. Oven Roasted Chicken
    ///  8. Roast Beef
    ///  9. Rotisserie Style Chicken
    ///  10. Spicy Italian
    ///  11. Steak And Cheese
    ///  12. Sweet Onion Teriyaki
    ///  13. Tuna
    ///  14. Turkey Breast
    ///  15. Veggie
    /// ~~~
    /// 
    /// Now we can see that we selected "Black Forest Ham" and we can continue by typing "c" to keep the current choice and
    /// "1" to select a six inch sandwich.
    /// ~~~{.txt}
    /// Please select a sandwich (current choice: Black Forest Ham)
    ///  1. BLT
    ///  2. Black Forest Ham
    ///  3. Buffalo Chicken
    ///  4. Chicken And Bacon Ranch Melt
    ///  5. Cold Cut Combo
    ///  6. Meatball Marinara
    ///  7. Oven Roasted Chicken
    ///  8. Roast Beef
    ///  9. Rotisserie Style Chicken
    ///  10. Spicy Italian
    ///  11. Steak And Cheese
    ///  12. Sweet Onion Teriyaki
    ///  13. Tuna
    ///  14. Turkey Breast
    ///  15. Veggie
    /// > c
    /// Please select a length(1. Six Inch, 2. Foot Long)
    /// > 1
    /// ~~~
    /// 
    /// In addition to typing numbers and commands you can also type in words from the choices.  Here we have typed "nine grain" which 
    /// is ambiguous and the FormFlow system automatically asks for clarification.
    /// ~~~{.txt}
    /// Please select a bread
    ///  1. Nine Grain Wheat
    ///  2. Nine Grain Honey Oat
    ///  3. Italian
    ///  4. Italian Herbs And Cheese
    ///  5. Flatbread
    /// > nine grain
    /// By "nine grain" bread did you mean(1. Nine Grain Honey Oat, 2. Nine Grain Wheat)
    /// > 1
    /// ~~~
    /// 
    /// What happens if you type in something which is not understood or a mixture of understood and not understood words?  
    /// In the below you can see both something that is not understood at all and also a mixture of understood and 
    /// not understood things.
    /// ~~~{.txt}
    /// Please select a cheese (1. American, 2. Monterey Cheddar, 3. Pepperjack)
    /// > amercan
    /// "amercan" is not a cheese option.
    /// > american smoked
    /// For cheese I understood American. "smoked" is not an option.
    /// ~~~
    /// 
    /// Some fields like SandiwchOrder.Toppings allow multiple choices. Here we are entering multiple choices and showing 
    /// another example of clarification.
    /// ~~~{.txt}
    /// Please select one or more toppings
    ///  1. Banana Peppers
    ///  2. Cucumbers
    ///  3. Green Bell Peppers
    ///  4. Jalapenos
    ///  5. Lettuce
    ///  6. Olives
    ///  7. Pickles
    ///  8. Red Onion
    ///  9. Spinach
    ///  10. Tomatoes
    /// > peppers, lettuce and tomatoe
    /// By "peppers" toppings did you mean(1. Green Bell Peppers, 2. Banana Peppers)
    /// > 1
    /// ~~~
    /// 
    /// At this point, I might wonder how much is left and I can ask about my progress so far by typing "status".  When
    /// I do so, I see my form and all that is left to fill out is my SandwichOrder.Sauce.
    /// ~~~{.txt}
    /// Please select one or more sauce
    ///  1. Honey Mustard
    ///  2. Light Mayonnaise
    ///  3. Regular Mayonnaise
    ///  4. Mustard
    ///  5. Oil
    ///  6. Pepper
    ///  7. Ranch
    ///  8. Sweet Onion
    ///  9. Vinegar
    /// > status
    /// * Sandwich: Black Forest Ham
    /// * Length: Six Inch
    /// * Bread: Nine Grain Honey Oat
    /// * Cheese: American
    /// * Toppings: Lettuce, Tomatoes, and Green Bell Peppers
    /// * Sauce: Unspecified  
    /// ~~~
    /// 
    /// I select "1" for "Honey Mustard" and I've reached the end and I'm asked to confirm my order.
    /// ~~~{.txt}
    /// Please select one or more sauce
    ///  1. Honey Mustard
    ///  2. Light Mayonnaise
    ///  3. Regular Mayonnaise
    ///  4. Mustard
    ///  5. Oil
    ///  6. Pepper
    ///  7. Ranch
    ///  8. Sweet Onion
    ///  9. Vinegar
    /// > 1
    /// Is ths your selection?
    /// * Sandwich: Black Forest Ham
    /// * Length: Six Inch
    /// * Bread: Nine Grain Honey Oat
    /// * Cheese: American
    /// * Toppings: Lettuce, Tomatoes, and Green Bell Peppers
    /// * Sauce: Honey Mustard
    /// >
    /// ~~~
    /// 
    /// If I say "no", then I get the option to change any part of the form.  In this case I change the length and then say "y"
    /// which then returns the completed form to the caller.
    /// ~~~{.txt}
    /// Is ths your selection?
    /// * Sandwich: Black Forest Ham
    /// * Length: Six Inch
    /// * Bread: Nine Grain Honey Oat
    /// * Cheese: American
    /// * Toppings: Lettuce, Tomatoes, and Green Bell Peppers
    /// * Sauce: Honey Mustard
    /// > no
    /// What do you want to change?
    ///  1. Sandwich(Black Forest Ham)
    ///  2. Length(Six Inch)
    ///  3. Bread(Nine Grain Honey Oat)
    ///  4. Cheese(American)
    ///  5. Toppings(Lettuce, Tomatoes, and Green Bell Peppers)
    ///  6. Sauce(Honey Mustard)
    /// > 2
    /// Please select a length(current choice: Six Inch) (1. Six Inch, 2. Foot Long)
    /// > 2
    /// Is ths your selection?
    /// * Sandwich: Black Forest Ham
    /// * Length: Foot Long
    /// * Bread: Nine Grain Honey Oat
    /// * Cheese: American
    /// * Toppings: Lettuce, Tomatoes, and Green Bell Peppers
    /// * Sauce: Honey Mustard
    ///
    /// > y
    /// ~~~
    /// At this point, the form is completed and will be returned to the parent dialog.  Throughout this interaction you can
    /// see that the automatically generated conversation:
    /// * Provided clear guidance and help  
    /// * Understands both numbers and textual entries  
    /// * Gives feedback on what is understood and what is not.  
    /// * Asks clarifying questions when needed.  
    /// * Allows navigating between the steps.  
    /// 
    /// All of this is pretty amazing for not having to do any of the work!  
    /// However, not every interaction was as good as you might want it to be.  That is why there are easy ways to provide:
    /// * Messages during the process of filling in a form.  
    /// * Custom prompts per field.  
    /// * Templates to use when automatically generating prompts or help.  
    /// * %Terms to match on.  
    /// * Whether to show choices and numbers or not.  
    /// * Fields that are optional.  
    /// * Conditional fields.  
    /// * Value validation  
    /// * and much more...
    /// 
    /// The next example shows how to improve the sandwich bot with attributes, business logic and the FormBuilder.
    ///
    /// \section annotatedSandwich Improved Sandwich Bot
    /// This example builds on the previous one by:
    /// * Adding some new field types including string and DateTime.  
    /// * Adding  attributes to add descriptions, terms, prompts and templates.  
    /// * Switching from fields to properties to incorporate business logic.  
    /// * Adding messages, flow control and confirmations.  
    /// 
    /// \subsection attributes Attributes
    /// %FormFlow includes some C# attributes you can add to your class to better control the dialog.  
    /// Here are the attributes:
    /// 
    /// Attribute | Purpose
    /// ----------| -------
    /// [Describe] | Change how a field or a value is shown in text.
    /// [Numeric] | Provide limits on the values accepted in a numeric field.
    /// [Optional]| Mark a field as optional which means that one choice is not to supply a value.
    /// [Pattern] | Define a regular expression to validate a string field.
    /// [Prompt]| Define a prompt to use when asking for a field.
    /// [Template] | Define a template that is used to generate prompts or values in prompts.
    /// [Terms] | Define the input terms that match a field or value.
    /// 
    /// Let's look at how these attributes can improve your bot.  First off, we might want to change the the prompt for SandwichOrder.Sandwich from the
    /// automatically generated "Please select a sandwich" to "What kind of sandwich would you like?". To do this we would use the [Prompt] attribute like this:
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip Prompt
    /// \until ]
    /// \skip SandwichOptions
    /// \until ;
    /// 
    /// One thing you will notice is the the prompt contains the funny strings "{&}" and "{||}".  
    /// That is because prompt and message strings are written in \ref patterns where you can
    /// have parts of the string that are filled in when the actual prompt or message is generated.  
    /// In this case "{&}" means fill in the description of the field and "{||}" means 
    /// show the choices that are possible.  The description of a field is automatically generated from the field name, 
    /// but you could also use a Describe attribute to override that.  
    /// By adding this attribute, the prompt for SandwichOrder.Sandwich now looks like:
    /// ~~~{.txt}
    /// What kind of sandwich would you like?
    /// 1. BLT
    /// 2. Black Forest Ham
    /// 3. Buffalo Chicken
    /// 4. Chicken And Bacon Ranch Melt
    /// 5. Cold Cut Combo
    /// 6. Meatball Marinara
    /// 7. Oven Roasted Chicken
    /// 8. Roast Beef
    /// 9. Rotisserie Style Chicken
    /// 10. Spicy Italian
    /// 11. Steak And Cheese
    /// 12. Sweet Onion Teriyaki
    /// 13. Tuna
    /// 14. Turkey Breast
    /// 15. Veggie
    /// >
    /// ~~~
    /// 
    /// There are lots of things you can control when specifiying a prompt.  For example with this prompt attribute:
    /// ~~~
    /// [Prompt("What kind of {&} would you like? {||}", ChoiceFormat="{1}")]
    /// ~~~
    /// The prompt would now no longer show numbers and the only accepted input would be words from the choices.
    /// ~~~{.txt}
    /// What kind of sandwich would you like?
    /// * BLT
    /// * Black Forest Ham
    /// * Buffalo Chicken
    /// * Chicken And Bacon Ranch Melt
    /// * Cold Cut Combo
    /// * Meatball Marinara
    /// * Oven Roasted Chicken
    /// * Roast Beef
    /// * Rotisserie Style Chicken
    /// * Spicy Italian
    /// * Steak And Cheese
    /// * Sweet Onion Teriyaki
    /// * Tuna
    /// * Turkey Breast
    /// * Veggie
    /// >
    /// ~~~
    /// 
    /// Going a step further, you could decide not to show the choices at all like this:
    /// ~~~
    /// [Prompt("What kind of {&} would you like?")]
    /// ~~~
    /// The prompt then would not show the choices at all, but they would still be available in help.
    /// ~~~{.txt}
    /// What kind of sandwich would you like?
    /// > ?
    /// * You are filling in the sandwich field. Possible responses:
    /// * You can enter in any words from the descriptions. (BLT, Black Forest Ham, Buffalo Chicken, Chicken And Bacon Ranch Melt, Cold Cut Combo, Meatball Marinara, Oven Roasted Chicken, Roast Beef, Rotisserie Style Chicken, Spicy Italian, Steak And Cheese, Sweet Onion Teriyaki, Tuna, Turkey Breast, and Veggie)
    /// * Back: Go back to the previous question.
    /// * Help: Show the kinds of responses you can enter.
    /// * Quit: Quit the form without completing it.
    /// * Reset: Start over filling in the form. (With defaults from your previous entries.)
    /// * Status: Show your progress in filling in the form so far.
    /// * You can switch to another field by entering its name. (Sandwich, Length, Bread, Cheese, Sauces, and Toppings).
    /// ~~~
    /// 
    /// It was great if you wanted to replace just one prompt, but you can also replace
    /// the templates that are used for autommatically generating your prompts.  Here we have redefined
    /// the default template used when you want to select one result from a set of choices to a different string and asked choices to always
    /// be listed one per line.  
    /// ~~~
    /// [Template(TemplateUsage.EnumSelectOne, "What kind of {&} would you like on your sandwich? {||}", ChoiceStyle = ChoiceStyleOptions.PerLine)]
    /// public class SandwichOrder
    /// ~~~
    /// With this change, here is how the cheese and bread prompts look.
    /// ~~~{.txt}
    /// What kind of bread would you like on your sandwich?
    ///  1. Nine Grain Wheat
    ///  2. Nine Grain Honey Oat
    ///  3. Italian
    ///  4. Italian Herbs And Cheese
    ///  5. Flatbread
    /// >
    /// What kind of cheese would you like on your sandwich? 
    ///  1. American
    ///  2. Monterey Cheddar
    ///  3. Pepperjack
    /// > 
    /// ~~~
    /// As you can see, both used the new template.
    /// 
    /// Now, there is still a problem with the SandwichOrder.Cheese field--how does someone indicate that they do not want cheese at all?
    /// One option would be to add an explicit NoCheese value to your enumeration, but a second option is to mark the field as optional using the
    /// [Optional] attribute.  An optional field has a "special" no preference value.  Below is an example of the [Optional] attribute.
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip Optional
    /// \until CheeseOptions
    /// With this change the cheese prompt now looks like this:
    /// ~~~{.txt}
    ///What kind of cheese would you like on your sandwich? (current choice: No Preference)
    ///  1. American
    ///  2. Monterey Cheddar
    ///  3. Pepperjack
    /// >
    /// ~~~
    /// And if you have a current value it looks like this:
    /// ~~~{.txt}
    /// What kind of cheese would you like on your sandwich? (current choice: American)
    ///  1. American
    ///  2. Monterey Cheddar
    ///  3. Pepperjack
    ///  4. No Preference
    /// >
    /// ~~~
    /// 
    /// One way to interject some variation in the prompts and messages you generate is to define multiple \ref patterns patterns to 
    /// randomly select between. Here we have redefined the TemplateUsage.NotUnderstood template so that there are two patterns for how
    /// to respond to unknown text.
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip NotUnderstood
    /// \until class
    /// Now when the user types something that is not understood, one of the two patterns will be randomly selected like this:
    /// ~~~{.txt}
    /// What size of sandwich do you want? (1. Six Inch, 2. Foot Long)
    /// > two feet
    /// I do not understand "two feet".
    /// > two feet
    /// Try again, I don't get "two feet"
    /// > 
    /// ~~~
    /// 
    /// One of the things you can override are the terms used to match user input to a field or a value in a field.  When matching user input,
    /// terms are used to identify possible meanings for what was typed.  
    /// 
    /// By default, terms are generated by taking the field or value name and following these steps:
    /// * Break on case changes and _.  
    /// * Generate each n-gram up to a maximum length.
    /// * Add s? to the end to support plurals.  
    /// For example, the value AngusBeefAndGarlicPizza would generate: 'angus?', 'beefs?', 'garlics?', 'pizzas?', 'angus? beefs?', 'garlics? pizzas?' and 'angus beef and garlic pizza'.
    /// The word "rotisserie" is one that is highly likely to be misspelled so here we have used a regular expression to make it more likely
    /// we will match what the user types.  Because we specify Terms.MaxPhrase, Language.GenerateTerms will also generate variations for us.
    /// 
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip Terms
    /// \until Rotisserie
    /// 
    /// Given the terms now we can match input like this.
    /// ~~~{.txt}
    /// What kind of sandwich would you like?
    ///  1. BLT
    ///  2. Black Forest Ham
    ///  3. Buffalo Chicken
    ///  4. Chicken And Bacon Ranch Melt
    ///  5. Cold Cut Combo
    ///  6. Meatball Marinara
    ///  7. Oven Roasted Chicken
    ///  8. Roast Beef
    ///  9. Rotisserie Style Chicken
    ///  10. Spicy Italian
    ///  11. Steak And Cheese
    ///  12. Sweet Onion Teriyaki
    ///  13. Tuna
    ///  14. Turkey Breast
    ///  15. Veggie
    /// > rotissary chechen
    /// For sandwich I understood Rotisserie Style Chicken. "chechen" is not an option.
    /// ~~~
    /// Attributes can also be used to validate values.  For example [Numeric] can be used to restrict the range of 
    /// allowed numbers.  In the below Rating must be a number between 1 and 5.
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip Numeric
    /// \until Rating
    ///
    /// Similarly [Pattern] can be used to specify a regular expression to validate a string field like PhoneNumber.
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip Pattern
    /// \until PhoneNumber
    /// 
    /// \subsection logic Adding Business Logic
    /// Sometimes there are complex interdependencies between fields or you need to 
    /// add logic to setting or getting a value.  Here we want to add support
    /// for including all toppings except some of them.  To do this, we add a validation function which complements
    /// the list if the Everything enum value is included:
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip nameof(Toppings)
    /// \until Message
    ///
    /// In addition to the processing we also need to add some terms to match expressions like
    /// "everything" and "not".  
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip ToppingOptions
    /// \until Everything
    ///
    /// Here is what the interaction with toppings looks like:
    /// ~~~{.txt}
    /// Please select one or more toppings (current choice: No Preference)
    ///  1. Everything
    ///  2. Avocado
    ///  3. Banana Peppers
    ///  4. Cucumbers
    ///  5. Green Bell Peppers
    ///  6. Jalapenos
    ///  7. Lettuce
    ///  8. Olives
    ///  9. Pickles
    ///  10. Red Onion
    ///  11. Spinach
    ///  12. Tomatoes
    /// > everything but jalapenos
    /// For sandwich toppings you have selected Avocado, Banana Peppers, Cucumbers, Green Bell Peppers, Lettuce, Olives, Pickles, Red Onion, Spinach, and Tomatoes.
    /// ~~~
    /// 
    /// \subsection controlFlow Using the Form Builder
    /// So far we have improved your dialog via attributes and business logic.  There is 
    /// another way to improve your dialog and that is through the FormBuilder.  The FormBuilder
    /// allows more fine-grained control over the steps in your conversation and lets you put in messages
    /// and more friendly confirmations.  By default the steps specified in the builder
    /// are executed in order.  (Steps might be skipped if there is already a value, or if there
    /// is explicit navigation.)  Here is a more complex usage of FormBuilder:
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip static
    /// \until .Build
    /// \until }
    /// 
    /// This looks complex, but that is because of the addition of advanced features like validation and dynamically defined fields.
    /// (See \ref dynamicFields for more information.)
    /// The main structure is all about defining the default step order.  Here are the steps: 
    /// * Show the welcome message.
    /// * Fill in SandwichOrder.Sandwich
    /// * Fill in SandwichOrder.Length    
    /// * Fill in SandwichOrder.Bread  
    /// * Fill in SandwichOrder.Cheese
    /// * Fill in SandwichOrder.Toppings and transform the value by complementing the list if it includes everything.
    /// * Show a message confirming the selected toppings.      
    /// * Fill in SandwichOrder.Sauces  
    /// * Dynamically defined field for SandwichOrder.Specials.  (See \ref dynamicFields for more information.)
    /// * Dynamically defined confirmation for the cost
    /// * Fill in SandwichOrder.DeliveryAddress and verify the resulting string.  If it does not start with a number we return a message.
    /// * Fill in SandwichOrder.DeliveryTime with a custom prompt.  
    /// * Confirm the order.    
    /// * Add any remaining fields in the order they are defined in your class.  (If this was left out, those steps to fill in those fields would not be included.)
    /// * Show a final thank you message.  
    /// * Define an OnCompletionAsync handler to process the order.
    /// 
    /// In the SandwichOrder.DeliveryTime prompt and the confirmation message you can see an instance of
    /// the \ref patterns where pattern elements like {Length} are filled in from your C# class
    /// before the string is shown to the user.
    /// 
    /// \subsection dynamicFields Dynamically Defined Fields, Confirmations and Messages
    /// FormBuilder also allows you to do other more advanced things like dynamically switch on
    /// and off parts of your form based on the state of your object or dynamically define fields
    /// rather than drive them off a C# class.  
    /// 
    /// In order to define a dynamic field, you can implement Advanced.IField yourself, 
    /// but it is easier to make use of the Advanced.FieldReflector class.  Imagine we would like to 
    /// create some specials for free drinks and cookies but only for foot-long sandwiches.  
    /// The first step for using Advanced.FieldReflector is to define
    /// the underlying field that will contain your dynamic value, like this:
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip Sauces
    /// \skip Optional
    /// \until Specials
    /// 
    /// We can apply normal attributes to this field like [Optional] to mark the field as allowing a no preference choice and by changing the template 
    /// value for TemplateUsage.NoPreference. 
    /// 
    /// Now we need to add the dynamic part to the FormBuilder like this:
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip nameof(Specials)
    /// \until }))
    /// 
    /// There are a couple of pieces here:
    /// * Advanced.Field.SetType sets the type of the field--in this case null which means enumeration.
    /// * Advanced.Field.SetActive provides a delegate that enables the field only when the length is a foot long.  
    /// * Advanced.Field.SetDefine provides an async delegate for defining the field.  The delegate is passed the current state object and also the Advanced.Field that is being dynamically defined.  
    /// The delegate uses the fluent methods found on the field to dynamically define values. In this case we define values as strings and supply the descriptions and terms for the value.    
    /// 
    /// Messages and confirmations can also be defined dynamically.  Messages and confirmations only run when the prior steps are inactive
    /// or are completed.  This is a confirmation that computes the cost of the sandwich:
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip Confirm
    /// \until })
    /// 
    /// It is also possible to create a %FormFlow experience without any C# class at all.  The easiest way to do that
    /// is to derive a class from Advanced.Field and implement the Advanced.IFieldState methods to get and set values and unknown values.  
    ///  
    /// \subsection localizingSection Localization
    /// Once you have a great bot working in a single language, you might want to enable it in other languages.  
    /// The localization language is determined by the current thread's [CurrentUICulture] and [CurrentCulture].  
    /// By default the culture comes from the Language field of the current message, but you can change that if you wish. 
    /// Depending on your bot, there can be up to 3 different sources of localized information including:
    /// * The built-in localization for PromptDialog and %FormFlow.  
    /// * A resource file generated from the static strings found in your form.
    /// * A resource file you create with strings for dynamically computed fields, messages or confirmations.
    /// 
    /// The static strings in a form include strings that are generated from the information in your C# class and from the strings you supply as prompts, 
    /// templates, messages or confirmations. It does not include strings generated from built-in templates since those are already localized.  Since many strings 
    /// are automatically generated, it is not easy to use normal C# resource strings directly.  For this reason we have provided the code to easily
    /// extract all of the static strings in a form by either:
    /// 1. Call IFormBuilder.SaveResource on your form to save a .resx file.
    /// 2. Use the supplied `rview` tool included in the nuget package to generate a resource file from your .dll or .exe by specifying the assembly containing your static form building method and the path to that method.  For example, the resource file Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder.resx was generated by calling:
    /// ~~~~
    /// rview -g Microsoft.Bot.Sample.AnnotatedSandwichBot.dll Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder.BuildForm
    /// ~~~~
    /// Here for example are a few lines from the automatically generated .resx file:
    /// \dontinclude AnnotatedSandwichBot/Resource/Microsoft.Bot.Sample.AnnotatedSandwichBot.SandwichOrder.resx
    /// \skip Specials_description;VALUE
    /// \until sandwichs?
    /// \until data
    /// 
    /// In order to make use of a generated resource file, add it to your project and then set the neutral language by:
    /// 1. Right-click on your project and select the 'Appliction' tab.
    /// 2. Click on the 'Assembly Information' button.
    /// 3. Select the language you developed your bot in from the `Neutral Language` drop down.
    /// 
    /// With these changes, when your form is created the IFormBuilder.Build method will automatically
    /// look for resources that contain your form type name and use them to localize all of the static strings in your form. 
    /// 
    /// Dynamic fields defined using Advanced.Field.SetDefine cannot be localized since the strings in them are used to dynamically construct values when
    /// actually filling out a form.  To localize these you can make use of the normal C# localization where a C# class for string resources 
    /// is automatically generated for you. 
    /// 
    /// Once you have the resource files added to your project you need to localize them.  The easiest way to do this to make use of the Multilingual App Toolkit [MAT].
    /// We use that for our own localizations.  (And would love if people want to contribute new or improved ones.)  
    /// If you install [MAT] you can enable it on a project by:
    /// 1. Select your project in the Visual Studio Solution Explorer
    /// 2. Click on `Tools->Multilingual App Toolkit->Enable selection` to enable your project.
    /// 3. Right click on the project and select `Multilingual App Toolkit->Add Translations` to select the translations.
    /// This will create industry standard [XLF] files which you can then automatically or manually translate.  
    /// 
    /// Putting this all together, here is what the Annotated Sandwich %Bot form builder would look like where the dynamic C# resources are found in DynamicSandwich:
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip BuildLocalizedForm
    /// \until return form
    /// \until };
    /// 
    /// And here is a snippet of a dialog generated by that form when the CurrentUICulture is French.
    /// ~~~
    /// Bienvenue sur le bot d'ordre "sandwich" !
    /// Quel genre de "sandwich" vous souhaitez sur votre "sandwich"?
    ///  1. BLT
    ///  2. Jambon Forêt Noire
    ///  3. Poulet Buffalo
    ///  4. Faire fondre le poulet et Bacon Ranch
    ///  5. Combo de coupe à froid
    ///  6. Boulette de viande Marinara
    ///  7. Poulet rôti au four
    ///  8. Rôti de boeuf
    ///  9. Rotisserie poulet
    ///  10. Italienne piquante
    ///  11. Bifteck et fromage
    ///  12. Oignon doux Teriyaki
    ///  13. Thon
    ///  14. Poitrine de dinde
    ///  15. Veggie
    /// > 2
    /// 
    /// Quel genre de longueur vous souhaitez sur votre "sandwich"?
    ///  1. Six pouces
    ///  2. Pied Long
    /// > ?
    /// * Vous renseignez le champ longueur.Réponses possibles:
    /// * Vous pouvez saisir un numéro 1-2 ou des mots de la description. (Six pouces, ou Pied Long)
    /// * Retourner à la question précédente.
    /// * Assistance: Montrez les réponses possibles.
    /// * Abandonner: Abandonner sans finir
    /// * Recommencer remplir le formulaire. (Vos réponses précédentes sont enregistrées.)
    /// * Statut: Montrer le progrès en remplissant le formulaire jusqu'à présent.
    /// * Vous pouvez passer à un autre champ en entrant son nom. ("Sandwich", Longueur, Pain, Fromage, Nappages, Sauces, Adresse de remise, Délai de livraison, ou votre expérience aujourd'hui).
    /// Quel genre de longueur vous souhaitez sur votre "sandwich"?
    ///  1. Six pouces
    ///  2. Pied Long
    /// > 1
    ///
    /// Quel genre de pain vous souhaitez sur votre "sandwich"?
    ///  1. Neuf grains de blé
    ///  2. Neuf grains miel avoine
    ///  3. Italien
    ///  4. Fromage et herbes italiennes
    ///  5. Pain plat
    /// > neuf
    /// Par pain "neuf" vouliez-vous dire (1. Neuf grains miel avoine, ou 2. Neuf grains de blé)
    /// ...
    /// ~~~
    /// 
    /// \subsection jsonForms JSON Schema FormFlow 
    /// In the examples we have seen so far, forms have been defined by a C# class. 
    /// An alternative way to define your forms is to use [JObject] as your state and define the schema
    /// through [JSON Schema].  The schema provides a way of describing the fields that make up your [JObject] 
    /// and also allow annotations similar to C# attributes for controlling prompts, templates and terms.  
    /// The advantage of using a [JObject] for your state is that the form definition is entirely driven by data
    /// rather than the static definition of your type in C#.  
    /// 
    /// In order to utilize this feature you need to ensure that you add the NuGet project [Microsoft.Bot.Builder.FormFlow.Json] to your project.  
    /// This defines the new namespace %Microsoft.Bot.Builder.FormFlow.Json that contains the code to allows using JSON Schema for %FormFlow.
    /// 
    /// %FormFlow makes use of a number of standard [JSON Schema](http://json-schema.org/latest/json-schema-validation.html) keywords include:
    /// * `type` -- Defines the fields type.
    /// * `enum` -- Defines the possible field values.
    /// * `minimum` -- Defines the minimum allowed value as described in <see cref="NumericAttribute"/>.
    /// * `maximum` -- Defines the maximum allowed value as described in <see cref="NumericAttribute"/>.
    /// * `required` -- Defines what fields are required.
    /// * `pattern` -- For string fields will be used to validate the entered pattern as described in <see cref="PatternAttribute"/>.
    /// 
    /// Templates and prompts use the same vocabulary as <see cref="TemplateAttribute"/> and <see cref="PromptAttribute"/>.  
    /// The property names are the same and the values are the same as those in the underlying C# enumeration.  
    /// For example to define a template to override the <see cref="TemplateUsage.NotUnderstood"/> template
    /// and specify a <see cref="TemplateBaseAttribute.ChoiceStyle"/> you would put this in your schema: 
    /// ~~~
    /// "Templates":{ "NotUnderstood": { Patterns: ["I don't get it"], "ChoiceStyle":"Auto"}}
    /// ~~~
    /// 
    /// %Extensions defined at the root fo the schema
    /// * `OnCompletion: script` -- C# script with arguments (<see cref="IDialogContext"/> context, JObject state) for completing form.
    /// * `References: [assemblyReference, ...]` -- Define references to include in scripts.  Paths should be absolute, or relative to the current directory.  By default Microsoft.Bot.Builder.dll is included.
    /// * `Imports: [import, ...]` -- Define imports to include in scripts with usings. By default these namespaces are included: Microsoft.Bot.Builder, Microsoft.Bot.Builder.Dialogs, Microsoft.Bot.Builder.FormFlow, Microsoft.Bot.Builder.FormFlow.Advanced, System.Collections.Generic, System.Linq)
    /// 
    /// %Extensions defined at the root of a schema or as a peer of the "type" property.  
    /// * `Templates:{TemplateUsage: { Patterns:[string, ...], &lt;args&gt; }, ...}` -- Define templates.
    /// * `Prompt: { Patterns:[string, ...] &lt;args&gt;}` -- Define a prompt.
    /// 
    /// %Extensions that are found in a property description as peers to the "type" property of a JSON Schema.
    /// * `DateTime:bool` -- Marks a field as being a DateTime field.
    /// * `Describe:string` -- Description of a field as described in <see cref="DescribeAttribute"/>.
    /// * `Terms:[string,...]` -- Regular expressions for matching a field value as described in <see cref="TermsAttribute"/>.
    /// * `MaxPhrase:int` -- This will run your terms through <see cref="Language.GenerateTerms(string, int)"/> to expand them.
    /// * `Values:{ string: {Describe:string, Terms:[string, ...], MaxPhrase}, ...}` -- The string must be found in the types "enum" and this allows you to override the automatically generated descriptions and terms.  If MaxPhrase is specified the terms are passed through <see cref="Language.GenerateTerms(string, int)"/>.
    /// * `Active:script` -- C# script with arguments (JObject state)->bool to test to see if field/message/confirm is active.
    /// * `Validate:script` -- C# script with arguments (JObject state, object value)->ValidateResult for validating a field value.
    /// * `Define:script` -- C# script with arguments (JObject state, Field&lt;JObject&gt; field) for dynamically defining a field.  
    /// * `Before:[confirm|message, ...]` -- Messages or confirmations before the containing field.
    /// * `After:[confirm|message, ...]` -- Messages or confirmations after the containing field.
    /// * `{Confirm:script|[string, ...], ...templateArgs}` -- With Before/After define a confirmation through either C# script with argument (JObject state) or through a set of patterns that will be randomly selected with optional template arguments.
    /// * `{Message:script|[string, ...] ...templateArgs}` -- With Before/After define a message through either C# script with argument (JObject state) or through a set of patterns that will be randomly selected with optional template arguments.
    /// * `Dependencies`:[string, ...]` -- Fields that this field, message or confirm depends on.
    /// 
    /// Scripts can be any C# code you would find in a method body.  You can add references through "References" and using through "Imports". Special global variables include:
    /// * `choice` -- internal dispatch for script to execute.
    /// * `state` -- JObject form state bound for all scripts.
    /// * `ifield` -- <see cref="IField{JObject}"/> to allow reasoning over the current field for all scripts except %Message/Confirm prompt builders.
    /// * `value` -- object value to be validated for Validate.
    /// * `field` -- <see cref="Field{JObject}"/> to allow dynamically updating a field in Define.
    /// * `context` -- <see cref="IDialogContext"/> context to allow posting results in OnCompletion.
    /// 
    /// %Fields defined through this class have the same ability to extend or override the definitions
    /// programatically as any other field.  They can also be localized in the same way.
    /// 
    /// The simplest way to define your form is to define everything including any C# code directly in 
    /// your schema definition.  Here for example is the [JSON Schema] that corresponds to the Annotated Sandwich %Bot we have defined so far.
    /// \include AnnotatedSandwichBot/AnnotatedSandwich.json
    /// 
    /// In order to make use of your [JSON Schema] you use FormBuilderJson which supports the same fluent interface
    /// as FormBuilder.  Here is the code to make use of the [JSON Schema] above to define exactly the same functionality:
    /// \dontinclude AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// \skip BuildJsonForm
    /// \until .Build
    /// \until }
    ///
    /// \subsection quitExceptions Handling Quit and Exceptions
    /// When the user types 'quit' or there is an exception while filling in a form using FormDialog it is useful to be able
    /// to know what step the 'quit' or exception happened, the state of the form and and what steps were successfully completed.
    /// All of these are passed back through the FormCanceledException<T> class.  Here is an example of how to catch the exception and send
    /// a message after either:
    /// * Successfully processing the order.  
    /// * When the user quit.  
    /// * When there is an exception.
    /// \dontinclude AnnotatedSandwichBot/Controllers/MessagesController.cs
    /// \skip MakeRootDialog
    /// \until });
    /// \until }
    /// 
    /// \subsection finalBot Final Sandwich Bot
    /// Here is the final SandwichOrder with attributes, business logic and a more complex form.
    /// \include AnnotatedSandwichBot/AnnotatedSandwich.cs
    /// 
    /// With all of these improvements, here is what the interaction looks like now.
    /// ~~~{.txt}
    /// Welcome to the sandwich order bot!
    ///
    /// What kind of sandwich would you like?
    ///  1. BLT
    ///  2. Black Forest Ham
    ///  3. Buffalo Chicken
    ///  4. Chicken And Bacon Ranch Melt
    ///  5. Cold Cut Combo
    ///  6. Meatball Marinara
    ///  7. Oven Roasted Chicken
    ///  8. Roast Beef
    ///  9. Rotisserie Style Chicken
    ///  10. Spicy Italian
    ///  11. Steak And Cheese
    ///  12. Sweet Onion Teriyaki
    ///  13. Tuna
    ///  14. Turkey Breast
    ///  15. Veggie
    /// > 2
    /// What size of sandwich do you want? (1. Six Inch, 2. Foot Long)
    /// > 2
    /// What kind of bread would you like on your sandwich?
    ///  1. Nine Grain Wheat
    ///  2. Nine Grain Honey Oat
    ///  3. Italian
    ///  4. Italian Herbs And Cheese
    ///  5. Flatbread
    /// > nine grain
    /// By "nine grain" bread did you mean (1. Nine Grain Honey Oat, 2. Nine Grain Wheat)
    /// > 1
    /// What kind of cheese would you like on your sandwich? (current choice: No Preference)
    ///  1. American
    ///  2. Monterey Cheddar
    ///  3. Pepperjack
    /// > 3
    /// Please select one or more toppings(current choice: No Preference)
    ///  1. Everything
    ///  2. Avocado
    ///  3. Banana Peppers
    ///  4. Cucumbers
    ///  5. Green Bell Peppers
    ///  6. Jalapenos
    ///  7. Lettuce
    ///  8. Olives
    ///  9. Pickles
    ///  10. Red Onion
    ///  11. Spinach
    ///  12. Tomatoes
    /// > everything but jalapenos
    /// For sandwich toppings you have selected Avocado, Banana Peppers, Cucumbers, Green Bell Peppers, Lettuce, Olives, Pickles, Red Onion, Spinach, and Tomatoes.
    ///
    /// Please select one or more sauces (current choice: No Preference)
    ///  1. Honey Mustard
    ///  2. Light Mayonnaise
    ///  3. Regular Mayonnaise
    ///  4. Mustard
    ///  5. Oil
    ///  6. Pepper
    ///  7. Ranch
    ///  8. Sweet Onion
    ///  9. Vinegar
    /// >
    /// What kind of specials would you like on your sandwich? (current choice: None)
    ///  1. Free cookie
    ///  2. Free large drink
    /// > 1
    /// Total for your sandwich is $6.50 is that ok?
    /// >  y
    /// Please enter delivery address
    /// > 123 State Street
    /// What time do you want your sandwich delivered? (current choice: No Preference)
    /// > 4:30pm
    /// Do you want to order your Foot Long Black Forest Ham on Nine Grain Honey Oat bread with Pepperjack, Avocado, Banana Peppers, Cucumbers, Green Bell Peppers, Lettuce, Olives, Pickles, Red Onion, Spinach, and Tomatoes to be sent to 123 State Street at 4:30 PM?
    /// > y
    /// Please enter a number between 1.0 and 5.0 for your experience today(current choice: No Preference)
    /// > 5
    /// Thanks for ordering a sandwich!
    /// Processed your order!
    /// ~~~
    /// 
    /// \section initialState Passing in Initial Form State and Entities
    /// When you launch a FormDialog, you can optionally pass in an instance of your state.
    /// If you do that, any step for filling a field is skipped if that field has a value unless you pass in
    /// FormOptions.PromptFieldsWithValues which will prompt for fields, but use the passed in state for defaults.
    /// 
    /// You can also pass in [LUIS] entities to bind to the state.  If the [EntityRecommendation.Type]
    /// is a path to a field in your C# class then the [EntityRecommendation.Entity] will be 
    /// passed through the recognizer to bind to your field.  Just like initial state, any step for 
    /// filling in that field will be skipped.
    /// 
    /// \section patterns Pattern Language
    /// One of the keys to creating a bot is being able to generate text that is clear and
    /// meaningful to the bot user.  This framework supports a pattern language with  
    /// elements that can be filled in at runtime.  Everything in a pattern that is not surrounded by curly braces
    /// is just passed straight through.  Anything in curly braces is substitued with values to make a string that can be
    /// shown to the user. Once substitution is done, some additional processing to remove double spaces and
    /// use the proper form of a/an is also done.
    /// 
    /// Possible curly brace pattern elements are outlined in the table below.  Within a pattern element, "<field>" refers to the  path within your form class to get
    /// to the field value.  So if I had a class with a field named "Size" you would refer to the size value with the pattern element {Size}.  
    /// "..." within a pattern element means multiple elements are allowed.  "<format>" within a pattern element means that you
    /// can optionally specify a regular C# format specifier, i.e. if "Rating" were a double field I could show it with
    /// two digits of precision by using the pattern element "{Rating:F2}".  "<n>" shows where you can specify a reference to the nth
    /// argument of a template.  (See TemplateUsage to see what arguments each template can use.)
    /// 
    /// Pattern Element | Description
    /// --------------- | -----------
    /// {<format>} | Value of the current field.
    /// {&} | Description of the current field.
    /// {<field><format>} | Value of a particular field. 
    /// {&<field>} | Description of a particular field.
    /// {\|\|} | Show the current choices which can be the current value, no preference or the possible values for enumerated fields.
    /// {[{<field><format>} ...]} | Create a list with all field values together utilizing [Separator] and [LastSeparator] to separate the individual values.
    /// {*} | Show one line for each active field with the description and current value.
    /// {*filled} | Show one line for each active field that has an actual value with the description and current value.
    /// {<nth><format>} | A regular C# format specifier that refers to the nth arg.  See TemplateUsage to see what args are available.
    /// {?<textOrPatternElement>...} | Conditional substitution.  If all referred to pattern elements have values, the values are substituted and the whole expression is used.
    ///
    /// Patterns are used in [Prompt] and [Template] attributes.  
    /// [Prompt] defines a prompt to the user for a particular field or confirmation.  
    /// [Template] is used to automatically construct prompts and other things like help.
    /// There is a built-in set of templates defined in FormConfiguration.Templates.
    /// A good way to see examples of the pattern language is to look at the templates defined there.
    /// A [Prompt] can be specified by using it as an attribute on a particular field or property or implicitly defined through IFormBuilder<T>.Field.
    /// A default [Template] can be overridden on a class or field basis.  
    /// Both prompts and templates support the formatting parameters outlined below.
    /// 
    /// Usage | Description
    /// ------|------------
    /// [AllowDefault] | When processing choices using {\|\|} controls whether the current value should be showed as a choice.
    /// [ChoiceCase] | When prcoessing choices for {\|\|} controls case normalization for each choice.
    /// [ChoiceFormat] | When processing choices for {\|\|} controls how each choice is formatted. {0} is the choice number and {1} the choice description.
    /// [ChoiceLastSeparator] | When inline choice lists are constructed for {\|\|} provides the separator before the last choice.
    /// [ChoiceParens] | When inline choice lists are constructed for {\|\|} indicates whether or not they are in parentheses.
    /// [ChoiceSeparator] | When inline choice lists are constructed for {\|\|} provides the separaotr before every choice except the last.
    /// [ChoiceStyle] | When processing choices using {\|\|} controls whether the choices are presented in line or per line.
    /// [Feedback] | For [Prompt] only controls feedback after user entry.
    /// [FieldCase] | Controls case normalization when displaying a field description.
    /// [LastSeparator] | When lists are constructed for {[]} provides the separator before the last item.
    /// [Separator] | When lists are constructed for {[]} provides the separator before every item except the last.
    /// [ValueCase] | Controls case normalization when displaying a field value.
    /// 
}