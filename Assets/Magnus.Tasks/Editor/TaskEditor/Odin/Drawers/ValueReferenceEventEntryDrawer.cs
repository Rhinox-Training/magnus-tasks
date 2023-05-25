using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Editor
{
    public class ValueReferenceEventEntryDrawer: SimpleOdinValueDrawer<ValueReferenceEventEntry>
    {
        private IReferenceResolver _referenceResolver;
        private ReferenceKey _currentReferenceKey;
        
        protected override void Initialize()
        {
            base.Initialize();
            _referenceResolver = Property.FindReferenceResolver();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var targetProperty = GetChildProperty(nameof(ValueReferenceEventEntry.Target));
            targetProperty.ValueEntry.OnValueChanged += OnTargetValueChanged;
        }

        protected override void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<ValueReferenceEventEntry> valueEntry)
        {
            GUILayout.BeginHorizontal();
            {
                var targetProperty = GetChildProperty<SerializableGuid>(nameof(ValueReferenceEventEntry.Target),
                    out var targetValueEntry);
                if (targetValueEntry.SmartValue == null)
                    targetValueEntry.SmartValue = SerializableGuid.Empty;

                if (!targetValueEntry.SmartValue.Equals(SerializableGuid.Empty) && _referenceResolver != null)
                    _currentReferenceKey = _referenceResolver.FindKey(targetValueEntry.SmartValue);

                GUILayout.BeginHorizontal(GUILayoutOptions.Width(150));
                {
                    targetProperty.Draw(null);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical();
                {
                    var actionProperty = GetChildProperty(nameof(ValueReferenceEventEntry.Action));
                    string selectedEntry = actionProperty.ValueEntry.WeakSmartValue == null ? "<None>" : actionProperty.ValueEntry.WeakSmartValue.GetType().Name;
                    var rect = EditorGUILayout.GetControlRect();
                    if (GUI.Button(rect, selectedEntry))
                    {
                        DrawTypeDropdown(rect, GetEventTypes(_currentReferenceKey.ValueType.Type), (selectedType) =>
                        {
                            if (selectedType.ContainsGenericParameters)
                                selectedType = selectedType.MakeGenericType(_currentReferenceKey.ValueType);
                            actionProperty.ValueEntry.WeakSmartValue = Activator.CreateInstance(selectedType);
                        });
                    }

                    GUILayout.BeginHorizontal();
                    actionProperty.Draw();
                    GUILayout.EndHorizontal();
                    // foreach (var child in actionProperty.Children)
                    // {
                    //     GUILayout.BeginHorizontal();
                    //     child.Draw();
                    //     GUILayout.EndHorizontal();
                    // }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void OnTargetValueChanged(int obj)
        {
            var actionValueEntry = GetChildProperty(nameof(ValueReferenceEventEntry.Action)).ValueEntry;
            GetChildProperty<SerializableGuid>(nameof(ValueReferenceEventEntry.Target), out var targetValueEntry);

            if (!targetValueEntry.SmartValue.Equals(SerializableGuid.Empty))
            {
                var newKey = _referenceResolver.FindKey(targetValueEntry.SmartValue);
                if (!IsNewTypeValidForAction(actionValueEntry, newKey.ValueType.Type))
                    actionValueEntry.WeakSmartValue = null;
                _currentReferenceKey = newKey;
            }
            else
            {
                _currentReferenceKey = null;
            }
        }

        private bool IsNewTypeValidForAction(IPropertyValueEntry actionValueEntry, Type referenceKeyType)
        {
            var action = actionValueEntry.WeakSmartValue;
            if (action == null)
                return true;
            var type = action.GetType();
            if (!type.ImplementsOrInherits(typeof(ValueReferenceEventAction<>)))
            {
                return false;
            }
            else
            {
                return type.GetGenericArguments()[0].ImplementsOrInherits(referenceKeyType);
            }
        }

        private ICollection<Type> GetEventTypes(Type referenceKeyType)
        {
            List<Type> types = new List<Type>();
            foreach (var type in AppDomain.CurrentDomain.GetDefinedTypesOfType<ValueReferenceEventAction>(true))
            {
                if (type.IsInterface || type.IsAbstract || !type.IsPublic)
                    continue;

                if (type.IsGenericTypeDefinition && type.GetGenericArguments().Length == 1)
                {
                    types.Add(type);
                    continue;
                }

                var baseType = type.GetGenericBaseType(typeof(ValueReferenceEventAction<>));
                if (baseType == null)
                    types.Add(type);
                else if (baseType.GetGenericArguments()[0].ImplementsOrInherits(referenceKeyType))
                    types.Add(type);
            }

            return types;
        }
    }
}