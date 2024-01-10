using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public class AutoCompleteSkipperHandler : ILevelLoadHandler
    {
        [ValueDropdown(nameof(GetTasks))]
        public ITaskObjectState Task;

        public SerializableGuid StepIDToSkipTo;

        public bool KillTaskOnCompleted;

        public int LoadOrder => LevelLoadOrder.AUTOCOMPLETE_LOADING;

        public event Action AutoCompleted;
        
        public IEnumerator<float> OnLevelLoad()
        {
            yield return 0.0f;
         
            // TODO: forcestart?
            // if (Task.State != TaskState.Running && Task is TaskBehaviour tb)
            //     TaskManager.Instance.ForceStartTask(tb);
            //
            int stepCount = AutoCompleteSkipperHelper.CalculateCompletionLength(Task, StepIDToSkipTo);
            while (Task.DoesActiveStepPrecede(StepIDToSkipTo))
            {
                // Current task's step is null or completed
                // the system progresses the task the next frame when done, not immediately
                if (!AutoCompletor.Instance.CanRun)
                    yield return GetProgress(stepCount);
                else
                {
                    AutoCompletor.Instance.Autocomplete();
                
                    // The step is being autocompleted (this can be for multiple conditions)
                    while (!AutoCompletor.Instance.IsIdle)
                        yield return GetProgress(stepCount);
                
                    yield return GetProgress(stepCount);
                }
            }
            
            AutoCompleted?.Invoke();

            if (KillTaskOnCompleted && Task is TaskBehaviour taskBehaviour)
                Utility.Destroy(taskBehaviour);

            yield return 1.0f;
        }

        private float GetProgress(int totalLength)
        {
            if (!TaskManager.HasInstance)
                return 0;

            return 0; // TODO:
            //return StepPathPlanner.CalculateDistance(Task.StartStep, Task.ActiveStepState) / (float) totalLength;
        }

        private ICollection<ValueDropdownItem> GetTasks()
        {
            if (!TaskManager.HasInstance)
                return Array.Empty<ValueDropdownItem>();

            return TaskManager.Instance.GetTasks().Select(x => new ValueDropdownItem(x.Name, x)).ToArray();
        }
    }
}