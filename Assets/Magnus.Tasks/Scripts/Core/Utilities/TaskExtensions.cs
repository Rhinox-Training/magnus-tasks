using Rhinox.Lightspeed;

namespace Rhinox.Magnus.Tasks
{
    public static class TaskExtensions
    {
        public static bool IsRunning(this ITaskObjectState task)
        {
            return task.State != TaskState.None &&
                   task.State != TaskState.Initialized &&
                   task.State != TaskState.Paused;
        }

        public static bool IsFinished(this ITaskObjectState task)
        {
            return task.State == TaskState.Finished;
        }
        
        public static bool DoesActiveStepPrecede(this ITaskObjectState task, SerializableGuid specificStep)
        {
            if (!task.IsRunning())
                return false;

            var targetStep = task.FindStepID(specificStep);
            if (targetStep == null)
                return false;
            
            return StepPathPlanner.CalculateDistance(task.ActiveStepState, null) > 0;
        }

        public static SerializableGuid FindStepID(this ITaskObjectState task, SerializableGuid stepId)
        {
            foreach (var step in task.EnumerateStepNodes())
            {
                if (step != null && step.ID == stepId)
                    return step.ID;
            }

            return null;
        }
        
    }
}