using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
#pragma warning disable CS1998

    [Serializable]
    public class CallDialog<T, R> : IDialog<T>
    {
        public delegate Task Resume(CallDialog<T, R> dialog, IDialogContext context, IAwaitable<R> result);

        private readonly IDialog<T> child;
        private readonly Resume resume;
        private T argument;

        public CallDialog(IDialog<T> child, Resume resume)
        {
            Field.SetNotNull(out this.child, nameof(child), child);
            Field.SetNotNull(out this.resume, nameof(resume), resume);
        }

        async Task IDialog<T>.StartAsync(IDialogContext context, IAwaitable<T> argument)
        {
            this.argument = await argument;
            await CallChild(context, null);
        }

        private async Task ChildDone(IDialogContext context, IAwaitable<R> result)
        {
            await resume(this, context, result);
        }

        public async Task CallChild(IDialogContext context, IAwaitable<object> ignored)
        {
            context.Call<IDialog<T>, T, R>(this.child, this.argument, ChildDone);
        }
    }
}
