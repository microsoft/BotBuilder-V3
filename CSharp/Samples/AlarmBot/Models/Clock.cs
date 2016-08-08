using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Bot.Sample.AlarmBot.Models
{
    public interface IClock
    {
        DateTime Now { get; }
    }

    public sealed class SystemClock : IClock
    {
        DateTime IClock.Now
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}