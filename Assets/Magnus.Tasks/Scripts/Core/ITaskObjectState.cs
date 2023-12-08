using System.Collections;
using System.Collections.Generic;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus.Tasks
{
    
    public interface ITaskObjectState
    {
        StepData StartStep { get; }
        IEnumerable<StepData> EnumerateStepNodes();
        ITagContainer TagContainer { get; }

        // State? TODO: migrate to separate thing?
        TaskState State { get; }
        BaseStepState ActiveStepState { get; }
        string Name { get; }

        void NotifyStepStarted(BaseStepState baseStep);
        void NotifyStepCompleted(BaseStepState baseStep);
        void StopTask();
        bool StartTask();

        bool IsFor(TaskObject taskData);
        void Update();
        BaseStepState GetStepState(SerializableGuid stepId);
    }
}