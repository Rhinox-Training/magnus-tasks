using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{    
    public enum PopulationMode
    {
        None,
        LoadStepsFromChildren,
        LinearAssemblerFromChildren
    }
    
    [SmartFallbackDrawn(false)]
    [RefactoringOldNamespace("Rhinox.VOLT.Training", "com.rhinox.volt.training")]
    public class BasicTask : TaskBehaviour
    {
        public PopulationMode AutoPopulate;

        [HideIf(nameof(_nonePopulation))]
        public bool ClearStepsInTaskData = false;

        private bool _nonePopulation => AutoPopulate == PopulationMode.None;

        protected virtual void Awake()
        {
            CheckTaskObject();
            switch (AutoPopulate)
            {
                case PopulationMode.LoadStepsFromChildren:
                    LoadStepsFromChildren();
                    break;
                case PopulationMode.LinearAssemblerFromChildren:
                    LoadStepsFromChildren();
                    SetStepSequenceLinear();
                    break;
            }
        }

        private void CheckTaskObject()
        {
            if (TaskData == null)
                TaskData = new TaskObject();
            else
            {
                if (ClearStepsInTaskData)
                    TaskData.Steps = new List<StepData>();
            }
        }

        private void LoadStepsFromChildren()
        {
            if (TaskData.Steps == null)
                TaskData.Steps = new List<StepData>();

            var steps = GetComponentsInChildren<StepBehaviour>();
            foreach (var step in steps)
            {
                if (step == null)
                    continue;

                if (step.StepData == null)
                {
                    PLog.Warn<MagnusLogger>($"Step '{step.name}' has no {nameof(StepBehaviour.StepData)} configured, skipping...");
                    continue;
                }

                TaskData.Steps.Add(step.StepData);
            }
        }

        private void SetStepSequenceLinear()
        {
            if (TaskData.StartStep == null)
                TaskData.StartStep = TaskData.Steps.FirstOrDefault();
            for (int i = 0; i < TaskData.Steps.Count; ++i)
            {
                var step = TaskData.Steps[i];
                if (step is BinaryStepData binaryStep)
                {
                    if (i < TaskData.Steps.Count - 1)
                        binaryStep.NextStep = TaskData.Steps[i + 1];
                }
            }
        }
        
        // TODO: Button support needs be handled differently
        //
        // [Button, ShowIf("@ValueReferenceLookup != null")]
        // [TabGroup("Configuration")]
        // private void RefreshValueReferencesInStep()
        // {
        //     var conditionSteps = TaskData.Steps
        //         .OfType<ConditionStepData>()
        //         .ToArray();
        //     
        //     Dictionary<string, object> constantOverridesToImport = new Dictionary<string, object>();
        //     foreach (var conditionStep in conditionSteps)
        //     {
        //         foreach (var condition in conditionStep.Conditions)
        //         {
        //             var condType = condition.GetType();
        //             var valueReferenceData = ValueReferenceHelper.GetValueReferenceDataForCondition(condType);
        //             if (valueReferenceData.Length == 0)
        //                 continue;
        //
        //             foreach (var field in valueReferenceData)
        //             {
        //                 object fieldValue = field.FindImportData(condition);
        //                 
        //                 var key = FindKeyOrCreateOverride(constantOverridesToImport, field, fieldValue);
        //                 constantOverridesToImport.Add(key, fieldValue);
        //             }
        //         }
        //     }
        //
        //     foreach (var key in constantOverridesToImport.Keys)
        //     {
        //         object fieldValue = constantOverridesToImport[key];
        //
        //         if (fieldValue != null)
        //         {
        //             var resolverType = typeof(ConstValueResolver<>).MakeGenericType(fieldValue.GetType());
        //             var resolverInstance = Activator.CreateInstance(resolverType);
        //             var setterField = resolverType.GetField("Value", BindingFlags.Instance | BindingFlags.Public);
        //             setterField.SetValue(resolverInstance, fieldValue);
        //
        //             fieldValue = resolverInstance;
        //         }
        //
        //         this.ValueReferenceLookup.Register(key, fieldValue as IValueResolver);
        //     }
        // }
        //
        // private static string FindKeyOrCreateOverride(Dictionary<string, object> constantOverridesToImport, ValueReferenceFieldData field, object fieldValue)
        // {
        //     string baseKey = field.DefaultKey;
        //     string key = baseKey;
        //     int overrideNumber = 2;
        //     while (constantOverridesToImport.ContainsKey(key) && constantOverridesToImport[key] != fieldValue)
        //     {
        //         key = baseKey + overrideNumber;
        //         overrideNumber++;
        //     }
        //
        //     return key;
        // }
    }
}