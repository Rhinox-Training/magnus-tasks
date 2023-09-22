using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Rhinox.VOLT.Training;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class SerializableGuidDrawer : SimpleOdinValueDrawer<SerializableGuid>
    {
        protected IReferenceResolver _referenceResolver;
        private ValueReferenceAttribute _referenceAttribute;

        protected override void Initialize()
        {
            base.Initialize();
            TryInitializeResolver();
            

            _referenceAttribute = Property.Attributes.OfType<ValueReferenceAttribute>().FirstOrDefault();
        }
        
        private void TryInitializeResolver()
        {
            if (_referenceResolver == null)
                _referenceResolver = Property.FindReferenceResolver();
        }

        protected override void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<SerializableGuid> valueEntry)
        {
            TryInitializeResolver();

            if (_referenceResolver == null)
            {
                // no way to resolve things so just hide it
                if (_referenceAttribute != null)
                    return;
                CallNextDrawer(label);
                return;
            }
            
            // Get Rects
            Rect rect = EditorGUILayout.GetControlRect();
            string labelString = string.Empty;
            if (label != null)
            {
                labelString = label.text;

                if (labelString.EndsWith("Identifier")) // Strip identifier
                {
                    labelString = labelString.Substring(0, labelString.Length - 10);
                    label.text = labelString;
                }

                GetLabelFieldRects(rect, out var labelRect, out var fieldRect);
                EditorGUI.LabelField(labelRect, label);
                rect = fieldRect;
            }

            if (!IsValueReferenceType(out ValueReferenceInfo info) || _referenceResolver == null)
            {
                EditorGUI.LabelField(rect, valueEntry.SmartValue.ToString());
                return;
            }
            
            string displayString = _referenceResolver.FindKey(valueEntry.SmartValue)?.DisplayName ?? "<Empty>";
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
                        valueEntry.SmartValue = x;
                    },
                    x => _referenceResolver.FindKey(x)?.DisplayName ?? "<Empty>");
            }
        }

        private bool IsValueReferenceType(out ValueReferenceInfo info)
        {
            info = default;
            if (_referenceAttribute != null)
            {
                Type declaringType = Property.ParentType;
                return _referenceAttribute.TryGetValueReference(declaringType, out info, Property.Parent.ValueEntry.WeakSmartValue);
            }
            
            // What? Would like some clarification as to why
            if (Property.Parent == null || Property.Parent.Parent == null || Property.Parent.Parent.Parent == null)
                return false;
            
            // Like what is Parent.Parent.Parent supposed to be here?
            // afaik this should work for things other than ConditionData
            var conditionData = (Property.Parent.Parent.Parent.ValueEntry.WeakSmartValue as ConditionData);
            if (conditionData == null)
                return false;
            
            var memberInfo = conditionData.GetMemberInfo(Property.Parent.ValueEntry.WeakSmartValue as ParamData);
            return ValueReferenceHelper.TryGetValueReference(memberInfo, out info, Property.ValueEntry);
        }
    }
}