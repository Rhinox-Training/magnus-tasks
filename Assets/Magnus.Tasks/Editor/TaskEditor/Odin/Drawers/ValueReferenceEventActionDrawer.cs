using System;
using System.Collections.Generic;
using Rhinox.GUIUtils.Odin.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Rhinox.VOLT.Editor
{
    public class ValueReferenceEventActionDrawer<T> : SimpleOdinValueDrawer<T> where T : ValueReferenceEventAction
    {
        protected override void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<T> valueEntry)
        {
            foreach (var child in Property.Children)
            {
                GUILayout.BeginHorizontal();
                child.Draw();
                GUILayout.EndHorizontal();
            }
        }
    }

    public class ValueReferenceEventAttributeProcessor<T> : OdinAttributeProcessor<T> where T : ValueReferenceEventAction
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
        }
    }
}