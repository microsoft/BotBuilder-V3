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
    public abstract class Dialog<TArgs, TResult> : IDialog
    {
        public string ID { private set; get; }

        public Dialog(string id)
        {
            this.ID = id;
        }

        public Dialog(Func<Dialog<TArgs,TResult>, string> nameProvider)
        {
            this.ID = nameProvider(this);
        }
        
        public virtual async Task<Connector.Message> BeginAsync(ISession session, Task<TArgs> taskArguments) 
        {
            return await this.ReplyReceivedAsync(session);
        }

        public abstract Task<Connector.Message> ReplyReceivedAsync(ISession session);

        public virtual async Task<Connector.Message> DialogResumedAsync(ISession session, Task<TResult> taskResult)
        {
            throw new NotImplementedException();
        }

        async Task<Connector.Message> IDialog.BeginAsync(ISession session, Task<object> arguments)
        {
            return await this.BeginAsync(session, Tasks.Cast<object, TArgs>(arguments));
        }

        async Task<Connector.Message> IDialog.DialogResumedAsync(ISession session, Task<object> result)
        {
            return await this.DialogResumedAsync(session, Tasks.Cast<object, TResult>(result));
        }
    }

   

  
}