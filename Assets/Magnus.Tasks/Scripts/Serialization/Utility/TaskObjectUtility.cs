using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.Vortex;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Magnus.Tasks
{
    public static class TaskObjectUtility
    {
        public static List<StepData> GenerateSteps(TaskObject dataTask, Transform parent)
        {
            PLog.Trace<VortexLogger>($"Generating steps from '{dataTask.Name}'. {dataTask.Steps?.Count} Steps found.");

            var list = new List<StepData>();

            if (dataTask.Steps == null)
                return list;
            
            SceneHierarchyTree.Freeze();
            for (var i = 0; i < dataTask.Steps.Count; i++)
            {
                StepData stepData = dataTask.Steps[i];
                string idName = "Step";
                if (stepData is TaskDataStepObject) 
                    idName = "Sub";
                var stepGo = Utility.Create($"{idName}-{i + 1:000} - {stepData.Name}", parent);
                var stepBeh = stepGo.AddComponent<StepBehaviour>();
                stepBeh.StepData = stepData;

                stepBeh.StepData = stepData;

                list.Add(stepData);
            }
            SceneHierarchyTree.UnFreeze();

            return list;
        }

        public static bool TryCreateStepState(StepData stepData, IReferenceResolver referenceResolver, out BaseStepState stepState)
        {
            BaseStepState step = null;
            TryApplyStepData(stepData, referenceResolver, ref step);
            
            if (stepData.SubStepData != null)
            {
                foreach (var subData in stepData.SubStepData)
                    TryApplyStepData(subData, referenceResolver, ref step);
            }

            stepState = step;
            return step != null;
        }

        private static bool TryApplyStepData(object stepData, IReferenceResolver hostResolver, ref BaseStepState step)
        {
            if (StepDataApplicatorFactory.CreateApplicator(stepData, out IStepDataApplicator applicator))
            {
                applicator.Apply(hostResolver, ref step);
                return true;
            }
            
            PLog.Error<VortexLogger>($"Could not find an applicator for '{stepData}'.");
            return false;
        }
    }
}