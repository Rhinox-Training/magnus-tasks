using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public static class AutoCompleteSkipperHelper
    {
        
        
        public static bool ShouldAutoCompleteStep(ITask task, SerializableGuid stepIDToSkipTo)
        {
            if (!TaskManager.HasInstance || TaskManager.Instance.CurrentTask == null)
                return false;

            if (TaskManager.Instance.CurrentTask != task)
                return false;

            return stepIDToSkipTo.IsNullOrEmpty() || TaskManager.Instance.CurrentTask.FindStep(stepIDToSkipTo).State == ProcessState.Finished;
        }
        
        public static int CalculateCompletionLength(ITask task, SerializableGuid idToSkipTo)
        {
            if (!TaskManager.HasInstance)
                return 1; // NOTE: Avoid divide by zero

            int count = 0;
            if (TaskManager.Instance.GetTasks().Contains(task))
            {
                
                if (idToSkipTo != null)
                    count = StepPathPlanner.CalculateDistance(task.StartStep, task.FindStep(idToSkipTo));
                else
                    count = StepPathPlanner.GetTaskLength(task);
            }
            else
                count = 1;
            
            return Mathf.Max(count, 1); // NOTE: Avoid divide by zero
        }
    }
}