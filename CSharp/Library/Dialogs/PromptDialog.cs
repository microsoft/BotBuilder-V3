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

    public class PromptDialogArgs
    {
        public PromptType DataType { set; get; }
        public int MaxRetries { set; get; }
        public string Prompt { set; get; }
        public string RetryPrompt { set; get; }
        public IList<string> Choices { set; get; }
    }

    public class PromptDialog : Dialog<PromptDialogArgs, object>
    {
        public static readonly PromptDialog Instance = new PromptDialog();

        private PromptDialog() :
            base(id: "6FC08CE1D79B4A9CA61D61C13BDBA54A")
        {
        }

        public override async Task<Connector.Message> BeginAsync(ISession session, Task<PromptDialogArgs> taskArguments)
        {
            var arguments = await taskArguments;
            SaveDialogState(session, arguments);
            return await session.CreateDialogResponse(new Message() { Text = arguments.Prompt });
        }

        public override async Task<Connector.Message> ReplyReceivedAsync(ISession session)
        {
            var state = LoadDialogState(session);
            switch (state.DataType)
            {
                case PromptType.Text:
                    if (!string.IsNullOrEmpty(session.Message.Text))
                    {
                        return await Succeeded(session, state, session.Message.Text);
                    }
                    else
                    {
                        return await Failed(session, state, state.RetryPrompt ??
                                string.Format("{0}\n{1}", "I didn't understand. Say something in reply", state.Prompt));
                    }
                case PromptType.Number:
                    var txt = session.Message.Text;
                    double number;
                    if (double.TryParse(txt, out number))
                    {
                        return await Succeeded(session, state, number);
                    }
                    else
                    {
                        return await Failed(session, state, state.RetryPrompt ??
                                string.Format("{0}\n{1}", "I didn't understand. Say something in reply", state.Prompt));
                    }
                case PromptType.Confirm:
                    switch (session.Message.Text.ToLower().Trim())
                    {
                        case "y":
                        case "yes":
                        case "ok":
                            return await Succeeded(session, state, true);
                        case "n":
                        case "no":
                            return await Succeeded(session, state, false);
                        default:
                            return await Failed(session, state, state.RetryPrompt == null ?
                                string.Format("{0}\n{1}", "I didn't understand. Valid replies are yes or no.", state.Prompt)
                                : state.RetryPrompt);
                    }
                case PromptType.Choice:
                    var text = session.Message.Text.ToLower();
                    var choice = state.Choices.Where(s => s.ToLower() == text).ToList();
                    if (choice.Count > 0)
                    {
                        return await Succeeded(session, state, choice.First());
                    }
                    else
                    {
                        return await Failed(session, state, state.RetryPrompt == null ?
                            string.Format("{0}\n{1}", "I didn't understand.", state.Prompt)
                            : state.RetryPrompt);
                    }
                default:
                    throw new DialogException(string.Format("Cannot handle this prompt type: {0}", state.DataType), this);
            }   
        }

        private async Task<Connector.Message> Succeeded<T>(ISession session, PromptDialogArgs state, T response)
        {
            return await session.EndDialogAsync(this, response);
        }

        private async Task<Connector.Message> Failed(ISession session, PromptDialogArgs state, string retryPrompt)
        {
            if (state.MaxRetries > 0)
            {
                state.MaxRetries--;
                SaveDialogState(session, state);
                return await session.CreateDialogResponse(retryPrompt);
            }
            else
            {
                return await session.EndDialogAsync(this, Tasks.Cancelled);
            }
        }

        private PromptDialogArgs LoadDialogState(ISession session)
        {
            var state = new PromptDialogArgs();
            foreach (PropertyInfo pi in state.GetType().GetProperties())
            {
                var value = session.Stack.GetLocal(pi.Name);
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
                session.Stack.SetLocal(pi.Name, pi.GetValue(args));
            }
        }

    }

    public class Prompts
    {
        private static async Task<Connector.Message> BeginDialogAsync(ISession session, PromptDialogArgs arguments)
        {
            var taskArguments = Task.FromResult((object) arguments);
            return await session.BeginDialogAsync(PromptDialog.Instance, taskArguments);
        }

        public static async Task<Connector.Message> Text(ISession session, string prompt, string retryPrompt = null, int maxRetries = 3)
        {
            return await BeginDialogAsync(session, new PromptDialogArgs()
            {
                DataType = PromptType.Text,
                Prompt = prompt,
                RetryPrompt = retryPrompt,
                MaxRetries = maxRetries
            });
        }

        public static async Task<Connector.Message> Confirm(ISession session, string prompt, string retryPrompt = null, int maxRetries = 3)
        {
            return await BeginDialogAsync(session, new PromptDialogArgs()
            {
                DataType = PromptType.Confirm,
                Prompt = prompt,
                RetryPrompt = retryPrompt,
                MaxRetries = maxRetries
            });
        }

        public static async Task<Connector.Message> Number(ISession session, string prompt, string retryPrompt = null, int maxRetries = 3)
        {
            return await BeginDialogAsync(session, new PromptDialogArgs()
            {
                DataType = PromptType.Number,
                Prompt = prompt,
                RetryPrompt = retryPrompt,
                MaxRetries = maxRetries
            });
        }

        public static async Task<Connector.Message> Choice(ISession session, string prompt, IList<string> choices, string retryPrompt = null, int maxRetries = 3)
        {
            return await BeginDialogAsync(session, new PromptDialogArgs()
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
