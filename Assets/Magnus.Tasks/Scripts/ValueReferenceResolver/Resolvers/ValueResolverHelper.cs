using System;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.Magnus.Tasks
{
    public static class ValueResolverHelper
    {
        public static IValueResolver Create(Type t)
        {
            if (t.InheritsFrom(typeof(UnityEngine.Object)))
            {
                var resolverType = typeof(UnityValueResolver<>).MakeGenericType(t);
                return Activator.CreateInstance(resolverType) as IValueResolver;
            }
            
            return new ConstValueResolver(t);
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
            
            return new ConstValueResolver(valueType, value);
        }
    }
}