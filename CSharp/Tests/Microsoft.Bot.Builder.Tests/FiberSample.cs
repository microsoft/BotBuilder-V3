using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Message = Microsoft.Bot.Connector.Message;
using Microsoft.Bot.Builder.Fibers;

namespace Microsoft.Bot.Builder.Tests
{
#pragma warning disable CS1998

    [TestClass]
    public sealed class FiberSample
    {
        [Serializable]
        public sealed class Parent
        {
            public static readonly Parent Instance = new Parent();
            private Parent()
            {
            }

            public async Task<IWait> StartAsync(IFiber fiber, IItem<object> arguments)
            {
                //Rest<Connector.Message> rest = MessageReceivedOne;
                //return fiber.Wait(rest);
                //return fiber.Wait(MessageReceivedOne);

                return fiber.Wait<Message>(MessageReceivedOne);
            }

            private async Task<IWait> MessageReceivedOne(IFiber fiber, IItem<Message> message)
            {
                var child = new Child();
                return fiber.Call<int, string>(child.StartAsync, 3, ChildFinished);
            }

            private async Task<IWait> ChildFinished(IFiber fiber, IItem<string> message)
            {
                return fiber.Wait<Message>(Cleanup);
            }

            private async Task<IWait> Cleanup(IFiber fiber, IItem<Message> message)
            {
                return fiber.Done("bye");
            }
        }

        [Serializable]
        public sealed class Child
        {
            private int count;

            public async Task<IWait> StartAsync(IFiber fiber, IItem<int> arguments)
            {
                this.count = await arguments;
                return fiber.Wait<Message>(MessageReceived);
            }

            private async Task<IWait> MessageReceived(IFiber fiber, IItem<Message> message)
            {
                --this.count;

                if (this.count > 0)
                {
                    return fiber.Wait<Message>(MessageReceived);
                }
                else
                {
                    return fiber.Done("hello");
                }
            }
        }

        [TestMethod]
        public async Task FiberAsync()
        {
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));

            FiberTests.AssertSerializable(ref fiber);

            const object Argument = null;
            //var method = Methods.Loop(Parent.Instance.StartAsync, 1);
            var method = Methods.Void<object>(Parent.Instance.StartAsync);
            //Rest<object> method = Parent.Instance.StartAsync;
            fiber.Call(method, Argument);
            Assert.AreEqual(Need.Wait, await fiber.PollAsync());

            FiberTests.AssertSerializable(ref fiber);

            fiber.Post(new Message());
            await FiberTestBase.PollAsync(fiber);
        }
    }
}
