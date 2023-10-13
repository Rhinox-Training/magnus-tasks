using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public class AutoCompleteSkipperOnLoad : MonoBehaviour, ILevelLoadHandler
    {
        [ValueDropdown(nameof(GetTasks))]
        public BaseTask Task;
        public int StepToSkipTo = -1;

        public bool KillTaskOnCompleted;

        public int LoadOrder => LevelLoadOrder.AUTOCOMPLETE_LOADING;

        public event Action AutoCompleted;

        private int GetStepCount()
        {
            if (!TaskManager.HasInstance)
                return 1; // NOTE: Avoid divide by zero

            int count = 0;
            foreach (var task in TaskManager.Instance.GetTasks())
            {
                if (Task == task)
                {
                    if (StepToSkipTo > 0)
                        count += Mathf.Min(StepToSkipTo, task.Steps.Count);
                    else
                        count += task.Steps.Count;
                    break;
                }
            }

            return Mathf.Max(count, 1); // NOTE: Avoid divide by zero
        }
        
        public IEnumerator<float> OnLevelLoad()
        {
            yield return 0.0f;
            
            if (Task.State != TaskState.Running)
                TaskManager.Instance.ForceStartTask(Task);
            
            float stepCount = GetStepCount();
            while (ShouldAutoCompleteStep())
            {
                // Current task's step is null or completed
                // the system progresses the task the next frame when done, not immediately
                if (!AutoCompletor.Instance.CanRun)
                    yield return GetProgress(0, stepCount);
                else
                {
                    AutoCompletor.Instance.Autocomplete();
                
                    // The step is being autocompleted (this can be for multiple conditions)
                    while (!AutoCompletor.Instance.IsIdle)
                        yield return GetProgress(0, stepCount);
                
                    yield return GetProgress(0, stepCount);
                }
            }
            
            AutoCompleted?.Invoke();

            if (KillTaskOnCompleted)
                Utility.Destroy(Task);

            yield return 1.0f;
        }
        
        private bool ShouldAutoCompleteStep()
        {
            if (!TaskManager.HasInstance || TaskManager.Instance.CurrentTask == null)
                return false;

            if (TaskManager.Instance.CurrentTask != Task)
                return true;

            if (StepToSkipTo < 0)
                return TaskManager.Instance.CurrentTask.CurrentStepId < TaskManager.Instance.CurrentTask.Steps.Count;

            return StepToSkipTo < 0 || TaskManager.Instance.CurrentTask.CurrentStepId < StepToSkipTo;
        }

        private float GetProgress(int start, float end)
        {
            if (!TaskManager.HasInstance || TaskManager.Instance.CurrentTask == null)
                return 0;
            
            if (TaskManager.Instance.CurrentTask != Task)
                return 0;
            
            return (start + TaskManager.Instance.CurrentTask.CurrentStepId) / end;
        }

        private ICollection<ValueDropdownItem> GetTasks()
        {
            if (!TaskManager.HasInstance)
                return Array.Empty<ValueDropdownItem>();

            return TaskManager.Instance.GetTasks().Select(x => new ValueDropdownItem(x.name, x)).ToArray();
        }
    }
}