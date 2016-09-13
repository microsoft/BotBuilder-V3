using Autofac;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Calling
{
    /// <summary>
    /// The calling bot interface.
    /// </summary>
    public interface ICallingBot
    {
        ICallingBotService CallingBotService { get; }
    }

    /// <summary>
    /// The top level composition root for calling SDK.
    /// </summary>
    public static partial class CallingConversation
    {
        public static readonly IContainer Container;

        static CallingConversation()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new CallingModule_MakeBot());
            Container = builder.Build();
        }

        /// <summary>
        /// Register the instance of calling bot responsible for handling the calling requests.
        /// </summary>
        /// <param name="MakeCallingBot"> The factory method to make the calling bot.</param>
        public static void RegisterCallingBot(Func<ICallingBotService, ICallingBot> MakeCallingBot)
        {
            CallingModule_MakeBot.Register(Container, MakeCallingBot);
        }

        /// <summary>
        /// Process a calling request within the calling conversaion.
        /// </summary>
        /// <param name="toBot"> The calling request sent to the bot.</param>
        /// <param name="callRequestType"> The type of calling request.</param>
        /// <returns> The response from the bot.</returns>
        public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage toBot, CallRequestType callRequestType)
        {
            using (var scope = CallingModule.BeginLifetimeScope(Container, toBot))
            {
                var bot = scope.Resolve<ICallingBot>();
                var context = scope.Resolve<CallingContext>();
                var parsedRequest = await context.ProcessRequest(callRequestType);
                Trace.TraceInformation($"Processing X-Microsoft-Skype-Chain-ID: {parsedRequest.SkypeChaindId}");
                if (parsedRequest.Faulted())
                {
                    return context.Request.CreateResponse(parsedRequest.ParseStatusCode);
                }
                else
                {
                    try
                    {
                        string res = string.Empty;
                        switch (callRequestType)
                        {
                            case CallRequestType.IncomingCall:
                                res = await bot.CallingBotService.ProcessIncomingCallAsync(parsedRequest.Content);
                                break;
                            case CallRequestType.CallingEvent:
                                res = await bot.CallingBotService.ProcessCallbackAsync(parsedRequest.Content, parsedRequest.AdditionalData);
                                break;
                            default:
                                throw new Exception($"Unsupported call request type: {callRequestType}");
                        }

                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(res, Encoding.UTF8, "application/json")
                        };
                    }
                    catch (Exception e)
                    {
                        return context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
                    }
                }
            }


        }
    }
}
