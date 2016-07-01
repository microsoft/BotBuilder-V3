using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is part of the "recognize" action. If the customer wants to collect digits - this needs to be specified.
    /// Ex: enter 5 digit zip code followed by pound sign.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class CollectDigits
    {
        /// <summary>
        /// Maximum number of digits expected
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public uint? MaxNumberOfDtmfs { get; set; }

        /// <summary>
        /// Stop tones specified to end collection
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<char> StopTones { get; set; }

        public void Validate()
        {
            bool stopTonesSet = this.StopTones != null && this.StopTones.Any();
            Utils.AssertArgument(
                this.MaxNumberOfDtmfs.GetValueOrDefault() > 0 || stopTonesSet,
                "For CollectDigits either stopTones or maxNumberOfDigits or both must be specified");

            if (this.MaxNumberOfDtmfs.HasValue)
            {
                Utils.AssertArgument(this.MaxNumberOfDtmfs.Value >= MinValues.NumberOfDtmfsExpected && this.MaxNumberOfDtmfs.Value <= MaxValues.NumberOfDtmfsExpected,
                    "MaxNumberOfDtmfs has to be specified in the range of {0} - {1}", MinValues.NumberOfDtmfsExpected, MaxValues.NumberOfDtmfsExpected);
            }

            if (stopTonesSet)
            {
                ValidDtmfs.Validate(this.StopTones);
            }
        }
    }
}
