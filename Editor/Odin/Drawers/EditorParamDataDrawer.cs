using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Rhinox.GUIUtils.Odin.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class EditorParamDataDrawer<T> : SimpleOdinValueDrawer<EditorParamData<T>>
    {
        protected override void OnInitialized()
        {
            var value = Property.ValueEntry.WeakSmartValue as EditorParamData<T>;
            var property = Property.FindChild(x => x.Name == nameof(ParamData.MemberData), false);
            property.Label = new GUIContent(value.Name);
        }

        protected override void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<EditorParamData<T>> valueEntry)
        {
            var value = Property.ValueEntry.WeakSmartValue as EditorParamData<T>;
            //EditorGUI.BeginChangeCheck();
            var property = GetChildProperty(nameof(EditorParamData<T>.SmartValue));
            property.Draw(label);

            // Propagate changes to MemberData TODO: do this better
            //if (EditorGUI.EndChangeCheck() && value != null)
            {
                //value.MemberData = value.SmartValue;
            }
        }
    }
}