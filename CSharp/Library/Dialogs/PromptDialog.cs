// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// The style of generated prompt
    /// </summary>
    public enum PromptSyle
    {
        /// <summary>
        /// Generate buttons for choices and let connector generate the right style based on channel capabilities
        /// </summary>
        Auto,
        
        /// <summary>
        /// Show choices on the same line.
        /// </summary>
        Inline,

        /// <summary>
        /// Show choices with one per line.
        /// </summary>
        PerLine,

        /// <summary>
        /// Do not show possible choices in the prompt
        /// </summary>
        None
    }

    /// <summary>   Dialog factory for simple prompts. </summary>
    public class PromptDialog
    {
        /// <summary>   Prompt for a string. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        public static void Text(IDialogContext context, ResumeAfter<string> resume, string prompt, string retry = null, int attempts = 3)
        {
            var child = new PromptString(prompt, retry, attempts);
            context.Call<string>(child, resume);
        }

        /// <summary>   Ask a yes/no question. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        /// <param name="promptStyle"> Style of the param <see cref="PromptSyle" /> </param>
        public static void Confirm(IDialogContext context, ResumeAfter<bool> resume, string prompt, string retry = null, int attempts = 3, PromptSyle promptStyle = PromptSyle.Auto)
        {
            var child = new PromptConfirm(prompt, retry, attempts, promptStyle);
            context.Call<bool>(child, resume);
        }

        /// <summary>   Prompt for a long. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        public static void Number(IDialogContext context, ResumeAfter<long> resume, string prompt, string retry = null, int attempts = 3)
        {
            var child = new PromptInt64(prompt, retry, attempts);
            context.Call<long>(child, resume);
        }

        /// <summary>   Prompt for a double. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        public static void Number(IDialogContext context, ResumeAfter<double> resume, string prompt, string retry = null, int attempts = 3)
        {
            var child = new PromptDouble(prompt, retry, attempts);
            context.Call<double>(child, resume);
        }

        /// <summary>   Prompt for one of a set of choices. </summary>
        /// <param name="context">  The context. </param>
        /// <param name="resume">   Resume handler. </param>
        /// <param name="options">  The possible options all of which must be convertible to a string.</param>
        /// <param name="prompt">   The prompt to show to the user. </param>
        /// <param name="retry">    What to show on retry. </param>
        /// <param name="attempts"> The number of times to retry. </param>
        /// <param name="promptStyle"> Style of the param <see cref="PromptSyle" /> </param>
        public static void Choice<T>(IDialogContext context, ResumeAfter<T> resume, IEnumerable<T> options, string prompt, string retry = null, int attempts = 3, PromptSyle promptStyle = PromptSyle.Auto)
        {
            var child = new PromptChoice<T>(options, prompt, retry, attempts, promptStyle);
            context.Call<T>(child, resume);
        }

        /// <summary>   Prompt for a text string. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Text(IDialogContext, ResumeAfter{string}, string, string, int)"/>.</remarks>
        [Serializable]
        public sealed class PromptString : Prompt<string>
        {
            /// <summary>   Constructor for a prompt string dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            public PromptString(string prompt, string retry, int attempts)
                : base(prompt, retry, attempts)
            {
            }

            protected override bool TryParse(Message message, out string result)
            {
                if (!string.IsNullOrWhiteSpace(message.Text))
                {
                    result = message.Text;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            protected override string DefaultRetry
            {
                get
                {
                    return Resources.PromptRetry + "\n" + this.prompt;
                }
            }
        }

        /// <summary>   Prompt for a confirmation. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Confirm(IDialogContext, ResumeAfter{bool}, string, string, int, PromptSyle)"/>.</remarks>
        [Serializable]
        public sealed class PromptConfirm : Prompt<bool>
        {
            /// <summary>   Constructor for a prompt confirmation dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            /// <param name="promptStyle"> Style of the param <see cref="PromptSyle" /> </param>
            public PromptConfirm(string prompt, string retry, int attempts, PromptSyle promptStyle = PromptSyle.Auto)
                : base(prompt, retry, attempts, promptStyle)
            {
            }

            protected override bool TryParse(Message message, out bool result)
            {
                var found = false;
                result = false;
                if (message.Text != null)
                {
                    var term = message.Text.Trim().ToLower();
                    if ((from r in Resources.MatchYes.SplitList() select r.ToLower()).Contains(term))
                    {
                        result = true;
                        found = true;
                    }
                    else if ((from r in Resources.MatchNo.SplitList() select r.ToLower()).Contains(term))
                    {
                        result = false;
                        found = true;
                    }
                }
                return found;
            }

            protected override Message MakePrompt(IDialogContext context, string prompt)
            {
                var msg = context.MakeMessage();
                var options = new String[] { Resources.MatchYes.SplitList().First(), Resources.MatchNo.SplitList().First() };
                msg.MakePrompt(prompt, options, promptStyle);
                return msg;
            }

            protected override string DefaultRetry
            {
                get
                {
                    return Resources.PromptRetry + "\n" + this.prompt;
                }
            }
        }

        /// <summary>   Prompt for a confirmation. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Number(IDialogContext, ResumeAfter{long}, string, string, int)"/>.</remarks>
        [Serializable]
        public sealed class PromptInt64 : Prompt<Int64>
        {
            /// <summary>   Constructor for a prompt int64 dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            public PromptInt64(string prompt, string retry, int attempts)
                : base(prompt, retry, attempts)
            {
            }

            protected override bool TryParse(Message message, out Int64 result)
            {
                return Int64.TryParse(message.Text, out result);
            }
        }

        /// <summary>   Prompt for a double. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Number(IDialogContext, ResumeAfter{double}, string, string, int)"/>.</remarks>
        [Serializable]
        public sealed class PromptDouble: Prompt<double>
        {
            /// <summary>   Constructor for a prompt double dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            public PromptDouble(string prompt, string retry, int attempts)
                : base(prompt, retry, attempts)
            {
            }
            
            protected override bool TryParse(Message message, out double result)
            {
                return double.TryParse(message.Text, out result);
            }
        }

        /// <summary>   Prompt for a choice from a set of choices. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Choice{T}(IDialogContext, ResumeAfter{T}, IEnumerable{T}, string, string, int, PromptSyle)"/>.</remarks>
        [Serializable]
        public class PromptChoice<T> : Prompt<T>
        {
            private readonly IEnumerable<T> options;

            /// <summary>   Constructor for a prompt choice dialog. </summary>
            /// <param name="options">Enumerable of the options to choose from.</param>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            /// <param name="promptStyle"> Style of the param <see cref="PromptSyle" /> </param>
            public PromptChoice(IEnumerable<T> options, string prompt, string retry, int attempts, PromptSyle promptStyle = PromptSyle.Auto)
                : base(prompt, retry, attempts, promptStyle)
            {
                SetField.NotNull(out this.options, nameof(options), options);
            }

            public virtual bool IsMatch(T option, string text)
            {
                return option.ToString().IndexOf(text.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0;
            }

            protected override Message MakePrompt(IDialogContext context, string prompt)
            {
                var msg = context.MakeMessage();
                msg.MakePrompt(prompt, options, promptStyle);
                return msg; 
            }

            protected override bool TryParse(Message message, out T result)
            {
                if (!string.IsNullOrWhiteSpace(message.Text))
                {
                    var selected = this.options
                        .Where(option => IsMatch(option, message.Text))
                        .ToArray();
                    if (selected.Length == 1)
                    {
                        result = selected[0];
                        return true;
                    }
                }

                result = default(T);
                return false;
            }
        }

    }

    public static partial class Extensions
    {
        /// <summary>
        /// Generates buttons from options and add them to the message.
        /// </summary>
        /// <remarks>
        /// <typeparamref name="T"/> should implement ToString().
        /// </remarks>
        /// <typeparam name="T"> Type of the options.</typeparam>
        /// <param name="message"> The message that the buttons will be added to.</param>
        /// <param name="options"> The options that cause generation of buttons.</param>
        public static void AddButtons<T>(this Message message, IEnumerable<T> options)
        {
            message.Attachments = new List<Attachment>();
            message.Attachments = options.GenerateButtons();
        }

        internal static IList<Attachment> GenerateButtons<T>(this IEnumerable<T> options)
        {
            var actions = new List<Connector.Action>();
            foreach (var option in options)
            {
                actions.Add(new Connector.Action
                {
                    Title = option.ToString(),
                    Message = option.ToString()
                });
            }

            var attachments = new List<Attachment>
            {
                new Attachment
                {
                    Actions  = actions
                }
            };

            return attachments; 
        }
        
        internal static void MakePrompt<T>(this Message message, string prompt, IEnumerable<T> options = null, PromptSyle promptStyle = PromptSyle.Auto)
        {
            switch (promptStyle)
            {
                case PromptSyle.Auto:
                    message.Text = prompt;
                    message.AddButtons(options);
                    break;
                case PromptSyle.Inline:
                    message.Text = $"{prompt} {FormFlow.Advanced.Language.BuildList(options.Select(option => option.ToString()), Resources.DefaultChoiceSeparator, Resources.DefaultChoiceLastSeparator)}";
                    break;
                case PromptSyle.PerLine:
                    message.Text = $"{prompt}\n{FormFlow.Advanced.Language.BuildList(options.Select(option => $"* {option.ToString()}"), "\n", "\n")}";
                    break;
                case PromptSyle.None:
                default:
                    message.Text = prompt;
                    break;
            }
        }
    }
}

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    
    [Serializable]
    public abstract class Prompt<T> : IDialog<T>
    {
        protected readonly string prompt;
        protected readonly string retry;
        protected int attempts;
        protected readonly PromptSyle promptStyle; 

        public Prompt(string prompt, string retry, int attempts, PromptSyle promptStyle = PromptSyle.Auto)
        {
            SetField.NotNull(out this.prompt, nameof(prompt), prompt);
            SetField.NotNull(out this.retry, nameof(retry), retry ?? prompt);
            this.promptStyle = promptStyle;
            this.attempts = attempts;
        }
        
        async Task IDialog<T>.StartAsync(IDialogContext context)
        {
            await context.PostAsync(this.MakePrompt(context, this.prompt)); 
            context.Wait(MessageReceived);
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<Message> message)
        {
            T result;
            if (this.TryParse(await message, out result))
            {
                context.Done(result);
            }
            else
            {
                --this.attempts;
                if (this.attempts > 0)
                {
                    var retry = this.retry ?? this.DefaultRetry;
                    await context.PostAsync(this.MakePrompt(context, this.retry));
                    context.Wait(MessageReceived);
                }
                else
                {
                    await context.PostAsync("too many attempts");
                    throw new Exception();
                }
            }
        }

        protected abstract bool TryParse(Message message, out T result);

        protected virtual Message MakePrompt(IDialogContext context, string prompt)
        {
            var msg = context.MakeMessage();
            msg.Text = prompt;
            return msg; 
        }

        protected virtual string DefaultRetry
        {
            get
            {
                return this.prompt;
            }
        }
    }
}