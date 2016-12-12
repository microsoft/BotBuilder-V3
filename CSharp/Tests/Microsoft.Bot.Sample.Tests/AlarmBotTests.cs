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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Tests;

using Moq;
using Autofac;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Sample.AlarmBot.Models;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Sample.AlarmBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Sample.Tests
{
    [TestClass]
    public sealed class AlarmBotTests_Luis : AlarmBotTest_Shared
    {
        public override void Customize(ContainerBuilder builder)
        {
            builder.RegisterType<AlarmLuisDialog>().As<IDialog<object>>().InstancePerDependency();
        }
    }

    [TestClass]
    public sealed class AlarmBotTests_Dispatch : AlarmBotTest_Shared
    {
        public override void Customize(ContainerBuilder builder)
        {
            builder.RegisterType<AlarmDispatchDialog>().As<IDialog<object>>().InstancePerDependency();
        }
    }

    public abstract class AlarmBotTest_Shared : LuisTestBase
    {
        private sealed class TestAlarmScheduler : IAlarmScheduler
        {
            private readonly ObservableCollection<IAlarmable> alarms = new ObservableCollection<IAlarmable>();
            ObservableCollection<IAlarmable> IAlarmScheduler.Alarms
            {
                get
                {
                    return alarms;
                }
            }
        }

        public abstract void Customize(ContainerBuilder builder);

        [TestMethod]
        public async Task AlarmDialog_Flow()
        {
            var luis = new Mock<ILuisService>(MockBehavior.Strict);
            var clock = new Mock<IClock>(MockBehavior.Strict);

            var now = new DateTime(2016, 08, 05, 15, 0, 0);
            clock.SetupGet(c => c.Now).Returns(now);

            var title = "title";
            var when = new DateTime(2016, 08, 05, 16, 0, 0);

            var entityTitle = EntityFor(AlarmBot.Dialogs.BuiltIn.Alarm.Title, title);
            var entityState = EntityFor(AlarmBot.Dialogs.BuiltIn.Alarm.Alarm_State, "on");
            var entityDate = EntityForDate(AlarmBot.Dialogs.BuiltIn.Alarm.Start_Date, when);
            var entityTime = EntityForTime(AlarmBot.Dialogs.BuiltIn.Alarm.Start_Time, when);

            SetupLuis<AlarmLuisDialog>(luis, "can you set an alarm for 4 PM", d => d.SetAlarm(null, null, null), 1.0, entityTitle, entityDate, entityTime);
            SetupLuis<AlarmLuisDialog>(luis, "can you turn off my alarm", d => d.TurnOffAlarm(null, null), 1.0, entityTitle);
            SetupLuis<AlarmLuisDialog>(luis, "can you turn on my alarm", d => d.SetAlarm(null, null, null), 1.0, entityTitle, entityState);
            SetupLuis<AlarmLuisDialog>(luis, "can you snooze my alarm", d => d.AlarmSnooze(null, null), 1.0, entityTitle);
            SetupLuis<AlarmLuisDialog>(luis, "can you delete my alarm", d => d.DeleteAlarm(null, null), 1.0, entityTitle);
            SetupLuis<AlarmLuisDialog>(luis, "how much time is remaining", d => d.AlarmOther(null, null), 1.0);
            SetupLuis<AlarmLuisDialog>(luis, "i would like a pony", d => d.None(null, null), 1.0);

            using (new FiberTestBase.ResolveMoqAssembly(luis.Object, clock.Object))
            using (var container = Build(Options.ResolveDialogFromContainer, luis.Object))
            {
                var builder = new ContainerBuilder();
                builder.RegisterModule(new AlarmModule());
                builder.RegisterType<TestAlarmScheduler>().Keyed<IAlarmScheduler>(FiberModule.Key_DoNotSerialize).AsImplementedInterfaces().SingleInstance();
                builder.Register(c => clock.Object).Keyed<IClock>(FiberModule.Key_DoNotSerialize).As<IClock>().SingleInstance();
                builder.Register(c => luis.Object).Keyed<ILuisService>(FiberModule.Key_DoNotSerialize).As<ILuisService>().SingleInstance();
                Customize(builder);
                builder.Update(container);

                var scheduler = container.Resolve<IAlarmScheduler>();
                Assert.AreEqual(0, scheduler.Alarms.Count);

                var toBot = MakeTestMessage();

                var token = CancellationToken.None;

                toBot.Text = "can you set an alarm for 4 PM";
                await PostActivityAsync(container, toBot, token);

                {
                    Assert.AreEqual(1, scheduler.Alarms.Count);
                    var alarm = (Alarm)scheduler.Alarms.Single();
                    Assert.AreEqual(when, alarm.When);
                    Assert.AreEqual(true, alarm.State);
                    Assert.AreEqual(title, alarm.Title);
                }

                toBot.Text = "can you turn off my alarm";
                await PostActivityAsync(container, toBot, token);

                {
                    Assert.AreEqual(1, scheduler.Alarms.Count);
                    var alarm = (Alarm)scheduler.Alarms.Single();
                    Assert.AreEqual(when, alarm.When);
                    Assert.AreEqual(false, alarm.State);
                    Assert.AreEqual(title, alarm.Title);
                }

                toBot.Text = "can you turn on my alarm";
                await PostActivityAsync(container, toBot, token);

                {
                    Assert.AreEqual(1, scheduler.Alarms.Count);
                    var alarm = (Alarm)scheduler.Alarms.Single();
                    Assert.AreEqual(when, alarm.When);
                    Assert.AreEqual(true, alarm.State);
                    Assert.AreEqual(title, alarm.Title);
                }

                toBot.Text = "how much time is remaining";
                await PostActivityAsync(container, toBot, token);

                {
                    Assert.AreEqual(1, scheduler.Alarms.Count);
                    var alarm = (Alarm)scheduler.Alarms.Single();
                    Assert.AreEqual(when, alarm.When);
                    Assert.AreEqual(true, alarm.State);
                    Assert.AreEqual(title, alarm.Title);
                }

                toBot.Text = "i would like a pony";
                await PostActivityAsync(container, toBot, token);

                {
                    Assert.AreEqual(1, scheduler.Alarms.Count);
                    var alarm = (Alarm)scheduler.Alarms.Single();
                    Assert.AreEqual(when, alarm.When);
                    Assert.AreEqual(true, alarm.State);
                    Assert.AreEqual(title, alarm.Title);
                }
                
                toBot.Text = "can you snooze my alarm";
                await PostActivityAsync(container, toBot, token);

                {
                    Assert.AreEqual(1, scheduler.Alarms.Count);
                    var alarm = (Alarm)scheduler.Alarms.Single();
                    Assert.AreEqual(when.AddMinutes(1), alarm.When);
                    Assert.AreEqual(true, alarm.State);
                    Assert.AreEqual(title, alarm.Title);
                }

                toBot.Text = "can you delete my alarm";
                await PostActivityAsync(container, toBot, token);

                {
                    Assert.AreEqual(0, scheduler.Alarms.Count);
                }
            }

            // verify we're actually calling the LUIS mock and not the actual LUIS service
            luis.VerifyAll();
        }
    }
}
