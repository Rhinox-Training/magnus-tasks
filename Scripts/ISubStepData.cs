using System;
using Rhinox.GUIUtils.Attributes;

namespace Rhinox.VOLT.Data
{
    public class SubDataContainerAttribute : Attribute
    {
        public string ConvertMethodName;

        public SubDataContainerAttribute(string convertMethodName)
        {
            ConvertMethodName = convertMethodName;
        }
    }
    
    [Serializable, AssignableTypeFilter(Expanded = true)]
    public abstract class BaseSubStepData
    {
        public abstract bool HasData();
    }
}