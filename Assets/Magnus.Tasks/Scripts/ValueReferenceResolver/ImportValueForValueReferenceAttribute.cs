using System;

public class ImportValueForValueReferenceAttribute : Attribute
{
    public string MemberName { get; }
    
    public ImportValueForValueReferenceAttribute(string memberName)
    {
        MemberName = memberName;
    }
    
    
}