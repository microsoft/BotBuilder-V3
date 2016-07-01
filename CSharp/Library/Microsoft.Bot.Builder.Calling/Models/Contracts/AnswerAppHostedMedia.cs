using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the action which customers can specify to indicate that the server call agent should accept the call but that the
    /// application will host the media.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class AnswerAppHostedMedia : ActionBase
    {
        public static readonly IEnumerable<ModalityType> DefaultAcceptModalityTypes = new ModalityType[] { ModalityType.Audio };

        private IEnumerable<ModalityType> acceptModalityTypes;

        public AnswerAppHostedMedia()
            : base(isStandaloneAction: true)
        {
            this.Action = ValidActions.AnswerAppHostedMediaAction;
        }

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

        /// <summary>
        /// Opaque object to pass media negotation configuration from the application to the ExecutionAgent.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public JObject MediaConfiguration { get; set; }

        public override void Validate()
        {
            base.Validate();
            Utils.AssertArgument(this.MediaConfiguration != null, "MediaConfiguration must not be null.");
            Utils.AssertArgument(this.MediaConfiguration.ToString().Length <= MaxValues.MediaConfigurationLength, "MediaConfiguration must serialize to less than or equal to {0} characters.", MaxValues.MediaConfigurationLength);
            Utils.AssertArgument(this.AcceptModalityTypes.Distinct().Count() == this.AcceptModalityTypes.Count(), "AcceptModalityTypes cannot contain duplicate elements.");
            Utils.AssertArgument(this.AcceptModalityTypes.All((m) => { return m != ModalityType.Unknown; }), "AcceptModalityTypes contains an unknown media type.");
            Utils.AssertArgument(this.AcceptModalityTypes.All((m) => { return m != ModalityType.VideoBasedScreenSharing; }), "AcceptModalityTypes cannot contain VideoBasedScreenSharing.");
        }
    }
}
