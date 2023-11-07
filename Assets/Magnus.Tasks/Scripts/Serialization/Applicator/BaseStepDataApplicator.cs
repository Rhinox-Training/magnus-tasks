using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [RegisterApplicator(typeof(StepData))]
    public class BaseStepDataApplicator : IStepDataApplicator<StepData>
    {
        public StepData Data { get; protected set; }

        public void Init(StepData data)
        {
            Data = data;
        }

        public virtual void Apply(GameObject host, IReferenceResolver hostResolver, ref BaseStep step)
        {
            step = host.AddComponent<ConditionStep>();
            SetBaseData(step);
        }

        protected void SetBaseData(BaseStep step)
        {
            step.ID = Data.ID;
            step.Title = Data.Name;
            step.Description = Data.Description;
            
            UnityEventDataUtility.AppendToUnityEvent(step, Data.OnStarted, ref step.StepStarted);
            UnityEventDataUtility.AppendToUnityEvent(step, Data.OnCompleted, ref step.StepCompleted);
        }
        
        
    }
}