using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public delegate Task<Connector.Message> ResumeDelegate(ISession session, Task<object> taskResult);

    public class CallDialog : IDialog
    {
        public CallDialog(string id, IDialog child, ResumeDelegate resume)
        {
            _id = id;
            _child = child;
            _resume = resume;
        }

        public string ID
        {
            get
            {
                return _id;
            }
        }

        public Task<Connector.Message> BeginAsync(ISession session, Task<object> taskArguments)
        {
            return session.BeginDialogAsync(_child, taskArguments);
        }

        public async Task<Connector.Message> DialogResumedAsync(ISession session, Task<object> taskResult)
        {
            return await _resume(session, taskResult);
        }

        public Task<Connector.Message> ReplyReceivedAsync(ISession session)
        {
            throw new NotImplementedException();
        }

        protected readonly string _id;
        protected readonly IDialog _child;
        protected readonly ResumeDelegate _resume;
    }
}
