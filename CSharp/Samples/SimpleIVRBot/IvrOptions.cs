using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Bot.Sample.SimpleIVRBot
{
    public static class IvrOptions
    {
        internal const string WelcomeMessage = "Hello, you have successfully contacted XY internet service provider.";

        internal const string MainMenuPrompt =
            "If you are a new client press 1, for technical support press 2, if you need information about payments press 3, to hear more about the company press 4. To repeat the options press 5.";

        internal const string NewClientPrompt =
            "To check our latest offer press 1, to order a new service press 2. Press the hash key to return to the main menu";

        internal const string SupportPrompt =
            "To check our current outages press 1, to contact the technical support consultant press 2. Press the hash key to return to the main menu";

        internal const string PaymentPrompt =
            "To get the payment details press 1, press 2 if your payment is not visible in the system. Press the hash key to return to the main menu";

        internal const string MoreInfoPrompt =
            "XY is the leading Internet Service Provider in Prague. Our company was established in 1995 and currently has 2000 employees.";

        internal const string NoConsultants =
            "Unfortunately there are no consultants available at this moment. Please leave your name, and a brief message after the signal. You can press the hash key when finished. We will call you as soon as possible.";

        internal const string Ending = "Thank you for leaving the message, goodbye";

        internal const string Offer = "You can sign up for 100 megabit connection just for 10 euros per month till the end of month";
        internal const string CurrentOutages = "There is currently 1 outage in Prague 5, we are working on fixing the issue";

        internal const string PaymentDetailsMessage =
            "You should do the wire transfer till the 5th day of month to account number 3983815";
    }
}