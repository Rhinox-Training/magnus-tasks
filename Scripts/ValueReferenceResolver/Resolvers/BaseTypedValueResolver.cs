using System;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus.Tasks
{
    [Serializable]
    public abstract class BaseValueResolver : IValueResolver
    {
        public abstract string SimpleName { get; }
        public virtual string ComplexName => null;
        public abstract Type GetTargetType();
        public abstract bool TryResolve(ref object value);

        public abstract bool Equals(IValueResolver other);
    }
    

    [Serializable, RefactoringOldNamespace("", "com.rhinox.volt.domain")]
    public abstract class BaseTypedValueResolver<T> : BaseValueResolver, IValueResolver<T>
    {
        public abstract bool TryResolveGeneric(ref T value);

        public override bool TryResolve(ref object value)
        {
            T typedValue = default;
            if (!TryResolveGeneric(ref typedValue))
                return false;
            value = typedValue;
            return true;
        }
        
        public override Type GetTargetType() => typeof(T);
    }
}