using System;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// By default Json.net doesn't know how to deserialize JSON data into Interfaces or abstract classes.
    /// This custom Converter helps deserialize "operationOutcomes" specified in JSON into respective concrete "OperationOutcome" classes.
    /// </summary>
    public class OperationOutcomeConverter : JsonCreationConverter<OperationOutcomeBase>
    {
        protected override OperationOutcomeBase Create(Type objectType, JObject jsonObject)
        {
            var type = (string)jsonObject.Property("type");
            if (String.Equals(type, ValidOutcomes.AnswerOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new AnswerOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.HangupOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new HangupOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.RejectOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new RejectOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.PlayPromptOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new PlayPromptOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.RecordOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new RecordOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.RecognizeOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new RecognizeOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.WorkflowValidationOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new WorkflowValidationOutcome();
            }

            throw new ArgumentException(String.Format("The given outcome type '{0}' is not supported!", type));
        }
    }
}
