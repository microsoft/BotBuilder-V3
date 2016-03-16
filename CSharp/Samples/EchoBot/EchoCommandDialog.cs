using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Sample.EchoBot
{
    public static class EchoCommandDialog
    {
        public static readonly CommandDialog<string, PromptDialogResult<bool>> Instance =
            new CommandDialog<string, PromptDialogResult<bool>>("echoCmd")
                .On(new Regex("^reset$", RegexOptions.IgnoreCase | RegexOptions.Compiled), async (session) =>
                {
                    return await Prompts.Confirm(session, "Are you sure you want to reset the count?");
                }, async (session, res) =>
                {
                    var promptRes = res as PromptDialogResult<bool>;
                    if (promptRes.Completed && promptRes.Response)
                    {
                        session.Stack.SetDialogState("count", 0);
                        return await session.CreateDialogResponse("Count reset!");
                    }
                    else
                    {
                        return await session.CreateDialogResponse("ok");
                    }
                })
                .OnDefault(async (session) =>
                {
                    var count = Convert.ToInt32(session.Stack.GetDialogState("count"));
                    session.Stack.SetDialogState("count", count + 1);
                    return await session.CreateDialogResponse(string.Format("{0} : I heard {1}", count, session.Message.Text));
                });
    }
}