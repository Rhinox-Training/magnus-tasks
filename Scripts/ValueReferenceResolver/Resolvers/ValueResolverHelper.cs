using System;
using Rhinox.Lightspeed.Reflection;
using Rhinox.VOLT.Data;

public static class ValueResolverHelper
{
    public static IValueResolver Create(Type t)
    {
        Type resolverType;
        
        if (t.InheritsFrom(typeof(UnityEngine.Object)))
            resolverType = typeof(UnityValueResolver<>).MakeGenericType(t);
        else
            resolverType = typeof(ConstValueResolver<>).MakeGenericType(t);
        
        return Activator.CreateInstance(resolverType) as IValueResolver;
    }

    public static IValueResolver CreateDefaultResolver(object value)
    {
        if (value == null) return null;
        
        Type valueType = value.GetType();

        if (valueType.InheritsFrom(typeof(UnityEngine.Object)))
        {
            var resolverType = typeof(UnityValueResolver<>).MakeGenericType(valueType);
            // Just using UnityValueResolver<UnityEngine.Object> to get a hardcoded value of the Create method; the result will be generic
            var createMethodInfo = resolverType.GetMethod(nameof(UnityValueResolver<UnityEngine.Object>.Create));
            return (IValueResolver) createMethodInfo.Invoke(null, new[] {value});
        }
        else
        {
            var resolverType = typeof(ConstValueResolver<>).MakeGenericType(valueType);
            var valueSetter = resolverType.GetProperty(nameof(ConstValueResolver<object>.Value));
            IValueResolver resolver = (IValueResolver) Activator.CreateInstance(resolverType);  
            valueSetter.SetValue(resolver, new[] {value});
            return resolver;
        }
    }
}