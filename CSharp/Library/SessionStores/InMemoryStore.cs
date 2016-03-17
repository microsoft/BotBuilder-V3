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

        async Task ISessionStore.LoadAsync(string sessionID, ISessionData data)
        {
            ISessionData mine = this.sessionByID.GetOrAdd(sessionID);
            mine.CopyTo(data);
        }

        async Task ISessionStore.SaveAsync(string sessionID, ISessionData data)
        {
            ISessionData mine = this.sessionByID.GetOrAdd(sessionID);
            data.CopyTo(mine);
        }
    }
}
