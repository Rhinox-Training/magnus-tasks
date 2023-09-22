using System;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus.Tasks
{
    [HideReferenceObjectPicker]
    public interface IValueResolver : IEquatable<IValueResolver>
    {
        string SimpleName { get; }
        string ComplexName { get; }

        Type GetTargetType();
        bool TryResolve(ref object value);
    }

    public interface IValueResolver<T> : IValueResolver
    {
        bool TryResolve(ref T value);
    }
}