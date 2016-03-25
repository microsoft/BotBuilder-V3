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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
#pragma warning disable CS1998

    public class CommandDialog : IDialog<object>
    {
        public class Command
        {
            public Regex Expression { set; get; }
            public ResumeAfter<Connector.Message> CommandHandler { set; get; }
        }

        private Command defaultCommand;
        private readonly List<Command> commands = new List<Command>();

        async Task IDialog<object>.StartAsync(IDialogContext context, IAwaitable<object> argument)
        {
            context.Wait(MessageReceived);
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<Connector.Message> message)
        {
            var text = (await message).Text;
            Command matched = null;
            for (int idx = 0; idx < commands.Count; idx++)
            {
                var handler = commands[idx];
                if (handler.Expression.Match(text).Success)
                {
                    matched = handler;
                    break;
                }
            }

            if (matched == null && this.defaultCommand != null)
            {
                matched = this.defaultCommand;
            }

            if (matched != null)
            {
                await matched.CommandHandler(context, message);
            }
            else
            {
                string error = $"CommandDialog doesn't have a registered command handler for this message: {text}";
                throw new Exception(error);
            }

        }

        public CommandDialog On(Regex expression, ResumeAfter<Connector.Message> handler, bool defaultHandler = false)
        {
            var command = new Command()
            {
                Expression = expression,
                CommandHandler = handler,
            };

            if (defaultHandler)
            {
                this.defaultCommand = command;
            }
            else
            {
                commands.Add(command);
            }

            return this; 
        }

        public CommandDialog OnDefault(ResumeAfter<Connector.Message> handler)
        {
            return this.On(null, handler, defaultHandler: true);
        }
    }
}
