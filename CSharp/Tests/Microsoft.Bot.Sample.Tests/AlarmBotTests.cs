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
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Builder.Luis.Models;

using Moq;
using Autofac;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Sample.SimpleAlarmBot;

namespace Microsoft.Bot.Sample.Tests
{
    [TestClass]
    public sealed class AlarmBotTests : LuisTestBase
    {
        [TestMethod]
        public async Task AlarmDialogFlow()
        {
            var luis = new Mock<ILuisService>();

            // arrange
            var now = DateTime.UtcNow;
            var entityTitle = EntityFor(SimpleAlarmDialog.Entity_Alarm_Title, "title");
            var entityDate = EntityFor(SimpleAlarmDialog.Entity_Alarm_Start_Date, now.ToString("d", DateTimeFormatInfo.InvariantInfo));
            var entityTime = EntityFor(SimpleAlarmDialog.Entity_Alarm_Start_Time, now.ToString("t", DateTimeFormatInfo.InvariantInfo));

            Func<IDialog<object>> MakeRoot = () => new SimpleAlarmDialog(luis.Object);
            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(luis.Object))
            using (var container = Build(Options.ScopedQueue, luis.Object))
            {
                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    // arrange
                    SetupLuis<SimpleAlarmDialog>(luis, a => a.SetAlarm(null, null), 1.0, entityTitle, entityDate, entityTime);

                    // act
                    await task.PostAsync(toBot, CancellationToken.None);

                    // assert
                    luis.VerifyAll();
                    AssertMentions("created", scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    // arrange
                    SetupLuis<SimpleAlarmDialog>(luis, a => a.FindAlarm(null, null), 1.0, entityTitle);

                    // act
                    await task.PostAsync(toBot, CancellationToken.None);

                    // assert
                    luis.VerifyAll();
                    AssertMentions("found", scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    // arrange
                    SetupLuis<SimpleAlarmDialog>(luis, a => a.AlarmSnooze(null, null), 1.0, entityTitle);

                    // act
                    await task.PostAsync(toBot, CancellationToken.None);

                    // assert
                    luis.VerifyAll();
                    AssertMentions("snoozed", scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    // arrange
                    SetupLuis<SimpleAlarmDialog>(luis, a => a.TurnOffAlarm(null, null), 1.0, entityTitle);

                    // act
                    await task.PostAsync(toBot, CancellationToken.None);

                    // assert
                    luis.VerifyAll();
                    AssertMentions("sure", scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    // arrange
                    toBot.Text = "blah";

                    // act
                    await task.PostAsync(toBot, CancellationToken.None);

                    // assert
                    luis.VerifyAll();
                    AssertMentions("sure", scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    // arrange
                    toBot.Text = "yes";

                    // act
                    await task.PostAsync(toBot, CancellationToken.None);

                    // assert
                    luis.VerifyAll();
                    AssertMentions("disabled", scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    // arrange
                    SetupLuis<SimpleAlarmDialog>(luis, a => a.DeleteAlarm(null, null), 1.0, entityTitle);

                    // act
                    await task.PostAsync(toBot, CancellationToken.None);

                    // assert
                    luis.VerifyAll();
                    AssertMentions("did not find", scope);
                }
            }
        }
    }
}
