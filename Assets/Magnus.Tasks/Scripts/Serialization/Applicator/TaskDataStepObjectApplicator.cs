using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [RegisterApplicator(typeof(TaskDataStepObject))]
    public class TaskDataStepObjectApplicator : IStepDataApplicator<TaskDataStepObject>
    {
        public TaskDataStepObject Data { get; private set; }

        public void Init(TaskDataStepObject data)
        {
            Data = data;
            data.RefreshTaskData();
        }
        
        public void Apply(GameObject host, IReferenceResolver hostResolver, ref BaseStep step)
        {
            var task = host.AddComponentWithInit<SubDataTask>(x =>
            {
                x.TaskId = Data.TaskId;
                // TODO use same reference? create new?
                x.LookupOverride = Data.LookupOverride;
                x.ID = Data.ID;
                
                x.RefreshTaskData();
            });

            if (task.Steps.Any())
            {
                UnityEventDataUtility.AppendToUnityEvent(hostResolver, Data.OnStarted, ref task.Steps.First().StepStarted);
                UnityEventDataUtility.AppendToUnityEvent(hostResolver, Data.OnCompleted, ref task.Steps.Last().StepCompleted);
            }
            else
                PLog.Warn<MagnusLogger>("SubDataTask initialized without steps, some events may not be called.");

            if (!Data.TagContainer.IsNullOrEmpty())
            {
                for (var i = 0; i < task.Steps.Count; i++)
                {
                    var subStep = task.Steps[i];
                    if (subStep.TagContainer == null)
                        subStep.TagContainer = new TagContainer();
                    
                    subStep.TagContainer.AddRange(Data.TagContainer.Tags);
                }
            }
        }

    }
}