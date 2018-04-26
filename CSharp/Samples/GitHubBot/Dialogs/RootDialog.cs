using System;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using AdaptiveCards;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Bot.Sample.GitHubBot.Dialogs
{
    /// <summary>
    /// This Dialog enables the user to issue a set of commands against GitHub
    /// to do things like list repostories, list notifications, and identify the user
    /// This Dialog also makes use of the GetTokenDialog to help the user login
    /// </summary>
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        /// <summary>
        /// This is the name of the OAuth Connection Setting that is configured for this bot
        /// </summary>
        private static string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        /// Supports the commands repos, notes, who, and signout against GitHub
        /// </summary>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            
            var message = activity.Text.ToLowerInvariant();

            if (message.ToLowerInvariant().Equals("repos"))
            {
                // Get a token for GitHub and then list repositories
                context.Call(CreateGetTokenDialog(), ListRepositories);
            }
            else if (message.ToLowerInvariant().Equals("notes"))
            {
                // Get a token for GitHub and then list notifications
                context.Call(CreateGetTokenDialog(), ListNotifications);
            }
            else if (message.ToLowerInvariant().Equals("who"))
            {
                // Get a token for GitHub and then display the GitHub profile
                context.Call(CreateGetTokenDialog(), ListUser);
            }
            else if (message.ToLowerInvariant().Equals("signout"))
            {
                // Signout is an important intent
                await Signout(context);
            }
            else
            {
                await context.PostAsync("You can type 'repos', 'notes', or 'who' to list things from GitHub.");
                context.Wait(MessageReceivedAsync);
            }
        }

        /// <summary>
        /// Signs the user out of the connection and notifies the user
        /// </summary>
        public static async Task Signout(IDialogContext context)
        {
            await context.SignOutUserAsync(ConnectionName);
            await context.PostAsync($"You have been signed out from GitHub.");
        }
        
        /// <summary>
        /// Creates a GetTokenDialog
        /// You can pass in localized strings for the label, button text, and retry response
        /// </summary>
        private GetTokenDialog CreateGetTokenDialog()
        {
            return new GetTokenDialog(
                ConnectionName,
                $"Please sign in to {ConnectionName} to proceed.",
                "Sign In",
                2,
                "Hmm. Something went wrong, let's try again.");
        }

        #region GitHub Tasks

        private async Task ListRepositories(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
        {
            // The GetTokenDialog returns a GetTokenResponse object containing the token
            var token = await tokenResponse;

            var gitHub = new GitHubClient(token.Token);

            // Get repositories and create a card with them
            var card = new AdaptiveCard();
            var container = new Container();
            card.Body.Add(container);
            container.Items.Add(new TextBlock()
            {
                Text = "Here are all of your GitHub repositories:",
                Weight = TextWeight.Bolder,
                Size = TextSize.Medium
            });
            var repos = await gitHub.GetRepositories();
            int i = 1;
            foreach (var r in repos)
            {
                var cs = new ColumnSet()
                {
                    Columns = new List<Column>() {
                        new Column() {
                            Size = ColumnSize.Auto,
                            Items = new List<CardElement>() {
                                new TextBlock() { Text = string.Format("**{0}.**", i++) }
                            }
                        },
                        new Column() {
                            Size = ColumnSize.Auto,
                            Items = new List<CardElement>() {
                                new TextBlock() { Text = string.Format("**{0}**", r.full_name) }
                            },
                            SelectAction = new OpenUrlAction() {
                                Title = "Go",
                                Url = r.html_url
                            }
                        },
                        new Column() {
                            Size = ColumnSize.Stretch,
                            Items = new List<CardElement>() {
                                new TextBlock() { Text = string.Format("**({0})**", r.owner.login), IsSubtle = true }
                            }
                        }
                    }
                };
                if (i > 1)
                {
                    cs.Separation = SeparationStyle.None;
                }
                container.Items.Add(cs);
            }

            var repoMessage = context.MakeMessage();
            if (repoMessage.Attachments == null)
            {
                repoMessage.Attachments = new List<Attachment>();
            }
            repoMessage.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
                Name = "Repositories"
            });
            await context.PostAsync(repoMessage);
        }

        private async Task ListNotifications(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
        {
            // The GetTokenDialog returns a GetTokenResponse object containing the token
            var token = await tokenResponse;

            var gitHub = new GitHubClient(token.Token);

            // Get all recent notifications and create a card with them
            var card = new AdaptiveCard();
            var container = new Container();
            card.Body.Add(container);
            container.Items.Add(new TextBlock()
            {
                Text = "Here are all of your GitHub notifications:",
                Weight = TextWeight.Bolder,
                Size = TextSize.Medium
            });
            var notes = await gitHub.GetNotifications(false);
            int i = 0;
            foreach (var r in notes)
            {
                i++;
                var cs = new ColumnSet()
                {
                    Columns = new List<Column>() {
                        new Column() {
                            Size = "2",
                            Items = new List<CardElement>() {
                                new TextBlock() { Text = string.Format("**{0}**", r.subject.title.Trim()), Wrap = true }
                            },
                            SelectAction = new OpenUrlAction() {
                                Title = "Go",
                                Url = r.subject.url.Replace("https://api.github.com/repos", "https://github.com")
                            }
                        },
                        new Column() {
                            Size = "1",
                            Items = new List<CardElement>() {
                                new TextBlock() { Text = string.Format("**({0})**", r.repository.name), IsSubtle = true, Wrap = true }
                            }
                        }
                    }
                };
                if (i > 1)
                {
                    //cs.Separation = SeparationStyle.None;
                }
                container.Items.Add(cs);
            }

            var reply = context.MakeMessage();
            if (reply.Attachments == null)
            {
                reply.Attachments = new List<Attachment>();
            }
            reply.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
                Name = "Notifications"
            });
            await context.PostAsync(reply);
        }
        
        private async Task ListUser(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
        {
            // The GetTokenDialog returns a GetTokenResponse object containing the token
            var token = await tokenResponse;
            
            var gitHub = new GitHubClient(token.Token);

            // Get the user and create a card with the user's profile
            var user = await gitHub.GetUser();
            string email = "<none>";
            if (user.email == null)
            {
                var userEmails = await gitHub.GetUserEmails();
                if (userEmails != null)
                {
                    var primary = userEmails.FirstOrDefault(x => x.primary);
                    if (primary != null && !string.IsNullOrWhiteSpace(primary.email))
                    {
                        email = primary.email;
                    }
                    else if (userEmails.Count > 0 && !string.IsNullOrWhiteSpace(userEmails[0].email))
                    {
                        email = userEmails[0].email;
                    }
                }
            }
            else
            {
                email = user.email;
            }
            var card = new AdaptiveCard();
            var container = new ColumnSet();
            card.Body.Add(container);
            container.Columns.Add(new Column()
            {
                Size = ColumnSize.Auto,
                Items = new List<CardElement>()
                {
                    new Image()
                    {
                        Url = user.avatar_url,
                        Size = ImageSize.Small,
                        Style = ImageStyle.Person
                    }
                }
            });
            container.Columns.Add(new Column()
            {
                Size = ColumnSize.Auto,
                Items = new List<CardElement>()
                {
                    new TextBlock()
                    {
                        Text = "You are signed into GitHub as **" + user.login + "**",
                    },
                    new TextBlock()
                    {
                        Text = "Email: " + email,
                        Separation = SeparationStyle.None,
                        IsSubtle = true
                    }
                }
            });

            var repoMessage = context.MakeMessage();
            if (repoMessage.Attachments == null)
            {
                repoMessage.Attachments = new List<Attachment>();
            }
            repoMessage.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
                Name = "User"
            });
            await context.PostAsync(repoMessage);
        }

        #endregion
    }
}
