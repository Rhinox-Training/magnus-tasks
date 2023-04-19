using System;
using System.IO;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Utilities;

public struct ValueReferenceInfo
{
    public MemberInfo IdentifierMember;
    public MemberInfo ValueMember;
        
    public Type ReferenceType;
    public string DefaultName;
}

public static class ValueReferenceHelper
{
    public static bool TryGetValueReference(this ValueReferenceAttribute valueRefAttr, Type declaringType, out ValueReferenceInfo info, object instance = null)
    {
        info = new ValueReferenceInfo
        {
            DefaultName = string.Empty
        };
        
        if (valueRefAttr == null)
            return false;

        if (!string.IsNullOrWhiteSpace(valueRefAttr.TargetField))
        {
            if (!ReflectionUtility.TryGetMember(declaringType, valueRefAttr.TargetField, out MemberInfo valueMember))
                throw new InvalidDataException($"Cannot find '{valueRefAttr.TargetField}' in '{declaringType?.Name}'.");
            
            info.ValueMember = valueMember;

            info.ReferenceType = valueMember.GetReturnType();
            info.DefaultName = valueMember.Name;
                
            return true;
        }

        // Resolve type if it is null
        if (valueRefAttr.ReferenceType == null)
        {
            if (string.IsNullOrWhiteSpace(valueRefAttr.TypeLookup))
                throw new InvalidDataException("Cannot find ReferenceType for ValueLookup, MethodLookup is empty");
            
            info.ReferenceType = ReflectionUtility.FetchValuePropertyHelper<Type>(declaringType, valueRefAttr.TypeLookup, instance);
        }
        else
        {
            info.ReferenceType = valueRefAttr.ReferenceType;
        }

        info.DefaultName = valueRefAttr.DefaultKeyName;
        return true;
    }
    
    public static bool TryGetValueReference(MemberInfo member, out ValueReferenceInfo info, object instance = null)
    {
        var valueRefAttr = member != null ? member.GetCustomAttribute<ValueReferenceAttribute>() : null;
        
        var success = TryGetValueReference(valueRefAttr, member?.DeclaringType, out info, instance);
        
        info.IdentifierMember = member;
        
        return success;
    }
}