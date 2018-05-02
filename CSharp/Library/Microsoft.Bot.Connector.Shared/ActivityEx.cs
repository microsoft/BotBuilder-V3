// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// An Activity is the basic communication type for the Bot Framework 3.0 protocol
    /// </summary>
    /// <remarks>
    /// The Activity class contains all properties that individual, more specific activities
    /// could contain. It is a superset type.
    /// </remarks>
    public partial class Activity :
        IActivity,
        IConversationUpdateActivity,
        IContactRelationUpdateActivity,
        IInstallationUpdateActivity,
        IMessageActivity,
        IMessageUpdateActivity,
        IMessageDeleteActivity,
        IMessageReactionActivity,
        ISuggestionActivity,
        ITraceActivity,
        ITypingActivity,
        IEndOfConversationActivity,
        IEventActivity,
        IInvokeActivity
    {
        /// <summary>
        /// Content-type for an Activity
        /// </summary>
        public const string ContentType = "application/vnd.microsoft.activity";

        partial void CustomInit()
        {
            Attachments = Attachments ?? new List<Attachment>();
            Entities = Entities ?? new List<Entity>();
        }

        /// <summary>
        /// Take a message and create a reply message for it with the routing information 
        /// set up to correctly route a reply to the source message
        /// </summary>
        /// <param name="text">text you want to reply with</param>
        /// <param name="locale">language of your reply</param>
        /// <returns>message set up to route back to the sender</returns>
        public Activity CreateReply(string text = null, string locale = null)
        {
            Activity reply = new Activity();
            reply.Type = ActivityTypes.Message;
            reply.Timestamp = DateTime.UtcNow;
            reply.From = new ChannelAccount(id: this.Recipient.Id, name: this.Recipient.Name);
            reply.Recipient = new ChannelAccount(id: this.From.Id, name: this.From.Name);
            reply.ReplyToId = this.Id;
            reply.ServiceUrl = this.ServiceUrl;
            reply.ChannelId = this.ChannelId;
            reply.Conversation = new ConversationAccount(isGroup: this.Conversation.IsGroup, id: this.Conversation.Id, name: this.Conversation.Name);
            reply.Text = text ?? String.Empty;
            reply.Locale = locale ?? this.Locale;
            return reply;
        }

        /// <summary>
        /// Extension data for overflow of properties
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; set; } = new JObject();

        /// <summary>
        /// Create an instance of the Activity class with IMessageActivity masking
        /// </summary>
        public static IMessageActivity CreateMessageActivity() { return new Activity(ActivityTypes.Message); }

        /// <summary>
        /// Create an instance of the Activity class with IContactRelationUpdateActivity masking
        /// </summary>
        public static IContactRelationUpdateActivity CreateContactRelationUpdateActivity() { return new Activity(ActivityTypes.ContactRelationUpdate); }

        /// <summary>
        /// Create an instance of the Activity class with IConversationUpdateActivity masking
        /// </summary>
        public static IConversationUpdateActivity CreateConversationUpdateActivity()
        {
            return new Activity(ActivityTypes.ConversationUpdate)
            {
                MembersAdded = new List<ChannelAccount>(),
                MembersRemoved = new List<ChannelAccount>()
            };
        }

        /// <summary>
        /// Create an instance of the Activity class with ITypingActivity masking
        /// </summary>
        public static ITypingActivity CreateTypingActivity() { return new Activity(ActivityTypes.Typing); }

        /// <summary>
        /// Create an instance of the Activity class with IActivity masking
        /// </summary>
        public static IActivity CreatePingActivity() { return new Activity(ActivityTypes.Ping); }

        /// <summary>
        /// Create an instance of the Activity class with IEndOfConversationActivity masking
        /// </summary>
        public static IEndOfConversationActivity CreateEndOfConversationActivity() { return new Activity(ActivityTypes.EndOfConversation); }

        /// <summary>
        /// Create an instance of the Activity class with an IEventActivity masking
        /// </summary>
        public static IEventActivity CreateEventActivity() { return new Activity(ActivityTypes.Event); }

        /// <summary>
        /// Create an instance of the Activity class with IInvokeActivity masking
        /// </summary>
        public static IInvokeActivity CreateInvokeActivity() { return new Activity(ActivityTypes.Invoke); }

        /// <summary>
        /// Create an instance of the TraceActivity 
        /// </summary>
        /// <param name="activity">Activity to reply to</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="value">value of the operation</param>
        /// <param name="valueType">valueType if helpful to identify the value schema (default is value.GetType().Name)</param>
        /// <param name="label">descritive label of context. (Default is calling function name)</param>
        public static ITraceActivity CreateTraceActivityReply(
            Activity activity, 
            string name, 
            string valueType = null, 
            object value = null, 
            [CallerMemberName] string label = null)
        {
            ITraceActivity traceActivity = activity == null ? new Activity() : activity.CreateReply();
            traceActivity.Name = name;
            traceActivity.Label = label;
            traceActivity.ValueType = valueType ?? value?.GetType().Name;
            traceActivity.Value = value;
            traceActivity.Type = "trace";
            return traceActivity;
        }

        /// <summary>
        /// True if the Activity is of the specified activity type
        /// </summary>
        protected bool IsActivity(string activity) { return string.Compare(this.Type?.Split('/').First(), activity, true) == 0; }

        /// <summary>
        /// Return an IMessageActivity mask if this is a message activity
        /// </summary>
        public IMessageActivity AsMessageActivity() { return IsActivity(ActivityTypes.Message) ? this : null; }

        /// <summary>
        /// Return an IContactRelationUpdateActivity mask if this is a contact relation update activity
        /// </summary>
        public IContactRelationUpdateActivity AsContactRelationUpdateActivity() { return IsActivity(ActivityTypes.ContactRelationUpdate) ? this : null; }

        /// <summary>
        /// Return an IInstallationUpdateActivity mask if this is a installation update activity
        /// </summary>
        public IInstallationUpdateActivity AsInstallationUpdateActivity() { return IsActivity(ActivityTypes.InstallationUpdate) ? this : null; }

        /// <summary>
        /// Return an IConversationUpdateActivity mask if this is a conversation update activity
        /// </summary>
        public IConversationUpdateActivity AsConversationUpdateActivity() { return IsActivity(ActivityTypes.ConversationUpdate) ? this : null; }

        /// <summary>
        /// Returns ITraceActivity if this is a trace activity, null otherwise
        /// </summary>
        public ITraceActivity AsTraceActivity() { return IsActivity(ActivityTypes.Trace) ? this : null; }

        /// <summary>
        /// Return an ITypingActivity mask if this is a typing activity
        /// </summary>
        public ITypingActivity AsTypingActivity() { return IsActivity(ActivityTypes.Typing) ? this : null; }

        /// <summary>
        /// Return an IEndOfConversationActivity mask if this is an end of conversation activity
        /// </summary>
        public IEndOfConversationActivity AsEndOfConversationActivity() { return IsActivity(ActivityTypes.EndOfConversation) ? this : null; }

        /// <summary>
        /// Return an IEventActivity mask if this is an event activity
        /// </summary>
        public IEventActivity AsEventActivity() { return IsActivity(ActivityTypes.Event) ? this : null; }

        /// <summary>
        /// Return an IInvokeActivity mask if this is an invoke activity
        /// </summary>
        public IInvokeActivity AsInvokeActivity() { return IsActivity(ActivityTypes.Invoke) ? this : null; }

        /// <summary>
        /// Return an IMessageUpdateAcitvity if this is a MessageUpdate activity
        /// </summary>
        /// <returns></returns>
        public IMessageUpdateActivity AsMessageUpdateActivity() { return IsActivity(ActivityTypes.MessageUpdate) ? this : null; }

        /// <summary>
        /// Return an IMessageDeleteActivity if this is a MessageDelete activity
        /// </summary>
        /// <returns></returns>
        public IMessageDeleteActivity AsMessageDeleteActivity() { return IsActivity(ActivityTypes.MessageDelete) ? this : null; }

        /// <summary>
        /// Return an IMessageReactionActivity if this is a MessageReaction activity
        /// </summary>
        /// <returns></returns>
        public IMessageReactionActivity AsMessageReactionActivity() { return IsActivity(ActivityTypes.MessageReaction) ? this : null; }

        /// <summary>
        /// Return an ISuggestionActivity if this is a Suggestion activity
        /// </summary>
        /// <returns></returns>
        public ISuggestionActivity AsSuggestionActivity() { return IsActivity(ActivityTypes.Suggestion) ? this : null; }

        /// <summary>
        /// Normalize activity type 
        /// </summary>
        /// <param name="type"> The type.</param>
        /// <returns> The activity type.</returns>
        public static string GetActivityType(string type)
        {
            if (String.Equals(type, ActivityTypes.Message, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Message;

            if (String.Equals(type, ActivityTypes.ContactRelationUpdate, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.ContactRelationUpdate;

            if (String.Equals(type, ActivityTypes.ConversationUpdate, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.ConversationUpdate;

            if (String.Equals(type, ActivityTypes.DeleteUserData, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.DeleteUserData;

            if (String.Equals(type, ActivityTypes.Typing, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Typing;

            if (String.Equals(type, ActivityTypes.Ping, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Ping;

            if (String.Equals(type, ActivityTypes.EndOfConversation, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.EndOfConversation;

            if (String.Equals(type, ActivityTypes.Event, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Event;

            if (String.Equals(type, ActivityTypes.InstallationUpdate, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.InstallationUpdate;

            if (String.Equals(type, ActivityTypes.Invoke, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Invoke;

            if (String.Equals(type, ActivityTypes.MessageDelete, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.MessageDelete;

            if (String.Equals(type, ActivityTypes.MessageReaction, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.MessageReaction;

            if (String.Equals(type, ActivityTypes.MessageUpdate, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.MessageUpdate;

            if (String.Equals(type, ActivityTypes.Suggestion, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Suggestion;

            return $"{Char.ToLower(type[0])}{type.Substring(1)}";
        }

        /// <summary>
        /// Checks if this (message) activity has content.
        /// </summary>
        /// <returns>Returns true, if this message has any content to send. False otherwise.</returns>
        public bool HasContent()
        {
            if (!String.IsNullOrWhiteSpace(this.Text))
                return true;

            if (!String.IsNullOrWhiteSpace(this.Summary))
                return true;

            if (this.Attachments != null && this.Attachments.Any())
                return true;

            if (this.ChannelData != null)
                return true;

            return false;
        }

        /// <summary>
        /// Resolves the mentions from the entities of this (message) activity.
        /// </summary>
        /// <returns>The array of mentions or an empty array, if none found.</returns>
        public Mention[] GetMentions()
        {
            return this.Entities?.Where(entity => String.Compare(entity.Type, "mention", ignoreCase: true) == 0)
                .Select(e => e.Properties.ToObject<Mention>()).ToArray() ?? new Mention[0];
        }

        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <param name="activity"></param>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <returns>typed object or default(TypeT)</returns>
        public TypeT GetChannelData<TypeT>()
        {
            if (this.ChannelData == null)
                return default(TypeT);
            if (this.ChannelData.GetType() == typeof(TypeT))
                return (TypeT)this.ChannelData;
            return ((JObject)this.ChannelData).ToObject<TypeT>();
        }

        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <param name="activity"></param>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <param name="instance">The resulting instance, if possible</param>
        /// <returns>
        /// <c>true</c> if value of <seealso cref="IActivity.ChannelData"/> was coerceable to <typeparamref name="TypeT"/>, <c>false</c> otherwise.
        /// </returns>
        public bool TryGetChannelData<TypeT>(out TypeT instance)
        {
            instance = default(TypeT);

            try
            {
                if (this.ChannelData == null)
                    return false;

                instance = this.GetChannelData<TypeT>();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }

    public static class ActivityExtensions
    {
        /// <summary>
        /// Get StateClient appropriate for this activity
        /// </summary>
        /// <param name="credentials">credentials for bot to access state api</param>
        /// <param name="serviceUrl">alternate serviceurl to use for state service</param>
        /// <param name="handlers"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        [System.Obsolete("Deprecated: This method will only get the default state client, if you have implemented a custom state client it will not retrieve it")]
        public static StateClient GetStateClient(this IActivity activity, MicrosoftAppCredentials credentials, string serviceUrl = null, params DelegatingHandler[] handlers)
        {
            bool useServiceUrl = (activity.ChannelId == "emulator");
            if (useServiceUrl)
                return new StateClient(new Uri(activity.ServiceUrl), credentials: credentials, handlers: handlers);

            if (serviceUrl != null)
                return new StateClient(new Uri(serviceUrl), credentials: credentials, handlers: handlers);

            return new StateClient(credentials, true, handlers);
        }

        /// <summary>
        /// Get StateClient appropriate for this activity
        /// </summary>
        /// <param name="microsoftAppId"></param>
        /// <param name="microsoftAppPassword"></param>
        /// <param name="serviceUrl">alternate serviceurl to use for state service</param>
        /// <param name="handlers"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static StateClient GetStateClient(this IActivity activity, string microsoftAppId = null, string microsoftAppPassword = null, string serviceUrl = null, params DelegatingHandler[] handlers) => GetStateClient(activity, new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword), serviceUrl, handlers);

        /// <summary>
        /// Return the "major" portion of the activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>normalized major portion of the activity, aka message/... will return "message"</returns>
        public static string GetActivityType(this IActivity activity)
        {
            var type = activity.Type.Split('/').First();
            return Activity.GetActivityType(type);
        }

        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <param name="activity"></param>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <returns>typed object or default(TypeT)</returns>
        public static TypeT GetChannelData<TypeT>(this IActivity activity)
        {
            if (activity.ChannelData == null)
                return default(TypeT);
            return ((JObject)activity.ChannelData).ToObject<TypeT>();
        }


        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <param name="activity"></param>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <param name="instance">The resulting instance, if possible</param>
        /// <returns>
        /// <c>true</c> if value of <seealso cref="IActivity.ChannelData"/> was coerceable to <typeparamref name="TypeT"/>, <c>false</c> otherwise.
        /// </returns>
        public static bool TryGetChannelData<TypeT>(this IActivity activity,
            out TypeT instance)
        {
            instance = default(TypeT);

            try
            {
                if (activity.ChannelData == null)
                {
                    return false;
                }

                instance = GetChannelData<TypeT>(activity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Is there a mention of Id in the Text Property 
        /// </summary>
        /// <param name="id">ChannelAccount.Id</param>
        /// <param name="activity"></param>
        /// <returns>true if this id is mentioned in the text</returns>
        public static bool MentionsId(this IMessageActivity activity, string id)
        {
            return activity.GetMentions().Where(mention => mention.Mentioned.Id == id).Any();
        }

        /// <summary>
        /// Is there a mention of Recipient.Id in the Text Property 
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>true if this id is mentioned in the text</returns>
        public static bool MentionsRecipient(this IMessageActivity activity)
        {
            return activity.GetMentions().Where(mention => mention.Mentioned.Id == activity.Recipient.Id).Any();
        }

        /// <summary>
        /// Remove recipient mention text from Text property
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>new .Text property value</returns>
        public static string RemoveRecipientMention(this IMessageActivity activity)
        {
            return activity.RemoveMentionText(activity.Recipient.Id);
        }

        /// <summary>
        /// Replace any mention text for given id from Text property
        /// </summary>
        /// <param name="id">id to match</param>
        /// <param name="activity"></param>
        /// <returns>new .Text property value</returns>
        public static string RemoveMentionText(this IMessageActivity activity, string id)
        {
            foreach (var mention in activity.GetMentions().Where(mention => mention.Mentioned.Id == id))
            {
                activity.Text = Regex.Replace(activity.Text, mention.Text, "", RegexOptions.IgnoreCase);
            }
            return activity.Text;
        }

        /// <summary>
        /// Get OAuthClient appropriate for this activity
        /// </summary>
        /// <param name="credentials">credentials for bot to access OAuth api</param>
        /// <param name="serviceUrl">alternate serviceurl to use for OAuth service</param>
        /// <param name="handlers"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static OAuthClient GetOAuthClient(this IActivity activity, MicrosoftAppCredentials credentials, string serviceUrl = null, params DelegatingHandler[] handlers)
        {
            if (serviceUrl != null)
                return new OAuthClient(new Uri(serviceUrl), credentials: credentials, handlers: handlers);

            return new OAuthClient(credentials, true, handlers);
        }

        /// <summary>
        /// Get OAuthClient appropriate for this activity
        /// </summary>
        /// <param name="microsoftAppId"></param>
        /// <param name="microsoftAppPassword"></param>
        /// <param name="serviceUrl">alternate serviceurl to use for state service</param>
        /// <param name="handlers"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static OAuthClient GetOAuthClient(this IActivity activity, string microsoftAppId = null, string microsoftAppPassword = null, string serviceUrl = null, params DelegatingHandler[] handlers) => GetOAuthClient(activity, new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword), serviceUrl, handlers);

        public static bool IsTokenResponseEvent(this IActivity activity)
        {
            return activity.Type == ActivityTypes.Event && ((IEventActivity)activity).Name == TokenOperations.TokenResponseOperationName;
        }

        public static bool IsTeamsVerificationInvoke(this IActivity activity)
        {
            return activity.Type == ActivityTypes.Invoke && ((IInvokeActivity)activity).Name == TokenOperations.TeamsVerificationCode;
        }

        public static TokenResponse ReadTokenResponseContent(this IActivity activity)
        {
            if (IsTokenResponseEvent(activity))
            {
                var content = ((IEventActivity)activity).Value as JObject;
                if (content != null)
                {
                    var tokenResponse = content.ToObject<TokenResponse>();
                    return tokenResponse;
                }
            }
            return null;
        }

        public static async Task<Activity> CreateOAuthReplyAsync(this IActivity activity, string connectionName, string text, string buttonLabel, bool asSignInCard = false)
        {
            var reply = ((Activity)activity).CreateReply();

            switch (activity.ChannelId)
            {
                case "msteams":
                case "cortana":
                case "skype":
                case "skypeforbusiness":
                    asSignInCard = true;
                    break;
            }

            if (asSignInCard)
            {
                var client = GetOAuthClient(activity);
                var link = await client.OAuthApi.GetSignInLinkAsync(activity, connectionName);
                reply.Attachments = new List<Attachment>() {
                    new Attachment()
                    {
                        ContentType = SigninCard.ContentType,
                        Content = new SigninCard()
                        {
                            Text = text,
                            Buttons = new CardAction[]
                            {
                                new CardAction() { Title = buttonLabel, Value = link, Type = ActionTypes.Signin }
                            },
                        }
                    }
                };
            }
            else
            {
                reply.Attachments = new List<Attachment>() {
                    new Attachment()
                    {
                        ContentType = OAuthCard.ContentType,
                        Content = new OAuthCard()
                        {
                            Text = text,
                            ConnectionName = connectionName,
                            Buttons = new CardAction[]
                            {
                                new CardAction() { Title = buttonLabel, Type = ActionTypes.Signin }
                            },
                        }
                    }
                };
            }

            return reply;
        }
    }
}
