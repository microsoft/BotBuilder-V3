using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
#pragma warning disable CS1998

    public class CallDialog<T> : IDialogNew
    {
        public delegate Task Resume(CallDialog<T> dialog, IDialogContext context, IAwaitable<T> result);

        private readonly IDialogNew child;
        private readonly Resume resume;

        public CallDialog(IDialogNew child, Resume resume)
        {
            Field.SetNotNull(out this.child, nameof(child), child);
            Field.SetNotNull(out this.resume, nameof(resume), resume);
        }

        async Task IDialogNew.StartAsync(IDialogContext context, IAwaitable<object> arguments)
        {
            await CallChild(context, arguments);
        }

        private async Task ChildDone(IDialogContext context, IAwaitable<T> result)
        {
            await resume(this, context, result);
        }

        public async Task CallChild(IDialogContext context, IAwaitable<object> result)
        {
            context.Call<IDialogNew, T>(this.child, ChildDone);
        }
    }
}
