
namespace Microsoft.Bot.Connector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Rest;


    public static partial class ConversationsExtensions
    {
        /// <summary>
        /// Create a new direct conversation between a bot and a user
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='bot'>Bot to create conversation from</param>
        /// <param name='user'>User to create conversation with</param>
        public static ResourceResponse CreateDirectConversation(this IConversations operations, ChannelAccount bot, ChannelAccount user)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).CreateConversationAsync(GetDirectParameters(bot, user)), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create a new direct conversation between a bot and a user
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='bot'>Bot to create conversation from</param>
        /// <param name='user'>User to create conversation with</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        public static async Task<ResourceResponse> CreateDirectConversationAsync(this IConversations operations, ChannelAccount bot, ChannelAccount user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _result = await operations.CreateConversationWithHttpMessagesAsync(GetDirectParameters(bot, user), null, cancellationToken).ConfigureAwait(false);
            return _result.HandleError<ResourceResponse>();
        }

        /// <summary>
        /// Create a new direct conversation between a bot and a user
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='botAddress'>Bot to create conversation from</param>
        /// <param name='userAddress'>User to create conversation with</param>
        public static ResourceResponse CreateDirectConversation(this IConversations operations, string botAddress, string userAddress)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).CreateConversationAsync(GetDirectParameters(botAddress, userAddress)), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create a new direct conversation between a bot and a user
        /// </summary>
        /// <param name='operations'>The operations group for this extension method.</param>
        /// <param name='botAddress'>Bot to create conversation from</param>
        /// <param name='userAddress'>User to create conversation with</param>
        /// <param name='cancellationToken'>The cancellation token</param>
        public static async Task<ResourceResponse> CreateDirectConversationAsync(this IConversations operations, string botAddress, string userAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _result = await operations.CreateConversationWithHttpMessagesAsync(GetDirectParameters(botAddress, userAddress), null, cancellationToken).ConfigureAwait(false);
            return _result.HandleError<ResourceResponse>();
        }

        /// <summary>
        /// Send an activity to a conversation
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send
        /// </param>
        public static APIResponse SendToConversation(this IConversations operations, Activity activity)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).SendToConversationAsync(activity, activity.Conversation.Id), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Send an activity to a conversation
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        public static Task<APIResponse> SendToConversationAsync(this IConversations operations, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return operations.SendToConversationAsync(activity, activity.Conversation.Id, cancellationToken);
        }

        /// <summary>
        /// Replyto an activity in an existing conversation
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send
        /// </param>
        public static APIResponse ReplyToActivity(this IConversations operations, Activity activity)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).ReplyToActivityAsync(activity.Conversation.Id, activity.ReplyToId, activity), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reply to an activity in an existing conversation
        /// </summary>
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='activity'>
        /// Activity to send
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        public static Task<APIResponse> ReplyToActivityAsync(this IConversations operations, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            // TEMP TODO REMOVE THIS AFTER SKYPE DEPLOYS NEW SERVICE WHICH PROPERLY IMPLEMENTS THIS ENDPOINT
            if (activity.ReplyToId == "0")
                return operations.SendToConversationAsync(activity);

            if (activity.ReplyToId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "ReplyToId");
            }

            return operations.ReplyToActivityAsync(activity.Conversation.Id, activity.ReplyToId, activity, cancellationToken);
        }

        private static ConversationParameters GetDirectParameters(string botId, string userId)
        {
            return new ConversationParameters() { Bot = new ChannelAccount(botId), Members = new ChannelAccount[] { new ChannelAccount(userId) } };
        }

        private static ConversationParameters GetDirectParameters(ChannelAccount bot, ChannelAccount user)
        {
            return new ConversationParameters() { Bot = bot, Members = new ChannelAccount[] { user } };
        }

    }
}
