using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder
{
    public abstract class Session : ISession
    {
        private readonly Message message;
        private readonly ISessionData sessionData;
        private readonly IDialogStack stack;
        private readonly IDialog defaultDialog;

        // TODO: add some sort of trampoline to avoid stack overflow
        public static async Task<TResult> DispatchAsync<TSession, TResult>(ISessionStore store, Message message, IDialogCollection dialogs, Func<ISessionData, IDialogStack, TSession> MakeSession, Func<TSession, Task<Connector.Message>, TResult> MakeResponse) where TSession: ISession
        {
            string sessionID = message.ConversationId;
            ISessionData sessionData = new InMemorySessionData();
            await store.LoadAsync(sessionID, sessionData);

            IDialogStack stack = new DialogStack(sessionData, dialogs);
            TSession session = MakeSession(sessionData, stack);

            Task<Connector.Message> taskResponse;
            try
            {
                taskResponse = session.DispatchAsync();

                await taskResponse.WithNoThrow();
            }
            finally
            {
                stack.Flush();
                await store.SaveAsync(sessionID, sessionData);
            }

            return MakeResponse(session, taskResponse);
        }

        public Session(Message message, ISessionData sessionData, IDialogStack stack, IDialog defaultDialog)
        {
            Field.SetNotNull(out this.message, nameof(message), message);
            Field.SetNotNull(out this.sessionData, nameof(sessionData), sessionData);
            Field.SetNotNull(out this.stack, nameof(stack), stack);
            Field.SetNotNull(out this.defaultDialog, nameof(defaultDialog), defaultDialog);
        }

        public Message Message { get { return this.message; } }


        ISessionData ISession.SessionData { get { return this.sessionData; } }
        IDialogFrame ISession.Stack { get { return this.stack; } }

        public virtual async Task<Connector.Message> DispatchAsync()
        {
            if (this.stack.Count == 0)
            {
                return await this.BeginDialogAsync(this.defaultDialog, Tasks.Null);
            }
            else
            {

                var leaf = this.stack.Peek();
                if (leaf != null)
                {
                    try
                    {
                        return await leaf.ReplyReceivedAsync(this);
                    }
                    catch (Exception error)
                    {
                        if (this.ThrowToUser(error))
                        {
                            this.stack.Clear();
                            throw;
                        }
                        else
                        {
                            return await this.UnwindStackAsync(leaf, error);
                        }
                    }
                }
                else
                {
                    this.stack.Clear();
                    return await this.BeginDialogAsync(this.defaultDialog, Tasks.Null);
                }
            }
        }

        public async Task<Connector.Message> BeginDialogAsync(IDialog dialog, Task<object> arguments)
        {
            ValidateCompletedTask(arguments, nameof(arguments));

            this.stack.Push(dialog);
            try
            {
                return await dialog.BeginAsync(this, arguments);
            }
            catch (Exception error)
            {
                if (this.ThrowToUser(error))
                {
                    this.stack.Clear();
                    throw;
                }
                else
                {
                    return await this.UnwindStackAsync(dialog, error);
                }
            }
        }

        public async Task<Connector.Message> EndDialogAsync(IDialog dialog, Task<object> result)
        {
            ValidateCompletedTask(result, nameof(result));
            this.ValidateTopOfStack(dialog);

            var child = this.stack.Pop();

            if (this.stack.Count > 0)
            {
                var parent = this.stack.Peek();
                try
                {
                    return await parent.DialogResumedAsync(this, result);
                }
                catch (Exception error)
                {
                    if (this.ThrowToUser(error))
                    {
                        this.stack.Clear();
                        throw;
                    }
                    else
                    {
                        return await this.UnwindStackAsync(parent, error);
                    }
                }
            }
            else
            {
                return await BeginDialogAsync(this.defaultDialog, Tasks.Null);
            }
        }

        private bool ThrowToUser(Exception error)
        {
            bool atRoot = this.stack.Count <= 1;
            return atRoot;
        }

        private async Task<Connector.Message> UnwindStackAsync(IDialog dialog, Exception error)
        {
            if (ThrowToUser(error))
            {
                throw new StackDisciplineException("exceptions should have been returned to user");
            }

            return await EndDialogAsync(dialog, Task<object>.Factory.FromException(error));
        }

        public async Task<Connector.Message> CreateDialogResponse(Message msg)
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

            return reply; 
        }

        public async Task<Connector.Message> CreateDialogResponse(string msg)
        {
            var message = new Message()
            {
                Text = msg
            };

            return await CreateDialogResponse(message);
        }

        private void ValidateTopOfStack(IDialog dialog)
        {
            var leaf = this.stack.Peek();
            if (dialog != leaf)
            {
                string message = $"stack discipline failure: expected {dialog.ID}, have {leaf.ID} at top of stack";
                throw new StackDisciplineException(message);
            }
        }

        private static void ValidateCompletedTask(Task task, string name)
        {
            if (! task.IsCompleted)
            {
                throw new TaskNotCompletedException($"{name}.Status == {task.Status}");
            }
        }
    }
}