using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public delegate Task<DialogResponse> ResumeDelegate(ISession session, DialogResult result);
    public class CallDialog : IDialog
    {
        public CallDialog(string id, IDialog child, ResumeDelegate resume)
        {
            _id = id;
            _child = child;
            _resume = resume;
        }

        public string Id
        {
            get
            {
                return _id;
            }
        }

        public Task<DialogResponse> BeginAsync(ISession session, object args = null)
        {
            return session.BeginDialogAsync(_child, args);
        }

        public Task<DialogResponse> DialogResumedAsync(ISession session, DialogResult result)
        {
            if (result.Completed)
            {
                return _resume(session, result);
            }
            else if (result.Exception != null)
            {
                return session.CreateDialogErrorResponse(errorMessage: result.Exception.Message);
            }
            return session.EndDialogAsync(this, result);
        }

        public Task<DialogResponse> ReplyReceivedAsync(ISession session)
        {
            throw new NotImplementedException();
        }

        protected readonly string _id;
        protected readonly IDialog _child;
        protected readonly ResumeDelegate _resume;
    }
}
