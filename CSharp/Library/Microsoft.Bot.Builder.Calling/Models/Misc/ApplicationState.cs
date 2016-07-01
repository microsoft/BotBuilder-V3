using System;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    public static class ApplicationState
    {
        /// <summary>
        /// helper method to validate appState
        /// </summary>
        /// <param name="appState"></param>
        public static void Validate(string appState)
        {
            if (!String.IsNullOrWhiteSpace(appState))
            {
                Utils.AssertArgument(appState.Length <= MaxValues.AppStateLength, "Appstate specified cannot exceed {0} bytes", MaxValues.AppStateLength);
            }
        }
    }
}
