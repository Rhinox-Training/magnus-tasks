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
        public ITask Task;

        public SerializableGuid StepIDToSkipTo;

        public bool KillTaskOnCompleted;

        public int LoadOrder => LevelLoadOrder.AUTOCOMPLETE_LOADING;

        public event Action AutoCompleted;
        
        public IEnumerator<float> OnLevelLoad()
        {
            yield return 0.0f;
            
            if (Task.State != TaskState.Running)
                TaskManager.Instance.ForceStartTask(Task);
            
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

            if (KillTaskOnCompleted)
                Utility.Destroy(Task);

            yield return 1.0f;
        }

        private float GetProgress(int totalLength)
        {
            if (!TaskManager.HasInstance || TaskManager.Instance.CurrentTask == null)
                return 0;
            
            if (TaskManager.Instance.CurrentTask != Task)
                return 0;

            return StepPathPlanner.CalculateDistance(Task.StartStep, Task.ActiveStep) / (float) totalLength;
        }

        private ICollection<ValueDropdownItem> GetTasks()
        {
            if (!TaskManager.HasInstance)
                return Array.Empty<ValueDropdownItem>();

            return TaskManager.Instance.GetTasks().Select(x => new ValueDropdownItem(x.name, x)).ToArray();
        }
    }
}