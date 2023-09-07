using System;

namespace Rhinox.VOLT.Data
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