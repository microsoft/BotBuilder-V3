using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.EchoBot
{
    public static class EchoCommandDialog
    {
        public static readonly CommandDialog<string, bool> Instance =
            new CommandDialog<string, bool>("echoCmd")
                .On(new Regex("^reset$", RegexOptions.IgnoreCase | RegexOptions.Compiled), async (session) =>
                {
                    return await Prompts.Confirm(session, "Are you sure you want to reset the count?");
                }, async (session, taskResult) =>
                {
                    if (taskResult.Status == TaskStatus.RanToCompletion)
                    {
                        var response = await taskResult;
                        if (response)
                        {
                            session.Stack.SetLocal("count", 0);
                            return await session.CreateDialogResponse("Count reset!");
                        }
                    }

                    return await session.CreateDialogResponse("ok");
                })
                .OnDefault(async (session) =>
                {
                    var count = Convert.ToInt32(session.Stack.GetLocal("count"));
                    session.Stack.SetLocal("count", count + 1);
                    return await session.CreateDialogResponse(string.Format("{0} : I heard {1}", count, session.Message.Text));
                });
    }
}