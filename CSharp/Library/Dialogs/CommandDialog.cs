using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
#pragma warning disable CS1998

    public class CommandDialog : IDialogNew
    {
        public class Command
        {
            public Regex Expression { set; get; }
            public ResumeAfter<Connector.Message> CommandHandler { set; get; }
        }

        private Command defaultCommand;
        private readonly List<Command> commands = new List<Command>();

        async Task IDialogNew.StartAsync(IDialogContext context, IAwaitable<object> arguments)
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
