using System.Collections.Generic;
using System.Linq;
using Rhinox.Utilities;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Rhinox.Vortex;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Magnus.Tasks
{
    public interface IStepDataApplicator
    {
        void Apply(GameObject host, IReferenceResolver hostResolver, ref BaseStep step);
    }

    public interface IStepDataApplicator<T> : IStepDataApplicator
    {
        void Init(T data);
        
        T Data { get; }
    }

    public static class DataTaskApplicationUtility
    {
        public static void AppendToUnityEvent(IReadOnlyReferenceResolver resolver, ValueReferenceEvent e, ref UnityEvent target)
        {
            if (e.Events == null) return;
            
            for (int i = 0; i < e.Events.Count; ++i)
                AppendToUnityEvent(resolver, e.Events[i], ref target);
        }
        
        public static void AppendToUnityEvent(IReadOnlyReferenceResolver resolver, ValueReferenceEventEntry entry, ref UnityEvent target)
        {
            // TODO: do this in another manner; note: Resolver is not yet ready when this is applied
            // Create local parameters of the things that will be scoped
            var targetGuid = entry.Target;
            var valueRefAction = entry.Action;
            var parameters = entry.Action.GetParameters();

            if (target == null)
                target = new UnityEvent();
            
            target.AddListener(() =>
            {
                // Delay the resolution of the resolver as long as possible
                resolver.Resolve(targetGuid, out object resolvedTarget);
                var del = valueRefAction.CreateDelegate(resolvedTarget);
                for (int i = 0; i < parameters.Length; ++i)
                {
                    if (parameters[i] is ArgumentDataContainer container)
                    {
                        object resolvedParameter = null;
                        if (!container.TryGetData(resolver, ref resolvedParameter))
                            PLog.Error<MagnusLogger>("Failed to resolve dynamic argument.");
                        parameters[i] = resolvedParameter;
                    }
                }
                del?.DynamicInvoke(parameters);
            });
        }
    }
    
    [RegisterApplicator(typeof(StepData))]
    public class BaseStepDataApplicator : IStepDataApplicator<StepData>
    {
        public StepData Data { get; protected set; }

        public void Init(StepData data)
        {
            Data = data;
        }

        public virtual void Apply(GameObject host, IReferenceResolver hostResolver, ref BaseStep step)
        {
            step = host.AddComponent<ConditionStep>();
            Apply(step);
        }

        protected void Apply(BaseStep step)
        {
            step.ID = Data.ID;
            step.Title = Data.Name;
            step.Description = Data.Description;
            
            DataTaskApplicationUtility.AppendToUnityEvent(step, Data.OnStarted, ref step.StepStarted);
            DataTaskApplicationUtility.AppendToUnityEvent(step, Data.OnCompleted, ref step.StepCompleted);
        }
        
        
    }

    [RegisterApplicator(typeof(ConditionStepObject))]
    public class ConditionStepDataApplicator : BaseStepDataApplicator, IStepDataApplicator<ConditionStepObject>
    {
        public new ConditionStepObject Data { get; protected set; }
        
        public void Init(ConditionStepObject data)
        {
            Data = data;
            base.Init(data);
        }

        public override void Apply(GameObject host, IReferenceResolver hostResolver, ref BaseStep step)
        {
            var conditionStep = host.AddComponent<ConditionStep>();
            step = conditionStep;
            
            if (conditionStep.Conditions == null)
                conditionStep.Conditions = new List<BaseCondition>();

            if (!Data.TagContainer.IsNullOrEmpty())
                conditionStep.TagContainer = new TagContainer(Data.TagContainer.Tags);

            conditionStep.OrderedConditions = Data.OrderedConditions;

            foreach (var conditionData in Data.Conditions)
            {
                if (!TaskObjectUtility.TryConvertCondition(conditionData, out BaseCondition condition))
                    continue;

                foreach (var paramData in conditionData.Params)
                {
                    var member = paramData.FindOn(condition, out string errorMessage);
                    if (!errorMessage.IsNullOrEmpty())
                    {
                        PLog.Warn<VortexLogger>(errorMessage);
                        continue;
                    }
                    if (member == null)
                    {
                        PLog.Warn<VortexLogger>($"Could not set param '{paramData.Name}' on type {condition.GetType().Name}.");
                        continue;
                    }
                    member.SetValue(condition, paramData.MemberData);
                }
                conditionStep.Conditions.Add(condition);
            }
            
            base.Apply(conditionStep);
        }
    }

    [RegisterApplicator(typeof(TaskDataStepObject))]
    public class TaskDataStepObjectApplicator : IStepDataApplicator<TaskDataStepObject>
    {
        public TaskDataStepObject Data { get; private set; }

        public void Init(TaskDataStepObject data)
        {
            Data = data;
            data.RefreshTaskData();
        }
        
        public void Apply(GameObject host, IReferenceResolver hostResolver, ref BaseStep step)
        {
            var task = host.AddComponentWithInit<SubDataTask>(x =>
            {
                x.TaskId = Data.TaskId;
                // TODO use same reference? create new?
                x.LookupOverride = Data.LookupOverride;
                x.ID = Data.ID;
                
                x.RefreshTaskData();
            });

            if (task.Steps.Any())
            {
                DataTaskApplicationUtility.AppendToUnityEvent(hostResolver, Data.OnStarted, ref task.Steps.First().StepStarted);
                DataTaskApplicationUtility.AppendToUnityEvent(hostResolver, Data.OnCompleted, ref task.Steps.Last().StepCompleted);
            }
            else
                PLog.Warn<MagnusLogger>("SubDataTask initialized without steps, some events may not be called.");

            if (!Data.TagContainer.IsNullOrEmpty())
            {
                for (var i = 0; i < task.Steps.Count; i++)
                {
                    var subStep = task.Steps[i];
                    if (subStep.TagContainer == null)
                        subStep.TagContainer = new TagContainer();
                    
                    subStep.TagContainer.AddRange(Data.TagContainer.Tags);
                }
            }
        }

    }
}