using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Internals.Fibers
{
    public static partial class Pair
    {
        public static Pair<T1, T2> Create<T1, T2>(T1 one, T2 two)
        where T1 : IEquatable<T1>, IComparable<T1>
        where T2 : IEquatable<T2>, IComparable<T2>
        {
            return new Pair<T1, T2>(one, two);
        }
    }

    public struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>, IComparable<Pair<T1, T2>>
        where T1 : IEquatable<T1>, IComparable<T1>
        where T2 : IEquatable<T2>, IComparable<T2>
    {
        public Pair(T1 one, T2 two)
        {
            this.One = one;
            this.Two = two;
        }

        public T1 One { get; }
        public T2 Two { get; }

        public int CompareTo(Pair<T1, T2> other)
        {
            var compare = this.One.CompareTo(other.One);
            if (compare != 0)
            {
                return compare;
            }

            return this.Two.CompareTo(other.Two);
        }

        public bool Equals(Pair<T1, T2> other)
        {
            return object.Equals(this.One, other.One)
                && object.Equals(this.Two, other.Two);
        }

        public override bool Equals(object other)
        {
            return other is Pair<T1, T2> && Equals((Pair<T1, T2>)other);
        }

        public override int GetHashCode()
        {
            return this.One.GetHashCode() ^ this.Two.GetHashCode();
        }
    }
}
