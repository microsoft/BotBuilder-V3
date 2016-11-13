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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// The top level entry point of the conversation
    /// </summary>
    public static class Conversation
    {
        /// <summary>
        /// The conversation container. <see cref="ConversationModule"/> for more detail.
        /// </summary>
        public static readonly IContainer Container;

        /// <summary>
        /// The bot authenticator.
        /// </summary>
        public static BotAuthenticator Authenticator => authenticator.Value;

        
        static Conversation()
        {
            Container = Dialogs.Conversation.Container;
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConversationModule());
            builder.Update(Container);
        }

        /// <summary>
        /// Process an incoming message within the conversation.
        /// </summary>
        /// <remarks>
        /// This method:
        /// 1. Update the required components in the container created by <see cref="Dialogs.Conversation"/>
        /// 2. Sends the <paramref name="toBot"/> to <see cref="Dialogs.Conversation"/>.
        /// 
        /// The <paramref name="MakeRoot"/> factory method is invoked for new conversations only,
        /// because existing conversations have the dialog stack and state serialized in the <see cref="IMessageActivity"/> data.
        /// </remarks>
        /// <param name="toBot">The message sent to the <see cref="Dialogs.Conversation"/></param>
        /// <param name="MakeRoot">The factory method to make the root dialog.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that represents the message to send inline back to the user.</returns>
        public static async Task SendAsync(IMessageActivity toBot, Func<IDialog<object>> MakeRoot, CancellationToken token = default(CancellationToken))
        {   
            UpdateContainer(toBot);
            await Dialogs.Conversation.SendAsync(toBot, MakeRoot, token);
        }

        /// <summary>
        /// Resume a conversation and post the data to the dialog waiting.
        /// </summary>
        /// <typeparam name="T"> Type of the data. </typeparam>
        /// <param name="resumptionCookie"> The id of the bot.</param>
        /// <param name="toBot"> The data sent to bot.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns> A task that represent the message to send back to the user after resumption of the conversation.</returns>
        public static async Task ResumeAsync<T>(ResumptionCookie resumptionCookie, T toBot,
            CancellationToken token = default(CancellationToken))
        {
            UpdateContainer(resumptionCookie.GetMessage());
            await Dialogs.Conversation.ResumeAsync(resumptionCookie, toBot, token);
        }


        private static readonly Lazy<BotAuthenticator> authenticator = new Lazy<BotAuthenticator>(() => new BotAuthenticator(new StaticCredentialProvider(Utils.GetAppSetting(AppSettingKeys.AppId), Utils.GetAppSetting(AppSettingKeys.Password)),
            Utils.GetOpenIdConfigurationUrl(), false));

        private static readonly Lazy<string> stateApi = new Lazy<string>(() => Utils.GetStateApiUrl());

        private static void UpdateContainer(IActivity activity)
        {
            var builder = new ContainerBuilder();

            builder.Register(c =>
            {
                if (activity.ChannelId == "emulator")
                {
                    // for emulator we should use serviceUri of the emulator for storage
                    return new StateClient(new Uri(activity.ServiceUrl));
                }
                else
                {
                    MicrosoftAppCredentials.TrustServiceUrl(stateApi.Value, DateTime.MaxValue);
                    return new StateClient(new Uri(stateApi.Value));
                }

            })
            .As<IStateClient>()
            .InstancePerLifetimeScope();
            builder.Update(Container);
        }
    }


    /// <summary>
    /// A helper class responsible for resolving the calling assembly
    /// </summary>
    public sealed class ResolveCallingAssembly : IDisposable
    {
        private readonly Assembly assembly;

        /// <summary>
        /// Creates and instance of ResovelCallingAssembly
        /// </summary>
        public ResolveCallingAssembly()
        {
            this.assembly = Assembly.GetCallingAssembly();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        void IDisposable.Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs arguments)
        {
            if (arguments.Name == this.assembly.FullName)
            {
                return this.assembly;
            }

            return null;
        }
    }
}
