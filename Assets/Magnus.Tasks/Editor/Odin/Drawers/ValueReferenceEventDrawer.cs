using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class ValueReferenceEventAttributeProcessor : OdinAttributeProcessor<ValueReferenceEvent>
    {
        protected IReferenceResolver _referenceResolver;

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            // Hide it when there is no context to resolve the event params
            if (!FindContext(property))
                attributes.Add(new HideInInspector());

            base.ProcessSelfAttributes(property, attributes);
        }

        private bool FindContext(InspectorProperty property)
        {
            _referenceResolver = property.FindReferenceResolver();

            return _referenceResolver != null;
        }
    }

    public class ValueReferenceEventDrawer : OdinValueDrawer<ValueReferenceEvent>
    {
        protected override void Initialize()
        {
            base.Initialize();
            var listProp = Property.FindChild(x => x.Name == nameof(ValueReferenceEvent.Events), false);
            if (listProp.ValueEntry.WeakSmartValue == null)
                listProp.ValueEntry.WeakSmartValue = new List<ValueReferenceEventEntry>();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            CallNextDrawer(label);
        }
    }
}