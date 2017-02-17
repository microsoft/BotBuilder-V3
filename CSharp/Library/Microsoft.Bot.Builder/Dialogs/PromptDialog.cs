// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
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

using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Resource;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// The style of generated prompt
    /// </summary>
    public enum PromptStyle
    {
        /// <summary>
        /// Generate buttons for choices and let connector generate the right style based on channel capabilities
        /// </summary>
        Auto,

        /// <summary>
        /// Generate keyboard card for choices that will be mapped to a 
        /// <see cref="HeroCard"/> or a keyboard, e.g. Facebook quick replies
        /// </summary>
        /// <remarks>
        /// Make sure to use <see cref="MapToChannelData_BotToUser"/> with <see cref="KeyboardCardMapper"/>
        /// when you use this option
        /// </remarks>
        Keyboard,

        /// <summary>
        /// Show choices as Text.
        /// </summary>
        /// <remarks> The prompt decides if it should generate the text inline or perline based on number of choices.</remarks>
        AutoText,

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

    /// <summary>
    /// Options for <see cref="PromptDialog"/>.
    /// </summary>
    /// <typeparam name="T"> The type of the options.</typeparam>
    [Serializable]
    public class PromptOptions<T>
    {
        /// <summary>
        /// The prompt.
        /// </summary>
        public readonly string Prompt;

        /// <summary>
        /// What to display on retry.
        /// </summary>
        public readonly string Retry;

        /// <summary>
        /// The choices to be returned when selected.
        /// </summary>
        public readonly IReadOnlyList<T> Options;

        /// <summary>
        /// The description of each possible option.
        /// </summary>
        /// <remarks>
        /// If this is null, then the descriptions will be the options converted to strings.
        /// Otherwise this should have the same number of values as Options and it contains the string to describe the value being selected.
        /// </remarks>
        public readonly IReadOnlyList<string> Descriptions;

        /// <summary>
        /// What to display when user didn't say a valid response after <see cref="Attempts"/>.
        /// </summary>
        public readonly string TooManyAttempts;

        /// <summary>
        /// Maximum number of attempts.
        /// </summary>
        public int Attempts { set; get; }

        /// <summary>
        /// Styler of the prompt <see cref="Dialogs.PromptStyler"/>.
        /// </summary>
        public readonly PromptStyler PromptStyler;

        /// <summary>
        /// Default retry prompt that is used if <see cref="Retry"/> is null.
        /// </summary>
        public string DefaultRetry { get; set; }

        /// <summary>
        /// Default <see cref="TooManyAttempts"/> string that is used if <see cref="TooManyAttempts"/> is null.
        /// </summary>
        protected string DefaultTooManyAttempts
        {
            get { return Resources.TooManyAttempts; }
        }

        /// <summary>
        /// Constructs the prompt options.
        /// </summary>
        /// <param name="prompt"> The prompt.</param>
        /// <param name="retry"> What to display on retry.</param>
        /// <param name="tooManyAttempts"> What to display when user didn't say a valid response after <see cref="Attempts"/>.</param>
        /// <param name="options"> The prompt choice values.</param>
        /// <param name="attempts"> Maximum number of attempts.</param>
        /// <param name="promptStyler"> The prompt styler.</param>
        /// <param name="descriptions">Descriptions for each prompt.</param>
        public PromptOptions(string prompt, string retry = null, string tooManyAttempts = null, IReadOnlyList<T> options = null, int attempts = 3, PromptStyler promptStyler = null, IReadOnlyList<string> descriptions = null)
        {
            SetField.NotNull(out this.Prompt, nameof(this.Prompt), prompt);
            this.Retry = retry;
            this.TooManyAttempts = tooManyAttempts ?? this.DefaultTooManyAttempts;
            this.Attempts = attempts;
            this.Options = options;
            this.Descriptions = descriptions;
            this.DefaultRetry = prompt;
            if (promptStyler == null)
            {
                promptStyler = new PromptStyler();
            }
            this.PromptStyler = promptStyler;
        }
    }

    /// <summary>
    /// Styles a prompt
    /// </summary>
    [Serializable]
    public class PromptStyler
    {
        /// <summary>
        /// Style of the prompt <see cref="Dialogs.PromptStyle"/>.
        /// </summary>
        public readonly PromptStyle PromptStyle;

        public PromptStyler(PromptStyle promptStyle = PromptStyle.Auto)
        {
            this.PromptStyle = promptStyle;
        }

        /// <summary>
        /// <see cref="PromptStyler.Apply(ref IMessageActivity, string)"/>.
        /// </summary>
        /// <typeparam name="T"> The type of the options.</typeparam>
        /// <param name="message"> The message.</param>
        /// <param name="prompt"> The prompt.</param>
        /// <param name="options"> The options.</param>
        /// <param name="promptStyle"> The prompt style.</param>
        /// <param name="descriptions">Descriptions for each option.</param>
        public static void Apply<T>(ref IMessageActivity message, string prompt, IReadOnlyList<T> options, PromptStyle promptStyle, IReadOnlyList<string> descriptions = null)
        {
            var styler = new PromptStyler(promptStyle);
            styler.Apply(ref message, prompt, options, descriptions);
        }

        /// <summary>
        /// Style a prompt and populate the <see cref="IMessageActivity.Text"/>.
        /// </summary>
        /// <param name="message"> The message that will contain the prompt.</param>
        /// <param name="prompt"> The prompt.</param>
        public virtual void Apply(ref IMessageActivity message, string prompt)
        {
            SetField.CheckNull(nameof(prompt), prompt);
            message.Text = prompt;
        }

        /// <summary>
        /// Style a prompt and populate the message based on <see cref="PromptStyler.PromptStyle"/>.
        /// </summary>
        /// <typeparam name="T"> The type of the options.</typeparam>
        /// <param name="message"> The message that will contain the prompt.</param>
        /// <param name="prompt"> The prompt.</param>
        /// <param name="options"> The options.</param>
        /// <param name="descriptions">Descriptions to display for each option.</param>
        /// <remarks>
        /// <typeparamref name="T"/> should implement <see cref="object.ToString"/> unless descriptions are supplied.
        /// </remarks>
        public virtual void Apply<T>(ref IMessageActivity message, string prompt, IReadOnlyList<T> options, IReadOnlyList<string> descriptions = null)
        {
            SetField.CheckNull(nameof(prompt), prompt);
            SetField.CheckNull(nameof(options), options);
            if (descriptions == null)
            {
                descriptions = (from option in options select option.ToString()).ToList();
            }
            switch (PromptStyle)
            {
                case PromptStyle.Auto:
                case PromptStyle.Keyboard:
                    if (options != null && options.Any())
                    {
                        if (PromptStyle == PromptStyle.Keyboard)
                        {
                            message.AddKeyboardCard(prompt, options, descriptions);
                        }
                        else
                        {
                            message.AddHeroCard(prompt, options, descriptions);
                        }
                    }
                    else
                    {
                        message.Text = prompt;
                    }
                    break;
                case PromptStyle.AutoText:
                    Apply(ref message, prompt, options, options?.Count() > 4 ? PromptStyle.PerLine : PromptStyle.Inline, descriptions);
                    break;
                case PromptStyle.Inline:
                    //TODO: Refactor buildlist function to a more generic namespace when changing prompt to use recognizers.
                    message.Text = $"{prompt} {FormFlow.Advanced.Language.BuildList(descriptions, Resources.DefaultChoiceSeparator, Resources.DefaultChoiceLastSeparator)}";
                    break;
                case PromptStyle.PerLine:
                    message.Text = $"{prompt}{Environment.NewLine}{FormFlow.Advanced.Language.BuildList(descriptions.Select(description => $"* {description}"), Environment.NewLine, Environment.NewLine)}";
                    break;
                case PromptStyle.None:
                default:
                    message.Text = prompt;
                    break;
            }
        }
    }

    /// <summary>   Dialog factory for simple prompts. </summary>
    /// <remarks>The exception <see cref="TooManyAttemptsException"/> will be thrown if the number of allowed attempts is exceeded.</remarks>
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
        /// <param name="promptStyle"> Style of the prompt <see cref="PromptStyle" /> </param>
        public static void Confirm(IDialogContext context, ResumeAfter<bool> resume, string prompt, string retry = null, int attempts = 3, PromptStyle promptStyle = PromptStyle.Auto)
        {
            Confirm(context, resume, new PromptOptions<string>(prompt, retry, attempts: attempts, options: PromptConfirm.Options.ToList(), promptStyler: new PromptStyler(promptStyle: promptStyle)));
        }

        /// <summary>
        /// Ask a yes/no questions.
        /// </summary>
        /// <param name="context"> The dialog context.</param>
        /// <param name="resume"> Resume handler.</param>
        /// <param name="promptOptions"> The options for the prompt, <see cref="PromptOptions{T}"/>.</param>
        public static void Confirm(IDialogContext context, ResumeAfter<bool> resume, PromptOptions<string> promptOptions)
        {
            var child = new PromptConfirm(promptOptions);
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
        /// <param name="promptStyle"> Style of the prompt <see cref="PromptStyle" /> </param>
        /// <param name="descriptions">Descriptions to display for choices.</param>
        public static void Choice<T>(IDialogContext context, ResumeAfter<T> resume, IEnumerable<T> options, string prompt, string retry = null, int attempts = 3, PromptStyle promptStyle = PromptStyle.Auto, IEnumerable<string> descriptions = null)
        {
            Choice(context, resume, new PromptOptions<T>(prompt, retry, attempts: attempts, options: options.ToList(), promptStyler: new PromptStyler(promptStyle), descriptions: descriptions?.ToList()));
        }

        /// <summary>
        /// Prompt for one of a set of choices.
        /// </summary>
        /// <remarks><typeparamref name="T"/> should implement <see cref="object.ToString"/></remarks>
        /// <typeparam name="T"> The type of the options.</typeparam>
        /// <param name="context"> The dialog context.</param>
        /// <param name="resume"> Resume handler.</param>
        /// <param name="promptOptions"> The prompt options.</param>
        public static void Choice<T>(IDialogContext context, ResumeAfter<T> resume, PromptOptions<T> promptOptions)
        {
            var child = new PromptChoice<T>(promptOptions);
            context.Call<T>(child, resume);
        }

        /// <summary>
        /// Prompt for an attachment
        /// </summary>
        /// <param name="context"> The dialog context. </param>
        /// <param name="resume"> Resume handler. </param>
        /// <param name="prompt"> The prompt to show to the user. </param>
        /// <param name="contentTypes">The optional content types the attachment type should be part of</param>
        /// <param name="retry"> What to show on retry</param>
        /// <param name="attempts"> The number of times to retry</param>
        public static void Attachment(IDialogContext context, ResumeAfter<IEnumerable<Attachment>> resume, string prompt, IEnumerable<string> contentTypes = null, string retry = null, int attempts = 3)
        {
            var child = new PromptAttachment(prompt, retry, attempts, contentTypes);
            context.Call<IEnumerable<Attachment>>(child, resume);
        }

        /// <summary>   Prompt for a text string. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Text(IDialogContext, ResumeAfter{string}, string, string, int)"/>.</remarks>
        [Serializable]
        public sealed class PromptString : Prompt<string, string>
        {
            /// <summary>   Constructor for a prompt string dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            public PromptString(string prompt, string retry, int attempts)
                : this(new PromptOptions<string>(prompt, retry, attempts: attempts)) { }

            /// <summary>   Constructor for a prompt string dialog. </summary>
            /// <param name="promptOptions"> THe prompt options.</param>
            public PromptString(PromptOptions<string> promptOptions)
                : base(promptOptions)
            {
                this.promptOptions.DefaultRetry = this.DefaultRetry;
            }

            protected override bool TryParse(IMessageActivity message, out string result)
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

            public string DefaultRetry
            {
                get
                {
                    return Resources.PromptRetry + Environment.NewLine + this.promptOptions.Prompt;
                }
            }
        }

        /// <summary>   Prompt for a confirmation. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Confirm(IDialogContext, ResumeAfter{bool}, string, string, int, PromptStyle)"/>.</remarks>
        [Serializable]
        public sealed class PromptConfirm : Prompt<bool, string>
        {
            /// <summary>
            /// Index of yes descriptions.
            /// </summary>
            public const int Yes = 0;

            /// <summary>
            /// Index of no descriptions.
            /// </summary>
            public const int No = 1;

            /// <summary>
            /// The yes, no options for confirmation prompt
            /// </summary>
            public static string[] Options { set; get; } = { Resources.MatchYes.SplitList().First(), Resources.MatchNo.SplitList().First() };

            /// <summary>
            /// The patterns for matching yes/no responses in the confirmation prompt.
            /// </summary>
            public static string[][] Patterns { get; set; } = { Resources.MatchYes.SplitList(), Resources.MatchNo.SplitList() };

            /// <summary>   Constructor for a prompt confirmation dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            /// <param name="promptStyle"> Style of the prompt <see cref="PromptStyle" /> </param>
            public PromptConfirm(string prompt, string retry, int attempts, PromptStyle promptStyle = PromptStyle.Auto)
                : this(new PromptOptions<string>(prompt, retry, attempts: attempts, options: Options.ToList(), promptStyler: new PromptStyler(promptStyle)))
            {
            }

            /// <summary>
            /// Constructor for a prompt confirmation dialog.
            /// </summary>
            /// <param name="promptOptions"> THe prompt options.</param>
            public PromptConfirm(PromptOptions<string> promptOptions)
                : base(promptOptions)
            {
                this.promptOptions.DefaultRetry = this.DefaultRetry;
            }


            protected override bool TryParse(IMessageActivity message, out bool result)
            {
                var found = false;
                result = false;
                if (message.Text != null)
                {
                    var term = message.Text.Trim().ToLower();
                    if ((from r in Patterns[Yes] select r.ToLower()).Contains(term))
                    {
                        result = true;
                        found = true;
                    }
                    else if ((from r in Patterns[No] select r.ToLower()).Contains(term))
                    {
                        result = false;
                        found = true;
                    }
                }
                return found;
            }

            public string DefaultRetry
            {
                get
                {
                    return Resources.PromptRetry + Environment.NewLine + this.promptOptions.Prompt;
                }
            }
        }

        /// <summary>   Prompt for a confirmation. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Number(IDialogContext, ResumeAfter{long}, string, string, int)"/>.</remarks>
        [Serializable]
        public sealed class PromptInt64 : Prompt<long, long>
        {
            /// <summary>   Constructor for a prompt int64 dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            public PromptInt64(string prompt, string retry, int attempts)
                : this(new PromptOptions<long>(prompt, retry, attempts: attempts)) { }

            /// <summary>   Constructor for a prompt int64 dialog. </summary>
            /// <param name="promptOptions"> THe prompt options.</param>
            public PromptInt64(PromptOptions<long> promptOptions)
                : base(promptOptions) { }

            protected override bool TryParse(IMessageActivity message, out Int64 result)
            {
                return Int64.TryParse(message.Text, out result);
            }
        }

        /// <summary>   Prompt for a double. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Number(IDialogContext, ResumeAfter{double}, string, string, int)"/>.</remarks>
        [Serializable]
        public sealed class PromptDouble : Prompt<double, double>
        {
            /// <summary>   Constructor for a prompt double dialog. </summary>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            public PromptDouble(string prompt, string retry, int attempts)
                : this(new PromptOptions<double>(prompt, retry, attempts: attempts)) { }

            /// <summary>   Constructor for a prompt double dialog. </summary>
            /// <param name="promptOptions"> THe prompt options.</param>
            public PromptDouble(PromptOptions<double> promptOptions)
                : base(promptOptions) { }

            protected override bool TryParse(IMessageActivity message, out double result)
            {
                return double.TryParse(message.Text, out result);
            }
        }

        /// <summary>   Prompt for a choice from a set of choices. </summary>
        /// <remarks>   Normally used through <see cref="PromptDialog.Choice{T}(IDialogContext, ResumeAfter{T}, IEnumerable{T}, string, string, int, PromptStyle, IEnumerable{string})"/>.</remarks>
        [Serializable]
        public class PromptChoice<T> : Prompt<T, T>
        {
            /// <summary>   Constructor for a prompt choice dialog. </summary>
            /// <param name="options">Enumerable of the options to choose from.</param>
            /// <param name="prompt">   The prompt. </param>
            /// <param name="retry">    What to display on retry. </param>
            /// <param name="attempts"> Maximum number of attempts. </param>
            /// <param name="promptStyle"> Style of the prompt <see cref="PromptStyle" /> </param>
            /// <param name="descriptions">Descriptions to show for each option.</param>
            public PromptChoice(IEnumerable<T> options, string prompt, string retry, int attempts, PromptStyle promptStyle = PromptStyle.Auto, IEnumerable<string> descriptions = null)
                : this(new PromptOptions<T>(prompt, retry, options: options.ToList(), attempts: attempts, promptStyler: new PromptStyler(promptStyle), descriptions: descriptions?.ToList()))
            {
            }

            /// <summary>
            /// Constructs a choice dialog.
            /// </summary>
            /// <param name="promptOptions"> The prompt options</param>
            public PromptChoice(PromptOptions<T> promptOptions)
                : base(promptOptions)
            {
                SetField.CheckNull(nameof(promptOptions.Options), promptOptions.Options);
            }

            public virtual Tuple<bool, int> ScoreMatch(T option, string input)
            {
                var trimmed = input.Trim();
                var text = option.ToString();
                bool occurs = text.IndexOf(trimmed, StringComparison.CurrentCultureIgnoreCase) >= 0;
                bool equals = text == trimmed;
                return occurs
                    ? Tuple.Create(equals, trimmed.Length)
                    : null;
            }

            protected override bool TryParse(IMessageActivity message, out T result)
            {
                if (!string.IsNullOrWhiteSpace(message.Text))
                {
                    var scores = from option in this.promptOptions.Options
                                 let score = ScoreMatch(option, message.Text)
                                 select new { score, option };

                    var winner = scores.MaxBy(s => s.score);
                    if (winner.score != null)
                    {
                        result = winner.option;
                        return true;
                    }
                }

                result = default(T);
                return false;
            }
        }

        /// <summary> Prompt for an attachment</summary>
        /// <remarks> Normally used through <see cref="PromptDialog.Attachment(IDialogContext, ResumeAfter{IEnumerable{Connector.Attachment}}, string, IEnumerable{string}, string, int)"/>.</remarks>
        [Serializable]
        public sealed class PromptAttachment : Prompt<IEnumerable<Attachment>, Attachment>
        {
            public IEnumerable<string> ContentTypes
            {
                get;
                private set;
            }

            /// <summary>   Constructor for a prompt attachment dialog. </summary> 
            /// <param name="prompt">   The prompt. </param> 
            /// <param name="retry">    What to display on retry. </param> 
            /// <param name="attempts"> The optional content types the attachment type should be part of.</param>
            /// <param name="contentTypes"> The content types that is used to filter the attachments. Null implies any content type.</param>
            public PromptAttachment(string prompt, string retry, int attempts, IEnumerable<string> contentTypes = null)
                : base(new PromptOptions<Attachment>(prompt, retry, attempts: attempts))
            {
                this.ContentTypes = contentTypes ?? new List<string>();
            }

            protected override bool TryParse(IMessageActivity message, out IEnumerable<Attachment> result)
            {
                if (message.Attachments != null && message.Attachments.Any())
                {
                    // Retrieve attachments corresponding to content types if any
                    result = ContentTypes.Any() ? message.Attachments.Join(ContentTypes, a => a.ContentType, c => c, (a, c) => a)
                                                         : message.Attachments;
                    return result != null && result.Any();
                }
                else
                {
                    result = null;
                    return false;
                }
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
        /// <param name="text"> The text in the <see cref="HeroCard"/>.</param>
        /// <param name="options"> The options that cause generation of buttons.</param>
        /// <param name="descriptions">Descriptions for each option.</param>
        public static void AddHeroCard<T>(this IMessageActivity message, string text, IEnumerable<T> options, IEnumerable<string> descriptions = null)
        {
            message.AttachmentLayout = AttachmentLayoutTypes.List;
            message.Attachments = options.GenerateHeroCard(text, descriptions);
        }

        /// <summary>
        /// Generates buttons from options and add them to the message.
        /// </summary>
        /// <remarks>
        /// <typeparamref name="T"/> should implement ToString().
        /// </remarks>
        /// <typeparam name="T"> Type of the options.</typeparam>
        /// <param name="message"> The message that the buttons will be added to.</param>
        /// <param name="text"> The text in the <see cref="HeroCard"/>.</param>
        /// <param name="options"> The options that cause generation of buttons.</param>
        /// <param name="descriptions">Descriptions for each option.</param>
        public static void AddKeyboardCard<T>(this IMessageActivity message, string text, IEnumerable<T> options,
            IEnumerable<string> descriptions = null)
        {
            message.AttachmentLayout = AttachmentLayoutTypes.List;
            message.Attachments = options.GenerateKeyboardCard(text, descriptions);
        }

        internal static IList<Attachment> GenerateHeroCard<T>(this IEnumerable<T> options, string text, IEnumerable<string> descriptions = null)
        {
            var attachments = new List<Attachment>
            {
                new HeroCard(text: text, buttons: GenerateButtons(options, descriptions)).ToAttachment()
            };

            return attachments;
        }

        internal static IList<Attachment> GenerateKeyboardCard<T>(this IEnumerable<T> options, string text, IEnumerable<string> descriptions = null)
        {
            var attachments = new List<Attachment>
            {
                new KeyboardCard(text: text, buttons: GenerateButtons(options, descriptions)).ToAttachment()
            };

            return attachments;
        }

        internal static IList<CardAction> GenerateButtons<T>(IEnumerable<T> options,
            IEnumerable<string> descriptions = null)
        {
            var actions = new List<CardAction>();
            int i = 0;
            var adescriptions = descriptions?.ToArray();
            foreach (var option in options)
            {
                var title = (adescriptions == null ? option.ToString() : adescriptions[i]);
                actions.Add(new CardAction
                {
                    Title = title,
                    Type = ActionTypes.ImBack,
                    Value = option.ToString()
                });
                ++i;
            }
            return actions;
        }
    }
}

namespace Microsoft.Bot.Builder.Dialogs.Internals
{

    [Serializable]
    public abstract class Prompt<T, U> : IDialog<T>
    {
        protected readonly PromptOptions<U> promptOptions;

        public Prompt(PromptOptions<U> promptOptions)
        {
            SetField.NotNull(out this.promptOptions, nameof(promptOptions), promptOptions);

        }

        async Task IDialog<T>.StartAsync(IDialogContext context)
        {
            await context.PostAsync(this.MakePrompt(context, promptOptions.Prompt, promptOptions.Options, promptOptions.Descriptions));
            context.Wait(MessageReceivedAsync);
        }

        protected virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            T result;
            if (this.TryParse(await message, out result))
            {
                context.Done(result);
            }
            else
            {
                --promptOptions.Attempts;
                if (promptOptions.Attempts >= 0)
                {
                    await context.PostAsync(this.MakePrompt(context, promptOptions.Retry ?? promptOptions.DefaultRetry, promptOptions.Options, promptOptions.Descriptions));
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    //too many attempts, throw.
                    await context.PostAsync(this.MakePrompt(context, promptOptions.TooManyAttempts));
                    throw new TooManyAttemptsException(promptOptions.TooManyAttempts);
                }
            }
        }

        protected abstract bool TryParse(IMessageActivity message, out T result);

        protected virtual IMessageActivity MakePrompt(IDialogContext context, string prompt, IReadOnlyList<U> options = null, IReadOnlyList<string> descriptions = null)
        {
            var msg = context.MakeMessage();
            if (options != null && options.Count > 0)
            {
                promptOptions.PromptStyler.Apply(ref msg, prompt, options, descriptions);
            }
            else
            {
                promptOptions.PromptStyler.Apply(ref msg, prompt);
            }
            return msg;
        }
    }
}