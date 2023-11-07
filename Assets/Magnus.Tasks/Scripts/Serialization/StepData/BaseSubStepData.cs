using System;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus.Tasks
{
    [Serializable, AssignableTypeFilter(Expanded = true)]
    [RefactoringOldNamespace("Rhinox.VOLT.Data", "com.rhinox.volt")]
    public abstract class BaseSubStepData
    {
        public abstract bool HasData();
    }
}