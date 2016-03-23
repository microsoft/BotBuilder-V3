using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public static partial class Field
    {
        public static void SetNotNull<T>(out T field, string name, T value) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            field = value;
        }

        public static void SetNotNullFrom<T>(out T field, string name, SerializationInfo info) where T : class
        {
            var value = (T)info.GetValue(name, typeof(T));
            Field.SetNotNull(out field, name, value);
        }
        public static void SetFrom<T>(out T field, string name, SerializationInfo info) where T : struct
        {
            var value = (T)info.GetValue(name, typeof(T));
            field = value;
        }
    }

    public static partial class Extensions
    {
        public static V GetOrAdd<K, V>(this Dictionary<K, V> valueByKey, K key) where V : new()
        {
            V value;
            if (!valueByKey.TryGetValue(key, out value))
            {
                value = new V();
                valueByKey.Add(key, value);
            }

            return value;
        }

        public static Task<T> FromException<T>(this TaskFactory<T> factory, Exception error)
        {
            var source = new TaskCompletionSource<T>();
            source.SetException(error);
            return source.Task;
        }
    }

    public static partial class Tasks
    {
        public static readonly Task<object> Cancelled;
        public static readonly Task<object> Null;

        static Tasks()
        {
            {
                var source = new TaskCompletionSource<object>();
                source.SetCanceled();
                Cancelled = source.Task;
            }

            {
                var source = new TaskCompletionSource<object>();
                source.SetResult(null);
                Null = source.Task;
            }
        }

        public static Task<R> Cast<T, R>(Task<T> task) where R : T
        {
            var source = new TaskCompletionSource<R>();
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    source.SetResult((R)task.Result);
                    break;
                case TaskStatus.Canceled:
                    source.SetCanceled();
                    break;
                case TaskStatus.Faulted:
                    var aggregate = task.Exception;
                    if (aggregate.InnerExceptions != null)
                    {
                        source.SetException(aggregate.InnerExceptions);
                    }
                    else
                    {
                        source.SetException(aggregate.InnerException);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return source.Task;
        }

        private static readonly Expression<Func<Task>> ExpressionCast = () => Cast<object, object>(null);
        private static readonly MethodInfo MethodCast = ((MethodCallExpression)ExpressionCast.Body).Method.GetGenericMethodDefinition();

        public static Task Cast<T>(Task<T> source, Type targetType)
        {
            var method = MethodCast.MakeGenericMethod(typeof(T), targetType);
            var invoke = (Func<Task<T>, Task>)method.CreateDelegate(typeof(Func<Task<T>, Task>));
            return invoke(source);
        }

        // http://blogs.msdn.com/b/pfxteam/archive/2011/01/13/10115642.aspx
        public struct NoThrowAwaitable<T>
        {
            private readonly Task<T> _task;

            public NoThrowAwaitable(Task<T> task)
            {
                _task = task;
            }

            public NoThrowAwaiter GetAwaiter()
            {
                return new NoThrowAwaiter(_task);
            }

            public struct NoThrowAwaiter : INotifyCompletion
            {
                private readonly Task<T> _task;
                private readonly TaskAwaiter<T> _awaiter;

                public NoThrowAwaiter(Task<T> task)
                {
                    _task = task;
                    _awaiter = task.GetAwaiter();
                }

                public bool IsCompleted { get { return _awaiter.IsCompleted; } }

                public void OnCompleted(Action continuation)
                {
                    _awaiter.OnCompleted(continuation);
                }

                public T GetResult()
                {
                    return (_task.Status == TaskStatus.RanToCompletion) ? _awaiter.GetResult() : default(T);
                }
            }
        }

        public static NoThrowAwaitable<T> WithNoThrow<T>(this Task<T> task)
        {
            return new NoThrowAwaitable<T>(task);
        }
    }
}
