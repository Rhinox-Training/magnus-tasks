using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class ConditionDataProcessor : OdinAttributeProcessor<ConditionData>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new HideDuplicateReferenceBoxAttribute());
            attributes.Add(new InlinePropertyAltAttribute());
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case nameof(ConditionData.ConditionType):
                    attributes.Add(new ShowReadOnlyAttribute());
                    break;
                case nameof(ConditionData.Params):
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