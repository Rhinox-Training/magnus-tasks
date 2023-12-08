using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
{
    [CustomPropertyDrawer(typeof(ConditionStepData))]
    public class ConditionStepObjectDrawer : BasePropertyDrawer<ConditionStepData, ConditionStepObjectDrawer.DrawerData>
    {
        public class DrawerData
        {
            public GenericHostInfo HostInfo;
            public TypePicker Picker;
            public Type SelectedType;
        }

        protected override float GetPropertyHeight(GUIContent label, in DrawerData data)
        {
            return GetInnerDrawerHeight(label) + CustomGUIUtility.Padding + EditorGUIUtility.singleLineHeight;
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

            var parentHost = data.HostInfo.Parent;
            var isUnityInspectorContext =
                parentHost != null && parentHost.GetReturnType().InheritsFrom(typeof(StepBehaviour));
            if (isUnityInspectorContext && SmartValue.Conditions != null & SmartValue.Conditions.Count > 1)
            {
                var secondOption = buttonRect.MoveDownLine();
                if (GUI.Button(secondOption, "Split Conditions Into Separate Steps"))
                {
                    var stepBehaviour = parentHost.GetSmartValue<StepBehaviour>();
                    SplitConditionsIntoSteps(stepBehaviour.gameObject, ref SmartValue.Conditions);
                    HostInfo.ForceNotifyValueChanged();
                }
            }
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

        private void AddEntry(GenericHostInfo hostInfo, Type type)
        {
            if (type == null)
                return;
            
            var conditionObj = Activator.CreateInstance(type) as BaseCondition;
            if (conditionObj.OnBetterConditionMet.Events == null)
                conditionObj.OnBetterConditionMet.Events = new List<ValueReferenceEventEntry>();
            var conditionData = ConditionDataHelper.FromCondition(conditionObj);

            var stepObject = hostInfo.GetSmartValue<ConditionStepData>();
            if (stepObject.Conditions == null)
                stepObject.Conditions = new List<BaseObjectDataContainer>();
            
            stepObject.Conditions.Add(conditionData);
            hostInfo.ForceNotifyValueChanged();
        }
        
        private void SplitConditionsIntoSteps(GameObject go, ref List<BaseObjectDataContainer> conditions)
        {
            var siblingIndex = go.transform.GetSiblingIndex();
            var newObjects = new GameObject[conditions.Count - 1];

            var foundNumberings = Utility.FindAlphabetNumbering(go.name);
            int baseNumber = 1;
            Group regexGroup = null;
            if (foundNumberings.Length == 1)
            {
                regexGroup = foundNumberings[0];
                baseNumber = Utility.AlphabetToNum(regexGroup.Value);
            }
            // 1 cause we skip the first condition (it is kept on this go)
            for (var i = 1; i < conditions.Count; ++i)
            {
                var nextName = go.name;
                if (regexGroup != null)
                {
                    var alphaNum = Utility.NumToAlphabet(baseNumber + i);
                    nextName = nextName.Replace(regexGroup.Index, regexGroup.Length, alphaNum);
                }

                var newGo = Utility.Create(nextName, go.transform.parent);
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(newGo, "Split Step Conditions");
#endif
                newGo.transform.SetSiblingIndex(siblingIndex + i);
                var newStep = newGo.AddComponent<StepBehaviour>();
                var conditionStepData = new ConditionStepData();
                conditionStepData.Conditions.Add(conditions[i]);

                newObjects[i - 1] = newGo;
            }
            
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(go, "Split Step Conditions");
#endif
            
            // remove conditions from original (don't do it earlier to leave for loop in peace
            conditions.RemoveRange(1, newObjects.Length);
        }
    }
}