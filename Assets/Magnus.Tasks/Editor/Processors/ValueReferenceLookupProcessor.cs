using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    public class ValueReferenceLookupProcessor : BaseAttributeProcessor<ValueReferenceLookup>
    {
        public override void ProcessType(ref List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new HideDuplicateReferenceBoxAttribute());
        }

        public override void ProcessMember(MemberInfo member, ref List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case nameof(ValueReferenceLookup.ValueResolversByKey):
                    attributes.Add(new DictionaryDrawerSettings {IsReadOnly = true});
                    break;
                case nameof(ValueReferenceLookup.DefaultsByType):
                    attributes.Add(new DictionaryDrawerSettings {IsReadOnly = true});
                    break;
            }
        }
        
#if ODIN_INSPECTOR
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            ProcessMember(member, ref attributes);
            switch (member.Name)
            {
                case nameof(ValueReferenceLookup.ValueResolversByKey):
                    attributes.Add(new LabelTextAttribute(parentProperty.Label.text));
                    break;
            }
        }
#endif
    }
}
