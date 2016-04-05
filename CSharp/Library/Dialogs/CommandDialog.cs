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

using Microsoft.Bot.Builder.Internals.Fibers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    #region Documentation
    /// <summary> Dialog that dispatches based on a regex matching input. </summary>
    #endregion

    [Serializable]
    public class CommandDialog<T> : IDialog<T>
    {
        [Serializable]
        public class Command
        {
            public Regex Expression { set; get; }
            public ResumeAfter<Connector.Message> CommandHandler { set; get; }
        }

        private Command defaultCommand;
        private readonly List<Command> commands = new List<Command>();
        private readonly Dictionary<Type, Delegate> resultHandlers = new Dictionary<Type, Delegate>();

        async Task IDialog<T>.StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceived);
        }

        #region Documentation
        /// <summary> Message handler of the command dialog.  </summary>
        /// <param name="context"> Dialog context. </param>
        /// <param name="message"> Message from the user. </param>
        #endregion
        public async Task MessageReceived(IDialogContext context, IAwaitable<Connector.Message> message)
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
        /// <summary>
        /// The result handler of the command dialog passed to the child dialogs. 
        /// </summary>
        /// <typeparam name="U"> The type of the result returned by the child dialog. </typeparam>
        /// <param name="context"> Dialog context. </param>
        /// <param name="result"> The result retured by the child dialog. </param>
        #endregion
        public async Task ResultHandler<U>(IDialogContext context, IAwaitable<U> result)
        {
            Delegate handler = null;
            if (resultHandlers.TryGetValue(typeof(U), out handler))
            {
                await ((ResumeAfter<U>)handler).Invoke(context, result);
                context.Wait(MessageReceived);
            }
            else
            {
                string error = $"CommandDialog doesn't have a registered result handler for this type: {typeof(U)}";
                throw new Exception(error);
            }
        }

        #region Documentation
        /// <summary> Define a handler that is fired on a regular expression match of a message. </summary>
        /// <param name="expression"> Regular expression to match. </param>
        /// <param name="handler"> Handler to call on match. </param>
        /// <returns> A CommandDialog. </returns>
        #endregion
        public CommandDialog<T> On(Regex expression, ResumeAfter<Connector.Message> handler)
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
        /// <summary> Define the default action if no match. </summary>
        /// <param name="handler"> Handler to call if no match. </param>
        /// <returns> A CommandDialog. </returns>
        #endregion
        public CommandDialog<T> OnDefault(ResumeAfter<Connector.Message> handler)
        {
            var command = new Command { CommandHandler = handler };
            this.defaultCommand = command;
            return this;
        }

        #region Documentation
        /// <summary> Define a result handler for specific result type returned by the child dialog. </summary>
        /// <typeparam name="U"> Type of the result returned by the child dialog started in command handler. </typeparam>
        /// <param name="handler"> Handler of the result. </param>
        /// <returns></returns>
        #endregion
        public CommandDialog<T> OnResult<U>(ResumeAfter<U> handler)
        {
            resultHandlers.Add(typeof(U), handler);
            return this;
        }
    }
}
