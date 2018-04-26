using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Graph;

namespace Microsoft.Bot.Sample.AadV2Bot
{
    public class SimpleGraphClient
    {
        private readonly string _token;

        public SimpleGraphClient(string token)
        {
            _token = token;
        }

        public async Task<bool> SendMail(string toAddress, string subject, string content)
        {
            try
            {
                var graphClient = GetAuthenticatedClient();

                List<Recipient> recipients = new List<Recipient>();
                recipients.Add(new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = toAddress
                    }
                });

                // Create the message.
                Message email = new Message
                {
                    Body = new ItemBody
                    {
                        Content = content,
                        ContentType = BodyType.Text,
                    },
                    Subject = subject,
                    ToRecipients = recipients
                };

                // Send the message.
                await graphClient.Me.SendMail(email, true).Request().PostAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<User> GetMe()
        {
            var graphClient = GetAuthenticatedClient();
            var me = await graphClient.Me.Request().GetAsync();
            return me;
        }

        public async Task<User> GetManager()
        {
            var graphClient = GetAuthenticatedClient();
            User manager = await graphClient.Me.Manager.Request().GetAsync() as User;
            return manager;
        }

        public async Task<List<Message>> GetRecentUnreadMail()
        {
            var graphClient = GetAuthenticatedClient();
            IMailFolderMessagesCollectionPage messages = await graphClient.Me.MailFolders.Inbox.Messages.Request().GetAsync();
            DateTime from = DateTime.Now.Subtract(TimeSpan.FromMinutes(30));
            List<Message> unreadMessages = new List<Message>();

            var done = false;
            while (messages?.Count > 0 && !done)
            {
                foreach (Message message in messages)
                {
                    if (message.ReceivedDateTime.HasValue && message.ReceivedDateTime.Value >= from)
                    {
                        if (message.IsRead.HasValue && !message.IsRead.Value)
                        {
                            unreadMessages.Add(message);
                        }
                    }
                    else
                    {
                        done = true;
                    }
                }

                messages = await messages.NextPageRequest.GetAsync();
            }

            return unreadMessages;
        }

        private GraphServiceClient GetAuthenticatedClient()
        {
            GraphServiceClient graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        string accessToken = _token;

                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");
                    }));
            return graphClient;
        }
    }
}