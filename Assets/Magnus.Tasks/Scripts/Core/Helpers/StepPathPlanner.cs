using System.Collections.Generic;

namespace Rhinox.Magnus.Tasks
{
    public static class StepPathPlanner
    {
        public static int CalculateDistance(BaseStep startStep, BaseStep endStep)
        {
            var visitedSet = new List<BaseStep>();
            visitedSet.Add(startStep);
            return -1;
        }

        public static int GetTaskLength(ITask task)
        {
            if (task == null || task.StartStep == null)
                return 0;

            var curStep = task.StartStep;
            int i = 0;
            while (curStep != null)
            {
                curStep = curStep.GetNextStep();
                ++i;
            }
            return i;
        }
    }
}