using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    public abstract class StepDataProcessor<T> : BaseAttributeProcessor<T> where T : StepData
    {
        public override void ProcessType(ref List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new HideDuplicateReferenceBoxAttribute());
        }

        public override void ProcessMember(MemberInfo memberInfo, ref List<Attribute> attributes)
        {
            switch (memberInfo.Name)
            {
                case nameof(StepData.TagContainer):
                    attributes.Add(new PropertyOrderAttribute(int.MinValue));
                    attributes.Add(new TitleAttribute("Info"));
                    attributes.Add(new VerticalGroupAttribute("CoreSettings", -100));
                    break;
                case nameof(StepData.ID):
                    attributes.Add(new PropertyOrderAttribute(-1));
                    attributes.Add(new DisplayAsStringAttribute());
                    attributes.Add(new ReadOnlyAttribute());
                    attributes.Add(new VerticalGroupAttribute("CoreSettings", -100));
                    break;
                case nameof(StepData.Name):
                    attributes.Add(new LabelWidthAttribute(50));
                    attributes.Add(new VerticalGroupAttribute("CoreSettings", -100));
                    break;
                case nameof(StepData.Description):
                    attributes.Add(new TextAreaAttribute(1,3));
                    attributes.Add(new ShowInInspectorAttribute());
                    attributes.Add(new VerticalGroupAttribute("CoreSettings", -100));
                    break;
                case nameof(StepData.OnStarted):
                    attributes.Add(new TabGroupAttribute($"${nameof(StepDataExtensions.GetEventsHeader)}"));
                    attributes.Add(new PropertyOrderAttribute(1000));
                    break;
                case nameof(StepData.OnCompleted):
                    attributes.Add(new TabGroupAttribute($"${nameof(StepDataExtensions.GetEventsHeader)}"));
                    attributes.Add(new PropertyOrderAttribute(1000));
                    break;
                case nameof(StepData.SubStepData):
                    attributes.Add(new TabGroupAttribute("Settings"));
                    break;
            }
        }
    }
}