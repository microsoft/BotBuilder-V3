using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public enum PromptType
    {
        Text,
        Number,
        Confirm,
        Choice
    }

    public class PromptDialogResult<T> : DialogResult
    {
        public PromptType? DataType { set; get; }
        public T Response { set; get; }
    }

    public class PromptDialogArgs
    {
        public PromptType DataType { set; get; }
        public int MaxRetries { set; get; }
        public string Prompt { set; get; }
        public string RetryPrompt { set; get; }
        public IList<string> Choices { set; get; }
    }

    public class PromptDialog : Dialog<PromptDialogArgs, DialogResult>
    {
        private string ArgsKey { get { return "PromptDialogArgs"; } }

        public static readonly PromptDialog Instance = new PromptDialog();

        private PromptDialog() :
            base(Id: "6FC08CE1D79B4A9CA61D61C13BDBA54A")
        {
        }

        public override async Task<DialogResponse> BeginAsync(ISession session, PromptDialogArgs args)
        {
            SaveDialogState(session, args);
            return await session.CreateDialogResponse(new Message() { Text = args.Prompt });
        }

        public override async Task<DialogResponse> ReplyReceivedAsync(ISession session)
        {
            var state = LoadDialogState(session);
            DialogResponse reply = null;
            switch (state.DataType)
            {
                case PromptType.Text:
                    if (!string.IsNullOrEmpty(session.Message.Text))
                    {
                        reply = await Succeeded(session, state, session.Message.Text);
                        break;
                    }
                    else
                    {
                        reply = await Failed<string>(session, state, state.RetryPrompt ??
                                string.Format("{0}\n{1}", "I didn't understand. Say something in reply", state.Prompt));
                        break;
                    }
                case PromptType.Number:
                    var txt = session.Message.Text;
                    var n = txt.IndexOf('.') >= 0 ? Convert.ToDouble(txt) : Convert.ToInt64(txt);
                    reply = await Succeeded(session, state, n);
                    break;
                case PromptType.Confirm:
                    switch (session.Message.Text.ToLower().Trim())
                    {
                        case "y":
                        case "yes":
                        case "ok":
                            reply = await Succeeded(session, state, true);
                            break;
                        case "n":
                        case "no":
                            reply = await Succeeded(session, state, false);
                            break;
                        default:
                            reply = await Failed<bool>(session, state, state.RetryPrompt == null ?
                                string.Format("{0}\n{1}", "I didn't understand. Valid replies are yes or no.", state.Prompt)
                                : state.RetryPrompt);
                            break;
                    }
                    break;
                case PromptType.Choice:
                    var text = session.Message.Text.ToLower();
                    var choice = state.Choices.Where(s => s.ToLower() == text).ToList();
                    if (choice.Count > 0)
                    {
                        reply = await Succeeded(session, state, choice.First());
                    }
                    else
                    {
                        reply = await Failed<string>(session, state, state.RetryPrompt == null ?
                            string.Format("{0}\n{1}", "I didn't understand.", state.Prompt)
                            : state.RetryPrompt);
                    }
                    break;
                default:
                    reply = await session.CreateDialogErrorResponse(errorMessage: string.Format("Cannot handle this prompt type: {0}", state.DataType));
                    break;
            }
            return reply;
        }

        private async Task<DialogResponse> Succeeded<T>(ISession session, PromptDialogArgs state, T response)
        {
            return await session.EndDialogAsync(this, new PromptDialogResult<T>()
            {
                Completed = true,
                Response = response,
                DataType = state.DataType
            });
        }

        private async Task<DialogResponse> Failed<T>(ISession session, PromptDialogArgs state, string retryPrompt)
        {
            if (state.MaxRetries > 0)
            {
                state.MaxRetries--;
                SaveDialogState(session, state);
                return await session.CreateDialogResponse(retryPrompt);
            }
            else
            {
                return await session.EndDialogAsync(this, new PromptDialogResult<T>()
                {
                    Completed = false,
                    Response = default(T),
                    DataType = state.DataType
                });
            }
        }

        private PromptDialogArgs LoadDialogState(ISession session)
        {
            var state = new PromptDialogArgs();
            foreach (PropertyInfo pi in state.GetType().GetProperties())
            {
                var value = session.Stack.GetDialogState(pi.Name);
                if (pi.PropertyType == typeof(PromptType))
                {
                    var dataType = Enum.Parse(typeof(PromptType), value.ToString());
                    pi.SetValue(state, dataType);
                }
                else if (pi.PropertyType == typeof(int))
                {
                    pi.SetValue(state, Convert.ToInt32(value));
                }
                else
                {
                    pi.SetValue(state, value);
                }
            }
            return state;
        }

        private void SaveDialogState(ISession session, PromptDialogArgs args)
        {
            foreach (PropertyInfo pi in args.GetType().GetProperties())
            {
                session.Stack.SetDialogState(pi.Name, pi.GetValue(args));
            }
        }

    }

    public class Prompts
    {
        public static async Task<DialogResponse> Text(ISession session, string prompt, string retryPrompt = null, int maxRetries = 3)
        {
            return await session.BeginDialogAsync(PromptDialog.Instance, new PromptDialogArgs()
            {
                DataType = PromptType.Text,
                Prompt = prompt,
                RetryPrompt = retryPrompt,
                MaxRetries = maxRetries
            });
        }

        public static async Task<DialogResponse> Confirm(ISession session, string prompt, string retryPrompt = null, int maxRetries = 3)
        {
            return await session.BeginDialogAsync(PromptDialog.Instance, new PromptDialogArgs()
            {
                DataType = PromptType.Confirm,
                Prompt = prompt,
                RetryPrompt = retryPrompt,
                MaxRetries = maxRetries
            });
        }

        public static async Task<DialogResponse> Number(ISession session, string prompt, string retryPrompt = null, int maxRetries = 3)
        {
            return await session.BeginDialogAsync(PromptDialog.Instance, new PromptDialogArgs()
            {
                DataType = PromptType.Number,
                Prompt = prompt,
                RetryPrompt = retryPrompt,
                MaxRetries = maxRetries
            });
        }

        public static async Task<DialogResponse> Choice(ISession session, string prompt, IList<string> choices, string retryPrompt = null, int maxRetries = 3)
        {
            return await session.BeginDialogAsync(PromptDialog.Instance, new PromptDialogArgs()
            {
                DataType = PromptType.Choice,
                Prompt = prompt,
                RetryPrompt = retryPrompt,
                MaxRetries = maxRetries,
                Choices = choices
            });
        }
    }
}
