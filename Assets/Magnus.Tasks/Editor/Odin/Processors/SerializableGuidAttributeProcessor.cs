using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class SerializableGuidAttributeProcessor : OdinAttributeProcessor<SerializableGuid>
    {
        protected IReferenceResolver _referenceResolver;
    
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new HideDuplicateReferenceBoxAttribute());
            attributes.Add(new InlinePropertyAltAttribute());

            var isReference = attributes.OfType<ValueReferenceAttribute>().Any();

            // Hide it when there is no context to resolve the event params
            if (!FindContext(property) && isReference)
                attributes.Add(new HideInInspector());
        
            base.ProcessSelfAttributes(property, attributes);
        }
        
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
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


        private bool FindContext(InspectorProperty property)
        {
            _referenceResolver = property.FindReferenceResolver();
        
            return _referenceResolver != null;
        }
    }
}