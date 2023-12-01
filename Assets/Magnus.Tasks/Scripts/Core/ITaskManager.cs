namespace Rhinox.Magnus.Tasks
{
    public interface ITaskManager
    {
        void NotifyStepStarted(ITaskState task, BaseStepState baseStep);
        void NotifyStepCompleted(ITaskState task, BaseStepState baseStep);
        void NotifyTaskCompleted(ITaskState task, bool hasFailed);
        void NotifyTaskStopped(ITaskState baseTask);
    }
}