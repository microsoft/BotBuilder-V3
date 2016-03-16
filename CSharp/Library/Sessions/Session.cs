using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Builder
{
    public abstract class Session : ISession
    {
        private readonly Message message;
        private readonly ISessionData sessionData;
        private readonly IDialogStack stack;
        private readonly IDialog defaultDialog;

        public static async Task<TResult> DispatchAsync<TSession, TResult>(ISessionStore store, Message message, IDialogCollection dialogs, Func<ISessionData, IDialogStack, TSession> MakeSession, Func<TSession, DialogResponse, TResult> MakeResponse) where TSession: ISession
        {
            string sessionId = message.ConversationId;
            ISessionData sessionData = new InMemorySessionData();
            await store.LoadAsync(sessionId, sessionData);

            IDialogStack stack = new DialogStack(sessionData, dialogs);
            TSession session = MakeSession(sessionData, stack);

            DialogResponse response;
            try
            {
                response = await session.DispatchAsync();
            }
            finally
            {
                stack.Flush();
                await store.SaveAsync(sessionId, sessionData);
            }

            return MakeResponse(session, response);
        }

        public Session(Message message, ISessionData sessionData, IDialogStack stack, IDialog defaultDialog)
        {
            Field.SetNotNull(ref this.message, nameof(message), message);
            Field.SetNotNull(ref this.sessionData, nameof(sessionData), sessionData);
            Field.SetNotNull(ref this.stack, nameof(stack), stack);
            Field.SetNotNull(ref this.defaultDialog, nameof(defaultDialog), defaultDialog);
        }

        public Message Message { get { return this.message; } }


        ISessionData ISession.SessionData { get { return this.sessionData; } }
        IDialogFrame ISession.Stack { get { return this.stack; } }

        public virtual async Task<DialogResponse> DispatchAsync()
        {
            DialogResponse reply;

            if (this.stack.Count == 0)
            {
                reply = await this.BeginDialogAsync(this.defaultDialog);
            }
            else
            {

                var leaf = this.stack.Peek();
                if (leaf != null)
                {
                    reply = await leaf.ReplyReceivedAsync(this);
                }
                else
                {
                    this.stack.Clear();
                    return await this.BeginDialogAsync(this.defaultDialog);
                }
            }

            return reply;
        }

        public async Task<DialogResponse> BeginDialogAsync(IDialog dialog, object args = null)
        {
            this.stack.Push(dialog);
            return await dialog.BeginAsync(this, args);
        
        }

        public async Task<DialogResponse> EndDialogAsync(IDialog dialog, DialogResult result)
        {
            this.ValidateTopOfStack(dialog);

            var child = this.stack.Pop();

            if (this.stack.Count > 0)
            {
                var parent = this.stack.Peek();
                return await parent.DialogResumedAsync(this, result);
            }
            else
            {
                return await BeginDialogAsync(this.defaultDialog);
            }
        }

        public async Task<DialogResponse> CreateDialogResponse(Message msg)
        {
            // To make sure that developer have called CreateReplyMessage() to create this message
            // we create the reply and copy the fields from the message the user passed in
            var reply = this.Message.CreateReplyMessage();
            
            reply.Text = msg.Text;
            reply.SourceText = msg.SourceText;
            reply.Attachments = msg.Attachments;
            reply.Created = DateTime.Now;
            reply.Language = msg.Language ?? Message.Language;
            reply.SourceLanguage = msg.SourceLanguage ?? Message.SourceLanguage;
            reply.Location = msg.Location ?? Message.Location;
            reply.Mentions = msg.Mentions ?? Message.Mentions;
            reply.Place = msg.Place ?? Message.Place;
            reply.Type = msg.Type ?? Message.Type;  
            
            var replyMessage = new DialogResponse
            {
                Reply = reply,
                DialogId = this.stack.Peek().Id,
                Error = null
            };

            return replyMessage; 
        }

        public async Task<DialogResponse> CreateDialogResponse(string msg)
        {
            var message = new Message()
            {
                Text = msg
            };

            return await CreateDialogResponse(message);
        }

        public async Task<DialogResponse> CreateDialogErrorResponse(HttpStatusCode status = HttpStatusCode.InternalServerError, string errorMsg = null)
        {
            var reply = new DialogResponse
            {
                Error = new HttpException((int)status, errorMsg),
            };

            return reply;
        }

        private void ValidateTopOfStack(IDialog dialog)
        {
            var leaf = this.stack.Peek();
            if (dialog != leaf)
            {
                string message = $"stack discipline failure: expected {dialog.Id}, have {leaf.Id} at top of stack";
                throw new InvalidOperationException(message);
            }
        }
    }
}