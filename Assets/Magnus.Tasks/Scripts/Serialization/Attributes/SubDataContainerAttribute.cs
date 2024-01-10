using System;

namespace Rhinox.Magnus.Tasks
{
    public class SubDataContainerAttribute : Attribute
    {
        public string ConvertMethodName;

        public SubDataContainerAttribute(string convertMethodName)
        {
            ConvertMethodName = convertMethodName;
        }
    }
}