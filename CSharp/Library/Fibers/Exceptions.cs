using System;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Fibers
{
    [Serializable]
    public abstract class InvalidWaitException : InvalidOperationException
    {
        private readonly IWait wait;

        public IWait Wait { get { return this.wait; } }

        protected InvalidWaitException(string message, IWait wait)
            : base(message)
        {
            Field.SetNotNull(out this.wait, nameof(wait), wait);
        }
        protected InvalidWaitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Field.SetNotNullFrom(out this.wait, nameof(this.wait), info);
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.wait), wait);
        }
    }

    [Serializable]
    public sealed class InvalidNeedException : InvalidWaitException
    {
        private readonly Need need;
        public Need Need { get { return this.need; } }

        public InvalidNeedException(IWait wait, Need need)
            : base($"invalid need: expected {need}, have {wait.Need}", wait)
        {
        }
        private InvalidNeedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.need = (Need) info.GetValue(nameof(this.need), typeof(Need));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.need), need);
        }
    }

    [Serializable]
    public sealed class InvalidTypeException : InvalidWaitException
    {
        private readonly Type type;

        public InvalidTypeException(IWait wait, Type type)
            : base($"invalid type: expected {wait.ItemType}, have {type.Name}", wait)
        {
            Field.SetNotNull(out this.type, nameof(type), type);
        }
        private InvalidTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Field.SetNotNullFrom(out this.type, nameof(this.type), info);
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.type), type);
        }
    }

    [Serializable]
    public sealed class InvalidNextException : InvalidWaitException
    {
        public InvalidNextException(IWait wait)
            : base($"invalid next: {wait}", wait)
        {
        }
        private InvalidNextException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
