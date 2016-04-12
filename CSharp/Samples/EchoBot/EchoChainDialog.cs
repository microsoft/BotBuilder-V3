using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Sample.EchoBot
{
    [Serializable]
    public class EchoChainDialog
    {
        public static readonly IDialog<string> dialog = Chain.PostToChain().Select(msg => msg.Text).Switch(
        new Chain.Case<string, IDialog<string>>(text =>
       {
           var regex = new Regex("^reset");
           return regex.Match(text).Success;
       }, (context, txt) =>
       {
           return Chain.From(() => new PromptDialog.PromptConfirm("Are you sure you want to reset the count?",
          "Didn't get that!", 3)).ContinueWith<bool, string>( async (ctx, res) =>
          {
              string reply;
              if (await res)
              {
                  ctx.UserData.SetValue("count", 0);
                  reply = "Reset count.";
              }
              else
              {
                  reply = "Did not reset count.";
              }
              return new Chain.ValueWrapperDialog<string>(reply);
          });
       }),
        new Chain.DefaultCase<string, IDialog<string>>((context, txt) =>
        {
            int count;
            context.UserData.TryGetValue("count", out count);
            context.UserData.SetValue("count", ++count);
            string reply = string.Format("{0}: You said {1}", count, txt);
            return new Chain.ValueWrapperDialog<string>(reply);
        })).Unwrap().PostToUser();
    }
}