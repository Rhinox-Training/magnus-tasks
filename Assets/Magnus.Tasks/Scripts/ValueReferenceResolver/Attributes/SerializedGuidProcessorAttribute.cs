using System;

namespace Rhinox.Magnus.Tasks
{
    public class SerializedGuidProcessorAttribute : Attribute
    {
        public string MemberName { get; }

        public SerializedGuidProcessorAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }
}