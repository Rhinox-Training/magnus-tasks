using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Magnus.Tasks
{
    [SmartFallbackDrawn(false)]
    [RefactoringOldNamespace("Rhinox.VOLT.Training", "com.rhinox.volt.training")]
    public class BasicTask : BaseTask
    {
        [TabGroup("Configuration")]
        public ValueReferenceLookup ValueReferenceLookup;

        [ShowInInspector, ReadOnly, HideInEditorMode, TabGroup("Configuration"), Space] private BaseStep[] _steps;
        public IReadOnlyList<BaseStep> Steps => _initialized ? _steps : GetComponentsInChildren<BaseStep>();
        
        [TabGroup("Events")] public UnityEvent OnTaskStarted;
        [TabGroup("Events")] public UnityEvent OnTaskStopped;
        [TabGroup("Events")] public UnityEvent OnTaskCompleted;

        protected override void Awake()
        {
            _steps = GetComponentsInChildren<BaseStep>();

            base.Awake();
        }

        protected override void OnStart()
        {
            base.OnStart();
            OnTaskStarted?.Invoke();
        }

        protected override void OnStop()
        {
            base.OnStop();
            OnTaskStopped?.Invoke();
        }

        protected override void OnCompleted()
        {
            base.OnCompleted();
            OnTaskCompleted?.Invoke();
        }

        [Button, ShowIf("@ValueReferenceLookup != null")]
        [TabGroup("Configuration")]
        private void RefreshValueReferencesInStep()
        {
            // TODO: TEMP code needs to be changed when no longer monoBehaviour
            var conditionSteps = GetComponentsInChildren<BaseStep>()
                .OfType<ConditionStep>()
                .Where(x => x.gameObject.activeSelf)
                .ToArray();

            var dict = new Dictionary<Type, ValueReferenceFieldData[]>();
            var conditionTypes = AppDomain.CurrentDomain.GetDefinedTypesOfType<BaseCondition>();
            foreach (var conditionType in conditionTypes)
            {
                List<ValueReferenceFieldData> fieldDatas = new List<ValueReferenceFieldData>();
                foreach (var field in conditionType.GetFieldsWithAttribute<ValueReferenceAttribute>())
                    fieldDatas.Add(ValueReferenceFieldData.Create(field));

                if (fieldDatas.Count > 0)
                {
                    if (!dict.ContainsKey(conditionType))
                        dict.Add(conditionType, fieldDatas.ToArray());
                }
            }

            Dictionary<string, object> constantOverridesToImport = new Dictionary<string, object>();
            foreach (var conditionStep in conditionSteps)
            {
                foreach (var condition in conditionStep.Conditions)
                {
                    var condType = condition.GetType();
                    if (!dict.ContainsKey(condType))
                        continue;

                    var fieldDatas = dict[condType];
                    foreach (var field in fieldDatas)
                    {
                        object fieldValue = field.FindImportData(condition);
                        
                        string baseKey = field.DefaultKey;
                        string key = baseKey;
                        int overrideNumber = 2;
                        while (constantOverridesToImport.ContainsKey(key) && constantOverridesToImport[key] != fieldValue)
                        {
                            key = baseKey + overrideNumber;
                            overrideNumber++;
                        }
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

        public override IEnumerable<BaseStep> GetStepNodes()
        {
            return Steps;
        }
    }
}