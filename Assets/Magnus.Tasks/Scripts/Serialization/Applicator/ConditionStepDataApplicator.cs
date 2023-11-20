using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Rhinox.Vortex;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [RegisterApplicator(typeof(ConditionStepObject))]
    public class ConditionStepDataApplicator : BaseStepDataApplicator<ConditionStepObject>
    {
        public override void Apply(GameObject host, IReferenceResolver hostResolver, ref BaseStep step)
        {
            var conditionStep = host.AddComponent<ConditionStep>();
            step = conditionStep;
            
            if (conditionStep.Conditions == null)
                conditionStep.Conditions = new List<BaseCondition>();

            conditionStep.OrderedConditions = Data.OrderedConditions;

            foreach (var conditionData in Data.Conditions)
            {
                var condition = ConditionDataHelper.ToCondition(conditionData);
                if (condition == null)
                    continue;

                conditionStep.Conditions.Add(condition);
            }
            
            SetBaseData(conditionStep);
        }
    }
}