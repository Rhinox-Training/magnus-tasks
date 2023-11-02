namespace Rhinox.Magnus.Tasks
{
    public interface ITaskManager
    {
        void NotifyStepStarted(ITask task, BaseStep baseStep);
        void NotifyStepCompleted(ITask task, BaseStep baseStep);
        void NotifyTaskCompleted(ITask task, bool hasFailed);
        void NotifyTaskStopped(ITask baseTask);
    }
}