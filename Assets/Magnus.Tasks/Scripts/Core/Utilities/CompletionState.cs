namespace Rhinox.Magnus.Tasks
{ 
    public enum CompletionState
    {
        None,
        Success,
        Failure
    }

    public static class CompletionStateExtensions
    {
        public static bool HasFailed(this CompletionState state)
        {
            return state == CompletionState.Failure;
        }
    }
}