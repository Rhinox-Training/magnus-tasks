using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public abstract class StepContainer : MonoBehaviour
    {
        public abstract IReadOnlyList<BaseStep> Steps { get; }
        
        public delegate IEnumerator AwaitStepEvent(BaseStep step);

        public event AwaitStepEvent PreStartStep;
        public event AwaitStepEvent PreStopStep;

        public IEnumerable<AwaitStepEvent> GetPreStartStepHandlers()
        {
            return PreStartStep?.GetInvocationList()?.OfType<AwaitStepEvent>();
        }
        
        public IEnumerable<AwaitStepEvent> GetPostStopStepHandlers()
        {
            return PreStopStep?.GetInvocationList()?.OfType<AwaitStepEvent>();
        }

        public abstract void NotifyStepStarted(BaseStep baseStep);
        public abstract void NotifyStepCompleted(BaseStep baseStep);
    }
}