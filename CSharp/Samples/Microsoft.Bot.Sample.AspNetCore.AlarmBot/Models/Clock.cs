using System;

namespace Microsoft.Bot.Sample.AspNetCore.AlarmBot.Models
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