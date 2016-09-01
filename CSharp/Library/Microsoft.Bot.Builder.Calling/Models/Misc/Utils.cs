using System;
using System.Globalization;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Utils class
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Argument checker
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void AssertArgument(bool condition, string format, params object[] args)
        {
            if (!condition)
            {
                var text = string.Format(CultureInfo.InvariantCulture, format, args);

                throw new ArgumentException(text);
            }
        }
    }
}
