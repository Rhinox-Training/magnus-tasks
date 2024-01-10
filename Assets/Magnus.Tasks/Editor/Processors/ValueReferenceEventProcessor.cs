using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus.Tasks.Editor
{
    public class ValueReferenceEventProcessor : BaseAttributeProcessor<ValueReferenceEvent>
    {
        public override void ProcessType(ref List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new HideDuplicateReferenceBoxAttribute());
            attributes.Add(new HideLabelAttribute());
        }
    }
}