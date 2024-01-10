using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus.Tasks.Editor
{
    public class TaskDataStepObjectProcessor : StepDataProcessor<TaskDataStepObject>
    {
        public override void ProcessMember(MemberInfo memberInfo, ref List<Attribute> attributes)
        {
            base.ProcessMember(memberInfo, ref attributes);
            switch (memberInfo.Name)
            {
                case nameof(TaskDataStepObject.LookupOverride):
                    attributes.Add(new HideIfAttribute($"@{nameof(TaskDataStepObject.TaskId)} < 0"));
                    break;
                case nameof(TaskDataStepObject.TaskId):
                    attributes.Add(new OnValueChangedAttribute(nameof(TaskDataStepObject.RefreshTaskData)));
                    attributes.Add(new DisableInPlayModeAttribute());
                    attributes.Add(new TaskSelectorAttribute());
                    break;
            }
        }
        
    }
}