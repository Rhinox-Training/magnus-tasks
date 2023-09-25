using System;

namespace Rhinox.Magnus.Tasks
{
    public class TaskSelectorAttribute : Attribute
    {
        public string Exclude;
    }
    
    public class StepSelectorAttribute : Attribute
    {
        public string TaskIdMember;

        public StepSelectorAttribute(string taskId)
        {
            TaskIdMember = taskId;
        }
    }
}