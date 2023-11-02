using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [SmartFallbackDrawn(false)]
    [RefactoringOldNamespace("Rhinox.VOLT.Training", "com.rhinox.volt.training")]
    public class BasicTask : TaskBehaviour
    {
        [TabGroup("Configuration")]
        public ValueReferenceLookup ValueReferenceLookup;

        [ShowInInspector, ReadOnly, HideInEditorMode, TabGroup("Configuration"), Space] 
        private BaseStep[] _steps;

        public override IEnumerable<BaseStep> EnumerateStepNodes()
        {
            return  _initialized ? _steps : GetComponentsInChildren<BaseStep>();
        }
        
        protected override void OnPreInitialize()
        {
            base.OnPreInitialize();
            _steps = GetComponentsInChildren<BaseStep>();
        }
        
        [Button, ShowIf("@ValueReferenceLookup != null")]
        [TabGroup("Configuration")]
        private void RefreshValueReferencesInStep()
        {
            var conditionSteps = EnumerateStepNodes()
                .OfType<ConditionStep>()
                .Where(x => x.gameObject.activeSelf)
                .ToArray();
            
            Dictionary<string, object> constantOverridesToImport = new Dictionary<string, object>();
            foreach (var conditionStep in conditionSteps)
            {
                foreach (var condition in conditionStep.Conditions)
                {
                    var condType = condition.GetType();
                    var valueReferenceData = ValueReferenceHelper.GetValueReferenceDataForCondition(condType);
                    if (valueReferenceData.Length == 0)
                        continue;

                    foreach (var field in valueReferenceData)
                    {
                        object fieldValue = field.FindImportData(condition);
                        
                        var key = FindKeyOrCreateOverride(constantOverridesToImport, field, fieldValue);
                        constantOverridesToImport.Add(key, fieldValue);
                    }
                }
            }

            foreach (var key in constantOverridesToImport.Keys)
            {
                object fieldValue = constantOverridesToImport[key];

                if (fieldValue != null)
                {
                    var resolverType = typeof(ConstValueResolver<>).MakeGenericType(fieldValue.GetType());
                    var resolverInstance = Activator.CreateInstance(resolverType);
                    var setterField = resolverType.GetField("Value", BindingFlags.Instance | BindingFlags.Public);
                    setterField.SetValue(resolverInstance, fieldValue);

                    fieldValue = resolverInstance;
                }

                this.ValueReferenceLookup.Register(key, fieldValue as IValueResolver);
            }
        }

        private static string FindKeyOrCreateOverride(Dictionary<string, object> constantOverridesToImport, ValueReferenceFieldData field, object fieldValue)
        {
            string baseKey = field.DefaultKey;
            string key = baseKey;
            int overrideNumber = 2;
            while (constantOverridesToImport.ContainsKey(key) && constantOverridesToImport[key] != fieldValue)
            {
                key = baseKey + overrideNumber;
                overrideNumber++;
            }

            return key;
        }
    }
}