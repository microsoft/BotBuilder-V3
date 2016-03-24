namespace Microsoft.Bot.Builder.Form
{
    /// \page examples
    /// 
    /// \section simplesandwichbot Simple Sandwich Bot
    /// As an example of %Form Flow in action, this will outline a simple sandwich ordering form that we will elaborate
    /// to show various features.  To start with the form stuff you need to create a C# class to represent your form.  Like this:
    /// \include SimpleSandwichBot/sandwich.cs
    /// 
    /// In order to connect your form to the bot framework you need to add it to your controller like this:
    /// \dontinclude SimpleSandwichBot/Controllers/MessagesController.cs
    /// \skip Post
    /// \until }
    /// 
    /// The combination of your C# class and connecting it to the Bot Framework is enough to automatically create a conversation.  
    /// Here is an example interaction that demonstrates some of the features offered by Form Flow.  A <b>></b> symbol shows 
    /// When a response is expected from the user.
    /// ~~~
    ///  Please select a sandwich
    ///  1. BLT
    ///  2. Black Forest Ham
    ///  3. Buffalo Chicken
    ///  4. Chicken And Bacon Ranch Melt
    ///  5. Cold Cut Combo
    ///  6. Meatball Marinara
    ///  7. Over Roasted Chicken
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
    /// choices that make up the list.  Each enumeration was broken into words based on case and _.  
    /// 
    /// Now what are the possible responses?  If you ask for "help" you can see the possibilities like this:
    /// ~~~
    /// > help
    /// * You are filling in the sandwich field.Possible responses:
    /// * You can enter a number 1-15 or words from the descriptions. (BLT, Black Forest Ham, Buffalo Chicken, Chicken And Bacon Ranch Melt, Cold Cut Combo, Meatball Marinara, Over Roasted Chicken, Roast Beef, Rotisserie Style Chicken, Spicy Italian, Steak And Cheese, Sweet Onion Teriyaki, Tuna, Turkey Breast, and Veggie)
    /// * Back: Go back to the previous question.
    /// * Help: Show the kinds of responses you can enter.
    /// * Quit: Quit the form without completing it.
    /// * Reset: Start over filling in the form. (With defaults of your previous entries.)
    /// * Status: Show your progress in filling in the form so far.
    /// * You can switch to another field by entering its name. (Sandwich, Length, Bread, Cheese, Toppings, and Sauce).
    /// ~~~  
    /// 
    /// The possibilities include responding with the number of the choice you want, or you can also use the words that
    /// are found in the choice descriptions.  There are also a number of commands that let you back up a step, get help, quit, start over 
    /// or get the progress so far.  Let's enter "2" to select "Black Forest Ham".
    /// ~~~
    ///  Please select a sandwich
    ///  1. BLT
    ///  2. Black Forest Ham
    ///  3. Buffalo Chicken
    ///  4. Chicken And Bacon Ranch Melt
    ///  5. Cold Cut Combo
    ///  6. Meatball Marinara
    ///  7. Over Roasted Chicken
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
    /// on your change your sandwich type you could type 'back' like this:
    /// ~~~
    /// > back
    /// Please select a sandwich(current choice: Black Forest Ham)
    ///  1. BLT
    ///  2. Black Forest Ham
    ///  3. Buffalo Chicken
    ///  4. Chicken And Bacon Ranch Melt
    ///  5. Cold Cut Combo
    ///  6. Meatball Marinara
    ///  7. Over Roasted Chicken
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
    /// ~~~
    /// Please select a sandwich (current choice: Black Forest Ham)
    ///  1. BLT
    ///  2. Black Forest Ham
    ///  3. Buffalo Chicken
    ///  4. Chicken And Bacon Ranch Melt
    ///  5. Cold Cut Combo
    ///  6. Meatball Marinara
    ///  7. Over Roasted Chicken
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
    /// is ambiguous and Form Flow system automatically asks for clarification.
    /// ~~~
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
    /// What happens if you type in something which is not understood or a mixture of understood and not understood?  In the below
    /// you can see both something that is not understood at all and also a mixture of understood and not understood things.
    /// ~~~
    /// Please select a cheese (1. American, 2. Monterey Cheddar, 3. Pepperjack)
    /// > amercan
    /// "amercan" is not a cheese option.
    /// > american smoked
    /// For cheese I understood American. "smoked" is not an option.
    /// ~~~
    /// 
    /// Some fields like SandiwchOrder.Toppings allow multiple possibilities. Here we are entering multiple possibilities and showing 
    /// another example of clarification.
    /// ~~~
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
    /// I do so, I see my form and all that is left is the SandwichOrder.Sauce.
    /// ~~~
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
    /// ~~~
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
    /// ~~~
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
    /// At this point, I've completed my form and will return to the parent dialog.  Throughout this interaction you can
    /// see that the automatically generated conversation:
    /// * Provided clear guidance and help  
    /// * Understands both numbers and textual entries  
    /// * Gives feedback on what is understood and what is not.  
    /// * Asks clarifying questions when needed.  
    /// * Allows navigating between the steps.  
    /// All of this is pretty amazing for not having to do any of the work!  However, not every interaction was as
    /// good as you might want it to be.  That is why there are easy ways to provide:
    /// * Messages during the process of filling in a form.  
    /// * Custom prompts per field.  
    /// * Templates to use when automatically generating prompts or help.  
    /// * Terms to match on.  
    /// * Whether to show choices and numbers or not.  
    /// * Fields that are optional.  
    /// * Conditional fields.  
    /// * Value validation  
    /// * and much more...
    /// The next example shows how to improve the sandwich bot.
    ///
    /// \section annotatedSandwich Annotated Sandwich Bot
    /// This example builds on the previous one by:
    /// * Adding  attributes to add descriptions, terms, prompts and templates.  
    /// * Switching from fields to properties to incorporate business logic.  
    /// * Adding messages, flow control and nice confirmations.  
    /// 
    /// \subsection attributes Attributes
    /// Form Flow includes some attributes you can add to your class to better control the dialog.  Here are the five attributes:
    /// 
    /// Attribute | Purpose
    /// ----------| -------
    /// Describe | Change how a field or a value is shown in text.
    /// Numeric | Provide limits on the values accepted in a numeric field.
    /// Optional | Mark a field as optional which means that one choice is not to supply a value.
    /// Prompt | Define a prompt to use when asking for a field.
    /// Template | Define a template that is used to generate prompts or values in prompts.
    /// Terms | Define the input terms that match a field or value.
    /// 
    /// Here is the SandwichOrder with attributes added and some business logic.
    /// \include AnnotatedSandwichBot/sandwich.cs
    /// 
}