using System;

namespace Rhinox.Magnus.Tasks
{
    public abstract class BaseBinaryStepState : BaseStepState
    {
        public override StepData GetNextStep()
        {
            if (!(Data is BinaryStepData binaryStepData))
                return base.GetNextStep();
            
            switch (CompletionState)
            {
                case CompletionState.None: // NOTE: when the state was not yet completed assume success for pathing reasons
                case CompletionState.Success:
                    return binaryStepData.NextStep;
                case CompletionState.Failure:
                    return binaryStepData.NextStepFailed;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}