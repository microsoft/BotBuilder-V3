using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class DialogResult
    {
        public bool Completed { set; get; }
        public string ChildId { set; get; }
        public Exception Exception { set; get; }
    }

    public class DialogResumeHandler<T> where T : DialogResult
    {
        public Func<ISession, T, Task<DialogResponse>> HandlerAsync { set; get; }
    }
    
    public interface IDialog
    {
        Task<DialogResponse> BeginAsync(ISession session, object args = null);
        Task<DialogResponse> ReplyReceivedAsync(ISession session);
        Task<DialogResponse> DialogResumedAsync(ISession session, DialogResult result);
        string Id { get; }
    }
}
