using Rhinox.Lightspeed;

namespace Rhinox.Magnus.Tasks
{
    public static class TaskExtensions
    {
        public static bool IsRunning(this ITaskState task)
        {
            return task.State != TaskState.None &&
                   task.State != TaskState.Initialized &&
                   task.State != TaskState.Paused;
        }

        public static bool IsFinished(this ITaskState task)
        {
            return task.State == TaskState.Finished;
        }
        
        public static bool DoesActiveStepPrecede(this ITaskState task, SerializableGuid specificStep)
        {
            if (!task.IsRunning())
                return false;

            var targetStep = task.FindStep(specificStep);
            if (targetStep == null)
                return false;
            
            return StepPathPlanner.CalculateDistance(task.ActiveStep, targetStep) > 0;
        }

        public static BaseStepState FindStep(this ITaskState task, SerializableGuid stepId)
        {
            foreach (var step in task.EnumerateStepNodes())
            {
                if (step != null && step.ID == stepId)
                    return step;
            }

            return null;
        }
        
    }
}