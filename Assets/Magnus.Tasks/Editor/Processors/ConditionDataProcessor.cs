using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    public class ConditionDataProcessor : BaseAttributeProcessor<ConditionData>
    {
        public override void ProcessType(ref List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new HideDuplicateReferenceBoxAttribute());
            attributes.Add(new InlinePropertyAltAttribute());
        }

        public override void ProcessMember(MemberInfo member, ref List<Attribute> attributes)
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