using System;

namespace Rhinox.Magnus.Tasks
{
    public static class DataSourceHelper
    {
        public static bool TryCreate(object value, Type type, out IMemberDataSource source)
        {
            var genericType = typeof(ConstantDataSource<>).MakeGenericType(type);
            source = UnitySafeActivator.CreateInstance<IMemberDataSource>(genericType);
            if (source != null)
                source.SetValue(value);
            return source != null;
        }
    }
}