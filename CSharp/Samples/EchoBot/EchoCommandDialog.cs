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
        public static readonly CommandDialog Instance =
            new CommandDialog()
                .On(new Regex("^reset$", RegexOptions.IgnoreCase | RegexOptions.Compiled), async (context) =>
                {
                    return await Prompts.Confirm(context, "Are you sure you want to reset the count?");
                }, async (context, response) =>
                {
                    var confirmation = await response;
                    if (confirmation)
                    {
                        await context.PostMessageAsync("Count reset!");
                    }
                    else
                    {
                        await context.PostMessageAsync("ok");
                    }
                })
                .OnDefault(async (context) =>
                {
                    var count = Convert.ToInt32(session.Stack.GetLocal("count"));
                    session.Stack.SetLocal("count", count + 1);
                    return await session.CreateDialogResponse(string.Format("{0} : I heard {1}", count, session.Message.Text));
                });
    }
}