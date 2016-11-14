using System;

using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Internals.Fibers
{

    public interface IStackStoreFactory<C>
    {
        IStore<IFiberLoop<C>> StoreFrom(string taskId, IBotDataBag dataBag);
    }

    public sealed class StoreFromStack<C> : IStackStoreFactory<C>
    {
        private readonly Func<string, IBotDataBag, IStore<IFiberLoop<C>>> make;
        
        public StoreFromStack(Func<string, IBotDataBag, IStore<IFiberLoop<C>>> make)
        {
            SetField.NotNull(out this.make, nameof(make), make);
        }
        IStore<IFiberLoop<C>> IStackStoreFactory<C>.StoreFrom(string stackId, IBotDataBag dataBag)
        {
            return this.make(stackId, dataBag);
        }
    }
}
