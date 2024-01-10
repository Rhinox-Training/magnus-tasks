namespace Rhinox.Magnus.Tasks
{
    public static class StepDataExtensions
    {
#if UNITY_EDITOR
        public static string GetEventsHeader(this StepData data)
        {
            int totalEventCount = (data.OnStarted.Events?.Count ?? 0) + (data.OnCompleted.Events?.Count ?? 0);
            return  $"Events ({totalEventCount})";
        }
#endif
    }
}