---
layout: page
title: Form bot
permalink: /en-us/azure-bot-service/templates/form/
weight: 12100
parent1: Azure Bot Service
parent2: Templates
---


The form bot template shows how to use forms to perform a guided conversation with the user. You typically use guided conversations when you need the user to answer a series of questions (for example, when ordering a sandwich). For information about using forms, see [FormFlow](/en-us/csharp/builder/sdkreference/forms.html).

The routing of the message is identical to the one presented in the [Basic bot template](/en-us/azure-bot-service/templates/basic/), please refer to that document for more info.


Most messages will have a Message activity type, and will contain the text and attachments that the user sent. If the message’s activity type is Message, the template posts the message to **MainDialog** in the context of the current message (see MainDialog.csx). 

{% highlight csharp %}
        switch (activity.GetActivityType())
        {
            case ActivityTypes.Message:
                await Conversation.SendAsync(activity, () => new MainDialog());
                break;
{% endhighlight %}

The **MainDialog** object inherits from **BasicFormFlowDialog**. The parameter to the **MainDialog** constructor is the name of the method (delegate) that builds the form. In this case, it’s the **BuildForm** method defined by **BasicFormFlowDialog** (see BasicFormFlowDialog.csx). The delegate is used later in the dialog’s **MessageReceivedAsync** method when it instantiates the **FormDialog** object.

{% comment %}
???
The **DefaultIfExceptionMethod** stops the propagation of the exception and returns the **BasicFormFlowDialog** object. 
??
{% endcomment %}


The MainDialog.csx file contains the root dialog that controls the conversation with the user. When the dialog’s instantiated, the dialog’s **StartAsync** method runs and calls IDialogContext.Wait with the continuation delegate that’s called when there is a new message. In the initial case, there is an immediate message available (the one that launched the dialog) and the message is immediately passed to the **MessageReceivedAsync** method.

{% highlight csharp %}
public class MainDialog: IDialog<BasicFormFlowDialog>
    {
        private readonly BuildFormDelegate<BasicFormFlowDialog> MakeForm;
        
        internal MainDialog(BuildFormDelegate<BasicFormFlowDialog> makeForm)
        {
            this.MakeForm = makeForm; 
        }

        public Task StartAsync(IDialogContext context)
        {
            try
            {
                context.Wait(MessageReceivedAsync);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }

            return Task.CompletedTask;
        }
{% endhighlight %}

The **MessageReceivedAsync** method creates the form and starts asking the questions. The parameters to the **FormDialog** constructor are the **BasicFormFlowDialog**, which defines the form (or list of questions), and the `MakeForm` variable, which contains the name of the delegate that builds the form. The delegate’s name is specified in the **Run** method when it processes the user’s original message. The **Call** method starts the form and specifies the delegate that handles the completed form. After the **FormCompleted** method finishes processing the user’s input, the method calls the IDialogContext.Wait method, which suspends the bot until it receives the next message.

{% highlight csharp %}
        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var sampleForm = new FormDialog<BasicFormFlowDialog>(new BasicFormFlowDialog(), MakeForm, FormOptions.PromptInStart, null);
            context.Call<BasicFormFlowDialog>(sampleForm, FormComplete);
        }

        private async Task FormComplete(IDialogContext context, IAwaitable<BasicFormFlowDialog> result)
        {
            BasicFormFlowDialog form = null;
            try
            {
                form = await result;
            }
            catch (OperationCanceledException)
            {
                await context.PostAsync("You canceled the form! Type anything to restart it.");
                return;
            }

            if (form != null)
            {
                await context.PostAsync("Thanks for completing the form! Just type anything to restart it.");
            }
            else
            {
                await context.PostAsync("Form returned empty response! Type anything to restart it.");
            }

            context.Wait(MessageReceivedAsync);
        }
{% endhighlight %}

The following shows the **BasicFormFlowDialog** object that defines the form. The public properties define the questions to ask. The Prompt property attribute contains the prompt text that’s shown to the user. Anything within curly brackets ({}) are substitution characters. For example, {&} tells the form to use the property’s name in the prompt. If the property’s data type is an enumeration, {\|\|} tells the form to display the enumeration’s values as the list of possible values that the user can choose from. For example, the data type for the Color property is ColorOptions. When the form asks the user for their favorite car color, the form will display **1. Red, 2. White, and 3. Blue** as possible values. For more information about substitution strings, see [Pattern Language](/en-us/csharp/builder/sdkreference/forms.html#patterns). 

{% highlight csharp %}
public enum CarOptions { Convertible=1, SUV, EV };
public enum ColorOptions { Red=1, White, Blue };

[Serializable]
public class BasicFormFlowDialog
{
    [Prompt("Hi! What is your {&}?")]
    public string Name { get; set; }

    [Prompt("Please select your favorite car type {||}")]
    public CarOptions Car { get; set; }

    [Prompt("Please select your favorite {&} {||}")]
    public ColorOptions Color { get; set; }

    public static IForm<BasicFormFlowDialog> BuildForm()
    {
        // Builds an IForm<T> based on BasicFormFlowDialog
        return new FormBuilder<BasicFormFlowDialog>().Build();
    }

    public static IFormDialog<BasicFormFlowDialog> BuildFormDialog()
    {
        // Generates a new FormDialog<T> based on IForm<BasicFormFlowDialog>
        return FormDialog.FromForm(BasicFormFlowDialog.BuildForm);
    }
}
{% endhighlight %}

The following are the project files used by the function; you should not have to modify any of them.

|**File**|**Description**
|function.json|This file contains the function’s bindings. You should not modify this file.
|project.json|This file contains the project’s NuGet references. You should only have to change this file if you add a new reference.
|project.lock.json|This file contains the list of packages. You should not modify this file.
|host.json|A metadata file that contains the global configuration options affecting the function.

{% comment %}
???
If they set up continuous integration, do they include these files as well?
???
{% endcomment %}

