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

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Class to collect and then replay activities as a transcript.
    /// </summary>
    public sealed class ReplayTranscript
    {
        private Func<IActivity, string> _header;
        private List<IActivity> _activities = new List<IActivity>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="header">Function for defining the transcript header on each message.</param>
        public ReplayTranscript(Func<IActivity, string> header = null)
        {
            if (_header == null)
            {
                _header = (activity) => $"({activity.From.Name} {activity.Timestamp:g})";
            }
        }

        /// <summary>
        /// Collect activity to replay.
        /// </summary>
        /// <param name="activity">Activity.</param>
        /// <returns>Task.</returns>
        public async Task Collect(IActivity activity)
        {
            _activities.Add(activity);
        }

        /// <summary>
        /// Replay collected transcript.
        /// </summary>
        /// <param name="botToUser">Where to post the transcript.</param>
        /// <returns>Task.</returns>
        public async Task Replay(IBotToUser botToUser)
        {
            foreach (var activity in _activities.Reverse<IActivity>())
            {
                if (activity is IMessageActivity)
                {
                    var intro = botToUser.MakeMessage();
                    intro.Text = _header(activity);
                    await botToUser.PostAsync(intro);

                    var act = activity as IMessageActivity;
                    var msg = botToUser.MakeMessage();
                    if (activity.ChannelId == msg.ChannelId)
                    {
                        msg.ChannelData = activity.ChannelData;
                    }
                    msg.AttachmentLayout = act.AttachmentLayout;
                    msg.Attachments = act.Attachments;
                    msg.Entities = act.Entities;
                    msg.Locale = act.Locale;
                    msg.Summary = act.Summary;
                    msg.Text = act.Text;
                    msg.TextFormat = act.TextFormat;
                    await botToUser.PostAsync(msg);
                }
            }
        }
    }
}
