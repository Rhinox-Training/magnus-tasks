using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;

[Serializable]
public struct ValueReferenceFieldData
{
    public SerializableFieldInfo Field;
    public string DefaultKey;
    public SerializableType ReferenceKeyType;
    public string ImportMemberTarget;

    public IValueResolver FindImportData(object instance)
    {
        if (string.IsNullOrWhiteSpace(ImportMemberTarget))
            return null;

        MemberInfo importMemberSource = Field.DeclaringType
            .GetMember(ImportMemberTarget, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(x => !(x is MethodInfo mi) || mi.GetParameters().Length == 0);

        object result;
        
        if (importMemberSource is MethodInfo methodSource)
            result = methodSource.Invoke(instance, null);
        else
            result = importMemberSource.GetValue(instance);
        
        if (result is IValueResolver resolver)
        {
            // Check if resolver is of a Valid Type
            if (resolver.GetType().IsGenericType && resolver.GetType().InheritsFrom(typeof(IValueResolver<>)))
            {
                var fieldType = resolver.GetType().GetGenericArguments().FirstOrDefault();
                if (!fieldType.InheritsFrom(ReferenceKeyType))
                    resolver = null;
            }
        }
        else
        {
            resolver = ValueResolverHelper.CreateDefaultResolver(result);
        }

        return resolver;
    }
    
    public static ValueReferenceFieldData Create(Type conditionType, string name)
    {
        foreach (var field in conditionType.GetFieldsWithAttribute<ValueReferenceAttribute>())
        {
            if (!field.Name.Equals(name, StringComparison.InvariantCulture))
                continue;

            return Create(field);
        }

        throw new InvalidOperationException($"Did not find field {name} for type {conditionType.Name}");
    }

    public static ValueReferenceFieldData Create(FieldInfo field)
    {
        if (!ValueReferenceHelper.TryGetValueReference(field, out ValueReferenceInfo info))
            throw new InvalidOperationException($"FieldInfo {field.Name} of Type {field.DeclaringType?.Name} does not have a valid ValueReferenceAttribute, can't create {nameof(ValueReferenceFieldData)}");
                  
        // TODO: 10/11/2020 what do I do with this
        // if (valRefAttr.ReferenceType == null)
        // {
        //     PLog.Error<VOLTLogger>($"Unable to RegisterDefault for field {field.Name}, ReferenceType specified by string instead of type (str: {valRefAttr.MethodLookUp})");
        //     return false;
        // }
        
        var importValAttr = field.GetCustomAttribute<ImportValueForValueReferenceAttribute>();
            
        return new ValueReferenceFieldData()
        {
            Field = new SerializableFieldInfo(field),
            DefaultKey = info.DefaultName,
            ReferenceKeyType = new SerializableType(info.ReferenceType),
            ImportMemberTarget = importValAttr?.MemberName ?? info.ValueMember?.Name
        };
    }
}