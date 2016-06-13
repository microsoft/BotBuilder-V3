
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
        /// Send an activity to an existing conversation
        /// </summary>
        /// System.IO.DirectoryNotFoundException: Could not find a part of the path
        /// 'C:\\\\source\\\\Intercom\\\\Channels\\\\SampleChannel\\\\Content\\\\Methods\\\\SendMessage.md'.
        /// at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
        /// at System.IO.FileStream.Init(String path, FileMode mode, FileAccess
        /// access, Int32 rights, Boolean useRights, FileShare share, Int32
        /// bufferSize, FileOptions options, SECURITY_ATTRIBUTES secAttrs, String
        /// msgPath, Boolean bFromProxy, Boolean useLongPath, Boolean checkHost)
        /// at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess
        /// access, FileShare share, Int32 bufferSize, FileOptions options, String
        /// msgPath, Boolean bFromProxy, Boolean useLongPath, Boolean checkHost)
        /// at System.IO.StreamReader..ctor(String path, Encoding encoding, Boolean
        /// detectEncodingFromByteOrderMarks, Int32 bufferSize, Boolean checkHost)
        /// at System.IO.File.InternalReadAllText(String path, Encoding encoding,
        /// Boolean checkHost)
        /// at System.IO.File.ReadAllText(String path)
        /// at MarkdownDocs.Program.Main(String[] args)
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='conversationId'>
        /// Conversation ID
        /// </param>
        /// <param name='activityId'>
        /// activityId the reply is to (OPTIONAL)
        /// </param>
        /// <param name='activity'>
        /// Activity to send
        /// </param>
        public static APIResponse ReplyToConversation(this IConversations operations, Activity activity)
        {
            return Task.Factory.StartNew(s => ((IConversations)s).ReplyToConversationAsync(activity.Conversation.Id, activity.ReplyToId ?? string.Empty, activity), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Send an activity to an existing conversation
        /// </summary>
        /// System.IO.DirectoryNotFoundException: Could not find a part of the path
        /// 'C:\\\\source\\\\Intercom\\\\Channels\\\\SampleChannel\\\\Content\\\\Methods\\\\SendMessage.md'.
        /// at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
        /// at System.IO.FileStream.Init(String path, FileMode mode, FileAccess
        /// access, Int32 rights, Boolean useRights, FileShare share, Int32
        /// bufferSize, FileOptions options, SECURITY_ATTRIBUTES secAttrs, String
        /// msgPath, Boolean bFromProxy, Boolean useLongPath, Boolean checkHost)
        /// at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess
        /// access, FileShare share, Int32 bufferSize, FileOptions options, String
        /// msgPath, Boolean bFromProxy, Boolean useLongPath, Boolean checkHost)
        /// at System.IO.StreamReader..ctor(String path, Encoding encoding, Boolean
        /// detectEncodingFromByteOrderMarks, Int32 bufferSize, Boolean checkHost)
        /// at System.IO.File.InternalReadAllText(String path, Encoding encoding,
        /// Boolean checkHost)
        /// at System.IO.File.ReadAllText(String path)
        /// at MarkdownDocs.Program.Main(String[] args)
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// <param name='conversationId'>
        /// Conversation ID
        /// </param>
        /// <param name='activityId'>
        /// activityId the reply is to (OPTIONAL)
        /// </param>
        /// <param name='activity'>
        /// Activity to send
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        public static Task<APIResponse> ReplyToConversationAsync(this IConversations operations, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return operations.ReplyToConversationAsync(activity.Conversation.Id, activity.ReplyToId ?? string.Empty, activity);
        }
    }
}
