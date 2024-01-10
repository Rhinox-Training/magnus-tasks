using System;

namespace Rhinox.Magnus.Tasks
{
    [Serializable]
    public abstract class StepTimingEventData
    {
        public StepTiming Timing = StepTiming.OnStart_EndOnComplete;
    }
}