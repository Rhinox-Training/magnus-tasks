using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Rhinox.Vortex;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
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
            
            SetBaseData(conditionStep);
        }
    }
}