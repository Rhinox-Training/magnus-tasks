using System;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    [CustomPropertyDrawer(typeof(ValueReferenceFieldData))]
    public class ValueReferenceFieldDataDrawer : BasePropertyDrawer<ValueReferenceFieldData>
    {
        private TypedHostInfoWrapper<SerializableFieldInfo> _fieldInfoProperty;

        protected override void OnUpdateActiveData()
        {
            base.OnUpdateActiveData();
            HostInfo.TryGetChild<SerializableFieldInfo>(nameof(ValueReferenceFieldData.Field), out _fieldInfoProperty);
        }

        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            Rect left = default, middle = default, right = default;
            if (position.IsValid())
                position.SplitX(0.33f * position.width, 0.66f * position.width, out left, out middle, out right);
        
            GUI.Label(left, _fieldInfoProperty.SmartValue.Name);
            GUI.Label(middle, SmartValue.DefaultKey);
            if (SmartValue.ImportMemberTarget != null)
                GUI.Label(right, SmartValue.ImportMemberTarget);
        }
    }
}