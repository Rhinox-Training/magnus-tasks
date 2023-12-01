using System.Collections;
using System.Collections.Generic;

namespace Rhinox.Magnus.Tasks
{
    
    public interface ITaskState
    {
        StepData StartStep { get; }
        IEnumerable<StepData> EnumerateStepNodes();
        ITagContainer TagContainer { get; }

        // State? TODO: migrate to separate thing?
        TaskState State { get; }
        BaseStepState ActiveStep { get; }
        
        void NotifyStepStarted(BaseStepState baseStep);
        void NotifyStepCompleted(BaseStepState baseStep);
    }
}