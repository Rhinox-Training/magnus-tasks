using System;

public class ImportToDataLayerAttribute : Attribute
{
    public string MemberName { get; }
    
    public ImportToDataLayerAttribute(string memberName)
    {
        MemberName = memberName;
    }
}