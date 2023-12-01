using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public abstract class BaseStepDataApplicator<T> : IStepDataApplicator<T> where T : StepData
    {
        public T Data { get; protected set; }

        public void Init(T data)
        {
            Data = data;
        }

        public abstract void Apply(IReferenceResolver hostResolver, ref BaseStepState stepState);

        protected void SetBaseData(BaseStepState step)
        {
            UnityEventDataUtility.AppendToUnityEvent(step, Data.OnStarted, ref step.StepStarted);
            UnityEventDataUtility.AppendToUnityEvent(step, Data.OnCompleted, ref step.StepCompleted);
        }
        
        
    }
}