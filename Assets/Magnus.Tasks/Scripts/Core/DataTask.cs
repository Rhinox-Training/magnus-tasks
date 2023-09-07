using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Magnus.Tasks;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Rhinox.Vortex;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.VOLT.Training
{
    public interface ITask
    {
        bool IsActive { get; }
        
        IReadOnlyList<BaseStep> Steps { get; }
        
        // TODO: currently returns true when the active step is the given step. Does this make sense?
        bool HasPassed(BaseStep step);
    }
    
    public interface IDataTaskIdentifier : ITask
    {
        TaskObject GetDataTask();

        int GetTaskId();
    }
    
    // TODO: do we even need this
    [SerializedGuidProcessor(nameof(LookupOverride))]
    public class DataTask : BaseTask, IDataTaskIdentifier, IValueReferenceResolverProvider
    {
        [TaskSelector]
        [OnValueChanged(nameof(RefreshTaskData))]
        public int TaskId = -1;
        
        [HideIf("@TaskId < 0")]
        public ValueReferenceLookupOverride LookupOverride;

        private IReadOnlyList<BaseStep> _generatedSteps;
        public override IReadOnlyList<BaseStep> Steps => _generatedSteps;

        [StepSelector(nameof(TaskId))]
        public SerializableGuid EndStep { get; set; } // Destroy everything after this step TODO do it better

        protected override void Awake()
        {
            // Generate the steps based on the DataTask
            RefreshTaskData();
            _generatedSteps = GenerateSteps();
            
            base.Awake();
        }

        private void RefreshTaskData()
        {
            if (TaskId < 0) return;
            
            // Find parent resolver, not including self TODO: improve
            IReferenceResolver parentResolver = null;
            if (transform.parent)
            {
                var parentResolverProvider = transform.parent.GetComponentInParent<IValueReferenceResolverProvider>();
                if (parentResolverProvider != null)
                    parentResolver = parentResolverProvider.GetReferenceResolver();
            }

            var currentOverride = LookupOverride?.Overrides;

            PLog.Info<MagnusLogger>($"Initializing DataTask '{this.name}' from data id: {TaskId}");
            LookupOverride = new ValueReferenceLookupOverride(TaskId)
            {
                Overrides = currentOverride,
                OverridesResolver = parentResolver
            };
        }

        private IReadOnlyList<BaseStep> GenerateSteps()
        {
            if (TaskId < 0)
            {
                PLog.Warn<MagnusLogger>($"Skipped GenerateSteps due to TaskId == 'TaskId'");
                return Array.Empty<BaseStep>();
            }

            var dataTask = GetDataTask();

            PLog.Info<MagnusLogger>($"Generating Steps for '{this.name}'...");
            var steps = TaskObjectUtility.GenerateSteps(dataTask, transform);
            PLog.Info<MagnusLogger>($"Generated {steps.Count} Steps for '{this.name}'");

            foreach (var step in steps)
                step.SetValueResolver(LookupOverride);
            
            // Not all steps generated may be returned directly, some steps may be generated seperatly
            // Therefore, return all children here instead of our list
            // i.e. SubDataTask - this generates its own steps and manages their ValueResolver, we do not want to know of them here.
            var taskSteps = transform.GetComponentsInChildren<BaseStep>();

            if (!EndStep.IsNullOrEmpty())
            {
                for (var i = taskSteps.Length - 1; i >= 0; i--)
                {
                    var step = taskSteps[i];
                    if (step.ID == EndStep)
                        break;
                    
                    // Disable or DestroyImmediate; otherwise GetComponentsInChildren will still pick them up
                    step.gameObject.SetActive(false);
                }
            }

            return taskSteps;
        }

        private void OnValidate()
        {
            RefreshTaskData();
        }
        
        public TaskObject GetDataTask()
        {
            var table = DataLayer.GetTable<TaskObject>();
            return table.GetData(TaskId);
        }

        public int GetTaskId() => TaskId;

        public IReferenceResolver GetReferenceResolver() => LookupOverride;
        
#if UNITY_EDITOR
        [Button(ButtonSizes.Small), PropertySpace(5), HideInPlayMode]
        private void Unfold()
        {
            var task = gameObject.AddComponent<BasicTask>();
            var steps = GenerateSteps();
            
            // Resolve all guids
            // TODO: manage this properly
            foreach (var step in steps.OfType<ConditionStep>())
            {
                foreach (var condition in step.Conditions)
                {
                    var type = condition.GetType();
                    var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                    var guidFields = fields
                        .Where(x => x.FieldType == typeof(SerializableGuid))
                        .ToArray();

                    foreach (var guidField in guidFields)
                    {
                        var name = guidField.Name.Replace("Identifier", "");
                        // try to find the matching field TODO: do this better
                        var targetField = fields.FirstOrDefault(x => x.Name == name);
                        if (targetField == null)
                        {
                            Debug.LogWarning($"Could not resolve {type.Name}::{guidField.Name}");
                            continue;
                        }

                        var guid = (SerializableGuid) guidField.GetValue(condition);
                        LookupOverride.Resolve(guid, out object value);
                        
                        targetField.SetValue(condition, value);
                    }
                }
            }
            
            DestroyImmediate(this);
        }
#endif
    }
}