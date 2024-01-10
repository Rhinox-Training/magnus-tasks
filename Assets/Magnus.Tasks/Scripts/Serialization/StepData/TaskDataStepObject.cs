using Rhinox.Perceptor;

namespace Rhinox.Magnus.Tasks
{
    // Does not need a SerializedGuidProcessor Attribute, due to never being used to do anything
    public class TaskDataStepObject : StepData
    {
        public int TaskId = -1;
        public ValueReferenceLookupOverride LookupOverride;

        public TaskDataStepObject() : base()
        {
            TagContainer = new TagContainer();
        }
        
        public void RefreshTaskData()
        {
            if (TaskId < 0) return;

            var currentOverride = LookupOverride?.Overrides;

            PLog.Info<MagnusLogger>($"Initializing TaskDataStepObject from data id: {TaskId}");
            LookupOverride = new ValueReferenceLookupOverride(TaskId)
            {
                Overrides = currentOverride
            };
        }
    }
}