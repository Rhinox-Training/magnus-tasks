using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Rhinox.VOLT.Editor
{
    public class ValueReferenceEventProcessor : OdinAttributeProcessor<ValueReferenceEvent>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new HideDuplicateReferenceBoxAttribute());
            attributes.Add(new HideLabelAttribute());
        }
    }
}