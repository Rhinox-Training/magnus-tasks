using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class ConditionStepObjectDrawer : SimpleOdinValueDrawer<ConditionStepObject>
    {
        private Rect _buttonRect;

        protected override void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<ConditionStepObject> valueEntry)
        {
            this.CallNextDrawer(label);
            
            if (GUILayout.Button("Add Condition"))
            {
                if (Event.current.type == EventType.Repaint)
                    _buttonRect = GUILayoutUtility.GetLastRect();
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x => !x.IsAbstract)
                    .Where(x => !x.IsGenericTypeDefinition)
                    .Where(x => typeof(BaseCondition).IsAssignableFrom(x))
                    .ToArray();
                
                DrawTypeDropdown(_buttonRect, types, x =>
                {
                    if (x != null)
                        AddEntry(x);
                });
            }
            else
                if (Event.current.type == EventType.Repaint)
                    _buttonRect = GUILayoutUtility.GetLastRect();
        }

        private void AddEntry(Type type)
        {
            var conditionObj = Activator.CreateInstance(type) as BaseCondition;
            if (conditionObj.OnBetterConditionMet.Events == null)
                conditionObj.OnBetterConditionMet.Events = new List<ValueReferenceEventEntry>();
            var conditionData = ConditionDataHelper.FromCondition(conditionObj);
            var conditionsProperty = Property.FindChild(x => x.Name == nameof(ConditionStepObject.Conditions), false);
            var conditionsValueEntry = conditionsProperty != null ? conditionsProperty.ValueEntry as IPropertyValueEntry<List<ConditionData>> : null;
       
            if (conditionsValueEntry.SmartValue == null)
                conditionsValueEntry.SmartValue = new List<ConditionData>();

            EditorParamDataHelper.ConvertToEditor(ref conditionData);

            conditionsValueEntry.SmartValue.Add(conditionData);
        }
    }
}