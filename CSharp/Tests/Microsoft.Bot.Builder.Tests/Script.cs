using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    class Script : DialogTestBase
    {
        public static async Task RecordScript(ILifetimeScope container,
            bool proactive,
            StreamWriter stream,
            Func<string> extraInfo,
            params string[] inputs)
        {
            var toBot = MakeTestMessage();
            using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
            {
                var task = scope.Resolve<IPostToBot>();
                var queue = scope.Resolve<Queue<IMessageActivity>>();
                Action drain = () =>
                {
                    stream.WriteLine($"{queue.Count()}");
                    while (queue.Count > 0)
                    {
                        var toUser = queue.Dequeue();
                        if (!string.IsNullOrEmpty(toUser.Text))
                        {
                            stream.WriteLine($"ToUserText:{JsonConvert.SerializeObject(toUser.Text)}");
                        }
                        else
                        {
                            stream.WriteLine($"ToUserButtons:{JsonConvert.SerializeObject(toUser.Attachments)}");
                        }
                    }
                };
                string result = null;
                var root = scope.Resolve<IDialog<object>>().Do(async (context, value) =>
                    result = JsonConvert.SerializeObject(await value));
                if (proactive)
                {
                    var loop = root.Loop();
                    var data = scope.Resolve<IBotData>();
                    await data.LoadAsync(CancellationToken.None);
                    var stack = scope.Resolve<IDialogTask>();
                    stack.Call(loop, null);
                    await stack.PollAsync(CancellationToken.None);
                    drain();
                }
                else
                {
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance(root)
                        .AsSelf()
                        .As<IDialog<object>>();
                    builder.Update((IContainer)container);
                }
                foreach (var input in inputs)
                {
                    stream.WriteLine($"FromUser:{JsonConvert.SerializeObject(input)}");
                    toBot.Text = input;
                    try
                    {
                        await task.PostAsync(toBot, CancellationToken.None);
                        drain();
                        if (extraInfo != null)
                        {
                            var extra = extraInfo();
                            stream.WriteLine(extra);
                        }
                    }
                    catch (Exception e)
                    {
                        stream.WriteLine($"Exception:{e.Message}");
                    }
                }
                if (result != null)
                {
                    stream.WriteLine($"Result: {result}");
                }
            }
        }

        public static string ReadLine(StreamReader stream, out string label)
        {
            string line = stream.ReadLine();
            label = null;
            if (line != null)
            {
                int pos = line.IndexOf(':');
                if (pos != -1)
                {
                    label = line.Substring(0, pos);
                    line = line.Substring(pos + 1);
                }
            }
            return line;
        }

        public static async Task VerifyScript(ILifetimeScope container, bool proactive, StreamReader stream, Action<string> extraCheck, string[] expected)
        {
            var toBot = DialogTestBase.MakeTestMessage();
            using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
            {
                var task = scope.Resolve<IPostToBot>();
                var queue = scope.Resolve<Queue<IMessageActivity>>();
                string input, label;
                Action check = () =>
                {
                    var count = int.Parse(stream.ReadLine());
                    Assert.AreEqual(count, queue.Count);
                    for (var i = 0; i < count; ++i)
                    {
                        var toUser = queue.Dequeue();
                        var expectedOut = ReadLine(stream, out label);
                        if (label == "ToUserText")
                        {
                            Assert.AreEqual(expectedOut, JsonConvert.SerializeObject(toUser.Text));
                        }
                        else
                        {
                            Assert.AreEqual(expectedOut, JsonConvert.SerializeObject(toUser.Attachments));
                        }
                    }
                    extraCheck?.Invoke(ReadLine(stream, out label));
                };
                string result = null;
                var root = scope.Resolve<IDialog<object>>().Do(async (context, value) => result = JsonConvert.SerializeObject(await value));
                if (proactive)
                {
                    var loop = root.Loop();
                    var data = scope.Resolve<IBotData>();
                    await data.LoadAsync(CancellationToken.None);
                    var stack = scope.Resolve<IDialogTask>();
                    stack.Call(loop, null);
                    await stack.PollAsync(CancellationToken.None);
                    check();
                }
                else
                {
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance(root)
                        .AsSelf()
                        .As<IDialog<object>>();
                    builder.Update((IContainer)container);
                }
                int current = 0;
                while ((input = ReadLine(stream, out label)) != null)
                {
                    if (input.StartsWith("\""))
                    {
                        input = input.Substring(1, input.Length - 2);
                        Assert.IsTrue(current < expected.Length && input == expected[current++]);
                        toBot.Text = input;
                        try
                        {
                            await task.PostAsync(toBot, CancellationToken.None);
                            check();
                        }
                        catch (Exception e)
                        {
                            Assert.AreEqual(ReadLine(stream, out label), e.Message);
                        }
                    }
                    else if (input.StartsWith("Result:"))
                    {
                        Assert.AreEqual(input.Substring(7), result);
                    }
                }
            }
        }

        public static async Task RecordDialogScript<T>(string filePath, IDialog<T> dialog, bool proactive, params string[] inputs)
        {
            using (var stream = new StreamWriter(filePath))
            using (var container = Build(Options.ResolveDialogFromContainer | Options.Reflection))
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(dialog)
                    .AsSelf()
                    .As<IDialog<object>>();
                builder.Update(container);
                await RecordScript(container, proactive, stream, null, inputs);
            }
        }

        public static string NewScriptPathFor(string pathScriptOld)
        {
            var pathScriptNew = Path.Combine
                (
                Path.GetDirectoryName(pathScriptOld),
                Path.GetFileNameWithoutExtension(pathScriptOld) + "-new" + Path.GetExtension(pathScriptOld)
                );
            return pathScriptNew;
        }

        public static async Task VerifyDialogScript<T>(string filePath, IDialog<T> dialog, bool proactive, params string[] inputs)
        {
            var newPath = NewScriptPathFor(filePath);
            File.Delete(newPath);
            try
            {
                using (var stream = new StreamReader(filePath))
                using (var container = Build(Options.ResolveDialogFromContainer | Options.Reflection))
                {
                    var builder = new ContainerBuilder();
                    builder
                        .RegisterInstance(dialog)
                        .AsSelf()
                        .As<IDialog<object>>();
                    builder.Update(container);
                    await VerifyScript(container, proactive, stream, null, inputs);
                }
            }
            catch (Exception)
            {
                // There was an error, so record new script and pass on error
                await RecordDialogScript(newPath, dialog, proactive, inputs);
                throw;
            }
        }
    }
}
