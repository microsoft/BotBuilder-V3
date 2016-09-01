using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is a helper class for validating dtmfs specified in strings
    /// </summary>
    public static class ValidDtmfs
    {
        /// <summary>
        /// list of valid dtmfs
        /// </summary>
        public static readonly Dictionary<char, int> ValidDtmfSet = new Dictionary<char, int>()
        {
            {'0', 0},
            {'1', 1},
            {'2', 2},
            {'3', 3},
            {'4', 4},
            {'5', 5},
            {'6', 6},
            {'7', 7},
            {'8', 8},
            {'9', 9},
            {'*', 10},
            {'#', 11},
            {'A', 12},
            {'B', 13},
            {'C', 14},
            {'D', 15},
        };

        public static void Validate(char digit)
        {
            Utils.AssertArgument(ValidDtmfSet.ContainsKey(digit), "'{0}' is not a valid dtmf", digit);
        }

        public static void Validate(IEnumerable<char> variations)
        {
            Utils.AssertArgument(variations != null, "Specified digit list is null");
            int count = variations.Count();
            Utils.AssertArgument(count > 0, "Specified digit list is empty");
            Utils.AssertArgument(count <= MaxValues.NumberOfStopTones, "Number of stop tones specified cannot exceed : {0}", MaxValues.NumberOfStopTones);
            foreach (char c in variations)
            {
                Validate(c);
            }
        }
    }
}
