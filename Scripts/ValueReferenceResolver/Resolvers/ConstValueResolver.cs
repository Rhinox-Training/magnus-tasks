using System;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;

[Serializable]
public class GenericValueResolver : ConstValueResolver<object>
{
}

[Serializable]
public class ConstValueResolver<T> : BaseValueResolver<T>
{
    public override string SimpleName => "Constant";
    public override string ComplexName => $"Constant Value of Type '{typeof(T).Name}'";

    [HideLabel]
    public T Value;

    public override bool TryResolve(ref T value)
    {
        value = Value;
        return true;
    }

    public override bool Equals(IValueResolver other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != this.GetType()) return false;
        return Equals((ConstValueResolver<T>) other);
    }

    protected bool Equals(ConstValueResolver<T> other)
    {
        return Equals(Value, other.Value);
    }
}



