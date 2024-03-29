﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Vortex;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    /// <summary>
    /// Essentially the same as a DataTask but without inheriting from BaseTask.
    /// Meaning it can be slotted underneath another task.
    /// </summary>
    [SerializedGuidProcessor(nameof(LookupOverride))]
    [StepDataGenerator(nameof(ToStepData))]
    public class SubDataTask : MonoBehaviour, IDataTaskIdentifier, IValueReferenceResolverProvider, IIdentifiable
    {
        [TaskSelector, DisableInPlayMode] [OnValueChanged(nameof(RefreshTaskData))]
        public int TaskId = -1;

        [HideIf("@TaskId < 0")] public ValueReferenceLookupOverride LookupOverride;

        private IReadOnlyList<BaseStep> _generatedSteps;
        public IReadOnlyList<BaseStep> Steps => _generatedSteps;

        public SerializableGuid ID { get; set; }

        public bool IsActive => !Steps.IsNullOrEmpty() && Steps.Any(x => x.IsActive);

        protected void Awake()
        {
            RefreshTaskData();

            // Generate the steps based on the DataTask
            _generatedSteps = GenerateSteps();
        }

        public void RefreshTaskData()
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

            PLog.Info<MagnusLogger>($"Initializing SubDataTask '{this.name}' from data id: {TaskId}");
            LookupOverride = new ValueReferenceLookupOverride(TaskId)
            {
                Overrides = currentOverride,
                OverridesResolver = parentResolver
            };
        }

        private ValueDropdownItem[] GetTasks()
        {
            var table = DataLayer.GetTable<TaskObject>();
            if (table == null) return Array.Empty<ValueDropdownItem>();
            return table.GetAllData()
                .Select(x => new ValueDropdownItem(x.Name, x.ID))
                .Prepend(new ValueDropdownItem("None", -1))
                .ToArray();
        }

        private IReadOnlyList<BaseStep> GenerateSteps()
        {
            if (TaskId < 0) return Array.Empty<BaseStep>();

            var dataTask = GetDataTask();
            var steps = TaskObjectUtility.GenerateSteps(dataTask, transform);

            foreach (var step in steps)
                step.SetValueResolver(LookupOverride);

            return steps;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
                RefreshTaskData();
        }

        public bool HasPassed(BaseStep step)
        {
            var stepI = Steps.IndexOf(step);
            var currentStepI = Steps.FindIndex(x => x.IsActive);
            return currentStepI >= stepI;
        }

        private StepData ToStepData()
        {
            return new TaskDataStepObject
            {
                TaskId = TaskId,
                LookupOverride = LookupOverride,
                ID = ID
            };
        }

        public TaskObject GetDataTask()
        {
            var table = DataLayer.GetTable<TaskObject>();
            return table.GetData(TaskId);
        }

        public int GetTaskId() => TaskId;

        public IReferenceResolver GetReferenceResolver() => LookupOverride;
    }
}