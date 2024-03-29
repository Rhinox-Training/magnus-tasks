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
        public static List<BaseStep> GenerateSteps(TaskObject dataTask, Transform parent)
        {
            PLog.Trace<VortexLogger>($"Generating steps from '{dataTask.Name}'. {dataTask.Steps?.Count} Steps found.");

            var list = new List<BaseStep>();

            if (dataTask.Steps == null)
                return list;
            
            SceneHierarchyTree.Freeze();
            for (var i = 0; i < dataTask.Steps.Count; i++)
            {
                StepData stepData = dataTask.Steps[i];
                BaseStep step = null;
                string idName = "Step";
                if (stepData is TaskDataStepObject) idName = "Sub";
                var stepGo = Utility.Create($"{idName}-{i+1:000} - {stepData.Name}", parent);
                TryApplyStepData(stepData, dataTask.Lookup, stepGo, ref step);

                if (stepData.SubStepData != null)
                {
                    foreach (var subData in stepData.SubStepData)
                        TryApplyStepData(subData, dataTask.Lookup, stepGo, ref step);
                }

                if (step != null)
                    list.Add(step);
            }
            SceneHierarchyTree.UnFreeze();

            return list;
        }

        private static bool TryApplyStepData(object stepData, IReferenceResolver hostResolver, GameObject stepGo, ref BaseStep step)
        {
            if (StepDataApplicatorFactory.CreateApplicator(stepData, out IStepDataApplicator applicator))
            {
                applicator.Apply(stepGo, hostResolver, ref step);
                return true;
            }
            
            PLog.Error<VortexLogger>($"Could not find an applicator for '{stepData}'.");
            return false;
        }

        public static bool TryConvertCondition(ConditionData conditionData, out BaseCondition condition)
        {
            condition = null;
            
            if (conditionData.ConditionType == null)
            {
                PLog.Error<VortexLogger>("Invalid condition data (Type == null)");
                return false;
            }

            var conditionType = conditionData.ConditionType.Type;
            if (conditionType == null)
            {
                PLog.Error<VortexLogger>("Invalid condition data (Type not found)");
                return false;
            }

            condition = Activator.CreateInstance(conditionData.ConditionType.Type) as BaseCondition;
            return true;
        }

        private static void AppendToUnityEvent(IReadOnlyReferenceResolver resolver, ValueReferenceEvent e, ref UnityEvent target)
        {
            if (e.Events == null) return;
            
            for (int i = 0; i < e.Events.Count; ++i)
                AppendToUnityEvent(resolver, e.Events[i], ref target);
        }
        
        private static void AppendToUnityEvent(IReadOnlyReferenceResolver resolver, ValueReferenceEventEntry entry, ref UnityEvent target)
        {
            // TODO Should we wait until usage of the event to resolve this variable?
            resolver.Resolve(entry.Target, out object resolvedTarget);
            
            var del = entry.Action.CreateDelegate(resolvedTarget);
            var parameters = entry.Action.GetParameters();
            
            target.AddListener(() => del?.DynamicInvoke(parameters));
        }
        
        private static void AppendToUnityEvent<T>(IReadOnlyReferenceResolver resolver, ValueReferenceEvent e, ref UnityEvent<T> target)
        {
            if (e.Events == null) return;
            
            for (int i = 0; i < e.Events.Count; ++i)
                AppendToUnityEvent(resolver, e.Events[i], ref target);
        }
        
        private static void AppendToUnityEvent<T>(IReadOnlyReferenceResolver resolver, ValueReferenceEventEntry entry, ref UnityEvent<T> target)
        {
            // TODO Should we wait until usage of the event to resolve this variable?
            resolver.Resolve(entry.Target, out T resolvedTarget);
            
            var del = entry.Action.CreateDelegate(resolvedTarget);
            var parameters = entry.Action.GetParameters();
            
            target.AddListener(x => del?.DynamicInvoke(parameters));
        }
        
        private static BetterEventEntry ConvertToBetterEventEntry(IReadOnlyReferenceResolver resolver, ValueReferenceEventEntry entry)
        {
            // TODO Should we wait until usage of the event to resolve this variable?
            resolver.Resolve(entry.Target, out object resolvedTarget);
            var convertedEntry = new BetterEventEntry(entry.Action.CreateDelegate(resolvedTarget), entry.Action.GetParameters());
            
            return convertedEntry;
        }
    }
}