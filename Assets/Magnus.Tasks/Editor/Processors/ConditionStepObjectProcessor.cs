using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus.Tasks.Editor
{
    public class ConditionStepObjectProcessor : StepDataProcessor<ConditionStepObject>
    {
        public override void ProcessMember(MemberInfo member, ref List<Attribute> attributes)
        {
            base.ProcessMember(member, ref attributes);
            switch (member.Name)
            {
                case nameof(ConditionStepObject.OrderedConditions):
                    attributes.Add(new TabGroupAttribute("Settings"));
                    break;
                case nameof(ConditionStepObject.Conditions):
                    attributes.Add(new TabGroupAttribute("Settings"));
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