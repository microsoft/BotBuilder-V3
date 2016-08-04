// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Microsoft.Bot.Builder.Luis
{
    public abstract class Resolution
    {
        public override string ToString()
        {
            var builder = new StringBuilder();
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                if (value != null)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(",");
                    }
                    builder.Append(property.Name);
                    builder.Append("=");
                    builder.Append(value);
                }
            }

            return builder.ToString();
        }
    }

    public static partial class BuiltIn
    {
        public static partial class DateTime
        {
            public enum DayPart
            {
                [Description("morning")]
                MO,
                [Description("midday")]
                MI,
                [Description("afternoon")]
                AF,
                [Description("evening")]
                EV,
                [Description("night")]
                NI
            }
            public sealed class DateTimeResolution : Resolution, IEquatable<DateTimeResolution>
            {
                public int? Year { get; set; }
                public int? Month { get; set; }
                public int? Day { get; set; }

                public int? Week { get; set; }
                public DayOfWeek? DayOfWeek { get; set; }

                public DayPart? DayPart { get; set; }

                public int? Hour { get; set; }
                public int? Minute { get; set; }
                public int? Second { get; set; }

                public bool Equals(DateTimeResolution other)
                {
                    return other != null
                        && this.Year == other.Year
                        && this.Month == other.Month
                        && this.Day == other.Day
                        && this.Week == other.Week
                        && this.DayOfWeek == other.DayOfWeek
                        && this.DayPart == other.DayPart
                        && this.Hour == other.Hour
                        && this.Minute == other.Minute
                        && this.Second == other.Second;
                }
                public override bool Equals(object other)
                {
                    return this.Equals(other as DateTimeResolution);
                }
                public override int GetHashCode()
                {
                    throw new NotImplementedException();
                }

                public const string PatternDate =
                @"
                    (?:
                        (?<year>X+|\d+)
                        (?:
                            -
                            (?<weekM>W)?
                            (?<month>X+|\d+)
                            (?:
                                -
                                (?<weekD>W)?
                                (?<day>X+|\d+)
                            )?
                        )?
                    )
                ";

                public const string PatternTime =
                @"
                    (?:
                        T
                        (?:
                            (?<part>MO|MI|AF|EV|NI)
                        |
                            (?<hour>X+|\d+)
                            (?:
                                :
                                (?<minute>X+|\d+)
                                (?:
                                    :
                                    (?<second>X+|\d+)
                                )?
                            )?
                        )
                    )
                ";

                public static readonly string Pattern = $"^({PatternDate}{PatternTime} | {PatternDate} | {PatternTime})$";
                public const RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace;
                public static readonly Regex Regex = new Regex(Pattern, Options);

                private static int? ParseIntOrNull(Group group)
                {
                    if (group.Success)
                    {
                        var text = group.Value;
                        int number;
                        if (int.TryParse(text, out number))
                        {
                            return number;
                        }
                        else if (text.Length > 0)
                        {
                            for (int index = 0; index < text.Length; ++index)
                            {
                                switch (text[index])
                                {
                                    case 'X':
                                    case 'x':
                                        continue;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }

                            // -1 means some variable X rather than missing "null" or specified constant value
                            return -1;
                        }
                    }

                    return null;
                }

                private static E? ParseEnumOrNull<E>(Group group) where E: struct
                {
                    if (group.Success)
                    {
                        var text = group.Value;
                        E result;
                        if (Enum.TryParse<E>(text, out result))
                        {
                            return result;
                        }
                    }

                    return null;
                }

                public static bool TryParse(string text, out DateTimeResolution resolution)
                {
                    var match = Regex.Match(text);
                    if (match.Success)
                    {
                        resolution = new DateTimeResolution();
                        var groups = match.Groups;
                        resolution.Year = ParseIntOrNull(groups["year"]);
                        bool weekM = groups["weekM"].Success;
                        if (weekM)
                        {
                            resolution.Week = ParseIntOrNull(groups["month"]);
                        }
                        else
                        {
                            resolution.Month = ParseIntOrNull(groups["month"]);
                        }

                        bool weekD = groups["weekD"].Success;
                        if (weekM || weekD)
                        {
                            resolution.DayOfWeek = (DayOfWeek?)ParseIntOrNull(groups["day"]);
                        }
                        else
                        {
                            resolution.Day = ParseIntOrNull(groups["day"]);
                        }

                        resolution.DayPart = ParseEnumOrNull<DayPart>(groups["part"]);
                        resolution.Hour = ParseIntOrNull(groups["hour"]);
                        resolution.Minute = ParseIntOrNull(groups["minute"]);
                        resolution.Second = ParseIntOrNull(groups["second"]);
                        return true;
                    }

                    resolution = null;
                    return false;
                }
            }

            public sealed class DurationResolution
            {

            }
        }
    }

    public interface IResolutionParser
    {
        bool TryParse(IDictionary<string, string> properties, out Resolution resolution); 
    }

    public sealed class ResolutionParser : IResolutionParser
    {
        bool IResolutionParser.TryParse(IDictionary<string, string> properties, out Resolution resolution)
        {
            string resolution_type;
            if (properties.TryGetValue("resolution_type", out resolution_type))
            {
                switch (resolution_type)
                {
                    case "builtin.datetime.date":
                        string date;
                        if (properties.TryGetValue("date", out date))
                        {
                            BuiltIn.DateTime.DateTimeResolution dateTime;
                            if (BuiltIn.DateTime.DateTimeResolution.TryParse(date, out dateTime))
                            {
                                resolution = dateTime;
                                return true;
                            }
                        }

                        break;
                    case "builtin.datetime.time":
                    case "builtin.datetime.set":
                        string time;
                        if (properties.TryGetValue("time", out time))
                        {
                            BuiltIn.DateTime.DateTimeResolution dateTime;
                            if (BuiltIn.DateTime.DateTimeResolution.TryParse(time, out dateTime))
                            {
                                resolution = dateTime;
                                return true;
                            }
                        }

                        break;
                }
            }

            resolution = null;
            return false;
        }
    }
}