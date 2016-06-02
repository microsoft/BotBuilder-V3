using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace Microsoft.Bot.Sample.GraphBot.Models
{
    [Serializable]
    public sealed class RootDialog : IDialog<object>
    {
        private readonly IGraphServiceClient client;
        private readonly Uri loginUri;
        private readonly ResumptionCookie cookie;
        public RootDialog(IGraphServiceClient client, Uri loginUri, ResumptionCookie cookie)
        {
            SetField.NotNull(out this.client, nameof(client), client);
            SetField.NotNull(out this.loginUri, nameof(loginUri), loginUri);
            SetField.NotNull(out this.cookie, nameof(cookie), cookie);
        }

        Task IDialog<object>.StartAsync(IDialogContext context)
        {
            try
            {
                context.Wait(MessageReceived);
                return Task.FromResult(0);
            }
            catch (Exception error)
            {
                return Task.FromException<int>(error);
            }
        }

        public async Task MessageReceived(IDialogContext context, IAwaitable<Connector.Message> result)
        {
            await SayMyName(context);
            context.Wait(MessageReceived);
        }

        public async Task SayMyName(IDialogContext context)
        {
            try
            {
                // make a simple request to Microsoft Graph API using the Active Directory access token.
                var me = await client.Me.Request().GetAsync();
                await context.PostAsync($"Your name is {me.DisplayName}");
            }
            catch (AdalSilentTokenAcquisitionException)
            {
                await context.PostAsync(loginUri.AbsoluteUri + UrlToken.Encode(cookie));
            }
        }
    }
}
