using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class ConditionStepObjectProcessor : StepDataProcessor<ConditionStepObject>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(parentProperty, member, attributes);
            switch (member.Name)
            {
                case nameof(ConditionStepObject.Conditions):
                    attributes.Add(new ListDrawerSettingsAttribute()
                    {
                        DraggableItems = false,
                        ShowPaging = true,
                        NumberOfItemsPerPage = 5,
                        Expanded = true,
                        HideAddButton = true
                    });
                    break;
            }
        }
    }
}