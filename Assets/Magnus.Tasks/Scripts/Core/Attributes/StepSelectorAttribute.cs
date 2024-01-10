using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public class StepSelectorAttribute : PropertyAttribute
    {
        public string TaskIdMember;

        public StepSelectorAttribute(string taskId)
        {
            TaskIdMember = taskId;
        }
    }
}