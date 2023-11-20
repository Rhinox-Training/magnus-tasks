using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    public class SerializableGuidAttributeProcessor : BaseAttributeProcessor<SerializableGuid>
    {
        protected IReferenceResolver _referenceResolver;
    
        public override void ProcessType(ref List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new HideDuplicateReferenceBoxAttribute());
            attributes.Add(new InlinePropertyAltAttribute());
        }
        
#if ODIN_INSPECTOR
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            ProcessType(ref attributes);
            var isReference = attributes.OfType<ValueReferenceAttribute>().Any();

            // Hide it when there is no context to resolve the event params
            if (!FindContext(property) && isReference)
                attributes.Add(new HideInInspector());
        }
#endif
        
        public override void ProcessMember(MemberInfo member, ref List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case nameof(SerializableGuid.SerializedBytes):
                    attributes.Add(new HideInInspector());
                    break;
                case nameof(SerializableGuid.GuidAsString):
                    attributes.Add(new ShowInInspectorAttribute());
                    attributes.Add(new HideLabelAttribute());
                    break;
            }
        }

#if ODIN_INSPECTOR
        private bool FindContext(InspectorProperty property)
        {
            _referenceResolver = property.FindReferenceResolver();
        
            return _referenceResolver != null;
        }
#endif
    }
}