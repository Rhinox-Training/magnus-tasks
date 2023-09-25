using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
{
    [CustomPropertyDrawer(typeof(ConditionStepObject))]
    public class ConditionStepObjectDrawer : BasePropertyDrawer<ConditionStepObject, ConditionStepObjectDrawer.DrawerData>
    {
        public class DrawerData
        {
            public GenericHostInfo HostInfo;
            public TypePicker Picker;
            public Type SelectedType;
        }

        protected override DrawerData CreateData(GenericHostInfo info)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => typeof(BaseCondition).IsAssignableFrom(x))
                .ToArray();
                    
            var picker = new TypePicker(types);
            
            var data = new DrawerData()
            {
                HostInfo = info,
                Picker = picker,
                SelectedType = null
            };
            
            picker.OptionSelected += (x) => data.SelectedType = x;
            return data;
        }

        protected override GenericHostInfo GetHostInfo(DrawerData data)
        {
            return data.HostInfo;
        }

        protected override void DrawProperty(Rect position, ref DrawerData data, GUIContent label)
        {
            var shiftedRect = CallInnerDrawer(position, label);

            var buttonRect = shiftedRect.AlignTop(EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, "Add Condition"))
            {
                data.Picker.Show(buttonRect);
            }

            if (Event.current.type == EventType.Layout)
            {
                if (data.SelectedType != null)
                {
                    AddEntry(data.HostInfo, data.SelectedType);
                    data.SelectedType = null;
                }
            }
        }

        private void AddEntry(GenericHostInfo hostInfo, Type type)
        {
            if (type == null)
                return;
            
            var conditionObj = Activator.CreateInstance(type) as BaseCondition;
            if (conditionObj.OnBetterConditionMet.Events == null)
                conditionObj.OnBetterConditionMet.Events = new List<ValueReferenceEventEntry>();
            var conditionData = ConditionDataHelper.FromCondition(conditionObj);

            var stepObject = hostInfo.GetSmartValue<ConditionStepObject>();
            // TODO: enable conversion
            //EditorParamDataHelper.ConvertToEditor(ref conditionData);
            if (stepObject.Conditions == null)
                stepObject.Conditions = new List<ConditionData>();
            stepObject.Conditions.Add(conditionData);
            hostInfo.Apply();
            // var conditionsProperty = Property.FindChild(x => x.Name == nameof(ConditionStepObject.Conditions), false);
            // var conditionsValueEntry = conditionsProperty != null ? conditionsProperty.ValueEntry as IPropertyValueEntry<List<ConditionData>> : null;
            //
            // if (conditionsValueEntry.SmartValue == null)
            //     conditionsValueEntry.SmartValue = new List<ConditionData>();
            //
            // EditorParamDataHelper.ConvertToEditor(ref conditionData);
            //
            // conditionsValueEntry.SmartValue.Add(conditionData);
        }

        protected override float GetPropertyHeight(GUIContent label, in DrawerData data)
        {
            return GetInnerDrawerHeight(label) + CustomGUIUtility.Padding + EditorGUIUtility.singleLineHeight;
        }
    }
}