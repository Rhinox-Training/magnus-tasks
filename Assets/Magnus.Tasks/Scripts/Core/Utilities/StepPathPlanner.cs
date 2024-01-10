using System.Collections.Generic;

namespace Rhinox.Magnus.Tasks
{
    public static class StepPathPlanner
    {
        public static int CalculateDistance(BaseStepState startStep, BaseStepState endStep)
        {
            var visitedSet = new List<BaseStepState>();
            visitedSet.Add(startStep);
            return -1;
        }

        public static int GetTaskLength(ITaskObjectState task)
        {
            // TODO: 
            // 1. Get current step length? Maybe add deduplication (by fetching state list from task)
            // 2. Project stepData objects forward till the end
            if (task == null || task.StartStep == null)
                return 0;

            var curStep = task.StartStep;
            int i = 0;
            while (curStep != null)
            {
                curStep = curStep.NextStep;
                ++i;
            }
            return i;
        }
    }
}