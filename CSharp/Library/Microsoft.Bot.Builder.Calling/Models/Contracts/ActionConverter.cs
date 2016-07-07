using System;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// By default Json.net doesn't know how to deserialize JSON data into Interfaces or abstract classes.
    /// This custom Converter helps deserialize "actions" specified in JSON into respective concrete "action" classes.
    /// </summary>
    public class ActionConverter : JsonCreationConverter<ActionBase>
    {
        protected override ActionBase Create(Type objectType, JObject jsonObject)
        {
            var actionProperties = jsonObject.Properties().Where(p => p != null && p.Name != null && String.Equals(p.Name, "action", StringComparison.OrdinalIgnoreCase));
            string type = null;

            if (actionProperties.Count() == 1)
            {
                type = (string)actionProperties.First();
            }
            else
            {
                throw new ArgumentException(String.Format("Expected single action."));
            }

            if (String.Equals(type, ValidActions.AnswerAction, StringComparison.OrdinalIgnoreCase))
            {
                return new Answer();
            }
            else if (String.Equals(type, ValidActions.HangupAction, StringComparison.OrdinalIgnoreCase))
            {
                return new Hangup();
            }
            else if (String.Equals(type, ValidActions.RejectAction, StringComparison.OrdinalIgnoreCase))
            {
                return new Reject();
            }
            else if (String.Equals(type, ValidActions.PlayPromptAction, StringComparison.OrdinalIgnoreCase))
            {
                return new PlayPrompt();
            }
            else if (String.Equals(type, ValidActions.RecordAction, StringComparison.OrdinalIgnoreCase))
            {
                return new Record();
            }
            else if (String.Equals(type, ValidActions.RecognizeAction, StringComparison.OrdinalIgnoreCase))
            {
                return new Recognize();
            }

            throw new ArgumentException(String.Format("The given action '{0}' is not supported!", type));
        }
    }
}
