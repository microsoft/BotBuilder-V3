using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public abstract class Dialog<TArgs, TResult> : IDialog where TResult : DialogResult
    {
        public string Id { private set; get; }

        public Dialog(string Id)
        {
            this.Id = Id;
        }

        public Dialog(Func<Dialog<TArgs,TResult>, string> nameProvider)
        {
            this.Id = nameProvider(this);
        }
        
        public virtual async Task<DialogResponse> BeginAsync(ISession session, TArgs args = default(TArgs)) 
        {
            return await this.ReplyReceivedAsync(session);
        }

        public abstract Task<DialogResponse> ReplyReceivedAsync(ISession session);

        public virtual async Task<DialogResponse> DialogResumedAsync(ISession session, TResult result = default(TResult))
        {
            return await session.CreateDialogResponse(message: (Message)null);
        }
        async Task<DialogResponse> IDialog.BeginAsync(ISession session, object args)
        {
            return await this.BeginAsync(session, (TArgs)args);
        }

        async Task<DialogResponse> IDialog.DialogResumedAsync(ISession session, DialogResult result)
        {
            return await this.DialogResumedAsync(session, (TResult)result);
        }
    }

   

  
}