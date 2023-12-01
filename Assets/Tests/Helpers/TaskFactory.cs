using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Magnus.Tasks;
using UnityEngine;

namespace Tests.Helpers
{
    public static class TaskFactory
    {
        public static TaskBehaviour BuildBasicTask()
        {
            var taskObject = new GameObject();
            var task = taskObject.AddComponent<BasicTask>();
            return task;
        }

        public static TaskBehaviour BuildBasicTaskFromConditions(params BaseCondition[] conditions)
        {
            var task = BuildBasicTask();

            foreach (var condition in conditions)
            {
                var conditionStep = task.gameObject.AddChildWithComponent<ConditionStepState>();
                conditionStep.Conditions = new List<BaseCondition>();
                conditionStep.Conditions.Add(condition);
            }

            task.GetOrAddComponent<LinearTaskAssembler>();

            return task;
        }
    }
}