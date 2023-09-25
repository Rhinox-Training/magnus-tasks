using System;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus.Tasks
{
    [Serializable, RefactoringOldNamespace("", "com.rhinox.volt.domain")]
    public abstract class BaseValueResolver<T> : IValueResolver<T>
    {
        public abstract string SimpleName { get; }
        public virtual string ComplexName => null;

        public abstract bool TryResolve(ref T value);

        bool IValueResolver.TryResolve(ref object value)
        {
            T typedValue = default;
            if (!TryResolve(ref typedValue))
                return false;
            value = typedValue;
            return true;
        }

        public Type GetTargetType() => typeof(T);

        public abstract bool Equals(IValueResolver other);
    }
}