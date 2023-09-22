using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Rhinox.VOLT.Training;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public abstract class StepDataProcessor<T> : OdinAttributeProcessor<T> where T : StepData
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new HideDuplicateReferenceBoxAttribute());
            // attributes.Add(new InlinePropertyAltAttribute());
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case nameof(StepData.Name):
                    //attributes.Add(new ShowInInspectorAttribute());
                    break;
                case nameof(StepData.Description):
                    attributes.Add(new ShowInInspectorAttribute());
                    attributes.Add(new MultilineAttribute(3));
                    break;
                case nameof(StepData.SubStepData):
                    attributes.Add(new ListDrawerSettingsAttribute { Expanded = true });
                    break;
            }
        }
    }
}