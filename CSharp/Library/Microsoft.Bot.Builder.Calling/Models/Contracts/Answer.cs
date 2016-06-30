using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the action which customers can specify to indicate that the server call agent should accept the call.
    /// The media is hosted by the server call agent
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Answer : ActionBase
    {
        public static readonly IEnumerable<ModalityType> DefaultAcceptModalityTypes = new ModalityType[] { ModalityType.Audio };

        private IEnumerable<ModalityType> acceptModalityTypes;

        /// <summary>
        /// The modality types the application will accept.  If none are specified,
        /// audio-only is assumed.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<ModalityType> AcceptModalityTypes
        {
            get
            {
                if (this.acceptModalityTypes == null || !this.acceptModalityTypes.Any())
                {
                    return DefaultAcceptModalityTypes;
                }
                else
                {
                    return this.acceptModalityTypes;
                }
            }

            set
            {
                this.acceptModalityTypes = value;
            }
        }

        public Answer()
        {
            this.Action = ValidActions.AnswerAction;
        }

        public override void Validate()
        {
            base.Validate();
            Utils.AssertArgument(this.AcceptModalityTypes.Distinct().Count() == this.AcceptModalityTypes.Count(), "AcceptModalityTypes cannot contain duplicate elements.");
            Utils.AssertArgument(this.AcceptModalityTypes.All((m) => { return m != ModalityType.Unknown; }), "AcceptModalityTypes contains an unknown media type.");
            Utils.AssertArgument(this.AcceptModalityTypes.All((m) => { return m != ModalityType.VideoBasedScreenSharing; }), "AcceptModalityTypes cannot contain VideoBasedScreenSharing.");
        }
    }
}
