using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public sealed class InMemoryStore : ISessionStore
    {
        private readonly Dictionary<string, InMemorySessionData> sessionByID = new Dictionary<string, InMemorySessionData>(); 

        async Task ISessionStore.LoadAsync(string Id, ISessionData data)
        {
            ISessionData mine = this.sessionByID.GetOrAdd(Id);
            mine.CopyTo(data);
        }

        async Task ISessionStore.SaveAsync(string Id, ISessionData data)
        {
            ISessionData mine = this.sessionByID.GetOrAdd(Id);
            data.CopyTo(mine);
        }
    }
}
