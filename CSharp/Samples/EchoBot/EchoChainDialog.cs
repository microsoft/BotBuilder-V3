using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.EchoBot
{
    [Serializable]
    public class EchoChainDialog
    {
        public static readonly IDialog<Connector.Message> dialog = Chain.PostToChain().Select(msg => msg.Text).Switch(
        new Chain.Case<string, IDialog<Connector.Message>>(text =>
       {
           var regex = new Regex("^reset");
           return regex.Match(text).Success;
       }, (context, txt) =>
       {
           return Chain.From(() => new PromptDialog.PromptConfirm("Are you sure you want to reset the count?",
          "Didn't get that!", 3)).ContinueWith<bool, Connector.Message>( async (ctx, res) =>
          {
              var reply = ctx.MakeMessage();
              if (await res)
              {
                  ctx.UserData.SetValue("count", 0);
                  reply.Text = "Reset count.";
              }
              else
              {
                  reply.Text = "Did not reset count.";
              }
              return new MessageWrapperDialog(reply);
          });
       }),
        new Chain.DefaultCase<string, IDialog<Connector.Message>>((context, txt) =>
        {
            int count;
            context.UserData.TryGetValue("count", out count);
            context.UserData.SetValue("count", ++count);
            var reply = context.MakeMessage();
            reply.Text = string.Format("{0}: You said {1}", count, txt);
            return new MessageWrapperDialog(reply); 
        })).Unwrap().PostToUser();
    }

    [Serializable]
    public class MessageWrapperDialog : IDialog<Connector.Message>
    {
        [Serializable]
        protected class PartialMessage
        {
            public string Text { set; get; }
        }

        protected readonly PartialMessage Message; 

        public MessageWrapperDialog(Connector.Message message)
        {
            this.Message = new PartialMessage
            {
                Text = message.Text
            };
        }

        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Text = this.Message.Text;
            context.Done<Connector.Message>(reply);
        }
    }
}