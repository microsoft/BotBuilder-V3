using System;
using System.Collections.Concurrent;

namespace Microsoft.Bot.Builder.Internals.Fibers
{

    public interface IStackStoreFactory<C>
    {
        IStore<IFiberLoop<C>> StoreFrom(string taskId);
    }

    public sealed class StoreFromStack<C> : IStackStoreFactory<C>
    {
        private readonly Func<string, IStore<IFiberLoop<C>>> make;
        private readonly ConcurrentDictionary<string, IStore<IFiberLoop<C>>> stores
            = new ConcurrentDictionary<string, IStore<IFiberLoop<C>>>();

        public StoreFromStack(Func<string, IStore<IFiberLoop<C>>> make)
        {
            SetField.NotNull(out this.make, nameof(make), make);
        }
        IStore<IFiberLoop<C>> IStackStoreFactory<C>.StoreFrom(string stackId)
        {
            return this.stores.GetOrAdd(stackId, this.make(stackId));
        }
    }
}
