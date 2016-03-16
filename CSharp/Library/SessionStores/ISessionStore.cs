using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface ISessionStore
    {
        Task LoadAsync(string Id, ISessionData sessionData);
        Task SaveAsync(string Id, ISessionData sessionData);
    }
}
