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

namespace Microsoft.Bot.Builder.Dialogs
{
    #region Documentation
    /// <summary>   Dialog that dispatches based on a regex matching input. </summary>
    #endregion
    public class CommandDialog : IDialog
    {
        public class Command
        {
            public Regex Expression { set; get; }
            public ResumeAfter<Connector.Message> CommandHandler { set; get; }
        }

        private Command defaultCommand;
        private readonly List<Command> commands = new List<Command>();

        async Task IDialog.StartAsync(IDialogContext context)
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
        #region Documentation
        /// <summary>   Define a handler that is fired on a regular expression match of a message. </summary>
        /// <param name="expression">       Regular expression to match. </param>
        /// <param name="handler">          Handler to call on match. </param>
        /// <returns>   A CommandDialog. </returns>
        #endregion
        public CommandDialog On(Regex expression, ResumeAfter<Connector.Message> handler)
        {
            var command = new Command
            {
                Expression = expression,
                CommandHandler = handler,
            };
            commands.Add(command);
            return this;
        }
        #region Documentation
        /// <summary>   Define the default action if no match. </summary>
        /// <param name="handler">  Handler to call if no match. </param>
        /// <returns>   A CommandDialog. </returns>
        #endregion
        public CommandDialog OnDefault(ResumeAfter<Connector.Message> handler)
        {
            var command = new Command { CommandHandler = handler };
            this.defaultCommand = command;
            return this;
        }
    }
}
