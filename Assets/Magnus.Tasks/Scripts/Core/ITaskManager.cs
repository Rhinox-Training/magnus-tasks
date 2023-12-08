namespace Rhinox.Magnus.Tasks
{
    public interface ITaskManager
    {
        void NotifyStepStarted(ITaskObjectState task, BaseStepState baseStep);
        void NotifyStepCompleted(ITaskObjectState task, BaseStepState baseStep);
        void NotifyTaskCompleted(ITaskObjectState task, bool hasFailed);
        void NotifyTaskStopped(ITaskObjectState baseTask);
    }
}