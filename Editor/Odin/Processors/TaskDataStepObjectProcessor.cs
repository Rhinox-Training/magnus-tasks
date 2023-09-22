using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class TaskDataStepObjectProcessor : StepDataProcessor<TaskDataStepObject>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(parentProperty, member, attributes);
            switch (member.Name)
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