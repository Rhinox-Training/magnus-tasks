using Rhinox.Lightspeed;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus.Tasks
{   
    [HideReferenceObjectPicker]
    [RefactoringOldNamespace("Rhinox.VOLT.Training", "com.rhinox.volt.training")]
    public abstract class BaseStepTimingEvent : IStepTimingEvent
    {
        public StepTiming Timing = StepTiming.OnStart_EndOnComplete;

        protected BaseStep _step;

        public virtual void Initialize(BaseStep step)
        {
            _step = step;

            // handle execution
            switch (Timing)
            {
                case StepTiming.OnStart:
                    if (_step.State == ProcessState.Running)
                        Execute();
                    else
                        _step.StepStarted.AddListener(Execute);
                    break;
                case StepTiming.OnComplete:
                    _step.StepCompleted.AddListener(Execute);
                    break;
                case StepTiming.OnStart_EndOnComplete:
                    _step.StepStarted.AddListener(Execute);
                    _step.StepCompleted.AddListener(StopExecution);
                    break;
            }
        }

        protected abstract void Execute();

        protected abstract void StopExecution();

#if UNITY_EDITOR
        [ShowInInspector, DisplayAsString, PropertyOrder(-1), HideLabel]
        public string Title => GetType().FullName;
#endif
    }
}