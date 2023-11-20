using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
{
    [CustomPropertyDrawer(typeof(SerializableGuid))]
    public class SerializableGuidDrawer : BasePropertyDrawer<SerializableGuid>
    {
        protected IReferenceResolver _referenceResolver;
        private ValueReferenceAttribute _referenceAttribute;

        protected override void OnUpdateActiveData()
        {
            base.OnUpdateActiveData();
            TryInitializeResolver();
            
            _referenceAttribute = HostInfo.GetAttribute<ValueReferenceAttribute>();
        }
        
        private void TryInitializeResolver()
        {
            if (_referenceResolver == null)
                _referenceResolver = this.FindReferenceResolver();
        }

        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            TryInitializeResolver();
            
            // Get Rects
            Rect rect = position;
            string labelString = string.Empty;
            if (label != null)
            {
                labelString = label.text;

                if (labelString.EndsWith("Identifier")) // Strip identifier
                {
                    labelString = labelString.Substring(0, labelString.Length - 10);
                    label.text = labelString;
                }

                rect.SplitX(0.5f * rect.width, out var labelRect, out var fieldRect);
                EditorGUI.LabelField(labelRect, label);
                rect = fieldRect;
            }

            if (!IsValueReferenceType(out ValueReferenceInfo info) || _referenceResolver == null)
            {
                EditorGUI.LabelField(rect, SmartValue.ToString());
                return;
            }
            
            string displayString = _referenceResolver.FindKey(SmartValue)?.DisplayName ?? "<Empty>";
            if (GUI.Button(rect /*fieldValRect*/, displayString))
            {
                IEnumerable<ReferenceKey> keys = info.ReferenceType != null
                    ? (IEnumerable<ReferenceKey>)_referenceResolver.GetKeysFor(info.ReferenceType)
                    : _referenceResolver.GetKeys();
                var guids = keys
                    .Select(x => x.Guid)
                    .ToList();
                
                guids.Insert(0, SerializableGuid.Empty);

                DrawDropdown(rect, labelString, guids, 
                    x =>
                    {
                        SmartValue = x as SerializableGuid;
                    },
                    x => _referenceResolver.FindKey(x as SerializableGuid)?.DisplayName ?? "<Empty>");
            }
        }

        private void DrawDropdown(Rect rect, string labelString, List<SerializableGuid> guids, Action<object> action, Func<object, string> func)
        {
            
        }

        private bool IsValueReferenceType(out ValueReferenceInfo info)
        {
            info = default;
            if (_referenceAttribute != null)
            {
                Type declaringType = HostInfo.Parent.GetReturnType();
                return _referenceAttribute.TryGetValueReference(declaringType, out info, HostInfo.Parent.GetSmartValue<object>());
            }

            return false;
            //
            // // What? Would like some clarification as to why
            // if (Property.Parent == null || Property.Parent.Parent == null || Property.Parent.Parent.Parent == null)
            //     return false;
            //
            // // Like what is Parent.Parent.Parent supposed to be here?
            // // afaik this should work for things other than ConditionData
            // var conditionData = (Property.Parent.Parent.Parent.ValueEntry.WeakSmartValue as ConditionData);
            // if (conditionData == null)
            //     return false;
            //
            // var memberInfo = conditionData.GetMemberInfo(Property.Parent.ValueEntry.WeakSmartValue as ParamData);
            // return ValueReferenceHelper.TryGetValueReference(memberInfo, out info, Property.ValueEntry);
        }
    }
}