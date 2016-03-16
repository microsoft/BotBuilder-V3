using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class CommandDialog<TArgs, TResult> : Dialog<TArgs, TResult> where TResult : DialogResult
    {
        public class CommandHandler
        {
            public Func<ISession, Task<DialogResponse>> HandlerAsync { set; get; }
        }

        public class CommandDialogEntry
        {
            public Regex Expression { set; get; }
            public CommandHandler CommandHandler { set; get; }
            public DialogResumeHandler<DialogResult> ResumeHandler { set; get; }
        }

        private const string ActiveHandlerField = "7CC22CC1BBBB_HANDLER";

        private CommandDialogEntry defaultCmdHandler;
        private List<CommandDialogEntry> CmdHandlers;

        public CommandDialog(string id)
            :base(id)
        {
            CmdHandlers = new List<CommandDialogEntry>();
        }
                
        public override async Task<DialogResponse> ReplyReceivedAsync(ISession session)
        {
            var txt = session.Message.Text;
            CommandDialogEntry matched = null; 
            for(int idx = 0; idx < CmdHandlers.Count; idx++)
            {
                var handler = CmdHandlers[idx];
                if(handler.Expression.Match(txt).Success)
                {
                    matched = handler;
                    session.Stack.SetDialogState(ActiveHandlerField, idx);
                    break;
                }
            }

            if(matched == null && this.defaultCmdHandler !=null)
            {
                matched = this.defaultCmdHandler;
                session.Stack.SetDialogState(ActiveHandlerField, -1);
            }

            if(matched != null)
            {
                return await matched.CommandHandler.HandlerAsync(session);
            }
            else
            {
                return await session.CreateDialogErrorResponse(errorMessage:
                    string.Format("CommandDialog doesn't have a registered command handler for this message: {0}", session.Message.Text));
            }
        }

        public override async Task<DialogResponse> DialogResumedAsync(ISession session, TResult result = null)
        {
            CommandDialogEntry curHandler = null;
            var idx = session.Stack.GetDialogState(ActiveHandlerField);
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
                return await curHandler.ResumeHandler.HandlerAsync(session, result);
            }
            else
            {
                return await base.DialogResumedAsync(session, result);
            }
        }

        public CommandDialog<TArgs,TResult> On(Regex expression, Func<ISession, Task<DialogResponse>> cmdHandler, Func<ISession, DialogResult, Task<DialogResponse>> resumeHanlder = null, bool defaultHandler = false)
        {
            var handler = new CommandDialogEntry()
            {
                Expression = expression,
                CommandHandler = new CommandHandler() { HandlerAsync = cmdHandler },
                ResumeHandler = new DialogResumeHandler<DialogResult>() { HandlerAsync = resumeHanlder }
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

        public CommandDialog<TArgs, TResult> OnDefault(Func<ISession, Task<DialogResponse>> cmdHandler, Func<ISession, DialogResult, Task<DialogResponse>> resumeHanlder = null)
        {
            this.On(null, cmdHandler, resumeHanlder, true);
            return this; 
        }

    }
}
