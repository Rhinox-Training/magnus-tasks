namespace Rhinox.Magnus.Tasks
{
    public enum ProcessState
    {
        None,
        Initialized,
        Loading,
        Running,
        CleaningUp,
        Finished
    }
    
    public enum TaskState
    {
        None,
        Initialized,
        Running,
        Paused,
        Finished
    }

    public static class ProcessStateExtensions
    {
        public static bool HasStarted(this ProcessState state)
        {
            return state > ProcessState.Initialized; // Already started
        }
        
        public static bool IsFinishingOrFinished(this ProcessState state)
        {
            return state == ProcessState.CleaningUp || state == ProcessState.Finished;
        }
    }

    public enum CompletionState
    {
        None,
        Success,
        Failure
    }
}