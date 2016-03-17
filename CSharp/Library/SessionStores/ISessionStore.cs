using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Persistent storage for session data.
    /// </summary>
    public interface ISessionStore
    {
        /// <summary>
        /// Load the session data from storage.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="sessionData">The target session data for loading.</param>
        /// <returns>A task that represents the completion of the load.</returns>
        Task LoadAsync(string sessionID, ISessionData sessionData);

        /// <summary>
        /// Save the session data to storage.
        /// </summary>
        /// <param name="sessionID"><The session ID./param>
        /// <param name="sessionData">The target session data for saving.</param>
        /// <returns>A task that represents the completion of the save.</returns>
        Task SaveAsync(string sessionID, ISessionData sessionData);
    }
}
