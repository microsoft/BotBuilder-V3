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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public class Prompts
    {
        public static void Text(IDialogContext context, ResumeAfter<string> resume, string prompt, string retry = null, int attempts = 3)
        {
            var child = new PromptText(prompt, retry, attempts);
            context.Call<PromptText, object, string>(child, resume);
        }

        public static void Confirm(IDialogContext context, ResumeAfter<bool> resume, string prompt, string retry = null, int attempts = 3)
        {
            var child = new PromptConfirm(prompt, retry, attempts);
            context.Call<PromptConfirm, object, bool>(child, resume);
        }

        public static void Number(IDialogContext context, ResumeAfter<int> resume, string prompt, string retry = null, int attempts = 3)
        {
            var child = new PromptInt32(prompt, retry, attempts);
            context.Call<PromptInt32, object, int>(child, resume);
        }

        public static void Number(IDialogContext context, ResumeAfter<float> resume, string prompt, string retry = null, int attempts = 3)
        {
            var child = new PromptFloat(prompt, retry, attempts);
            context.Call<PromptFloat, object, float>(child, resume);
        }

        public static void Choice<T>(IDialogContext context, ResumeAfter<T> resume, IEnumerable<T> options, string prompt, string retry = null, int attempts = 3)
        {
            var child = new PromptChoice<T>(options, prompt, retry, attempts);
            context.Call<PromptChoice<T>, object, T>(child, resume);
        }

        [Serializable]
        private abstract class Prompt<T> : IDialog<object>
        {
            protected readonly string prompt;
            protected readonly string retry;
            protected int attempts;

            public Prompt(string prompt, string retry, int attempts)
            {
                Field.SetNotNull(out this.prompt, nameof(prompt), prompt);
                Field.SetNotNull(out this.retry, nameof(retry), retry ?? prompt);
                this.attempts = attempts;
            }

            async Task IDialog<object>.StartAsync(IDialogContext context, IAwaitable<object> argument)
            {
                await context.PostAsync(this.prompt);
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
                        await context.PostAsync(retry);
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

            protected virtual string DefaultRetry
            {
                get
                {
                    return this.prompt;
                }
            }
        }

        [Serializable]
        private sealed class PromptText : Prompt<string>
        {
            public PromptText(string prompt, string retry, int attempts)
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
                    return "I didn't understand. Say something in reply.\n" + this.prompt;
                }
            }
        }

        [Serializable]
        private sealed class PromptConfirm : Prompt<bool>
        {
            public PromptConfirm(string prompt, string retry, int attempts)
                : base(prompt, retry, attempts)
            {
            }

            protected override bool TryParse(Message message, out bool result)
            {
                switch (message.Text)
                {
                    case "y":
                    case "yes":
                    case "ok":
                        result = true;
                        return true;
                    case "n":
                    case "no":
                        result = false;
                        return true;
                    default:
                        result = false;
                        return false;
                }
            }

            protected override string DefaultRetry
            {
                get
                {
                    return "I didn't understand. Valid replies are yes or no.\n" + this.prompt;
                }
            }
        }

        [Serializable]
        private sealed class PromptInt32 : Prompt<Int32>
        {
            public PromptInt32(string prompt, string retry, int attempts)
                : base(prompt, retry, attempts)
            {
            }

            protected override bool TryParse(Message message, out Int32 result)
            {
                return Int32.TryParse(message.Text, out result);
            }
        }

        [Serializable]
        private sealed class PromptFloat : Prompt<float>
        {
            public PromptFloat(string prompt, string retry, int attempts)
                : base(prompt, retry, attempts)
            {
            }

            protected override bool TryParse(Message message, out float result)
            {
                return float.TryParse(message.Text, out result);
            }
        }

        [Serializable]
        private class PromptChoice<T> : Prompt<T>
        {
            private readonly IEnumerable<T> options;

            public PromptChoice(IEnumerable<T> options, string prompt, string retry, int attempts)
                : base(prompt, retry, attempts)
            {
                Field.SetNotNull(out this.options, nameof(options), options);
            }

            public virtual bool IsMatch(T option, string text)
            {
                return option.ToString().IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0;
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
}