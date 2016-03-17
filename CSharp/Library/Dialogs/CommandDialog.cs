using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class CommandDialog<TArgs, TResult> : Dialog<TArgs, TResult>
    {
        public delegate Task<Connector.Message> CommandHandler(ISession session);

        public class CommandDialogEntry
        {
            public Regex Expression { set; get; }
            public CommandHandler CommandHandler { set; get; }
            public ResumeHandler<TResult> ResumeHandler { set; get; }
        }

        private const string ActiveHandlerField = "7CC22CC1BBBB_HANDLER";

        private CommandDialogEntry defaultCmdHandler;
        private List<CommandDialogEntry> CmdHandlers;

        public CommandDialog(string id)
            :base(id)
        {
            CmdHandlers = new List<CommandDialogEntry>();
        }
                
        public override async Task<Connector.Message> ReplyReceivedAsync(ISession session)
        {
            var txt = session.Message.Text;
            CommandDialogEntry matched = null; 
            for(int idx = 0; idx < CmdHandlers.Count; idx++)
            {
                var handler = CmdHandlers[idx];
                if(handler.Expression.Match(txt).Success)
                {
                    matched = handler;
                    session.Stack.SetLocal(ActiveHandlerField, idx);
                    break;
                }
            }

            if(matched == null && this.defaultCmdHandler !=null)
            {
                matched = this.defaultCmdHandler;
                session.Stack.SetLocal(ActiveHandlerField, -1);
            }

            if(matched != null)
            {
                return await matched.CommandHandler(session);
            }
            else
            {
                throw new Exception(string.Format("CommandDialog doesn't have a registered command handler for this message: {0}", session.Message.Text));
            }
        }

        public override async Task<Connector.Message> DialogResumedAsync(ISession session, Task<TResult> taskResult)
        {
            CommandDialogEntry curHandler = null;
            var idx = session.Stack.GetLocal(ActiveHandlerField);
            var handlerIdx = idx != null ? Convert.ToInt32(idx) : -1;
            if(handlerIdx >= 0 && handlerIdx < CmdHandlers.Count)
            {
                curHandler = CmdHandlers[handlerIdx];
            }
            else if(this.defaultCmdHandler != null)
            {
                curHandler = this.defaultCmdHandler;
            }
            if(curHandler != null && curHandler.ResumeHandler != null)
            {
                return await curHandler.ResumeHandler(session, taskResult);
            }
            else
            {
                return await base.DialogResumedAsync(session, taskResult);
            }
        }

        public CommandDialog<TArgs, TResult> On(Regex expression, CommandHandler commandHandler, ResumeHandler<TResult> resumeHandler = null, bool defaultHandler = false)
        {
            var handler = new CommandDialogEntry()
            {
                Expression = expression,
                CommandHandler = commandHandler,
                ResumeHandler = resumeHandler
            };

            if (defaultHandler)
            {
                this.defaultCmdHandler = handler;
            }
            else
            {
                CmdHandlers.Add(handler);
            }

            return this; 
        }

        public CommandDialog<TArgs, TResult> OnDefault(CommandHandler commandHandler, ResumeHandler<TResult> resumeHandler = null)
        {
            this.On(null, commandHandler, resumeHandler, true);
            return this; 
        }

    }
}
