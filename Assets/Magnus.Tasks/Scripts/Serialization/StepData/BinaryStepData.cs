namespace Rhinox.Magnus.Tasks
{
    public abstract class BinaryStepData : StepData
    {
        public StepData NextStep;
        public StepData NextStepFailed;
    }
}