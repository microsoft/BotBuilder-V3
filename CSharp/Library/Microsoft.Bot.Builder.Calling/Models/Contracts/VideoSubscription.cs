using System;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    [JsonObject(MemberSerialization.OptOut)]
    public class VideoSubscription : ActionBase    
    {
        /// <summary>
        /// Opaque string to facilitate app developers to pass their custom data in this field. 
        /// This field is the same value that was passed 'response' by the customer.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string AppState { get; set; }

        /// <summary>
        /// Sequence ID of video socket. Index from 0-9
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public uint SocketId { get; set; }

        /// <summary>
        /// Identity of the participant whose video is pinned if VideoMode is set to controlled
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string ParticipantIdentity { get; set; }

        /// <summary>
        /// VideoMode indicates whether the socket is pinned to a particular participant
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public VideoSubscriptionMode VideoSubscriptionMode { get; set; }

        /// <summary>
        /// Indicates whether the video is from the camera or from screen sharing
        /// Unknown, Video and VideoBasedScreenSharing are supported modalities for this request
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public ModalityType VideoModality { get; set; }

        /// <summary>
        /// Indicates the video resolution format.Default value is "sd360p".
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public ResolutionFormat VideoResolution { get; set; }

        public VideoSubscription()
        {
            this.Action = ValidActions.VideoSubscriptionAction;
        }

        public override void Validate()
        {
            base.Validate();
            Utils.AssertArgument((10 > this.SocketId), "Socket id should be in the range 0-9");
            Utils.AssertArgument(this.VideoModality != ModalityType.Audio, "Audio modality is not supported for this operation");
            ApplicationState.Validate(this.AppState);

            if (VideoSubscriptionMode  == VideoSubscriptionMode.Manual)
            {
                Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.ParticipantIdentity), "Participant identity cannot be null or empty when VideoSubscriptionMode is set to Manual");
                Utils.AssertArgument(VideoModality != ModalityType.Unknown, "VideoModality cannot be unknown when VideoSubscriptionMode is set to Manual");
            }
            else if (VideoSubscriptionMode == VideoSubscriptionMode.Auto)
            {
                Utils.AssertArgument(String.IsNullOrWhiteSpace(this.ParticipantIdentity), "Participant identity should not be specified when VideoSubscriptionMode is set to Auto");
                Utils.AssertArgument(VideoModality == ModalityType.Unknown, "VideoModality should not be set");
            }
        }
    }
}
