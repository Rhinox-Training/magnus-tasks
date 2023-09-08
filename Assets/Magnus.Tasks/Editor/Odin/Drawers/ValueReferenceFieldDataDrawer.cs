using System;
using System.Reflection;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Editor
{
    public class ValueReferenceFieldDataDrawer : SimpleOdinValueDrawer<ValueReferenceFieldData>
    {
        protected override void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<ValueReferenceFieldData> valueEntry)
        {
            GetChildProperty<SerializableFieldInfo>(nameof(ValueReferenceFieldData.Field), out var fieldInfoProperty);

            
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(fieldInfoProperty.SmartValue.Name);
                GUILayout.Label(valueEntry.SmartValue.DefaultKey);
                if (valueEntry.SmartValue.ImportMemberTarget != null)
                    GUILayout.Label(valueEntry.SmartValue.ImportMemberTarget);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}