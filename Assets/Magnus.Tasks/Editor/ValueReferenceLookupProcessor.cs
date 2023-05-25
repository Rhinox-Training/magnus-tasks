#if ODIN_INSPECTOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class ValueReferenceLookupProcessor : OdinAttributeProcessor<ValueReferenceLookup>
{
    public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
    {
        attributes.Add(new HideReferenceObjectPickerAttribute());
        attributes.Add(new HideDuplicateReferenceBoxAttribute());
    }

    public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        switch (member.Name)
        {
            case nameof(ValueReferenceLookup.ValueResolversByKey):
                attributes.Add(new LabelTextAttribute(parentProperty.Label.text));
                attributes.Add(new DictionaryDrawerSettings { IsReadOnly = true });
                break;
            case nameof(ValueReferenceLookup.DefaultsByType):
                attributes.Add(new DictionaryDrawerSettings { IsReadOnly = true });
                break;
        }
    }
}
#endif
