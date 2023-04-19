using System;

public class ValueReferenceAttribute : Attribute
{
    public string TargetField { get; }
    
    public Type ReferenceType { get; }
    public string TypeLookup { get; }

    public string DefaultKeyName { get; private set; }

    public ValueReferenceAttribute(string fieldName)
    {
        TargetField = fieldName;
    }
    
    public ValueReferenceAttribute(Type type, string defaultKeyName)
    {
        ReferenceType = type;
        DefaultKeyName = defaultKeyName;
    }
    
    public ValueReferenceAttribute(string methodNameForTypeLookup, string defaultKeyName)
    {
        TypeLookup = methodNameForTypeLookup;
        DefaultKeyName = defaultKeyName;
    }
}


