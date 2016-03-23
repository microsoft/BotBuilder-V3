using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Bot.Builder.Fibers;
using System.IO;
using Moq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Tests
{
#pragma warning disable CS1998

    [TestClass]
    public abstract class FiberTestBase
    {
        public interface IMethod
        {
            Task<IWait> CodeAsync<T>(IFiber fiber, IAwaitable<T> item);
        }

        public static Moq.Mock<IMethod> MockMethod()
        {
            var method = new Moq.Mock<IMethod>(Moq.MockBehavior.Loose);
            return method;
        }

        public static Expression<Func<IAwaitable<T>, bool>> Item<T>(T value)
        {
            return item => value.Equals(item.GetAwaiter().GetResult());
        }

        protected sealed class CodeException : Exception
        {
        }

        public static bool ExceptionOfType<T, E>(IAwaitable<T> item) where E : Exception
        {
            try
            {
                item.GetAwaiter().GetResult();
                return false;
            }
            catch (E)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Expression<Func<IAwaitable<T>, bool>> ExceptionOfType<T, E>() where E : Exception
        {
            return item => ExceptionOfType<T, E>(item);
        }

        public static async Task PollAsync(IFiberLoop fiber)
        {
            IWait wait;
            do
            {
                wait = await fiber.PollAsync();
            }
            while (wait.Need != Need.None && wait.Need != Need.Done);
        }

        public static void AssertSerializable<T>(ref T item, params object[] instances) where T : class
        {
            var formatter = Serialization.MakeBinaryFormatter(instances);

            //var surrogate = new Surrogate();
            //var selector = new SurrogateSelector();
            //selector.AddSurrogate(typeof(FrameFactory), new StreamingContext(StreamingContextStates.All), surrogate);
            //formatter.SurrogateSelector = selector;

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, item);
                stream.Position = 0;
                item = (T)formatter.Deserialize(stream);
            }
        }
    }

    [TestClass]
    public sealed class FiberTests : FiberTestBase
    {
        [TestMethod]
        public async Task Fiber_Is_Serializable()
        {
            // arrange
            var waits = new WaitFactory();
            var frames = new FrameFactory(waits);
            var fiber = new Fiber(frames);

            // assert
            var previous = fiber;
            AssertSerializable(ref fiber, waits, frames);
            Assert.IsFalse(object.ReferenceEquals(previous, fiber));
            Assert.IsTrue(object.ReferenceEquals(previous.Factory, fiber.Factory));
        }

        [TestMethod]
        public async Task Fiber_With_Wait_Is_Serializable()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var method = MockMethod();
            var value = 42;
            method
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value))))
                .ReturnsAsync(NullWait.Instance);

            // act
            fiber.Call(method.Object.CodeAsync, value);

            // assert
            AssertSerializable(ref fiber);
            var next = await fiber.PollAsync();
            Assert.AreEqual(Need.None, next.Need);
        }


        [TestMethod]
        public async Task Fiber_NoCall_NeedNone()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));

            // assert
            var next = await fiber.PollAsync();
            Assert.AreEqual(Need.None, next.Need);
        }

        [TestMethod]
        public async Task Fiber_OneCall_NeedDone()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var method = MockMethod();
            var value = 42;
            method
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value))))
                .ReturnsAsync(NullWait.Instance);

            // act
            fiber.Call(method.Object.CodeAsync, value);

            // assert
            var next = await fiber.PollAsync();
            Assert.AreEqual(Need.Done, next.Need);
            method.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Fiber_OneCall_ThenDone_Throws()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var method = MockMethod();
            method
                .Setup(m => m.CodeAsync(fiber, It.IsAny<IAwaitable<int>>()))
                .Returns(async () => { return fiber.Done(42); });

            // act
            fiber.Call(method.Object.CodeAsync, 42);
            await PollAsync(fiber);

            // assert
            method.VerifyAll();
        }

        [TestMethod]
        public async Task Code_Is_Called()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var method = MockMethod();
            var value = 42;
            method
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value))))
                .ReturnsAsync(NullWait.Instance);

            // act
            fiber.Call(method.Object.CodeAsync, value);
            await PollAsync(fiber);

            // assert
            method.VerifyAll();
        }

        [TestMethod]
        public async Task Code_Call_Code()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var method = MockMethod();
            var valueOne = 42;
            var valueTwo = "hello world";
            method
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(valueOne))))
                .Returns(async () => { return fiber.Call(method.Object.CodeAsync, valueTwo); });
            method
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(valueTwo))))
                .ReturnsAsync(NullWait.Instance);

            // act
            fiber.Call(method.Object.CodeAsync, valueOne);
            await PollAsync(fiber);

            // assert
            method.VerifyAll();
        }

        [TestMethod]
        public async Task Code_Call_Method_With_Return()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var methodOne = MockMethod();
            var methodTwo = MockMethod();
            var value1 = 42;
            var value2 = "hello world";
            var value3 = Guid.NewGuid();
            methodOne
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value1))))
                .Returns(async () => { return fiber.Call<string, Guid>(methodTwo.Object.CodeAsync, value2, methodOne.Object.CodeAsync); });
            methodTwo
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value2))))
                .Returns(async () => { return fiber.Done(value3); });
            methodOne
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value3))))
                .ReturnsAsync(NullWait.Instance);

            // act
            fiber.Call(methodOne.Object.CodeAsync, value1);
            await PollAsync(fiber);

            // assert
            methodOne.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNeedException))]
        public async Task Code_Call_Method_No_Return_Throws()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var methodOne = MockMethod();
            var methodTwo = MockMethod();
            var value1 = 42;
            var value2 = "hello world";
            var value3 = Guid.NewGuid();
            methodOne
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value1))))
                .Returns(async () => { return fiber.Call<string>(methodTwo.Object.CodeAsync, value2); });
            methodTwo
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value2))))
                .Returns(async () => { return fiber.Done(value3); });

            // act
            fiber.Call(methodOne.Object.CodeAsync, value1);
            await PollAsync(fiber);

            // assert
            methodOne.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(CodeException))]
        public async Task Code_Throws_To_User()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var method = MockMethod();
            var value = 42;
            method
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value))))
                .Returns(async () => { throw new CodeException(); });

            // act
            fiber.Call(method.Object.CodeAsync, value);
            await PollAsync(fiber);

            // assert
            method.VerifyAll();
        }

        // TODO: maybe test for unobserved exceptions sent to callers?

        [TestMethod]
        public async Task Code_Call_Method_That_Throws_To_Code()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var methodOne = MockMethod();
            var methodTwo = MockMethod();
            var value1 = 42;
            var value2 = "hello world";
            var value3 = Guid.NewGuid();
            methodOne
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value1))))
                .Returns(async () => { return fiber.Call<string, Guid>(methodTwo.Object.CodeAsync, value2, methodOne.Object.CodeAsync); });
            methodTwo
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value2))))
                .Returns(async () => { throw new CodeException(); });
            methodOne
                .Setup(m => m.CodeAsync(fiber, It.Is(ExceptionOfType<Guid, CodeException>())))
                .ReturnsAsync(NullWait.Instance);

            // act
            fiber.Call(methodOne.Object.CodeAsync, value1);
            await PollAsync(fiber);

            // assert
            methodOne.VerifyAll();
        }

        [TestMethod]
        public async Task Code_Item_Variance()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var method = MockMethod();
            string valueAsString = "hello world";
            object valueAsObject = valueAsString;
            method
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(valueAsObject))))
                .ReturnsAsync(NullWait.Instance);

            // act
            fiber.Call(method.Object.CodeAsync, valueAsString);
            await PollAsync(fiber);

            // assert
            method.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNeedException))]
        public async Task Poll_Is_Not_Reentrant()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var method = MockMethod();
            var value = 42;
            method
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value))))
                .Returns(async () => { await fiber.PollAsync(); return null; });

            // act
            fiber.Call(method.Object.CodeAsync, value);
            await PollAsync(fiber);

            // assert
            method.VerifyAll();
        }

        [TestMethod]
        public async Task Method_Void()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var method = MockMethod();
            var value = "hello world";
            method
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value))))
                .Returns(async () => fiber.Done(42));

            // act
            var loop = Methods.Void<string>(method.Object.CodeAsync);
            fiber.Call(loop, value);
            await PollAsync(fiber);

            // assert
            method.Verify(m => m.CodeAsync(fiber, It.Is(Item(value))), Times.Exactly(1));
        }

        [TestMethod]
        public async Task Method_Loop()
        {
            // arrange
            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var method = MockMethod();
            var value = "hello world";
            method
                .Setup(m => m.CodeAsync(fiber, It.Is(Item(value))))
                .Returns(async () => fiber.Done(42) );

            // act
            const int CallCount = 5;
            var loop = Methods.Void(Methods.Loop<string>(method.Object.CodeAsync, CallCount));
            fiber.Call(loop, value);
            await PollAsync(fiber);

            // assert
            method.Verify(m => m.CodeAsync(fiber, It.Is(Item(value))), Times.Exactly(CallCount));
        }
    }
}
