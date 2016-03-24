using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
#pragma warning disable CS1998

    [Serializable]
    public class CallDialog<T> : IDialog
    {
        public delegate Task Resume(CallDialog<T> dialog, IDialogContext context, IAwaitable<T> result);

        private readonly IDialog child;
        private readonly Resume resume;

        public CallDialog(IDialog child, Resume resume)
        {
            Field.SetNotNull(out this.child, nameof(child), child);
            Field.SetNotNull(out this.resume, nameof(resume), resume);
        }

        async Task IDialog.StartAsync(IDialogContext context, IAwaitable<object> arguments)
        {
            await CallChild(context, arguments);
        }

        private async Task ChildDone(IDialogContext context, IAwaitable<T> result)
        {
            await resume(this, context, result);
        }

        public async Task CallChild(IDialogContext context, IAwaitable<object> result)
        {
            context.Call<IDialog, T>(this.child, ChildDone);
        }
    }
}
