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

using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// The azure bot utilities and helpers.
    /// </summary>
    public static class AzureBot
    {
        /// <summary>
        /// The bot authenticator.
        /// </summary>
        public static BotAuthenticator Authenticator => authenticator.Value;
        
        /// <summary>
        /// Update the <see cref="Conversation.Container"/> for azure bots.
        /// </summary>
        public static void Initialize()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AzureModule());
            builder.Update(Conversation.Container);
        }
        
        internal static readonly Lazy<string> stateApi = new Lazy<string>(() => Utils.GetStateApiUrl());

        private static readonly Lazy<BotAuthenticator> authenticator = new Lazy<BotAuthenticator>(() => new BotAuthenticator(new StaticCredentialProvider(Utils.GetAppSetting(AppSettingKeys.AppId), Utils.GetAppSetting(AppSettingKeys.Password)),
            Utils.GetOpenIdConfigurationUrl(), false));
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
