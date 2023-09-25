using System;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;

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
    
    [Serializable, AssignableTypeFilter(Expanded = true)]
    [RefactoringOldNamespace("Rhinox.VOLT.Data", "com.rhinox.volt")]
    public abstract class BaseSubStepData
    {
        public abstract bool HasData();
    }
}