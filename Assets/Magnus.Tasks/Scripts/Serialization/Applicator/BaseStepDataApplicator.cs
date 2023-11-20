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

        public abstract void Apply(GameObject host, IReferenceResolver hostResolver, ref BaseStep step);

        protected void SetBaseData(BaseStep step)
        {
            step.ID = Data.ID;
            step.Title = Data.Name;
            step.Description = Data.Description;
            
            if (!Data.TagContainer.IsNullOrEmpty())
                step.TagContainer = new TagContainer(Data.TagContainer.Tags);
            
            UnityEventDataUtility.AppendToUnityEvent(step, Data.OnStarted, ref step.StepStarted);
            UnityEventDataUtility.AppendToUnityEvent(step, Data.OnCompleted, ref step.StepCompleted);
        }
        
        
    }
}