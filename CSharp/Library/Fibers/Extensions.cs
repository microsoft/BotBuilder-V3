using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Fibers
{
    public static partial class Extensions
    {
        // TODO: split off R to get better type inference on T
        public static IWait Call<T, R>(this IFiber fiber, Rest<T> invokeHandler, T item, Rest<R> returnHandler)
        {
            fiber.NextWait<R>().Wait(returnHandler);
            return fiber.Call<T>(invokeHandler, item);
        }

        public static IWait Call<T>(this IFiber fiber, Rest<T> invokeHandler, T item)
        {
            fiber.Push();
            var wait = fiber.NextWait<T>();
            wait.Wait(invokeHandler);
            wait.Post(item);
            return wait;
        }

        public static IWait Wait<T>(this IFiber fiber, Rest<T> resumeHandler)
        {
            var wait = fiber.NextWait<T>();
            wait.Wait(resumeHandler);
            return wait;
        }

        public static IWait Done<T>(this IFiber fiber, T item)
        {
            fiber.Done();
            var wait = fiber.Wait;
            wait.Post(item);
            return wait;
        }

        public static void Post<T>(this IFiber fiber, T item)
        {
            fiber.Wait.Post(item);
        }

        public static Task<T> ToTask<T>(this IAwaitable<T> item)
        {
            var source = new TaskCompletionSource<T>();
            try
            {
                var result = item.GetAwaiter().GetResult();
                source.SetResult(result);
            }
            catch (Exception error)
            {
                source.SetException(error);
            }

            return source.Task;
        }
    }
}
