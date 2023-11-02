using System.Collections;
using System.Collections.Generic;

namespace Rhinox.Magnus.Tasks
{
    
    public interface ITask
    {
        BaseStep StartStep { get; }
        IEnumerable<BaseStep> EnumerateStepNodes();

        // State? TODO: migrate to separate thing?
        TaskState State { get; }
        BaseStep ActiveStep { get; }
        
        void NotifyStepStarted(BaseStep baseStep);
        void NotifyStepCompleted(BaseStep baseStep);
    }
}