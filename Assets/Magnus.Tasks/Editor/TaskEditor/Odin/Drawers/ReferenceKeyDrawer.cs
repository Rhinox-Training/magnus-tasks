using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Editor
{
    public class ReferenceKeyDrawer : OdinValueDrawer<ReferenceKey>
    {
        private GUIContent _valueLabel;
        
        protected override void Initialize()
        {
            base.Initialize();
            var tooltip = ValueEntry.SmartValue.Guid.ToString();
            _valueLabel = new GUIContent(ValueEntry.SmartValue.ValueType.Name, tooltip);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();

            if (label != GUIContent.none)
                EditorGUILayout.PrefixLabel(_valueLabel);
            
            ValueEntry.SmartValue.CustomName = EditorGUILayout.TextField(ValueEntry.SmartValue.DisplayName);

            if (ValueEntry.SmartValue.CustomName == ValueEntry.SmartValue.Name)
                ValueEntry.SmartValue.CustomName = null;
            
            EditorGUILayout.EndHorizontal();
        }
    }

    public class ReferenceKeyAttributeProcessor : OdinAttributeProcessor<ReferenceKey>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new BoldLabelAttribute());
            attributes.Add(new InlinePropertyAltAttribute());
        }
    }
}