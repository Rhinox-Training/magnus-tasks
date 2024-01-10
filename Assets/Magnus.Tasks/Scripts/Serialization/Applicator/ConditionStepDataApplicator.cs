using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Rhinox.Vortex;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [RegisterApplicator(typeof(ConditionStepData))]
    public class ConditionStepDataApplicator : BaseStepDataApplicator<ConditionStepData>
    {
        public override void Apply(IReferenceResolver hostResolver, ref BaseStepState stepState)
        {
            var conditionStepState = new ConditionStepState();
            conditionStepState.Data = Data;
            
            if (conditionStepState.Conditions == null)
                conditionStepState.Conditions = new List<BaseCondition>();

            conditionStepState.OrderedConditions = Data.OrderedConditions;

            foreach (var conditionData in Data.Conditions)
            {
                var condition = ObjectDataContainer.BuildInstance<BaseCondition>(conditionData);
                if (condition == null)
                    continue;

                conditionStepState.Conditions.Add(condition);
            }
            
            SetBaseData(conditionStepState);
            stepState = conditionStepState;
        }
    }
}