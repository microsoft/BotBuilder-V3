using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class DialogResponse 
    {
        /// <summary>
        /// The Id of the dialog that created the response
        /// </summary>
        public string DialogId { get; set; }

        /// <summary>
        /// Reply generate by current dialog in response to incoming message
        /// </summary>
        public Connector.Message Reply { get; set; }

        /// <summary>
        /// It will be set if the incoming message causes an error in the dialog system
        /// </summary>
        public Exception Error { get; set; }
    }

    public interface ISession
    {
        Message Message { get; }
        ISessionData SessionData { get; }
        IDialogFrame Stack { get; }
        Task<DialogResponse> DispatchAsync();
        Task<DialogResponse> BeginDialogAsync(IDialog dialog, object args = null);
        Task<DialogResponse> EndDialogAsync(IDialog dialog, DialogResult result);

        Task<DialogResponse> CreateDialogErrorResponse(HttpStatusCode status = HttpStatusCode.InternalServerError, string errorMessage = null);
        Task<DialogResponse> CreateDialogResponse(string message);
        Task<DialogResponse> CreateDialogResponse(Message message);
    }
}
